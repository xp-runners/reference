using System;

namespace Xp.Runners.IO
{
    public class FileFormatException : FormatException 
    {
        public FileFormatException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}