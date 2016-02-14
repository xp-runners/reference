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

        [Fact]
        public void uses_dotdir_in_home_on_unix()
        {
            Assert.Equal(
                new string[] {
                    Paths.Compose(".", ComposerLocations.VENDOR),
                    Paths.Compose(Paths.Home(), ".composer", ComposerLocations.VENDOR),
                },
                ComposerLocations.For(PlatformID.Unix).ToArray()
            );
        }
    }
}