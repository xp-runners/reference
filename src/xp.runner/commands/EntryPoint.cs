using System;

namespace Xp.Runners.Commands
{
    public class EntryPoint
    {
        const int VENDOR = 1;
        const int NAME = 2;
        const int COMMAND = 3;

        private string type;
        private string module;

        /// <summary>Creates a new entry point from a file, e.g. xp.xp-framework.amunittest.test</summary>
        public EntryPoint(string file)
        {
                var spec = file.Split('.');
                if (spec.Length <= NAME)
                {
                    throw new ArgumentException("Malformed input string `" + file + "`");
                }

                type = string.Format(
                    "xp.{0}.{1}Runner",
                    spec[NAME],
                    spec.Length > COMMAND ? spec[COMMAND].UpperCaseFirst() : string.Empty
                );
                module = spec[VENDOR] + "/" + spec[NAME];
        }

        /// <summary>The entry point type name, e.g. xp.unittest.TestRunner</summary>
        public string Type { get { return type; } }

        /// <summary>The entry point's module, e.g. xp-framework/unittest</summary>
        public string Module { get { return module; } }

    }
}