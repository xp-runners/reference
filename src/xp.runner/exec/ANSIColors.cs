using System;
using System.IO;

namespace Xp.Runners
{
    public static class ANSIColors
    {
        /// <summary>Dark colors</summary>
        public const int DARK = 0;

        /// <summary>Bright colors</summary>
        public const int BRIGHT = 8;

        /// <summary>The color lookup</summary>
        public static readonly ConsoleColor[] Lookup = { 
            ConsoleColor.Black,
            ConsoleColor.DarkRed,
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkYellow,
            ConsoleColor.DarkBlue,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkCyan,
            ConsoleColor.Gray,

            ConsoleColor.Black,
            ConsoleColor.Red,
            ConsoleColor.Green,
            ConsoleColor.Yellow,
            ConsoleColor.Blue,
            ConsoleColor.Magenta,
            ConsoleColor.Cyan,
            ConsoleColor.White 
        }; 
    }
}