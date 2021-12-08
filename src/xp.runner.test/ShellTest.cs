using Xunit;
using Xunit.Extensions;
using System;
using System.Linq;
using Xp.Runners.IO;

namespace Xp.Runners.Test
{
    public class ShellTest
    {
        [Fact]
        public void empty_string()
        {
            Assert.Throws<ArgumentException>(() => Shell.Parse(""));
        }

        [Fact]
        public void executable()
        {
            Assert.Equal(new Shell("php"), Shell.Parse("php"));
        }

        [Fact]
        public void leading_space()
        {
            Assert.Equal(new Shell("php"), Shell.Parse(" php"));
        }

        [Fact]
        public void quoted_executable()
        {
            Assert.Equal(new Shell("php"), Shell.Parse("\"php\""));
        }

        [Fact]
        public void quoted_retains_space()
        {
            Assert.Equal(new Shell("C:/Programs (x86)/php"), Shell.Parse("\"C:/Programs (x86)/php\""));
        }

        [Fact]
        public void executable_and_single_option()
        {
            Assert.Equal(
                new Shell("php", new string[] { "-dextension_dir=ext" }),
                Shell.Parse("php -dextension_dir=ext")
            );
        }

        [Fact]
        public void executable_and_two_options()
        {
            Assert.Equal(
                new Shell("php", new string[] { "-n", "-dextension_dir=ext" }),
                Shell.Parse("php -n -dextension_dir=ext")
            );
        }

        [Fact]
        public void partially_quoted_option()
        {
            Assert.Equal(
                new Shell("php", new string[] { "-dextension_dir=\"C:/Programs (x86)/php/ext\"" }),
                Shell.Parse("php -dextension_dir=\"C:/Programs (x86)/php/ext\"")
            );
        }

        [Fact]
        public void completey_quoted_option()
        {
            Assert.Equal(
                new Shell("php", new string[] { "\"test\"" }),
                Shell.Parse("php \"test\"")
            );
        }

        [Fact]
        public void inline_quoted_option()
        {
            Assert.Equal(
                new Shell("php", new string[] { "a\"test\"b" }),
                Shell.Parse("php a\"test\"b")
            );
        }

        [Fact]
        public void unclosed_quoted_option()
        {
            Assert.Equal(
                new Shell("php", new string[] { "\"test\"" }),
                Shell.Parse("php \"test")
            );
        }

        [Theory]
        [InlineData("*")]
        [InlineData("?")]
        [InlineData("^5.0")]
        public void escapes(string input)
        {
            Assert.Equal("'" + input + "'", Shell.Escape(new string[] { input }).First());
        }

        [Theory]
        [InlineData("composer")]
        [InlineData("xp-framework/core")]
        public void does_not_escape(string input)
        {
            Assert.Equal(input , Shell.Escape(new string[] { input }).First());
        }

        [Theory]
        [InlineData(null, "xp-framework/core")]
        [InlineData("5.0", "5.0")]
        public void escaping_skips_oper_null(string input, string expected)
        {
            Assert.Equal(expected, Shell.Escape(new string[] { "xp-framework/core", input }).Last());
        }
    }
}