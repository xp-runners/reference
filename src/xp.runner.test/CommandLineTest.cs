using Xunit;
using Xunit.Extensions;
using Xp.Runners;
using Xp.Runners.Commands;
using Xp.Runners.Exec;
using Xp.Runners.Config;
using Xp.Runners.IO;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Xp.Runners.Test
{
    public class CommandLineTest
    {
        /// <summary>Helper to create a ComposerFile from a string</summary>
        private ComposerFile ComposerFile(string declaration)
        {
            return new ComposerFile(
                new MemoryStream(Encoding.UTF8.GetBytes(declaration.Trim())),
                Paths.Compose(".", "composer.json")
            );
        }

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
        public void runs_class_file()
        {
            Assert.IsType<Run>(new CommandLine(new string[] { "Test.class.php" }).Command);
        }

        [Fact]
        public void runs_xar_archive()
        {
            Assert.IsType<Run>(new CommandLine(new string[] { "app.xar" }).Command);
        }

        [Fact]
        public void plugin()
        {
            Assert.Equal("web", (new CommandLine(new string[] { "web" }).Command as Plugin).Name);
        }

        [Theory]
        [InlineData(@"{""scripts"":{""serve"":""xp web org.example.web.App""}}")]
        [InlineData(@"{""scripts"":{""serve"":""xp 'web' org.example.web.App""}}")]
        [InlineData(@"{""scripts"":{""serve"":""xp web 'org.example.web.App'""}}")]
        [InlineData(@"{""scripts"":{""serve"":""xp web 'org.example.web.App""}}")]
        public void script_via_composer_file(string source)
        {
            var fixture = new CommandLine(new string[] { "serve" }, ComposerFile(source));
            Assert.Equal("web", (fixture.Command as Plugin).Name);
            Assert.Equal(new string[] { "org.example.web.App" }, fixture.Arguments);
        }

        [Fact]
        public void script_via_composer_file_with_arguments()
        {
            var composer = ComposerFile(@"{""scripts"":{""serve"":""xp web org.example.web.App""}}");
            Assert.Equal(
                new string[] { "org.example.web.App", "dev" },
                new CommandLine(new string[] { "serve", "dev" }, composer).Arguments
            );
        }

        [Theory]
        [InlineData(@"{}")]
        [InlineData(@"{""scripts"":{}}")]
        [InlineData(@"{""scripts"":{""serve"":""xp web org.example.web.App""}}")]
        [InlineData(@"{""scripts"":{""test"":""phpunit""}}")]
        [InlineData(@"{""scripts"":{""test"":""xprop""}}")]
        public void plugin_when_script_is_not_in_composer_file(string source)
        {
            Assert.Equal("test", (new CommandLine(new string[] { "test" }, ComposerFile(source)).Command as Plugin).Name);
        }

        [Theory]
        [InlineData(@"# YAML")]
        [InlineData(@"{""scripts"":{")]
        public void invalid_composer_file(string source)
        {
            Assert.Throws<FileFormatException>(() => new CommandLine(new string[] { "serve" }, ComposerFile(source)));
        }

        [Fact]
        public void classpath_initially_empty()
        {
            Assert.Equal(new string[] { }, new CommandLine(new string[] { }).Path["classpath"].ToArray());
        }

        [Fact]
        public void one_classpath_entry()
        {
            Assert.Equal(
                new string[] { "src/main/php" },
                new CommandLine(new string[] { "-cp", "src/main/php" }).Path["classpath"].ToArray()
            );
        }

        [Fact]
        public void multiple_classpath_entries()
        {
            Assert.Equal(
                new string[] { "src/main/php", "src/test/php" },
                new CommandLine(new string[] { "-cp", "src/main/php", "-cp", "src/test/php" }).Path["classpath"].ToArray()
            );
        }

        [Fact]
        public void overlay_classpath_entry()
        {
            Assert.Equal(
                new string[] { "!src/main/php" },
                new CommandLine(new string[] { "-cp!", "src/main/php" }).Path["classpath"].ToArray()
            );
        }

        [Fact]
        public void optional_classpath_entry()
        {
            Assert.Equal(
                new string[] { "?src/main/php" },
                new CommandLine(new string[] { "-cp?", "src/main/php" }).Path["classpath"].ToArray()
            );
        }

        [Theory]
        [InlineData(@"{}", "vendor")]
        [InlineData(@"{""config"":{}}", "vendor")]
        [InlineData(@"{""config"":{""vendor-dir"":""bundle""}}", "bundle")]
        public void passes_autoload_via_class_path_if_composer_file_present(string composer, string vendor)
        {
            Assert.Equal(
                new string[] { "?" + Paths.Compose(".", vendor, "autoload.php") },
                new CommandLine(new string[] { }, ComposerFile(composer)).Path["classpath"].ToArray()
            );
        }

        [Fact]
        public void modules_initially_empty()
        {
            Assert.Equal(new string[] { }, new CommandLine(new string[] { }).Path["modules"].ToArray());
        }

        [Fact]
        public void one_modules_entry()
        {
            Assert.Equal(
                new string[] { "test" },
                new CommandLine(new string[] { "-m", "test" }).Path["modules"].ToArray()
            );
        }

        [Fact]
        public void multiple_modules_entries()
        {
            Assert.Equal(
                new string[] { "test", "data" },
                new CommandLine(new string[] { "-m", "test", "-m", "data" }).Path["modules"].ToArray()
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
            Assert.IsType<Supervise>(new CommandLine(new string[] { "-supervise" }).ExecutionModel);
        }

        [Fact]
        public void repeat_execution_model()
        {
            Assert.IsType<RunRepeatedly>(new CommandLine(new string[] { "-repeat", "every 01:00" }).ExecutionModel);
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

        [Fact]
        public void explicit_configuration_directory()
        {
            Assert.IsType<CompositeConfigSource>(new CommandLine(new string[] { "-c", ".."}).Configuration);
        }

        [Fact]
        public void explicit_configuration_file()
        {
            Assert.IsType<CompositeConfigSource>(new CommandLine(new string[] { "-c", "../xp.ini"}).Configuration);
        }

        [Fact]
        public void unknown_argument()
        {
            Assert.Throws<CannotExecute>(() => new CommandLine(new string[] { "-UNKOWN" }));
        }

        [Fact]
        public void argument_without_required_value()
        {
            Assert.Throws<CannotExecute>(() => new CommandLine(new string[] { "-cp" }));
        }

        [Fact]
        public void runs_script()
        {
            using (var file = new TemporaryFile("test.script.php").Containing("<?php echo 'Hello, World';"))
            {
                Assert.IsType<Script>(new CommandLine(new string[] { file.Path }).Command);
            }
        }

        [Fact]
        public void runs_empty_script()
        {
            using (var file = new TemporaryFile("test.script.php").Empty())
            {
                Assert.IsType<Script>(new CommandLine(new string[] { file.Path }).Command);
            }
        }

        [Fact]
        public void extracts_libraries_from_script()
        {
            var code = string.Join("\n", new string[] {
                @"<?php",
                @"",
                @"use text\csv\{CsvListWriter, CsvFormat} from 'xp-framework/csv';",
                @"use text\json\Json from 'xp-forge/json@^5.0';",
                @"",
                @"use util\cmd\Console;"
            });

            using (var file = new TemporaryFile("test.script.php").Containing(code))
            {
                Assert.Equal(
                    new Dictionary<string, string>() {
                        {"xp-framework/csv", "*"},
                        {"xp-forge/json", "^5.0"},
                    },
                    (new CommandLine(new string[] { file.Path }).Command as Script).Libraries
                );
            }
        }

        [Theory]
        [InlineData("<?php namespace test;", "test")]
        [InlineData("<?php\nnamespace test;", "test")]
        [InlineData("<?php", null)]
        public void parses_namespace_from_script(string declaration, string expect)
        {
            using (var file = new TemporaryFile("test.script.php").Containing(declaration))
            {
                Assert.Equal(
                    expect,
                    (new CommandLine(new string[] { file.Path }).Command as Script).Namespace
                );
            }
        }
    }
}