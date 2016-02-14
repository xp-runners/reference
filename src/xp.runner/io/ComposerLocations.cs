using System;
using System.IO;
using System.Collections.Generic;

namespace Xp.Runners.IO
{
    /// See https://getcomposer.org/doc/03-cli.md#composer-home
    static class ComposerLocations
    {
        public const string VENDOR = "vendor";

        /// <summary>Returns well-known locations of Composer directories</summary>
        public static IEnumerable<string> For(PlatformID platform, Func<string, bool> exists)
        {
            yield return Paths.Compose(".", VENDOR);

            if (PlatformID.Unix == platform)
            {
                yield return Paths.Compose(Paths.Home(), ".composer", VENDOR);
            }
            else if (PlatformID.MacOSX == platform)
            {
                yield return Paths.Compose(Paths.Home(), ".composer", VENDOR);
            }
            else
            {
                yield return Paths.Compose(Environment.SpecialFolder.ApplicationData, "Composer", VENDOR);
            }
        }

        /// <summary>Returns well-known locations of Composer directories for a given platform:
        /// 1. Check for Windows. Return $APPDATA/Composer
        /// 2. Return $HOME/.composer
        /// </summary>
        public static IEnumerable<string> For(PlatformID platform)
        {
            return For(platform, Directory.Exists);
        }
    }
}
