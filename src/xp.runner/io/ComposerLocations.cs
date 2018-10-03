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

            // Don't use Paths.UserDir() here as the order of preference is a bit different:
            // 1. If ~/.composer exists, it is preferred over the XDG config dir
            // 2. We will always use $APPDATA/Composer on Windows, ignoring $HOME
            if (PlatformID.Unix == platform)
            {
                var composer = Paths.Compose(Paths.Home(), ".composer");
                if (!exists(composer) && Paths.UseXDG())
                {
                    yield return Paths.Compose(
                        Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? Paths.Compose(Paths.Home(), ".config"),
                        "composer",
                        VENDOR
                    );
                }
                else
                {
                    yield return Paths.Compose(composer, VENDOR);
                }
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
        /// 2. Return $HOME/.composer if it exists and is a directory
        /// 3. Check if XDG_ environment variables exist, return $(XDG_CONFIG_HOME ?? HOME/.config)/composer
        /// 4. Return $HOME/.composer
        /// </summary>
        public static IEnumerable<string> For(PlatformID platform)
        {
            return For(platform, Directory.Exists);
        }
    }
}
