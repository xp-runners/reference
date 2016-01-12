using Xunit;
using System.IO;
using System.Text;
using Xp.Runners.Commands;

namespace Xp.Runners.Test
{
    public class ComposerFileTest
    {
        /// <summary>Helper to create a ComposerFile from a string</summary>
        private ComposerFile ComposerFile(string declaration)
        {
            return new ComposerFile(new MemoryStream(Encoding.UTF8.GetBytes(declaration.Trim())));
        }

        [Fact]
        public void name()
        {
            using (var composer = ComposerFile(@"{""name"" : ""test"" }"))
            {
                Assert.Equal("test", composer.Definitions.Name);
            }
        }

        [Fact]
        public void empty_requirements()
        {
            using (var composer = ComposerFile(@"{""require"" : { } }"))
            {
                Assert.Equal(0, composer.Definitions.Require.Count);
            }
        }

        [Fact]
        public void php_requirement()
        {
            using (var composer = ComposerFile(@"{""require"" : {""php"" : ""5.4+""} }"))
            {
                Assert.Equal("5.4+", composer.Definitions.Require["php"]);
            }
        }

        [Fact]
        public void library_requirements()
        {
            using (var composer = ComposerFile(@"{
                ""require"" : {
                    ""xp-framework/core""           : ""^6.10"",
                    ""xp-framework/io-collections"" : ""^6.5""
                }
            }"))
            {
                Assert.Equal("^6.10", composer.Definitions.Require["xp-framework/core"]);
                Assert.Equal("^6.5", composer.Definitions.Require["xp-framework/io-collections"]);
            }
        }
    }
}