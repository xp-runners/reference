using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace Xp.Runners.Exec
{

    public class Serve : ExecutionModel
    {

        /// <summary>Execute the process and return its exitcode</summary>
        public override int Execute(Process proc, Encoding encoding)
        {
            Run(proc, encoding, () =>
            {
                Console.Out.WriteLine("[xp::serve#{0}] - Press <Enter> to exit", proc.Id);

                System.Threading.Thread.Sleep(1000);
                if (proc.HasExited) return;

                Console.Read();
                proc.Kill();
                Console.Out.WriteLine("[xp::serve#{0}] - Shutting down...", proc.Id);
            });
            return 0;
        }
    }
}