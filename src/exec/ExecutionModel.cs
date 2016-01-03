using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

public abstract class ExecutionModel
{
    /// <summary>Execute the process and return its exitcode</summary>
    public abstract int Execute(Process proc);

    protected int Run(Process proc)
    {
        try
        {
            proc.Start();
            proc.WaitForExit();
            return proc.ExitCode;
        }
        catch (SystemException e)
        {
            throw new EntryPointNotFoundException(proc.StartInfo.FileName + ": " + e.Message, e);
        }
        finally
        {
            proc.Close();
        }
    }
}