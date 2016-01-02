using System;
using System.Diagnostics;

public class RunOnce : ExecutionModel
{
    /// <summary>Execute the process and return its exitcode</summary>
    public int Execute(Process proc)
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