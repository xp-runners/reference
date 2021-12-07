using System;

namespace Xp.Runners
{
    public class CannotExecute : Exception
    {
        public string Origin;

        public CannotExecute(string message, string origin= null) : base(message)
        {
            Origin = origin;
        }

        public CannotExecute(string message, Exception source) : base(message, source)
        {
            Origin = null;
        }
    }
}