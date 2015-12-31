using System;
using System.Diagnostics;

public abstract class Command
{
    private ConfigSource configuration;

    /// The configuration, pointing to an object to access xp.ini
    public ConfigSource Configuration
    {
        get { return configuration; }
    }

    /// The configuration
    public Command(ConfigSource configuration)
    {
        this.configuration = configuration;
    }

    /// Create command line. Overwrite in subclasses!
    protected abstract string ArgumentsFor(CommandLine cmd);

    /// Entry point
    public int Execute(CommandLine cmd)
    {
        var proc = new Process();
        var runtime = Configuration.GetRuntime();

        proc.StartInfo.RedirectStandardOutput = false;
        proc.StartInfo.RedirectStandardError = false;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.FileName = Configuration.GetExecutable(runtime) ?? "php";
        proc.StartInfo.Arguments = ArgumentsFor(cmd);

        try
        {
            proc.Start();
            proc.WaitForExit(); 
            return proc.ExitCode;
        }
        catch (SystemException e) 
        {
            throw new EntryPointNotFoundException(proc.StartInfo.FileName + ": " + e.Message, e);
        } 
        finally
        {
            proc.Close();
        }
    }
}