using System;
using System.Linq;

/// run <class> <arguments>
public class Run : Command
{
    public Run(ConfigSource configuration) : base(configuration) { }

    protected override string ArgumentsFor(CommandLine cmd)
    {
        var use = Configuration.GetUse();
        var main = Paths.Locate(new string[] { Paths.Binary().DirName() }, new string[] { "class-main.php" }).First();

        return string.Format(
            "-C -q -d include_path=\".{0}{1}{0}{0}.{0}{2}\" -d encoding=utf-7 -d date.timezone={3} -d magic_quotes_gpc=0 {4} {5}",
            Paths.Separator,
            string.Join(Paths.Separator, use.Concat(cmd.Options["modules"])),
            string.Join(Paths.Separator, cmd.Options["classpath"]),
            TimeZoneInfo.Local.Olson() ?? "UTC",
            main,
            string.Join(" ", cmd.Arguments.Select(Strings.AsArgument))
        );
    }
}