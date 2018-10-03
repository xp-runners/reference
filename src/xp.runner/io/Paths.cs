using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using Xp.Runners;

namespace Xp.Runners.IO
{
    static class Paths
    {
        private static string separator = new string(new char[] { Path.PathSeparator });

        /// <summary>The path separator char, ";" on Windows, ":" on Un*x, as a string</summary>
        public static string Separator {
            get { return separator; }
        }

        /// <summary>Returns the directory name of a given file name</summary>
        public static string DirName(this string filename)
        {
            return filename.Substring(0, filename.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        }

        /// <summary>Try to locate a given file inside multiple base paths. Same as Locate() but does not
        /// throw an exception but yields an empty result.</summary>
        public static IEnumerable<string> TryLocate(IEnumerable<string> bases, IEnumerable<string> files)
        {
            foreach (var path in bases)
            {
                foreach (var file in files)
                {
                    var qualified = path.TrimEnd(new char[] { Path.DirectorySeparatorChar }) + Path.DirectorySeparatorChar + file;
                    if (File.Exists(qualified))
                    {
                        yield return qualified;
                    }
                }
            }
        }

        /// <summary>Locate a given file inside multiple base paths. Throw an exception if the file cannot
        /// be found. Similar to what is done when looking up program names in $ENV{PATH}.</summary>
        public static IEnumerable<string> Locate(IEnumerable<string> bases, IEnumerable<string> files)
        {
            var found = false;
            foreach (var result in TryLocate(bases, files))
            {
                yield return result;
                found = true;
            }

            if (!found)
            {
                throw new FileNotFoundException("Cannot find [" + string.Join(", ", files) + "] in [" + string.Join(", ", bases) + "]");
            }
        }

        /// <summary>Returns home path</summary>
        public static string Home()
        {
            return Environment.GetEnvironmentVariable("HOME") ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }

        /// <summary>Returns whether the environment indicates this system conforms to
        /// https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html</summary>
        public static bool UseXDG()
        {
            foreach (string variable in Environment.GetEnvironmentVariables().Keys)
            {
                if (variable.StartsWith("XDG_")) return true;
            }
            return false;
        }

        /// <summary>Returns the user-specific config directory. Respects $HOME, XDG-compliant
        /// systems and falls back to using %APPDATA%</summary>
        public static string UserDir(string name)
        {
            if (UseXDG())
            {
                return Compose(
                    Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? Compose(Home(), ".config"),
                    name
                );
            }

            var home = Environment.GetEnvironmentVariable("HOME");
            if (String.IsNullOrEmpty(home))
            {
                return Compose(Environment.SpecialFolder.ApplicationData, Strings.UpperCaseFirst(name));
            }
            else
            {
                return Compose(home, "." + name);
            }

        }

        /// <summary>Resolve a path. If the path is actually a shell link (.lnk file), this link's target path is used</summary>
        public static string Resolve(string path)
        {
            var info = new FileInfo(path);
            var normalized = info.FullName.TrimEnd(new char[] { Path.DirectorySeparatorChar });
            if (!info.Exists)
            {
                var link = normalized + ".lnk";
                if (File.Exists(link)) 
                {
                    return Shortcuts.Resolve(link);
                }
            }
            return normalized;
        }

        /// <summary>Translate a list of paths</summary>
        public static IEnumerable<string> Translate(string root, string[] paths)
        {
            var homePath = Home();
            var directorySeparator = new string(new char[] { Path.DirectorySeparatorChar });
            foreach (var path in paths)
            {
                // Normalize path
                var normalized = path.Replace('/', Path.DirectorySeparatorChar);

                if (normalized.StartsWith("~"))
                {
                    // Path in home directory
                    yield return Resolve(Compose(homePath, normalized.Substring(1)));
                } 
                else if (normalized.Substring(1).StartsWith(":\\") || normalized.StartsWith(directorySeparator))
                {
                    // Fully qualified path
                    yield return Resolve(normalized);
                }
                else
                {
                    // Relative path, prepend root
                    yield return Resolve(Compose(root, normalized));
                }
            }
        }

        /// <summary>Composes a path name of two or more components - varargs</summary>
        public static string Compose(params string[] components) 
        {
            return string.Join(new string(new char[] { Path.DirectorySeparatorChar }), components
                .Where(c => !string.IsNullOrEmpty(c))
                .Select(c => c.TrimEnd(new char[] { Path.DirectorySeparatorChar }))
                .ToArray()
            );
        }
        
        /// <summary>Composes a path name of a special folder and a string component</summary>
        public static string Compose(Environment.SpecialFolder special, params string[] components) 
        {
            var args = new string[1 + components.Length];
            args[0] = Environment.GetFolderPath(special);
            Array.Copy(components, 0, args, 1, components.Length);
            return Compose(args);
        }

        /// <summary>Return binary file of currently executing process</summary>
        public static string Binary()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var main = Process.GetCurrentProcess().MainModule.FileName;

            if (Path.GetFileNameWithoutExtension(assembly.Location) == Path.GetFileNameWithoutExtension(main))
            {
                return main;
            }

            // We arrive here when run by Mono, with MainModule.FileName = "/path/to/mono". If so, use
            // codebase, which is a file://-URI. Caution: Inside applications created with mkbundle, the
            // codebase is worthless, as it always contains the current directory. This is why we compare
            // the filenames above, and short-circuit!
            var uri = new Uri(assembly.CodeBase);
            if (uri.IsFile)
            {
                return Uri.UnescapeDataString(uri.AbsolutePath.Replace('/', Path.DirectorySeparatorChar));
            }
            else
            {
                throw new IOException("Don't know how to handle " + uri.AbsoluteUri);
            }
        }
    }
}