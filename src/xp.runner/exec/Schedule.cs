using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Xp.Runners.Exec
{
    public class Schedule
    {
        const string INTERVAL_FORMAT = "hh\\:mm\\:ss";

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
        public Schedule(string schedule): this(schedule.Split(','))
        {
        }

        /// <summary>Creates a new schedule from a list of definitions</summary>
        public Schedule(IEnumerable<string> schedule)
        {
            // Defaults: Run forever without pause
            until = exitcode => false;
            wait = TimeSpan.Zero;
            diff = start => wait;

            foreach (var definition in schedule.Where(definition => !String.IsNullOrEmpty(definition)))
            {
                var args = definition.Split(' ');
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
                    throw new ArgumentException("Missing argument for definition `" + definition + "'");
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
                    until = ConditionFrom(args[1]);
                }
                else
                {
                    throw new ArgumentException("Cannot parse definition `" + definition + "', expecting every, after or until");
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

        /// <summary>Parses a stop condition from a string</summary>
        private Func<int, bool> ConditionFrom(string input)
        {
            if ("success" == input)
            {
                return exitcode => (exitcode == 0);
            }
            else if ("error" == input)
            {
                return exitcode => (exitcode != 0);
            }
            else
            {
                var codes = input.Split('|').Select(chunk => Convert.ToInt32(chunk)).ToArray();
                return exitcode => (Array.IndexOf(codes, exitcode) != -1);
            }
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
        public bool Continue(Action<TimeSpan> waitFor)
        {
            if (condition) return false;

            if (delay < TimeSpan.Zero)
            {
                Console.WriteLine(
                    "\x1b[33;1mWarning: Execution time exceeded scheduled {0} by {1}, running immediately\x1b[0m",
                    wait.ToString(INTERVAL_FORMAT),
                    delay.Negate().ToString(INTERVAL_FORMAT)
                );
            }
            else
            {
                waitFor(delay);
            }
            return true;
        }

        /// <summary>Returns whether execution should continue</summary>
        public bool Continue()
        {
            return Continue(delay => Thread.Sleep(Convert.ToInt32(delay.TotalMilliseconds)));
        }
    }
}