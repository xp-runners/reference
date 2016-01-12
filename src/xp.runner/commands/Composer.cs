using System.Runtime.Serialization;
using System.Collections.Generic;
using Xp.Runners;

namespace Xp.Runners.Commands
{
    /// <summary>A stripped-down representation of a composer.json file</summary>
    [DataContract]
    public class Composer
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "require")]
        public Dictionary<string, string> Require { get; set; }
    }
}