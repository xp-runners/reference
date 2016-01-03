using Xunit;
using Xp.Runners;

namespace Xp.Runners.Test
{
    public class StringsTest
    {
        [Fact]
        public void ucfirst_empty()
        {
            Assert.Equal("", "".UpperCaseFirst());
        }

        [Fact]
        public void ucfirst_single()
        {
            Assert.Equal("T", "t".UpperCaseFirst());
        }

        [Fact]
        public void ucfirst()
        {
            Assert.Equal("Test", "test".UpperCaseFirst());
        }
    }
}