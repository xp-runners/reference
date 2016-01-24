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
        protected override void Initialize(CommandLine cmd, ConfigSource configuration)
        {
            var arg = cmd.Arguments.FirstOrDefault();
            if (null == arg) 
            {
                arguments = new string[] { HELP, Topic("xp/runtime", "xp") };
                modules = new string[] { };
            }
            else if (arg.StartsWith("/"))
            {
                arguments = new string[] { HELP, Topic("xp/runtime", arg.Substring(1)) };
                modules = new string[] { };
            }
            else if (arg.Contains("/"))
            {
                var topic = arg.Split(new char[] { '/' }, 2);
                var plugin = new Plugin(topic[0]);
                arguments = new string[] { HELP, Topic(plugin.EntryPoint.Package.Replace('.', '/'), topic[1]) };
                modules = plugin.Modules;
            }
            else if (null != Type.GetType("Xp.Runners.Commands." + arg.UpperCaseFirst()))
            {
                arguments = new string[] { HELP, Topic("xp/runtime", arg) };
                modules = new string[] { };
            }
            else
            {
                var plugin = new Plugin(arg);
                arguments = new string[] { HELP, plugin.EntryPoint.Type };
                modules = plugin.Modules;
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