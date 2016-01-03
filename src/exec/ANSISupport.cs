using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Xp.Runners
{
    public class ANSISupport : TextWriter
    {
        private TextWriter output;
        private StringBuilder sequence;
        private int intensity;

        /// <summary>Encoding.</summary>
        public override Encoding Encoding { get { return output.Encoding; } }

        /// <summary>Creates a writer to one of the console streams which supports ANSI color</summary>
        public ANSISupport(TextWriter output)
        {
            this.output = output;
            this.intensity = ANSIColors.DARK;
        }

        private static Dictionary<string, Action<ANSISupport>> SEQUENCES = new Dictionary<string, Action<ANSISupport>>()
        {
            { "0", (context) => Console.ResetColor() },
            { "1", (context) => context.intensity = ANSIColors.BRIGHT },
            { "22", (context) => context.intensity = ANSIColors.DARK },

            { "30", (context) => Console.ForegroundColor = ANSIColors.Lookup[0 + context.intensity] },
            { "31", (context) => Console.ForegroundColor = ANSIColors.Lookup[1 + context.intensity] },
            { "32", (context) => Console.ForegroundColor = ANSIColors.Lookup[2 + context.intensity] },
            { "33", (context) => Console.ForegroundColor = ANSIColors.Lookup[3 + context.intensity] },
            { "34", (context) => Console.ForegroundColor = ANSIColors.Lookup[4 + context.intensity] },
            { "35", (context) => Console.ForegroundColor = ANSIColors.Lookup[5 + context.intensity] },
            { "36", (context) => Console.ForegroundColor = ANSIColors.Lookup[6 + context.intensity] },
            { "37", (context) => Console.ForegroundColor = ANSIColors.Lookup[7 + context.intensity] },

            { "40", (context) => Console.BackgroundColor = ANSIColors.Lookup[0 + context.intensity] },
            { "41", (context) => Console.BackgroundColor = ANSIColors.Lookup[1 + context.intensity] },
            { "42", (context) => Console.BackgroundColor = ANSIColors.Lookup[2 + context.intensity] },
            { "43", (context) => Console.BackgroundColor = ANSIColors.Lookup[3 + context.intensity] },
            { "44", (context) => Console.BackgroundColor = ANSIColors.Lookup[4 + context.intensity] },
            { "45", (context) => Console.BackgroundColor = ANSIColors.Lookup[5 + context.intensity] },
            { "46", (context) => Console.BackgroundColor = ANSIColors.Lookup[6 + context.intensity] },
            { "47", (context) => Console.BackgroundColor = ANSIColors.Lookup[7 + context.intensity] },
        };

        /// <summary>Handles an escape sequence</summary>
        private void Handle(string sequence)
        {
            foreach (var mode in sequence.Split(';'))
            {
                if (SEQUENCES.ContainsKey(mode))
                {
                    SEQUENCES[mode](this);    
                }
            }
        }

        /// <summary>Writes a character to the console</summary>
        public override void Write(char value)
        {
            if ('\x1b' == value)
            {
                sequence = new StringBuilder();
            } 
            else if (sequence == null)
            {
                output.Write(value);
            }
            else if ('m' == value)
            {
                Handle(sequence.ToString().Substring(1));
                sequence = null;
            }
            else if (('0' <= value && value <= '9') || ';' == value || ('[' == value && sequence.Length == 0))
            {
                sequence.Append(value);
            }
            else
            {
                output.Write(sequence);     // Hit an unknown escape sequence, pass through
                sequence = null;
            }
        }
    }
}