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
            int code = -1;
            do
            {
                var start = DateTime.Now;
                code = Run(proc, encoding);
                var elapsed = DateTime.Now - start;

                if (code > 0)
                {
                    if (elapsed.Seconds < WAIT_FOR_STARTUP)
                    {
                        Console.WriteLine();
                        Console.WriteLine("*** Process exited too quickly, aborting");
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

            return code;
        }
    }
}