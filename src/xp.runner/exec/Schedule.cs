using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Xp.Runners.Exec
{
    public class Schedule
    {
        const string INTERVAL_FORMAT = "hh\\:mm\\:ss";

        // Recalculated on runs
        private TimeSpan delay;
        private bool condition;

        // Updated by "at" mode
        private TimeSpan wait;

        // Unchanged after constructor
        private Func<int, bool> until;
        private Func<DateTime, DateTime, TimeSpan> next;

        /// <summary>Creates a new schedule from a string</summary>
        public Schedule(string schedule): this(schedule.Split(','), DateTime.Now) { }

        /// <summary>Creates a new schedule from a string</summary>
        public Schedule(string schedule, DateTime now): this(schedule.Split(','), now) { }

        /// <summary>Creates a new schedule from a list of definitions</summary>
        public Schedule(IEnumerable<string> schedule): this(schedule, DateTime.Now) { }

        /// <summary>Creates a new schedule from a list of definitions</summary>
        public Schedule(IEnumerable<string> schedule, DateTime now)
        {
            // Defaults: Start running immediately, forever without pause
            delay = TimeSpan.Zero;
            until = exitcode => false;
            wait = TimeSpan.Zero;
            next = (start, end) => TimeSpan.Zero;

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
                    next = (start, end) => wait - (end - start);
                }
                else if ("after" == args[0])
                {
                    wait = SpanFrom(args[1]);
                    next = (start, end) => wait;
                }
                else if ("at" == args[0])
                {
                    var times = args.Skip(1).Select(TimeFrom).ToArray();
                    var offset = 0;

                    // Delay until next
                    next = (start, end) => {
                        wait = times[offset];
                        var wrap = offset > 0 || start.TimeOfDay <= wait ? start : start.AddDays(1);
                        var target = new DateTime(wrap.Year, wrap.Month, wrap.Day, wait.Hours, wait.Minutes, wait.Seconds);

                        Console.WriteLine("> \x1b[32;1mNext run on {0}\x1b[0m", target);
                        if (++offset >= times.Length) offset = 0;
                        return target - end;
                    };

                    // Initial delay
                    delay = next(now, now);
                }
                else if ("until" == args[0])
                {
                    until = ConditionFrom(args[1]);
                }
                else
                {
                    throw new ArgumentException(string.Format(
                        "Cannot parse definition `{0}', expecting one of `forever', `immediately', `every', `after', `at' or `until'",
                        args[0]
                    ));
                }
            }

            // First Continue() should always run
            condition = false;
        }

        /// <summary>Parses a string into a timespan. Accepts mm:ss and hh:mm:ss</summary>
        private TimeSpan SpanFrom(string input)
        {
            var c = input.Split(':');
            switch (c.Length)
            {
                case 2: return new TimeSpan(0, Convert.ToInt32(c[0]), Convert.ToInt32(c[1]));
                case 3: return new TimeSpan(Convert.ToInt32(c[0]), Convert.ToInt32(c[1]), Convert.ToInt32(c[2]));
            }
            throw new FormatException("Cannot parse span `" + input + "'");
        }

        /// <summary>Parses a string into a date. Accepts hh:mm and hh:mm:ss</summary>
        private TimeSpan TimeFrom(string input)
        {
            var c = input.Split(':');
            switch (c.Length)
            {
                case 2: return new TimeSpan(Convert.ToInt32(c[0]), Convert.ToInt32(c[1]), 0);
                case 3: return new TimeSpan(Convert.ToInt32(c[0]), Convert.ToInt32(c[1]), Convert.ToInt32(c[2]));
            }
            throw new FormatException("Cannot parse time `" + input + "'");
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
        public int Run(Func<int> block, DateTime start, TimeSpan runtime)
        {
            var exitcode = block();
            delay = next(start, start + runtime);
            condition = until(exitcode);
            return exitcode;
        }

        /// <summary>Runs a block and calculates continuation and delay</summary>
        public int Run(Func<int> block)
        {
            var start = DateTime.Now;
            var exitcode = block();
            delay = next(start, DateTime.Now);
            condition = until(exitcode);
            return exitcode;
        }

        /// <summary>Returns whether execution should continue and waits</summary>
        public bool Continue(Action<TimeSpan> waitFor)
        {
            if (condition) return false;

            if (delay < TimeSpan.Zero)
            {
                Console.WriteLine(
                    "> \x1b[33;1mExecution time exceeded scheduled {0} by {1}, running immediately\x1b[0m",
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