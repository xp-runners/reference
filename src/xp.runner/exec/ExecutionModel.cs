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

        /// <summary>Run the process and return its exitcode</summary>
        protected int Run(Process proc, Encoding encoding)
        {
            // var original = Console.OutputEncoding;

            proc.StartInfo.RedirectStandardOutput = !Console.IsOutputRedirected;
            proc.StartInfo.RedirectStandardError = !Console.IsErrorRedirected;

            // Console.CancelKeyPress += (sender, args) => Console.OutputEncoding = original;
            // Console.OutputEncoding = encoding;

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
                throw new FileNotFoundException(proc.StartInfo.FileName + ": " + e.Message, e);
            }
            finally
            {
                // Console.OutputEncoding = original;
                proc.Close();
            }
        }
    }
}