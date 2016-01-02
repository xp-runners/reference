using System;
using System.IO;
using System.Diagnostics;

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
    public int Execute(Process proc)
    {
        using (watcher) 
        {
            watcher.EnableRaisingEvents = true;
            do
            {
                try
                {
                    proc.Start();
                    proc.WaitForExit();
                }
                catch (SystemException e) 
                {
                    throw new EntryPointNotFoundException(proc.StartInfo.FileName + ": " + e.Message, e);
                }
                finally
                {
                    proc.Close();
                }
            } while (!watcher.WaitForChanged(WatcherChangeTypes.Changed).TimedOut);
        }

        return 0;
    }
}