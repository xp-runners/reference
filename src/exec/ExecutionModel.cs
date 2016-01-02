using System.Diagnostics;

public interface ExecutionModel
{
    /// <summary>Execute the process and return its exitcode</summary>
    int Execute(Process proc);
}