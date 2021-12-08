using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Xp.Runners.IO
{
    class Shell
    {
        private string executable;
        private string[] arguments;

        /// <summary>Executable file name</summary>
        public string Executable { get { return executable; } }

        /// <summary>Arguments</summary>
        public string[] Arguments { get { return arguments; } }

        /// <summary>Create instance</summary>
        public Shell(string executable) : this(executable, new string[] { }) { }

        /// <summary>Create instance</summary>
        public Shell(string executable, string[] arguments)
        {
            this.executable = executable;
            this.arguments = arguments;
        }

        /// <summary>Tokenize input</summary>
        private static IEnumerable<string> Tokens(string input)
        {
            var offset = 0;
            var result = new StringBuilder("");

            while (offset < input.Length)
            {
                var pos = input.IndexOfAny(new char[] { ' ', '"' }, offset);
                if (-1 == pos)
                {
                    yield return result.Append(input.Substring(offset)).ToString();
                    result.Clear();
                    break;
                }
                else if (input[pos] == ' ')
                {
                    yield return result.Append(input.Substring(offset, pos - offset)).ToString();
                    result.Clear();
                }
                else if (input[pos] == '"')
                {
                    pos = input.IndexOf('"', pos + 1);
                    if (-1 == pos) // An unclosed string: Fix it by appending a closing quote
                    {
                        yield return result.Append(input.Substring(offset)).Append('"').ToString();
                        result.Clear();
                        break;
                    }
                    result.Append(input.Substring(offset, pos - offset + 1));
                }

                offset = pos + 1;
            }

            yield return result.ToString();
        }

        /// <summary>Parse input</summary>
        public static Shell Parse(string executable)
        {
            var components = Tokens(executable).Where(token => !string.IsNullOrEmpty(token)).ToArray();
            switch (components.Length)
            {
                case 0: throw new ArgumentException("Cannot parse empty shell command");
                case 1: return new Shell(components[0].Trim(new char[] { '"' }));
                default: return new Shell(components[0].Trim(new char[] { '"' }), components.Skip(1).ToArray());
            }
        }

        /// <summary>Escape arguments</summary>
        public static IEnumerable<string> Escape(IEnumerable<string> arguments)
        {
            foreach (var arg in arguments)
            {
                if (null == arg)
                {
                    // Skip
                }
                else if (0 == arg.Length || arg.IndexOfAny(new char[] { '?', '*', '>', '<', '|', '"', '^', '~', '\\' }) >= 0)
                {
                    yield return "'" + arg + "'";
                }
                else
                {
                    yield return arg;
                }
            }
        }

        public override int GetHashCode()
        {
            var code = base.GetHashCode() ^ executable.GetHashCode();
            foreach (var argument in arguments)
            {
                code = code * 31 + argument.GetHashCode();
            }
            return code;
        }

        public override bool Equals(Object o)
        {
            if (o is Shell)
            {
                var cmp = o as Shell;
                return executable.Equals(cmp.executable) && arguments.SequenceEqual(cmp.arguments);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(
                "{0}(Executable: {1}, Arguments: [{2}])",
                typeof(Shell),
                executable,
                string.Join(", ", arguments)
            );
        }
    }
}