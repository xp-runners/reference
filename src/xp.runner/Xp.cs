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
            using (new ConsoleOutput())
            {
                try
                {
                    if (File.Exists(ComposerFile.NAME))
                    {
                        try
                        {
                            return new CommandLine(args, new ComposerFile(ComposerFile.NAME)).Execute();
                        }
                        catch (FileFormatException e)
                        {
                            Console.Error.WriteLine("Warning: {0}", e.Message);
                            // Fall through, continuing as if no composer file was present
                        }
                    }

                    return new CommandLine(args).Execute();
                }
                catch (CannotExecute e)
                {
                    new Output(Console.Error).Origin(e.Origin ?? Paths.Binary()).Error(e.Message, e.Advice);
                    return 2;
                }
                catch (EntryPointNotFoundException e)
                {
                    new Output(Console.Error).Origin(Paths.Binary()).Error("Problem executing runtime", e.Message);
                    return 2;
                }
            }
        }
    }
}