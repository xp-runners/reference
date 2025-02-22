using System;
using System.Text;
using System.Diagnostics;
using Xp.Runners;

namespace Xp.Runners.Exec
{
    public class RunRepeatedly : ExecutionModel
    {
        private Schedule schedule;

        /// <summary>Creates a new repeating runner</summary>
        public RunRepeatedly(string spec)
        {
            try
            {
                schedule = new Schedule(spec);
            }
            catch (Exception e)
            {
                throw new CannotExecute("Repeat: " + e.Message, e);
            }
        }

        /// <summary>Returns the model's name</summary>
        public override string Name { get { return "repeat"; } }

        /// <summary>Returns the model's schedule</summary>
        public Schedule Schedule { get { return schedule; } }

        /// <summary>Execute the process and return its exitcode</summary>
        public override int Execute(Process proc)
        {
            int exitcode = 0;

            while (schedule.Continue())
            {
                exitcode = schedule.Run(() => Run(proc));
            }
            return exitcode;
        }
    }
}