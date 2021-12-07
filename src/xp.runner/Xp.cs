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
                    Output.Origin(Console.Error, e.Origin ?? Paths.Binary());
                    Output.Message(Console.Error, e.Message);
                    if (e.Advice != null)
                    {
                        Output.Separator(Console.Error);
                        Console.Error.WriteLine(e.Advice.TrimEnd());
                    }
                    return 2;
                }
                catch (EntryPointNotFoundException e)
                {
                    Output.Origin(Console.Error, Paths.Binary());
                    Output.Message(Console.Error, "Problem executing runtime: " + e.Message);
                    return 2;
                }
            }
        }
    }
}