using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

public abstract class Command
{

    /// <summary>Main script, e.g. "class-main.php". Overwrite in subclasses if necessary!</summary>
    protected virtual string MainFor(CommandLine cmd)
    {
        return Paths.Locate(new string[] { Paths.Binary().DirName() }, new string[] { "class-main.php" }).First();
    }

    /// <summary>Additional modules to load. Overwrite in subclasses if necessary!</summary>
    protected virtual IEnumerable<string> ModulesFor(CommandLine cmd)
    {
        return new string[] { };
    }

    /// <summary>Command line arguments. Overwrite in subclasses if necessary!</summary>
    protected virtual IEnumerable<string> ArgumentsFor(CommandLine cmd)
    {
        return cmd.Arguments;
    }

    /// <summary>Entry point</summary>
    public int Execute(CommandLine cmd, ConfigSource configuration)
    {
        var proc = new Process();
        var runtime = configuration.GetRuntime();

        proc.StartInfo.RedirectStandardOutput = false;
        proc.StartInfo.RedirectStandardError = false;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.FileName = configuration.GetExecutable(runtime) ?? "php";
        proc.StartInfo.Arguments = string.Format(
            "-C -q -d include_path=\".{0}{1}{0}{0}.{0}{2}\" -d encoding=utf-7 -d date.timezone={3} -d magic_quotes_gpc=0 {4} {5}",
            Paths.Separator,
            string.Join(Paths.Separator, configuration.GetUse().Concat(cmd.Options["modules"]).Concat(ModulesFor(cmd))),
            string.Join(Paths.Separator, cmd.Options["classpath"]),
            TimeZoneInfo.Local.Olson() ?? "UTC",
            MainFor(cmd),
            string.Join(" ", ArgumentsFor(cmd).Select(Strings.AsArgument))
        );

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