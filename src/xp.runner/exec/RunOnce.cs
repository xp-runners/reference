using System.Text;
using System.Diagnostics;

namespace Xp.Runners.Exec
{
    public class RunOnce : ExecutionModel
    {
        /// <summary>Returns the model's name</summary>
        public override string Name { get { return "default"; } }

        /// <summary>Execute the process and return its exitcode</summary>
        public override int Execute(Process proc)
        {
            return Run(proc);
        }
    }
}