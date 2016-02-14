using Xunit;
using Xunit.Extensions;
using System;
using System.Collections;
using System.Linq;
using Xp.Runners.IO;

namespace Xp.Runners.Test
{
    public class ComposerLocationsTest : IDisposable
    {
        private IDictionary environment;

        public ComposerLocationsTest()
        {
            environment = Environment.GetEnvironmentVariables();
            foreach (string name in environment.Keys)
            {
                if (name.StartsWith("XDG_")) Environment.SetEnvironmentVariable(name, null);
            }
        }

        public void Dispose()
        {
            foreach (string name in environment.Keys)
            {
                Environment.SetEnvironmentVariable(name, environment[name] as string);
            }
        }

        [Fact]
        public void uses_appdata_on_windows()
        {
            Assert.Equal(
                new string[] {
                    Paths.Compose(".", ComposerLocations.VENDOR),
                    Paths.Compose(Environment.SpecialFolder.ApplicationData, "Composer", ComposerLocations.VENDOR),
                },
                ComposerLocations.For(PlatformID.Win32NT).ToArray()
            );
        }

        [Fact]
        public void uses_dotdir_in_home_on_macosx()
        {
            Assert.Equal(
                new string[] {
                    Paths.Compose(".", ComposerLocations.VENDOR),
                    Paths.Compose(Paths.Home(), ".composer", ComposerLocations.VENDOR),
                },
                ComposerLocations.For(PlatformID.MacOSX).ToArray()
            );
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void uses_composerdotdir_on_unix(bool exists)
        {
            Assert.Equal(
                new string[] {
                    Paths.Compose(".", ComposerLocations.VENDOR),
                    Paths.Compose(Paths.Home(), ".composer", ComposerLocations.VENDOR),
                },
                ComposerLocations.For(PlatformID.Unix, (dir) => exists).ToArray()
            );
        }

        [Fact]
        public void uses_xdg_config_home_if_available_and_composerdotdir_does_not_exist()
        {
            var config = Paths.Compose(Paths.Home(), ".configuration");
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", config);
            Assert.Equal(
                new string[] {
                    Paths.Compose(".", ComposerLocations.VENDOR),
                    Paths.Compose(config, "composer", ComposerLocations.VENDOR),
                },
                ComposerLocations.For(PlatformID.Unix, (dir) => false).ToArray()
            );
        }

        [Fact]
        public void uses_xdg_default_config_home_if_composerdotdir_does_not_exist()
        {
            Environment.SetEnvironmentVariable("XDG_RUNTIME_DIR", "/run/user/1000");
            Assert.Equal(
                new string[] {
                    Paths.Compose(".", ComposerLocations.VENDOR),
                    Paths.Compose(Paths.Home(), ".config", "composer", ComposerLocations.VENDOR),
                },
                ComposerLocations.For(PlatformID.Unix, (dir) => false).ToArray()
            );
        }

        [Fact]
        public void does_not_use_xdg_config_home_if_composerdotdir_exists()
        {
            Environment.SetEnvironmentVariable("XDG_RUNTIME_DIR", "/run/user/1000");
            Assert.Equal(
                new string[] {
                    Paths.Compose(".", ComposerLocations.VENDOR),
                    Paths.Compose(Paths.Home(), ".composer", ComposerLocations.VENDOR),
                },
                ComposerLocations.For(PlatformID.Unix, (dir) => true).ToArray()
            );
        }

    }
}