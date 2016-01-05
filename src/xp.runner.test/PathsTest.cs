using Xunit;
using System;
using System.Linq;
using System.IO;
using Xp.Runners.IO;

namespace Xp.Runners.Test
{
    public class PathsTest
    {
        [Fact]
        public void path_separator()
        {
            Assert.Equal(Path.PathSeparator.ToString(), Paths.Separator);
        }

        [Fact]
        public void dirname_of_empty()
        {
            Assert.Equal("", "".DirName());
        }

        [Fact]
        public void dirname_of_file()
        {
            Assert.Equal(
                "path" + Path.DirectorySeparatorChar,
                "path" + Path.DirectorySeparatorChar + "file.txt".DirName()
            );
        }

        [Fact]
        public void useless_compose_usage()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Paths.Compose());
        }

        [Fact]
        public void compose_single_component()
        {
            Assert.Equal("path", Paths.Compose("path"));
        }

        [Fact]
        public void compose_two_components()
        {
            Assert.Equal(
                "path" + Path.DirectorySeparatorChar + "file.txt",
                Paths.Compose("path", "file.txt")
            );
        }

        [Fact]
        public void compose_three_components()
        {
            Assert.Equal(
                "vendor" + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "cli",
                Paths.Compose("vendor", "bin", "cli")
            );
        }

        [Fact]
        public void compose_specialfolder()
        {
            Assert.Equal(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Paths.Compose(Environment.SpecialFolder.ApplicationData)
            );
        }

        [Fact]
        public void compose_specialfolder_with_component()
        {
            Assert.Equal(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Composer",
                Paths.Compose(Environment.SpecialFolder.ApplicationData, "Composer")
            );
        }

        [Fact]
        public void binary()
        {
            Assert.NotEqual("", Paths.Binary());
        }

        [Fact]
        public void translate_empty_paths()
        {
            Assert.Equal(new string[] { }, Paths.Translate(".", new string[] { }).ToArray());
        }

        [Fact]
        public void translate_absolute_path()
        {
            var cwd = Directory.GetCurrentDirectory();
            Assert.Equal(
                new string[] { cwd },
                Paths.Translate(".", new string[] { cwd }).ToArray()
            );
        }

        [Fact]
        public void translate_relative_path()
        {
            var cwd = Directory.GetCurrentDirectory();
            Assert.Equal(
                new string[] { Paths.Compose(cwd, "src") },
                Paths.Translate(cwd, new string[] { "src" }).ToArray()
            );
        }

        [Fact]
        public void translate_home_path()
        {
            var home = Environment.GetEnvironmentVariable("HOME") ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            Assert.Equal(
                new string[] { home },
                Paths.Translate(".", new string[] { "~" }).ToArray()
            );
        }

        [Fact]
        public void translate_path_inside_home()
        {
            var home = Environment.GetEnvironmentVariable("HOME") ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            Assert.Equal(
                new string[] { Paths.Compose(home, "devel") },
                Paths.Translate(".", new string[] { "~/devel" }).ToArray()
            );
        }

        [Fact]
        public void locate_dotnet_framework()
        {
            var framework = Paths.Compose(Environment.SpecialFolder.Windows, "Microsoft.NET", "Framework");
            if (!Directory.Exists(framework)) return;   // Skip

            var search = Directory.GetDirectories(framework, "v*.*");

            Assert.NotEqual(
                new string[] { },
                Paths.Locate(search, new string[] { "csc.exe" }).ToArray()
            );
        }

        [Fact]
        public void locate_non_existant_file()
        {
            Assert.Throws<FileNotFoundException>(() => Paths.Locate(new string[] { "." }, new string[] { "this-file-does-not-exist" }).ToArray());
        }

        [Fact]
        public void try_locate_file()
        {
            Assert.Equal(
                new string[] { },
                Paths.TryLocate(new string[] { "." }, new string[] { "this-file-does-not-exist" }).ToArray()
            );
        }

    }
}