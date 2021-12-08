using System;
using System.IO;
using System.Collections.Generic;
using Xp.Runners.IO;

namespace Xp.Runners
{
    public class Output
    {
        public TextWriter Writer;

        public Output()
        {
            this.Writer = new StringWriter();
        }

        public Output(TextWriter writer)
        {
            this.Writer = writer;
        }

        /// <summary>Append a line</summary>
        public Output Line(params string[] args)
        {
            foreach (var s in args)
            {
                Writer.Write(s);
            }
            Writer.WriteLine();
            return this;
        }

        /// <summary>Append @[ORIGIN] in yellow</summary>
        public Output Origin(string origin)
        {
            Writer.WriteLine("\x1b[33m@{0}\x1b[0m", origin);
            return this;
        }

        /// <summary>Append a command</summary>
        public Output Command(string command, params string[] args)
        {
            Writer.WriteLine("\x1b[34m{0} {1}\x1b[0m", command, string.Join(" ", Shell.Escape(args)));
            return this;
        }

        /// <summary>Append a header with a separator below</summary>
        public Output Header(string message)
        {
            Writer.WriteLine("\x1b[1m{0}\x1b[0m", message);
            Writer.WriteLine("════════════════════════════════════════════════════════════════════════");
            Writer.WriteLine();
            return this;
        }

        /// <summary>Display an error message and an optional advice</summary>
        public Output Error(string message, string advice = null)
        {
            Writer.WriteLine("\x1b[41;1;37m ERROR \x1b[0;37m {0}\x1b[0m", message);
            if (advice != null)
            {
                Writer.WriteLine();
                Writer.WriteLine(advice.TrimEnd());
            }
            return this;
        }

        /// <summary>Append > [HEADER] and indented lines unless it is empty</summary>
        public Output Section(string header, Output section)
        {

            var s = section.ToString();
            if (s.Length > 0)
            {
                Writer.WriteLine("> {0}", header);
                Writer.WriteLine("  " + s.Replace("\n", "\n  "));
            }
            return this;
        }

        /// <summary>Invoke a given action for each element in the given iteration</summary>
        public Output Each<T>(IEnumerable<T> iteration, Action<Output, T> invoke)
        {
            foreach (var element in iteration)
            {
                invoke(this, element);
            }
            return this;
        }

        /// <summary>Creates a string representation</summary>
        public override string ToString()
        {
            return Writer is StringWriter ? Writer.ToString() : Writer.GetType().Name;
        }
    }
}