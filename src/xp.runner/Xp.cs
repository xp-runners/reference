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
                // TBI new IniConfigSource(new Ini(Paths.Compose(Environment.SpecialFolder.LocalApplicationData, "Xp", ini))),
                new IniConfigSource(new Ini(Paths.Compose(Paths.Binary().DirName(), ini)))
            );
        }

        /// <summary>Entry point</summary>
        public static int Main(string[] args)
        {
            var cmd = new CommandLine(args);
            try
            {
                return cmd.Command.Execute(cmd, TheConfiguration());
            }
            catch (FileNotFoundException e) 
            {
                Console.Error.WriteLine("Executing command {0} raised {1}", cmd.Command, e.Message);
                return 2;
            } 
        }
    }
}