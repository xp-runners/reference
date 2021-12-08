using System;
using System.Text;

namespace Xp.Runners
{
    public class Output : IFormattable
    {
        private StringBuilder buffer;

        public Output()
        {
            buffer = new StringBuilder();
        }

        /// <summary>Append a line</summary>
        public Output Line(params string[] args)
        {
            foreach (var s in args)
            {
                buffer.Append(s);
            }
            buffer.Append(Environment.NewLine);
            return this;
        }

        /// <summary>Append @[ORIGIN] in yellow</summary>
        public Output Origin(string origin)
        {
            buffer
                .Append("\x1b[33m@")
                .Append(origin)
                .Append("\x1b[0m")
                .Append(Environment.NewLine)
            ;
            return this;
        }

        /// <summary>Append a message in bold</summary>
        public Output Message(string message)
        {
            buffer
                .Append("\x1b[1m")
                .Append(message)
                .Append("\x1b[0m")
                .Append(Environment.NewLine)
            ;
            return this;
        }

        /// <summary>Display an error message and an optional advice</summary>
        public Output Error(string message, string advice = null)
        {
            buffer
                .Append("\x1b[41;1;37m ERROR \x1b[0;37m ")
                .Append(message)
                .Append("\x1b[0m")
                .Append(Environment.NewLine)
            ;
            if (advice != null)
            {
                buffer.Append(Environment.NewLine).Append(advice.TrimEnd()).Append(Environment.NewLine);
            }
            return this;
        }

        /// <summary>Append a separator line</summary>
        public Output Separator()
        {
            buffer
                .Append("════════════════════════════════════════════════════════════════════════")
                .Append(Environment.NewLine)
                .Append(Environment.NewLine)
            ;
            return this;
        }

        /// <summary>IFormattable implementation</summary>
        public string ToString(string format, IFormatProvider provider)
        {
            return this.buffer.ToString();
        }
    }
}