using Xunit;
using Xunit.Extensions;
using System;
using Xp.Runners.Commands;

namespace Xp.Runners.Test
{
    public class EntryPointTest
    {
        [Theory]
        [InlineData("xp.xp-framework.unittest")]
        [InlineData("xp.xp-framework.unittest.test")]
        public void module(string file)
        {
            Assert.Equal("xp-framework/unittest", new EntryPoint(file).Module);
        }

        [Fact]
        public void type_when_given_module()
        {
            Assert.Equal("xp.unittest.Runner", new EntryPoint("xp.xp-framework.unittest").Type);
        }

        [Fact]
        public void type_when_given_module_and_command()
        {
            Assert.Equal("xp.unittest.TestRunner", new EntryPoint("xp.xp-framework.unittest.test").Type);
        }

        [Theory]
        [InlineData("")]
        [InlineData("xp")]
        [InlineData("xp.")]
        [InlineData("xp.xp-framework")]
        public void malformed(string file)
        {
            Assert.Throws<ArgumentException>(() => new EntryPoint(file));
        }
    }
}