using System;
using System.IO;
using Xp.Runners.IO;
using Xp.Runners.Commands;
using Xp.Runners.Config;

namespace Xp.Runners
{
    public class Xp
    {

        /// <summary>Creates a command line based on a composer file</summary>
        public static CommandLine WithComposer(string[] args)
        {
            try
            {
                return new CommandLine(args, new ComposerFile(ComposerFile.NAME));
            }
            catch (FileFormatException e)
            {
                Console.Error.WriteLine("Warning: {0}", e.Message);
                // Fall through, continuing as if no composer file was present
                return new CommandLine(args);
            }
        }

        /// <summary>Entry point</summary>
        public static int Main(string[] args)
        {
            using (new ConsoleOutput())
            {
                var commandLine = File.Exists(ComposerFile.NAME) ? WithComposer(args) : new CommandLine(args);

                // Load existing environment files
                commandLine.TryAddEnv(".env");
                var env = Environment.GetEnvironmentVariable("XP_ENV");
                if (null != env)
                {
                    commandLine.TryAddEnv(".env." + env);
                }

                try
                {
                    return commandLine.Execute();
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