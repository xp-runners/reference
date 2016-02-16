using Xunit;
using Xunit.Extensions;
using Xp.Runners;
using Xp.Runners.Commands;
using Xp.Runners.Exec;
using Xp.Runners.Config;

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
        public void supply_run_command()
        {
            Assert.IsType<Run>(new CommandLine(new string[] { "run", "Test" }).Command);
        }

        [Fact]
        public void omit_run_command_but_pass_type()
        {
            Assert.IsType<Run>(new CommandLine(new string[] { "Test" }).Command);
        }

        [Theory]
        [InlineData("-?")]
        [InlineData("help")]
        public void help(string arg)
        {
            Assert.IsType<Help>(new CommandLine(new string[] { arg }).Command);
        }

        [Theory]
        [InlineData("-v")]
        [InlineData("version")]
        public void version(string arg)
        {
            Assert.IsType<Version>(new CommandLine(new string[] { arg }).Command);
        }

        [Theory]
        [InlineData("-e")]
        [InlineData("eval")]
        public void eval(string arg)
        {
            Assert.IsType<Eval>(new CommandLine(new string[] { arg }).Command);
        }

        [Theory]
        [InlineData("-w")]
        [InlineData("write")]
        public void write(string arg)
        {
            Assert.IsType<Write>(new CommandLine(new string[] { arg }).Command);
        }

        [Theory]
        [InlineData("-d")]
        [InlineData("dump")]
        public void dump(string arg)
        {
            Assert.IsType<Dump>(new CommandLine(new string[] { arg }).Command);
        }

        [Theory]
        [InlineData("ar")]
        public void ar(string arg)
        {
            Assert.IsType<Ar>(new CommandLine(new string[] { arg }).Command);
        }

        [Fact]
        public void classpath_initially_empty()
        {
            Assert.Equal(new string[] { }, new CommandLine(new string[] { }).Options["classpath"].ToArray());
        }

        [Fact]
        public void one_classpath_entry()
        {
            Assert.Equal(
                new string[] { "src/main/php" },
                new CommandLine(new string[] { "-cp", "src/main/php" }).Options["classpath"].ToArray()
            );
        }

        [Fact]
        public void multiple_classpath_entries()
        {
            Assert.Equal(
                new string[] { "src/main/php", "src/test/php" },
                new CommandLine(new string[] { "-cp", "src/main/php", "-cp", "src/test/php" }).Options["classpath"].ToArray()
            );
        }

        [Fact]
        public void overlay_classpath_entry()
        {
            Assert.Equal(
                new string[] { "!src/main/php" },
                new CommandLine(new string[] { "-cp!", "src/main/php" }).Options["classpath"].ToArray()
            );
        }

        [Fact]
        public void optional_classpath_entry()
        {
            Assert.Equal(
                new string[] { "?src/main/php" },
                new CommandLine(new string[] { "-cp?", "src/main/php" }).Options["classpath"].ToArray()
            );
        }

        [Fact]
        public void modules_initially_empty()
        {
            Assert.Equal(new string[] { }, new CommandLine(new string[] { }).Options["modules"].ToArray());
        }

        [Fact]
        public void one_modules_entry()
        {
            Assert.Equal(
                new string[] { "test" },
                new CommandLine(new string[] { "-m", "test" }).Options["modules"].ToArray()
            );
        }

        [Fact]
        public void multiple_modules_entries()
        {
            Assert.Equal(
                new string[] { "test", "data" },
                new CommandLine(new string[] { "-m", "test", "-m", "data" }).Options["modules"].ToArray()
            );
        }

        [Fact]
        public void runonce_is_default_execution_model()
        {
            Assert.IsType<RunOnce>(new CommandLine(new string[] { }).ExecutionModel);
        }

        [Fact]
        public void watch_execution_model()
        {
            Assert.IsType<RunWatching>(new CommandLine(new string[] { "-watch", "." }).ExecutionModel);
        }

        [Fact]
        public void supervise_execution_model()
        {
            Assert.IsType<Supervise>(new CommandLine(new string[] { "-supervise", "." }).ExecutionModel);
        }

        [Theory]
        [InlineData(".")]
        [InlineData("src")]
        public void watch_execution_model_path(string path)
        {
            Assert.Equal(path, (new CommandLine(new string[] { "-watch", path }).ExecutionModel as RunWatching).Path);
        }

        [Fact]
        public void arguments_initially_empty()
        {
            Assert.Equal(new string[] { }, new CommandLine(new string[] { }).Arguments);
        }

        [Fact]
        public void one_argument()
        {
            Assert.Equal(
                new string[] { "Test" },
                new CommandLine(new string[] { "run", "Test" }).Arguments
            );
        }

        [Fact]
        public void multiple_arguments()
        {
            Assert.Equal(
                new string[] { "Test", "1", "2", "3" },
                new CommandLine(new string[] { "run", "Test", "1", "2", "3" }).Arguments
            );
        }

        [Fact]
        public void default_configuration()
        {
            Assert.IsType<CompositeConfigSource>(new CommandLine(new string[] { }).Configuration);
        }

        [Fact]
        public void no_configuration()
        {
            Assert.IsType<EnvironmentConfigSource>(new CommandLine(new string[] { "-n" }).Configuration);
        }
    }
}