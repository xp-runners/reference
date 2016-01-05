using System.Text;
using System.Diagnostics;

namespace Xp.Runners.Exec
{
    public class RunOnce : ExecutionModel
    {
        /// <summary>Execute the process and return its exitcode</summary>
        public override int Execute(Process proc, Encoding encoding)
        {
            return Run(proc, encoding);
        }
    }
}