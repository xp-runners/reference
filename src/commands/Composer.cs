using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Xp.Runners
{
    /// <summary>A stripped-down representation of a composer.json file</summary>
    [DataContract]
    public class Composer
    {
        public const string FILENAME = "composer.json";

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "require")]
        public Dictionary<string, string> Require { get; set; }
    }
}