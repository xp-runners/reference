using Xunit;
using Xunit.Extensions;
using Xp.Runners.Config;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Xp.Runners.Test
{
    public class IniTest : IDisposable
    {
        private List<string> fixtures = new List<string>();

        private Ini fixture(string source)
        {
            var name = Path.GetTempFileName();
            fixtures.Add(name);
            File.WriteAllText(name, source);
            return new Ini(name);
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
        public void exists()
        {
            Assert.False(new Ini("does-not-exist.ini").Exists());
            Assert.True(fixture("").Exists());
        }

        [Fact]
        public void can_parse_empty()
        {
            var ini = fixture("");
            Assert.Equal(new string[] {}, ini.Keys("default").ToArray());
        }

        [Fact]
        public void keys_in_default_section()
        {
            var ini = fixture("a=b\nc=d");
            Assert.Equal(new string[] {"a", "c"}, ini.Keys("default").ToArray());
        }

        [Fact]
        public void keys_in_section()
        {
            var ini = fixture("[section]\na=b\nc=d");
            Assert.Equal(new string[] {"a", "c"}, ini.Keys("section").ToArray());
        }

        [Fact]
        public void keys()
        {
            var ini = fixture("a=b\nc=d\n[section]\nb=a\nd=c");
            Assert.Equal(new string[] {"a", "c"}, ini.Keys("default").ToArray());
            Assert.Equal(new string[] {"b", "d"}, ini.Keys("section").ToArray());
        }

        [Fact]
        public void keys_contains_key_with_empty_value()
        {
            Assert.Equal(new string[] {"key"}, fixture("key=").Keys("default").ToArray());
        }

        [Theory]
        [InlineData("key=value", "value")]
        [InlineData("key=`value`", "value")]
        [InlineData("key='value'", "value")]
        [InlineData("key=\"value\"", "value")]
        [InlineData("key='\"Test\"'", "\"Test\"")]
        [InlineData("key='Timm\\'s'", "Timm's")]
        [InlineData("key=`Type \\`sh\\``", "Type `sh`")]
        [InlineData("key=\"A \\\"test\\\"\"", "A \"test\"")]
        public void get_key_from_default_section(string section, string expected)
        {
            Assert.Equal(expected, fixture(section).Get("default", "key"));
        }

        [Fact]
        public void get_key_from_section()
        {
            Assert.Equal("value", fixture("[section]\nkey=value").Get("section", "key"));
        }

        [Fact]
        public void get_empty_value()
        {
            Assert.Equal(null, fixture("key=").Get("default", "key"));
        }

        [Theory]
        [InlineData("key= value")]
        [InlineData("key =value")]
        [InlineData("key = value")]
        [InlineData(" key = value")]
        [InlineData("\tkey=value")]
        [InlineData("key=\tvalue")]
        public void key_and_values_are_trimmed(string section)
        {
            Assert.Equal("value", fixture(section).Get("default", "key"));
        }

        [Theory]
        [InlineData("runtime")]
        [InlineData("default")]
        public void default_value_returned_for_nonexistant_key_from_get(string section)
        {
            var defaultValue = "defaulted";
            Assert.Equal(defaultValue, fixture("").Get(section, "key", defaultValue));
        }

        [Fact]
        public void default_value_returned_for_empty_key_from_get()
        {
            var defaultValue = "defaulted";
            Assert.Equal(defaultValue, fixture("key=").Get("default", "key", defaultValue));
        }

        [Fact]
        public void get_all_key_from_default_section()
        {
            var ini = fixture("key=value");
            Assert.Equal(new string[] {"value"}, ini.GetAll("default", "key").ToArray());
        }

        [Fact]
        public void get_all_key_from_section()
        {
            var ini = fixture("[section]\nkey=value");
            Assert.Equal(new string[] {"value"}, ini.GetAll("section", "key").ToArray());
        }

        [Fact]
        public void get_all_empty_value()
        {
            var ini = fixture("key=");
            Assert.Equal(new string[] {}, ini.GetAll("default", "key").ToArray());
        }

        [Theory]
        [InlineData("runtime")]
        [InlineData("default")]
        public void default_value_returned_for_nonexistant_key_from_get_all(string section)
        {
            var ini = fixture("");
            var defaultValue = new string[] {"defaulted"};
            Assert.Equal(defaultValue, ini.GetAll(section, "key", defaultValue));
        }

        [Theory]
        [InlineData("")]
        [InlineData("\n\n")]
        [InlineData("; Comment")]
        [InlineData("; Line 1\n; Line 2")]
        [InlineData("; Configuration\n\n; Example")]
        public void comments_and_empty_lines_are_ignored(string lines)
        {
            var ini = fixture(lines + "\nkey=value");
            Assert.Equal(new string[] {"key"}, ini.Keys("default"));
        }

        [Theory]
        [InlineData("key")]
        [InlineData("# Hash-comments are not supported")]
        [InlineData("!what?")]
        public void non_section_lines_without_equals_signs_are_ignored(string lines)
        {
            var ini = fixture(lines + "\nkey=value");
            Assert.Equal(new string[] {"key"}, ini.Keys("default"));
        }
    }
}