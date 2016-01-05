using Xunit;
using Xp.Runners;

namespace Xp.Runners.Test
{
    public class ArgumentsTest
    {
        [Fact]
        public void empty()
        {
            Assert.Equal("\"\"", "".Encode());
        }

        [Fact]
        public void seven_bit_string()
        {
            Assert.Equal("\"Test\"", "Test".Encode());
        }

        [Fact]
        public void umlaut_string()
        {
            Assert.Equal("\"+ANw-ber\"", "Über".Encode());
        }

        [Fact]
        public void unicode_string()
        {
            Assert.Equal("\"MTR +AD0- +bi+UNQ-\"", "MTR = 港鐵".Encode());
        }

        [Fact]
        public void string_with_double_quote_inside()
        {
            Assert.Equal("\"He said +ACI-Hello+ACI-\"", "He said \"Hello\"".Encode());
        }

        [Fact]
        public void string_with_backslash_inside()
        {
            Assert.Equal("\"Root is C:+AFw-\"", "Root is C:\\".Encode());
        }
    }
}