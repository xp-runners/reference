using System;
using System.IO;
using Xp.Runners.IO;
using Xp.Runners.Commands;
using Xp.Runners.Config;

namespace Xp.Runners
{
    public class Xp
    {
        /// <summary>Entry point</summary>
        public static int Main(string[] args)
        {
            using (new Output())
            {
                try
                {
                    if (File.Exists(ComposerFile.NAME))
                    {
                        return new CommandLine(args, new ComposerFile(ComposerFile.NAME)).Execute();
                    }
                    else
                    {
                        return new CommandLine(args).Execute();
                    }
                }
                catch (CannotExecute e)
                {
                    Console.Error.WriteLine("\x1b[33m@{0}\x1b[0m", e.Origin ?? Paths.Binary());
                    Console.Error.WriteLine(e.Message);
                    return 2;
                }
                catch (EntryPointNotFoundException e)
                {
                    Console.Error.WriteLine("\x1b[33m@{0}\x1b[0m", Paths.Binary());
                    Console.Error.WriteLine("Problem executing runtime: {0}", e.Message);
                    return 2;
                }
            }
        }
    }
}