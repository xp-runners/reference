using Xunit;
using Xp.Runners;

namespace Xp.Runners.Test
{
    public class CommandLineTest
    {
        [Fact]
        public void default_command_is_help()
        {
            Assert.IsType<Help>(new CommandLine(new string[] { }).Command);
        }

        [Fact]
        public void supply_command()
        {
            Assert.IsType<Run>(new CommandLine(new string[] { "run", "Test" }).Command);
        }
    }
}