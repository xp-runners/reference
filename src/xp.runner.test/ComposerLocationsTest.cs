using Xunit;
using Xunit.Extensions;
using System;
using System.Linq;
using Xp.Runners.IO;

namespace Xp.Runners.Test
{
    public class ComposerLocationsTest
    {

        [Fact]
        public void uses_appdata_on_windows()
        {
            Assert.Equal(
                new string[] {
                    ".",
                    Paths.Compose(Environment.SpecialFolder.ApplicationData, "Composer"),
                },
                ComposerLocations.For(PlatformID.Win32NT).ToArray()
            );
        }

        [Fact]
        public void uses_dotdir_in_home_on_macosx()
        {
            Assert.Equal(
                new string[] {
                    ".",
                    Paths.Compose(Paths.Home(), ".composer"),
                },
                ComposerLocations.For(PlatformID.MacOSX).ToArray()
            );
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void uses_composerdotdir_on_unix(bool exists)
        {
            using (new ModifiedEnvironment().RemoveAny("XDG_"))
            {
                Assert.Equal(
                    new string[] {
                        ".",
                        Paths.Compose(Paths.Home(), ".composer"),
                    },
                    ComposerLocations.For(PlatformID.Unix, (dir) => exists).ToArray()
                );
            }
        }

        [Fact]
        public void uses_xdg_config_home_if_available_and_composerdotdir_does_not_exist()
        {
            var config = Paths.Compose(Paths.Home(), ".configuration");
            using (new ModifiedEnvironment().RemoveAny("XDG_").With("XDG_CONFIG_HOME", config))
            {
                Assert.Equal(
                    new string[] {
                        ".",
                        Paths.Compose(config, "composer"),
                    },
                    ComposerLocations.For(PlatformID.Unix, (dir) => false).ToArray()
                );
            }
        }

        [Fact]
        public void uses_xdg_default_config_home_if_composerdotdir_does_not_exist()
        {
            using (new ModifiedEnvironment().RemoveAny("XDG_").With("XDG_RUNTIME_DIR", "/run/user/1000"))
            {
                Assert.Equal(
                    new string[] {
                        ".",
                        Paths.Compose(Paths.Home(), ".config", "composer"),
                    },
                    ComposerLocations.For(PlatformID.Unix, (dir) => false).ToArray()
                );
            }
        }

        [Fact]
        public void does_not_use_xdg_config_home_if_composerdotdir_exists()
        {
            using (new ModifiedEnvironment().RemoveAny("XDG_").With("XDG_RUNTIME_DIR", "/run/user/1000"))
            {
                Assert.Equal(
                    new string[] {
                        ".",
                        Paths.Compose(Paths.Home(), ".composer"),
                    },
                    ComposerLocations.For(PlatformID.Unix, (dir) => true).ToArray()
                );
            }
        }
    }
}