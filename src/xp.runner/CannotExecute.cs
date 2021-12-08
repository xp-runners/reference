using System;

namespace Xp.Runners
{
    public class CannotExecute : Exception
    {
        public string Origin;
        public string Advice;

        /// <summary>Creates an error from a message and an optional origin</summary>
        public CannotExecute(string message, string origin= null) : base(message)
        {
            Origin = origin;
        }

        /// <summary>Creates an error from a message and source</summary>
        public CannotExecute(string message, Exception source) : base(message, source)
        {
            Origin = null;
        }

        /// <summary>Sets advice - steps users can perform to prevent this error</summary>
        public CannotExecute Advise(string advice)
        {
            Advice = advice;
            return this;
        }
    }
}