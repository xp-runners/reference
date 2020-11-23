using System.Collections.Generic;
using Xp.Runners;

namespace Xp.Runners.Commands
{
    /// <summary>A stripped-down representation of a composer.json file</summary>
    public class Composer
    {
        public string Name { get; set; }

        public Dictionary<string, string> Require { get; set; }

        public Dictionary<string, string> Scripts { get; set; }
    }
}