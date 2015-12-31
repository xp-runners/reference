using System;

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
        var cmd = new CommandLine(args);

        var command = Type.GetType(cmd.Command.UpperCaseFirst());
        if (null == command)
        {
            Console.Error.WriteLine("Unknown command `{0}`", cmd.Command);
            return 2;
        }

        try
        {
            return (Activator.CreateInstance(command, new object[] { TheConfiguration() }) as Command).Execute(cmd);
        }
        catch (EntryPointNotFoundException e) 
        {
            Console.Error.WriteLine("Executing command {0} raised {1}", cmd.Command, e.Message);
            return 2;
        } 
    }
}