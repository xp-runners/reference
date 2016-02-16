using System;
using Xp.Runners.IO;
using Xp.Runners.Config;

namespace Xp.Runners
{
    public class Xp
    {
        /// <summary>Entry point</summary>
        public static int Main(string[] args)
        {
            try
            {
                return new CommandLine(args).Execute();
            }
            catch (NotImplementedException e)
            {
                Console.Error.WriteLine("Command not implemented: {0}", e.Message);
                return 2;
            }
            catch (EntryPointNotFoundException e)
            {
                Console.Error.WriteLine("Problem executing runtime: {0}", e.Message);
                return 2;
            }
        }
    }
}