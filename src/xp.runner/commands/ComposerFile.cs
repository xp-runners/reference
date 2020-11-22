using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;
using Xp.Runners;

namespace Xp.Runners.Commands
{
    /// <summary>Reads composer.json files</summary>
    public class ComposerFile : IDisposable
    {
        public const string NAME = "composer.json";

        private Composer definitions;
        private XmlDictionaryReader input;

        /// <summary>Creates an instance from a given stream</summary>
        public ComposerFile(Stream input)
        {
            this.input = JsonReaderWriterFactory.CreateJsonReader(input, new XmlDictionaryReaderQuotas());
        }

        /// <summary>Creates an instance from a given file name</summary>
        public ComposerFile(string file) : this(new FileStream(file, FileMode.Open, FileAccess.Read))
        {
        }

        /// <summary>Structure of a JSON document as a structure useable for lookups</summary>
        private IEnumerable<KeyValuePair<string, string>> StructureOf(XmlDictionaryReader input)
        {
            var path = new List<string>();
            do
            {
                if (XmlNodeType.Text == input.NodeType)
                {
                    yield return new KeyValuePair<string, string>(string.Join("\\", path), input.Value);
                }
                else if (XmlNodeType.Element == input.NodeType)
                {
                    var name = input.LocalName;

                    input.MoveToFirstAttribute();
                    do
                    {
                        if ("item" == input.LocalName) name = input.Value;
                    } while (input.MoveToNextAttribute());

                    input.MoveToElement();
                    path.Add(name);
                }
                else if (XmlNodeType.EndElement == input.NodeType)
                {
                    path.RemoveAt(path.Count - 1);
                }
            } while (input.Read());
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
                        var lookup = StructureOf(input).ToLookup(item => item.Key, item => item.Value);

                        definitions = new Composer();
                        definitions.Name = lookup[@"root\name"].FirstOrDefault();
                        definitions.Require = lookup
                            .Where(pair => pair.Key.StartsWith(@"root\require\"))
                            .ToDictionary(value => value.Key.Substring(@"root\require\".Length), value => value.First())
                        ;
                        definitions.Scripts = lookup
                            .Where(pair => pair.Key.StartsWith(@"root\scripts\"))
                            .ToDictionary(value => value.Key.Substring(@"root\scripts\".Length), value => value.First())
                        ;
                    }
                }
                return definitions;
            }
        }

        /// <summary>For use in `using`</summary>
        public void Dispose()
        {
            input.Close();
        }
    }
}