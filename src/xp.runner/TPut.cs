using System;
using System.Reflection;
using System.Collections.Generic;

namespace Xp.Runners
{
    public class TPut
    {
        /// <summary>Handles argument handling</summary>
        private static IEnumerable<string> QueriesPassed(string[] args)
        {
            if ("-S" == args[0])
            {
                string line;
                while (null != (line = Console.ReadLine()))
                {
                    yield return line.Trim();
                }
            }
            else
            {
                yield return args[0];
            }
        }

        /// <summary>Drop-in replacement for the `tput` command. Implements
        /// only three capabilites: lines, cols and bel.</summary>
        public static int Main(string[] args)
        {
            var capabilities = new Dictionary<string, Func<object>>()
            {
                { "lines", () => Console.WindowHeight },
                { "cols", () => Console.WindowWidth },
                { "bel", () => { Console.Beep(); return null; }}
            };

            if (0 == args.Length)
            {
                Console.Error.WriteLine("usage: tput [-V] [-S] capname");
                return 1;
            }
            else if ("-V" == args[0])
            {
                Console.WriteLine(Assembly.GetExecutingAssembly().GetName());
                return 0;
            }

            foreach (var query in QueriesPassed(args))
            {
                if (capabilities.ContainsKey(query))
                {
                    Console.WriteLine(capabilities[query]());
                }
                else
                {
                    Console.Error.WriteLine("tput: unknown terminfo capability '{0}'", query);
                    return 1;       
                }
            }
            return 0;
        }
    }
}