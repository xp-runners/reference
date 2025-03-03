﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace Xp.Runners.Config
{
    public class Ini
    {
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
            lock(this) 
            {
                if (parsed && !reset) return;    // Short-circuit this

                var section = "default";
                sections[section] = new Dictionary<string, List<string>>();
                foreach (string line in File.ReadAllLines(FileName))
                {
                    if (line.StartsWith("[")) 
                    {
                        section = line.Substring(1, line.Length - 1 - 1);    
                        sections[section] = new Dictionary<string, List<string>>();
                    }
                    else if (line.Contains("="))
                    {
                        var pair = line.Split(SEPARATOR, 2);
                        var key = pair[0].Trim();
                        var val = pair[1].Trim();
                        if (!sections[section].ContainsKey(key)) 
                        {
                            sections[section][key] = new List<string>();
                        }
                        if (0 == val.Length) continue;

                        // Handle double and single quotes as well as backticks, and their escaping.
                        if (val.StartsWith("\"") && val.EndsWith("\""))
                        {
                            sections[section][key].Add(val.Substring(1, val.Length - 2).Replace("\\\"", "\""));
                        }
                        else if (val.StartsWith("'") && val.EndsWith("'"))
                        {
                            sections[section][key].Add(val.Substring(1, val.Length - 2).Replace("\\'", "'"));
                        }
                        else if (val.StartsWith("`") && val.EndsWith("`"))
                        {
                            sections[section][key].Add(val.Substring(1, val.Length - 2).Replace("\\`", "`"));
                        }
                        else
                        {
                            sections[section][key].Add(val);
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
            return sections[section][key].FirstOrDefault() ?? defaultValue;
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

        /// <summary>Tests for equality</summary>
        public override bool Equals(Object obj)
        {
            return obj is Ini && ((Ini) obj).file == this.file;
        }

        /// <summary>Hashcode implementation</summary>
        public override int GetHashCode()
        {
            return this.file.GetHashCode();
        }
    }
}