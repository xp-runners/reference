using System;

namespace Xp.Runners
{
    public class EntryPoint
    {
        const int VENDOR = 1;
        const int NAME = 2;
        const int COMMAND = 3;

        private string command;
        private string package;
        private string type;
        private string module;

        /// <summary>Creates a new entry point from a file, e.g. xp.xp-framework.unittest.test</summary>
        public EntryPoint(string file)
        {
                var spec = file.Split('.');
                if (spec.Length <= NAME)
                {
                    throw new ArgumentException("Malformed input string `" + file + "`");
                }

                command = spec[spec.Length > COMMAND ? COMMAND : NAME];
                package = "xp." + spec[NAME];
                type = package + "." + (spec.Length > COMMAND ? spec[COMMAND].UpperCaseFirst() : string.Empty) + "Runner";
                module = spec[VENDOR] + "/" + spec[NAME];
        }

        /// <summary>The entry point's name, e.g. test</summary>
        public string Command { get { return command; } }

        /// <summary>The entry point's package name, e.g. xp.unittest</summary>
        public string Package { get { return package; } }

        /// <summary>The entry point type name, e.g. xp.unittest.TestRunner</summary>
        public string Type { get { return type; } }

        /// <summary>The entry point's module, e.g. xp-framework/unittest</summary>
        public string Module { get { return module; } }

    }
}