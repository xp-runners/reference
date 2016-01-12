using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System;
using System.IO;
using Xp.Runners;

namespace Xp.Runners.Commands
{
    /// <summary>Reads composer.json files</summary>
    public class ComposerFile : IDisposable
    {
        public const string NAME = "composer.json";

        private static DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(Composer), new DataContractJsonSerializerSettings {
            UseSimpleDictionaryFormat = true
        });

        private Composer definitions;
        private Stream input;

        /// <summary>Creates an instance from a given stream</summary>
        public ComposerFile(Stream input)
        {
            this.input = input;
        }

        /// <summary>Creates an instance from a given file name</summary>
        public ComposerFile(string file) : this(new FileStream(file, FileMode.Open))
        {
        }

        /// <summary>Reads definitions lazily</summary>
        public Composer Definitions
        {
            get
            {
                lock (this)
                {
                    if (null == definitions)
                    {
                        definitions = json.ReadObject(input) as Composer;
                    }
                }
                return definitions;
            }
        }

        /// <summary>For use in `using`</summary>
        public void Dispose()
        {
            input.Dispose();
        }
    }
}