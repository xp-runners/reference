using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Xp.Runners.Exec
{
    public abstract class ExecutionModel
    {
        private static PassThrough passThrough = new PassThrough();

        /// <summary>Execute the process and return its exitcode</summary>
        public abstract int Execute(Process proc, Encoding encoding);

        private IStdStreamReader Redirect(StreamReader input, TextWriter output)
        {
            var reader = new StdStreamReader(output.Encoding, this);
            reader.DataReceivedEvent += (sender, e) => output.Write(e.Data);
            reader.Start(input);
            return reader;
        }

        /// <summary>Check whether it's necessary to rewrite ANSI color</summary>
        private bool RewriteANSI(bool redirected)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return !redirected;
            }
            else
            {
                return false;
            }
        }

        /// <summary>Run the process and return its exitcode</summary>
        protected int Run(Process proc, Encoding encoding, Func<int> wait = null)
        {
            Encoding original = null;
            int result;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                original = Console.OutputEncoding;
                Console.CancelKeyPress += (sender, args) => Console.OutputEncoding = original;
                Console.OutputEncoding = encoding;
            }

            proc.StartInfo.RedirectStandardOutput = RewriteANSI(Console.IsOutputRedirected);
            proc.StartInfo.RedirectStandardError = RewriteANSI(Console.IsErrorRedirected);

            try
            {
                proc.Start();
                var stdout = proc.StartInfo.RedirectStandardOutput ? Redirect(proc.StandardOutput, new ANSISupport(Console.Out)) : passThrough;
                var stderr = proc.StartInfo.RedirectStandardError ? Redirect(proc.StandardError, new ANSISupport(Console.Error)) : passThrough;

                if (null == wait)
                {
                    proc.WaitForExit();
                    result = proc.ExitCode;
                }
                else
                {
                    result = wait();
                }

                stdout.WaitForEnd();
                stderr.WaitForEnd();

                return result;
            }
            catch (SystemException e)
            {
                throw new EntryPointNotFoundException(proc.StartInfo.FileName + ": " + e.Message, e);
            }
            finally
            {
                if (original != null) Console.OutputEncoding = original;
                proc.Close();
            }
        }
    }
}