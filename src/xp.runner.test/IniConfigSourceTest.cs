using Xunit;
using Xunit.Extensions;
using Xp.Runners.Config;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Xp.Runners.IO;

namespace Xp.Runners.Test
{
    public class IniConfigSourceTest : IDisposable
    {
        private List<string> fixtures = new List<string>();

        private IniConfigSource fixture(string source)
        {
            var name = Path.GetTempFileName();
            fixtures.Add(name);
            File.WriteAllText(name, source);
            return new IniConfigSource(new Ini(name));
        }

        /// Ensures the files are deleted
        public void Dispose() {
          foreach (var name in fixtures)
          {
              if (File.Exists(name))
              {
                  File.Delete(name);
              }
          }
        }

        [Fact]
        public void no_use_by_default()
        {
            Assert.Null(fixture("").GetUse());
        }

        [Fact]
        public void path_in_use_is_expanded()
        {
            Assert.Equal(
                Paths.Translate(",", new string[] { "~/devel/xp/core" }),
                fixture("use=~/devel/xp/core").GetUse().ToArray()
            );
        }

        [Fact]
        public void no_runtime_by_default()
        {
            Assert.Null(fixture("").GetRuntime());
        }

        [Fact]
        public void runtime()
        {
            Assert.Equal("7.1", fixture("rt=7.1").GetRuntime());
        }

        [Fact]
        public void no_executable_by_default()
        {
            Assert.Null(fixture("").GetExecutable(null));
        }

        [Fact]
        public void executable()
        {
            Assert.Equal("/usr/bin/php", fixture("[runtime]\ndefault=/usr/bin/php").GetExecutable(null));
        }

        [Fact]
        public void specific_executable()
        {
            Assert.Equal(
                "/usr/bin/php7",
                fixture("[runtime]\ndefault=/usr/bin/php\n[runtime@7.1]\ndefault=/usr/bin/php7").GetExecutable("7.1")
            );
        }

        [Fact]
        public void no_extensions_by_default()
        {
            Assert.Equal(new string[] { }, fixture("").GetExtensions(null));
        }

        [Fact]
        public void extensions()
        {
            Assert.Equal(
                new string[] { "php_mysql.dll", "php_sybase_ct.dll" },
                fixture("[runtime]\nextension=php_mysql.dll\nextension=php_sybase_ct.dll").GetExtensions(null).ToArray()
            );
        }

        [Fact]
        public void named_extension()
        {
            Assert.Equal(
                new string[] { Environment.OSVersion.Platform == PlatformID.Win32NT ? "php_mysql.dll" : "mysql.so" },
                fixture("[runtime]\nextension=mysql").GetExtensions(null).ToArray()
            );
        }

        [Fact]
        public void no_args_by_default()
        {
            Assert.Equal(
                new Dictionary<string, IEnumerable<string>>(),
                fixture("").GetArgs(null)
            );
        }

        [Fact]
        public void args()
        {
            Assert.Equal(
                new string[] { "extension_dir=/usr/lib/php7" },
                fixture("[runtime]\nextension_dir=/usr/lib/php7").GetArgs(null)
                    .Select(pair => string.Format("{0}={1}", pair.Key, string.Join("|", pair.Value)))
                    .ToArray()
            );
        }
    }
}