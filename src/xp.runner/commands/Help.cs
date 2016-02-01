using System;
using System.Linq;
using System.Collections.Generic;
using Xp.Runners;
using Xp.Runners.Config;

namespace Xp.Runners.Commands
{
    /// <summary>help [$command]</summary>
    public class Help : Command
    {
        private const string HELP = "xp.runtime.Help";

        private IEnumerable<string> arguments;
        private IEnumerable<string> modules;

        /// <summary>Returns the resource corresponding to the topic and package</summary>
        private string Topic(string package, string topic)
        {
            return string.Format("@{0}/{1}.md", package, string.IsNullOrEmpty(topic) || "*"  == topic ? "index" : topic);
        }

        /// <summary>Initialize this command</summary>
        public override void Initialize(CommandLine cmd, ConfigSource configuration)
        {
            var arg = cmd.Arguments.FirstOrDefault();
            if (null == arg) 
            {
                arguments = new string[] { HELP, Topic("xp/runtime", "xp") };
                modules = new string[] { };
                return;
            }
            else if (arg.StartsWith("/"))
            {
                arguments = new string[] { HELP, Topic("xp/runtime", arg.Substring(1)) };
                modules = new string[] { };
                return;
            }
            else
            {
                var topic = arg.Split(new char[] { '/' }, 2);
                if (null == Type.GetType("Xp.Runners.Commands." + topic[0].UpperCaseFirst()))
                {
                    var plugin = new Plugin(topic[0]);
                    plugin.Initialize(cmd, configuration);

                    arguments = new string[] { HELP, topic.Length > 1 ? Topic(plugin.EntryPoint.Package.Replace('.', '/'), topic[1]) : plugin.EntryPoint.Type };
                    modules = plugin.Modules;
                }
                else
                {
                    arguments = new string[] { HELP, Topic("xp/runtime", topic.Length > 1 ? topic[0] + '/' + topic[1] : topic[0]) };
                    modules = new string[] { };
                }
            }
        }

        /// <summary>Command line arguments.</summary>
        protected override IEnumerable<string> ArgumentsFor(CommandLine cmd)
        {
            return arguments;
        }

        /// <summary>Command line arguments.</summary>
        protected override IEnumerable<string> ModulesFor(CommandLine cmd)
        {
            return modules;
        }
    }
}