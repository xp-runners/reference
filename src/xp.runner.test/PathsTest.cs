using Xunit;
using Xunit.Extensions;
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
            Assert.Equal("", Paths.Compose());
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
        public void compose_with_empty_component()
        {
            Assert.Equal(
                "path" + Path.DirectorySeparatorChar + "file.txt",
                Paths.Compose("path", "", "file.txt")
            );
        }

        [Fact]
        public void compose_with_null_component()
        {
            Assert.Equal(
                "path" + Path.DirectorySeparatorChar + "file.txt",
                Paths.Compose("path", null, "file.txt")
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
            var home = Paths.Home();
            Assert.Equal(
                new string[] { home },
                Paths.Translate(".", new string[] { "~" }).ToArray()
            );
        }

        [Fact]
        public void translate_path_inside_home()
        {
            var home = Paths.Home();
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

        [Theory]
        [InlineData(null, false)]
        [InlineData("/home/thekid/.config", true)]
        public void xdg_system_detection(string config, bool expect)
        {
            using (new ModifiedEnvironment().RemoveAny("XDG_").With("XDG_CONFIG_HOME", config))
            {
                Assert.Equal(expect, Paths.UseXDG());
            }
        }

        [Fact]
        public void configdir_uses_appdata_if_no_home_variable_is_set()
        {
            using (new ModifiedEnvironment().With("HOME", null))
            {
                Assert.Equal(Paths.Compose(Environment.SpecialFolder.ApplicationData, "Xp", "test"), Paths.ConfigDir("test"));
            }
        }

        [Fact]
        public void configdir_uses_xdg_config_dir_on_xdg_systems()
        {
            using (new ModifiedEnvironment().With("HOME", "home").With("XDG_CONFIG_HOME", "xdg/.config"))
            {
                Assert.Equal(Paths.Compose("xdg", ".config", "xp", "test"), Paths.ConfigDir("test"));
            }
        }

        [Fact]
        public void configdir_uses_dot_dir_in_home_by_default()
        {
            using (new ModifiedEnvironment().With("HOME", "home").RemoveAny("XDG_"))
            {
                Assert.Equal(Paths.Compose(Paths.Home(), ".xp", "test"), Paths.ConfigDir("test"));
            }
        }
    }
}