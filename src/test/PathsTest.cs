using Xunit;
using Xp.Runners;
using System;
using System.IO;

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
    }
}