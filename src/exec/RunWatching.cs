using System.IO;
using System.Diagnostics;

namespace Xp.Runners
{

    public class RunWatching : ExecutionModel
    {
        private FileSystemWatcher watcher;

        public RunWatching(string path)
        {
            watcher = new FileSystemWatcher {
                Path = path,
                IncludeSubdirectories = true,
                Filter = "*.*"
            };
        }

        /// <summary>Execute the process and return its exitcode</summary>
        public override int Execute(Process proc)
        {
            using (watcher)
            {
                watcher.EnableRaisingEvents = true;
                do
                {
                    Run(proc);
                } while (!watcher.WaitForChanged(WatcherChangeTypes.Changed).TimedOut);
            }

            return 0;
        }
    }
}