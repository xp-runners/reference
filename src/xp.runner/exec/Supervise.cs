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

        /// <summary>Execute the process and return its exitcode</summary>
        public override int Execute(Process proc, Encoding encoding)
        {
            int code = -1;
            do
            {
                if (code > 0)
                {
                    Console.WriteLine("*** Process exited with exitcode {0}, respawning...", code);
                    Thread.Sleep(WAIT_BEFORE_RESPAWN * 1000);
                }

                code = Run(proc, encoding);
            } while (code > 0);

            return code;
        }
    }
}