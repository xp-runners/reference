using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Xp.Runners.IO
{
    static class Cygwin
    {
        private const string CYGDRIVE_PATH = "/cygdrive/";
        private const string INSTALLATIONS = @"Software\Cygwin\Installations";

        private static byte[] SYMLINK_COOKIE = new byte[] { 33, 60, 115, 121, 109, 108, 105, 110, 107, 62 };

        /// <summary>Determine whether we're runnning inside Cygwin</summary>
        public static bool Active { get { return null != Environment.GetEnvironmentVariable("SHELL"); } }

        /// <summary>Determine Cygwin installation directories. Caches information</summary>
        public static IEnumerable<string> Installations()
        {
            var installed = Registry.CurrentUser.OpenSubKey(INSTALLATIONS) ?? Registry.LocalMachine.OpenSubKey(INSTALLATIONS);
            if (null == installed)
            {
                throw new NotSupportedException("Cannot determine Cygwin path via registry [" + INSTALLATIONS + "]");
            }

            return installed.GetValueNames()
                .Select(key => installed.GetValue(key) as string)
                .Select(path => path.Replace(@"\??\", ""))
            ;
        }

        /// <summary>Resolve directory. Supports /cygdrive and absolute paths</summary>
        public static string Resolve(string path)
        {
            if (path.StartsWith(CYGDRIVE_PATH))
            {
                return path[CYGDRIVE_PATH.Length] + ":" + path.Substring(CYGDRIVE_PATH.Length + 1);
            }

            var absolute = path.Replace("/", Path.DirectorySeparatorChar.ToString());
            return Installations()
                .Where(Directory.Exists)
                .Select(root => root + absolute)
                .FirstOrDefault()
            ;
        }

        /// <summary>Checks whether a file is a cygwin symlink file, and resolves it. See the
        /// Cygwin docs, https://cygwin.com/cygwin-ug-net/using.html#pathnames-symlinks</summary>
        public static string TryResolveSymlinkFile(FileInfo info)
        {
            if ((info.Attributes & FileAttributes.System) != FileAttributes.System) return null;

            using (var stream = info.OpenRead())
            {
                var cookie = new byte[SYMLINK_COOKIE.Length];
                stream.Read(cookie, 0, SYMLINK_COOKIE.Length);
                if (!cookie.SequenceEqual(SYMLINK_COOKIE)) return null;

                using (var text = new StreamReader(stream, true))
                {
                    return text.ReadToEnd().TrimEnd('\0');
                }
            }
        }
    }
}