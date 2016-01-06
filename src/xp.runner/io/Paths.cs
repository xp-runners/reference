using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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
                    var qualified = path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar + file;
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

        /// <summary>Resolve a path. If we're inside Cygwin, try to resolve absolute paths and
        /// paths pointing to home directories relative to its installation directory. Otherwise,
        /// if the file doesn't exist or is not a directory, also check for shortcuts and symlinks</summary>
        public static string Resolve(string path)
        {
            if (Cygwin.Active && path.StartsWith("/"))
            {
                path = Cygwin.Resolve(path) ?? path;
            }

            var info = new FileInfo(path);
            var normalized = info.FullName.TrimEnd(Path.DirectorySeparatorChar);

            if (!info.Exists)
            {
                var link = normalized + ".lnk";
                return File.Exists(link) ? Shortcuts.Resolve(link) : normalized;
            }
            else if ((info.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return normalized;
            }
            else
            {
                return Cygwin.TryResolveSymlinkFile(info) ?? normalized;
            }
        }

        /// <summary>Translate a list of paths</summary>
        public static IEnumerable<string> Translate(string root, string[] paths)
        {
            var HOME = Environment.GetEnvironmentVariable("HOME") ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            foreach (var path in paths)
            {
                // Normalize path
                var normalized = path.Replace('/', Path.DirectorySeparatorChar);

                if (normalized.StartsWith("~"))
                {
                    // Path in home directory
                    yield return Resolve(Compose(HOME, normalized.Substring(1)));
                } 
                else if (normalized.Substring(1).StartsWith(":\\") || normalized.StartsWith("\\\\")) 
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
            var s = new StringBuilder();
            foreach (var component in components)
            {
                s.Append(component.TrimEnd(Path.DirectorySeparatorChar)).Append(Path.DirectorySeparatorChar);
            }
            s.Length--;           // Remove last directory separator
            return s.ToString();
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
            // Codebase is a URI. file:///F:/cygwin/home/Timm Friebe/bin/xp.exe
            var uri = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
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