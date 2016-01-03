using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace Xp.Runners.Config
{
    class Ini
    {
        private const int KEY = 0;
        private const int VALUE = 1;
        private static char[] SEPARATOR = new char[] { '=' };

        private string file;
        private Dictionary<string, Dictionary<string, List<string>>> sections;
        private bool parsed;

        /// <summary>Filename which to parse</summary>
        public string FileName 
        {
            get { return file; }
            set { 
                file = value;
                sections = new Dictionary<string, Dictionary<string, List<string>>>();
                parsed = false;
            }
        }

        /// <summary>Creates a new INI file</summary>
        public Ini(string file) 
        {
            FileName = file;
        }

        /// <summary>Test whether the underlying file exists yet</summary>
        public bool Exists()
        {
            return File.Exists(FileName);
        }

        /// <summary>Parse</summary>
        private void Parse(bool reset)
        {
            if (!Exists()) return;

            lock(this) 
            {
                if (parsed && !reset) return;    // Short-circuit this

                string section = "default";
                sections[section] = new Dictionary<string, List<string>>();
                foreach (string line in File.ReadAllLines(FileName))
                {
                    if (string.IsNullOrEmpty(line) || line.StartsWith(";")) 
                    {
                        continue;
                    } 
                    else if (line.StartsWith("[")) 
                    {
                        section = line.Substring(1, line.Length - 1 - 1);    
                        sections[section] = new Dictionary<string, List<string>>();
                        continue;
                    }
                    else
                    {
                        var pair = line.Split(SEPARATOR, 2);
                        if (!sections[section].ContainsKey(pair[KEY])) 
                        {
                            sections[section][pair[KEY]] = new List<string>();
                        }
                        if (!String.IsNullOrEmpty(pair[VALUE]))
                        {
                            sections[section][pair[KEY]].Add(pair[VALUE]);
                        }
                    }
                }
                parsed = true;
            }
        }

        /// <summary>Gets a single value for a given key</summary>
        public string Get(string section, string key, string defaultValue = null)
        {
            Parse(false);
            if (!sections.ContainsKey(section)) return defaultValue;
            if (!sections[section].ContainsKey(key)) return defaultValue;
            return sections[section][key].FirstOrDefault();
        }   

        /// <summary>Gets all values for a key</summary>
        public IEnumerable<string> GetAll(string section, string key, IEnumerable<string> defaultValue = null)
        {
            Parse(false);
            if (!sections.ContainsKey(section)) return defaultValue;
            if (!sections[section].ContainsKey(key)) return defaultValue;
            return sections[section][key];
        }   

        /// <summary>Gets all keys in a section</summary>
        public IEnumerable<string> Keys(string section, IEnumerable<string> defaultValue = null)
        {
            Parse(false);
            if (!sections.ContainsKey(section)) return defaultValue;
            return sections[section].Keys;
        }
    }
}