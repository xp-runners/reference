using System;
using Xp.Runners.IO;
using Xp.Runners.Config;

namespace Xp.Runners
{
    public class Xp
    {
        const string ini = "xp.ini";

        /// <summary>Retrieve configuration via xp.ini</summary>
        private static ConfigSource TheConfiguration()
        {
            var home = Environment.GetEnvironmentVariable("HOME");
            return new CompositeConfigSource(
                new EnvironmentConfigSource(),
                new IniConfigSource(new Ini(Paths.Compose(".", ini))),
                null != home ? new IniConfigSource(new Ini(Paths.Compose(home, ".xp", ini))) : null,
                new IniConfigSource(new Ini(Paths.Compose(Environment.SpecialFolder.LocalApplicationData, "Xp", ini))),
                new IniConfigSource(new Ini(Paths.Compose(Paths.Binary().DirName(), ini)))
            );
        }

        /// <summary>Entry point</summary>
        public static int Main(string[] args)
        {
            try
            {
                var cmd = new CommandLine(args);
                return cmd.Execute(TheConfiguration());
            }
            catch (NotImplementedException e)
            {
                Console.Error.WriteLine("Command not implemented: {0}", e.Message);
                return 2;
            }
            catch (EntryPointNotFoundException e)
            {
                Console.Error.WriteLine("Problem executing runtime: {0}", e.Message);
                return 2;
            }
        }
    }
}