using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Xp.Runners
{
    class Output : IDisposable
    {
        Encoding original = null;
        TextWriter output = null;
        TextWriter error = null;

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetStdHandle(int handle);

        const int STD_OUTPUT_HANDLE = -11;
        const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        /// <summary>Starts output, enabling ANSI color support when necessary</summary>
        public Output()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                original = Console.OutputEncoding;

                Console.CancelKeyPress += (sender, args) => Console.OutputEncoding = original;
                Console.OutputEncoding = Encoding.UTF8;

                // See "Console Virtual Terminal Sequences"
                // https://msdn.microsoft.com/en-us/library/windows/desktop/mt638032(v=vs.85).aspx
                int mode;
                IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);
                GetConsoleMode(handle, out mode);
                mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                SetConsoleMode(handle, mode);
            }
        }

        /// <summary>Display @[ORIGIN] in yellow</summary>
        public static void Origin(TextWriter output, string origin)
        {
            output.WriteLine("\x1b[33m@{0}\x1b[0m", origin);
        }

        /// <summary>Display a message in bold</summary>
        public static void Message(TextWriter output, string origin)
        {
            output.WriteLine("\x1b[1m{0}\x1b[0m", origin);
        }

        /// <summary>Display a separator line</summary>
        public static void Separator(TextWriter output)
        {
            output.WriteLine("════════════════════════════════════════════════════════════════════════");
            output.WriteLine();
        }

        public void Dispose()
        {
            if (null != original)
            {
                Console.OutputEncoding = original;
            }
            if (null != output)
            {
                Console.SetOut(output);
            }
            if (null != error)
            {
                Console.SetError(error);
            }
        }        
    }
}