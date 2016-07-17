using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Xp.Runners.Exec
{
    public class Schedule
    {
        private TimeSpan wait;
        private Func<int, bool> until;

        private TimeSpan delay;
        private bool condition;
        private Func<DateTime, TimeSpan> diff;

        /// <summary>Returns how long we need to wait</summary>
        public TimeSpan Wait { get { return wait; } }

        /// <summary>Returns stop condition</summary>
        public Func<int, bool> Until { get { return until; } }

        /// <summary>Creates a new schedule from a string</summary>
        public Schedule(string specs): this(specs.Split(','))
        {
        }

        /// <summary>Creates a new schedule from a list of spec</summary>
        public Schedule(IEnumerable<string> specs)
        {
            // Defaults: Run forever without pause
            until = exitcode => false;
            wait = TimeSpan.Zero;
            diff = start => wait;

            foreach (var spec in specs.Where(spec => !String.IsNullOrEmpty(spec)))
            {
                var args = spec.Split(' ');
                if ("forever" == args[0])
                {
                    until = exitcode => false;
                }
                else if ("immediately" == args[0])
                {
                    wait = TimeSpan.Zero;
                }
                else if (args.Length < 2)
                {
                    throw new ArgumentException("Missing argument for spec `" + spec + "'");
                }
                else if ("every" == args[0])
                {
                    wait = SpanFrom(args[1]);
                    diff = start => wait - (DateTime.Now - start);
                }
                else if ("after" == args[0])
                {
                    wait = SpanFrom(args[1]);
                    diff = start => wait;
                }
                else if ("until" == args[0])
                {
                    if ("success" == args[1])
                    {
                        until = exitcode => (exitcode == 0);
                    }
                    else if ("error" == args[1])
                    {
                        until = exitcode => (exitcode != 0);
                    }
                    else
                    {
                        var codes = args[1].Split('|').Select(chunk => Convert.ToInt32(chunk)).ToArray();
                        until = exitcode => (Array.IndexOf(codes, exitcode) != -1);
                    }
                }
                else
                {
                    throw new ArgumentException("Cannot parse spec `" + spec + "', expecting every, after or until");
                }
            }
        }

        /// <summary>Parses a string into a timespan. Accepts mm:ss and hh:mm:ss</summary>
        private TimeSpan SpanFrom(string input)
        {
            var c = input.Split(':');
            switch (c.Length)
            {
                case 2: return new TimeSpan(0, Convert.ToInt32(c[0]), Convert.ToInt32(c[1]));
                case 3: return new TimeSpan(Convert.ToInt32(c[0]), Convert.ToInt32(c[0]), Convert.ToInt32(c[1]));
            }
            throw new FormatException("Cannot parse span `" + input + "'");
        }

        /// <summary>Runs a block and calculates continuation and delay</summary>
        public int Run(Func<int> block, DateTime start)
        {
            int exitcode = block();
            delay = diff(start);
            condition = until(exitcode);
            return exitcode;
        }

        /// <summary>Runs a block and calculates continuation and delay</summary>
        public int Run(Func<int> block)
        {
            return Run(block, DateTime.Now);
        }

        /// <summary>Returns whether execution should continue and waits</summary>
        public bool Continue(Action<TimeSpan> wait)
        {
            if (condition) return false;

            wait(delay);
            return true;
        }

        /// <summary>Returns whether execution should continue</summary>
        public bool Continue()
        {
            return Continue(delay => Thread.Sleep(Convert.ToInt32(delay.TotalMilliseconds)));
        }
    }
}