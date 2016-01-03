using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace Xp.Runners
{
    public abstract class ExecutionModel
    {
        private static PassThrough passThrough = new PassThrough();

        /// <summary>Execute the process and return its exitcode</summary>
        public abstract int Execute(Process proc);

        private IStdStreamReader Redirect(StreamReader input, TextWriter output)
        {
            var reader = new StdStreamReader();
            reader.DataReceivedEvent += (sender, e) => output.Write(e.Data);
            reader.Start(input);
            return reader;
        }

        /// <summary>Run the process and return its exitcode</summary>
        protected int Run(Process proc)
        {
            proc.StartInfo.RedirectStandardOutput = !Console.IsOutputRedirected;
            proc.StartInfo.RedirectStandardError = !Console.IsErrorRedirected;

            try
            {
                proc.Start();
                var stdout = Console.IsOutputRedirected ? passThrough : Redirect(proc.StandardOutput, new ANSISupport(Console.Out));
                var stderr = Console.IsErrorRedirected ? passThrough : Redirect(proc.StandardError, new ANSISupport(Console.Error));

                proc.WaitForExit();
                stdout.WaitForEnd();
                stderr.WaitForEnd();
                return proc.ExitCode;
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