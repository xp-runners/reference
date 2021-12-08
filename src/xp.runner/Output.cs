using System.IO;

namespace Xp.Runners
{
    public class Output
    {
        private TextWriter writer;

        public Output(TextWriter writer)
        {
            this.writer = writer;
        }

        /// <summary>Append a line</summary>
        public Output Line(params string[] args)
        {
            foreach (var s in args)
            {
                writer.Write(s);
            }
            writer.WriteLine();
            return this;
        }

        /// <summary>Append @[ORIGIN] in yellow</summary>
        public Output Origin(string origin)
        {
            writer.WriteLine("\x1b[33m@{0}\x1b[0m", origin);
            return this;
        }

        /// <summary>Append a message in bold</summary>
        public Output Message(string message)
        {
            writer.WriteLine("\x1b[1m@{0}\x1b[0m", message);
            return this;
        }

        /// <summary>Display an error message and an optional advice</summary>
        public Output Error(string message, string advice = null)
        {
            writer.WriteLine("\x1b[41;1;37m ERROR \x1b[0;37m {0}\x1b[0m", message);
            if (advice != null)
            {
                writer.WriteLine();
                writer.WriteLine(advice.TrimEnd());
            }
            return this;
        }

        /// <summary>Append a separator line</summary>
        public Output Separator()
        {
            writer.WriteLine("════════════════════════════════════════════════════════════════════════");
            writer.WriteLine();
            return this;
        }
    }
}