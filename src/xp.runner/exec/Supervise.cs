using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace Xp.Runners.Exec
{
    public class Supervise : ExecutionModel
    {
        const int WAIT_BEFORE_RESPAWN = 1;
        const int WAIT_FOR_STARTUP = 2;
        private static byte[] QUIT = new byte[] { 81, 85, 73, 84, 13, 10 };  // "QUIT\r\n"

        /// <summary>Returns the model's name</summary>
        public override string Name { get { return "supervise"; } }

        /// <summary>Execute the process and return its exitcode</summary>
        public override int Execute(Process proc)
        {
            Action shutdown = proc.Kill;

            // Read from STDIN
            var cancel = new ManualResetEvent(false);
            var buffer = new byte[256];
            var stdin = Console.OpenStandardInput();
            var result = stdin.BeginRead(buffer, 0, buffer.Length, ar => cancel.Set(), null);

            // Set up signalling socket. Once PHP connects to it, rewrite shutdown action
            // to a graceful variant: Sending a quit message to the signalling socket.
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
            sock.Listen(1);
            sock.BeginAccept(
              ar => {
                  var listener = ((Socket)ar.AsyncState).EndAccept(ar);
                  shutdown = () =>
                  {
                      listener.Send(QUIT);
                      listener.Shutdown(SocketShutdown.Both);
                      listener.Close();
                      proc.WaitForExit();
                  };
              },
              sock
            );

            proc.StartInfo.EnvironmentVariables["XP_SIGNAL"] = ((IPEndPoint)sock.LocalEndPoint).Port.ToString();
            proc.StartInfo.RedirectStandardInput = true;
            proc.EnableRaisingEvents = true;
            proc.Exited += (sender, args) => cancel.Set();

            int code = -1;
            do
            {
                var start = DateTime.Now;
                code = Run(proc, () =>
                {
                    cancel.Reset();
                    proc.StandardInput.Close();
                    cancel.WaitOne();

                    if (result.IsCompleted)
                    {
                        Console.Write("> Shut down ");
                        stdin.EndRead(result);
                        shutdown();
                        return 0;
                    }
                    else
                    {
                        proc.WaitForExit();
                        return proc.ExitCode;
                    }
                });
                var elapsed = DateTime.Now - start;

                if (code != 0)
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
            } while (code != 0);

            // Either via user-interactive shutdown or via runtime exiting itself
            stdin.Close();
            sock.Close();
            return code;
        }
    }
}