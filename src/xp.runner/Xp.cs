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
                        return new CommandLine(args, new ComposerFile(ComposerFile.NAME)).Execute();
                    }
                    else
                    {
                        return new CommandLine(args).Execute();
                    }
                }
                catch (CannotExecute e)
                {
                    Console.Error.Write(new Output().Origin(e.Origin ?? Paths.Binary()).Error(e.Message, e.Advice));
                    return 2;
                }
                catch (EntryPointNotFoundException e)
                {
                    Console.Error.Write(new Output().Origin(Paths.Binary()).Error("Problem executing runtime", e.Message));
                    return 2;
                }
            }
        }
    }
}