using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace Xp.Runners.Exec
{
    public class Supervise : ExecutionModel
    {
        const int WAIT_BEFORE_RESPAWN = 1;
        const int WAIT_FOR_STARTUP = 2;

        /// <summary>Execute the process and return its exitcode</summary>
        public override int Execute(Process proc, Encoding encoding)
        {
            var cancel = new ManualResetEvent(false);
            var buffer = new byte[256];
            var stdin = Console.OpenStandardInput();
            var result = stdin.BeginRead(buffer, 0, buffer.Length, ar => cancel.Set(), null);

            proc.StartInfo.RedirectStandardInput = true;
            proc.EnableRaisingEvents = true;
            proc.Exited += (sender, args) => cancel.Set();

            int code = -1;
            do
            {
                var start = DateTime.Now;
                code = Run(proc, encoding, () =>
                {
                    cancel.Reset();
                    proc.StandardInput.Close();
                    cancel.WaitOne();

                    if (result.IsCompleted)
                    {
                        Console.WriteLine("==> Shut down");
                        stdin.EndRead(result);
                        proc.Kill();
                        return 0;
                    }
                    else
                    {
                        proc.WaitForExit();
                        return proc.ExitCode;
                    }
                });
                var elapsed = DateTime.Now - start;

                if (code > 0)
                {
                    if (elapsed.Seconds < WAIT_FOR_STARTUP)
                    {
                        Console.WriteLine();
                        Console.WriteLine("*** Process exited right after being started, aborting");
                        stdin.Close();
                        return code;
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("*** Process exited with exitcode {0}, respawning...", code);
                        Thread.Sleep(WAIT_BEFORE_RESPAWN * 1000);
                    }
                }
            } while (code > 0);

            // Either via user-interactive shutdown or via runtime exiting itself
            stdin.Close();
            return code;
        }
    }
}