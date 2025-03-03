using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Xp.Runners.Exec
{
    public abstract class ExecutionModel
    {

        /// <summary>Returns the model's name</summary>
        public abstract string Name { get; }

        /// <summary>Execute the process and return its exitcode</summary>
        public abstract int Execute(Process proc);

        /// <summary>Run the process and return its exitcode</summary>
        protected int Run(Process proc, Func<int> wait = null)
        {
            proc.StartInfo.RedirectStandardOutput = false;
            proc.StartInfo.RedirectStandardError = false;

            try
            {
                proc.Start();

                if (null == wait)
                {
                    proc.WaitForExit();
                    return proc.ExitCode;
                }
                else
                {
                    return wait();
                }
            }
            catch (SystemException e)
            {
                throw new EntryPointNotFoundException(proc.StartInfo.FileName + ": " + e.Message, e);
            }
            finally
            {
                proc.Close();
            }
        }
    }
}