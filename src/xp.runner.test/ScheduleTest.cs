using System;
using Xunit;
using Xunit.Extensions;
using Xp.Runners.Exec;

namespace Xp.Runners.Test
{
    public class ScheduleTest
    {
        const int PRECISION = 1;

        private TimeSpan runtime = TimeSpan.FromSeconds(30.0);

        [Theory]
        [InlineData("")]
        [InlineData("immediately")]
        public void immediate(string input)
        {
            var schedule = new Schedule(input);
            Assert.Equal(0.0, schedule.Wait.TotalSeconds, PRECISION);
        }

        [Fact]
        public void every_five_minutes()
        {
            var schedule = new Schedule("every 5:00");
            Assert.Equal(300.0, schedule.Wait.TotalSeconds, PRECISION);
        }

        [Fact]
        public void after_thirty_seconds()
        {
            var schedule = new Schedule("after 0:30");
            Assert.Equal(30.0, schedule.Wait.TotalSeconds, PRECISION);
        }

        [Theory]
        [InlineData("")]
        [InlineData("forever")]
        public void forever(string input)
        {
            var schedule = new Schedule(input);
            Assert.Equal(false, schedule.Until(0));
        }

        [Theory]
        [InlineData("until 0")]
        [InlineData("until success")]
        public void until_success(string input)
        {
            var schedule = new Schedule(input);
            Assert.Equal(true, schedule.Until(0));
        }

        [Theory]
        [InlineData("until 255")]
        [InlineData("until error")]
        public void until_error(string input)
        {
            var schedule = new Schedule(input);
            Assert.Equal(true, schedule.Until(255));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(255)]
        public void run(int exitcode)
        {
            var schedule = new Schedule("");
            Assert.Equal(exitcode, schedule.Run(() => exitcode));
        }

        [Fact]
        public void run_passes_on_exceptions()
        {
            Assert.Throws<InvalidOperationException>(() => new Schedule("").Run(() => {
                throw new InvalidOperationException("Expected");
            }));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(255)]
        public void continues_forever_by_default(int exitcode)
        {
            var schedule = new Schedule("");
            schedule.Run(() => exitcode);
            Assert.Equal(true, schedule.Continue());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(255)]
        public void continues_forever_when_given_forever(int exitcode)
        {
            var schedule = new Schedule("forever");
            schedule.Run(() => exitcode);
            Assert.Equal(true, schedule.Continue());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(255)]
        public void continue_until_success(int exitcode)
        {
            var schedule = new Schedule("until success");

            schedule.Run(() => exitcode);
            Assert.Equal(true, schedule.Continue());

            schedule.Run(() => 0);
            Assert.Equal(false, schedule.Continue());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(255)]
        public void continue_until_error(int exitcode)
        {
            var schedule = new Schedule("until error");

            schedule.Run(() => 0);
            Assert.Equal(true, schedule.Continue());

            schedule.Run(() => exitcode);
            Assert.Equal(false, schedule.Continue());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(255)]
        public void continue_until_codes(int exitcode)
        {
            var schedule = new Schedule("until 1|255");

            schedule.Run(() => 0);
            Assert.Equal(true, schedule.Continue());

            schedule.Run(() => exitcode);
            Assert.Equal(false, schedule.Continue());
        }

        [Fact]
        public void continue_immediately()
        {
            var schedule = new Schedule("immediately");
            schedule.Run(() => 0);

            var delayed = TimeSpan.Zero;
            schedule.Continue(delay => delayed = delay);
            Assert.Equal(TimeSpan.Zero, delayed);
        }

        [Fact]
        public void continue_after_one_second()
        {
            var schedule = new Schedule("after 0:01");

            schedule.Run(() => 0);

            var delayed = TimeSpan.Zero;
            schedule.Continue(delay => delayed = delay);
            Assert.Equal(1.0, delayed.TotalSeconds, PRECISION);
        }

        [Fact]
        public void delay_running_every()
        {
            var schedule = new Schedule("every 1:00");
            schedule.Run(() => 0, DateTime.Now - runtime);

            var delayed = TimeSpan.Zero;
            schedule.Continue(delay => delayed = delay);
            Assert.Equal(30.0, delayed.TotalSeconds, PRECISION);
        }

        [Fact]
        public void delay_running_after()
        {
            var schedule = new Schedule("after 1:00");
            schedule.Run(() => 0, DateTime.Now - runtime);

            var delayed = TimeSpan.Zero;
            schedule.Continue(delay => delayed = delay);
            Assert.Equal(60.0, delayed.TotalSeconds, PRECISION);
        }

        [Fact]
        public void breaks_immediately_when_condition_reached()
        {
            var schedule = new Schedule("after 0:03,until success");
            schedule.Run(() => 0);
            Assert.Equal(false, schedule.Continue(delay => {
                throw new InvalidOperationException("Should not be reached!");
            }));
        }

        [Theory]
        [InlineData("everi 1:00")]
        [InlineData("afters 0:30")]
        [InlineData("untyl 2")]
        public void unparseable_spec(string input)
        {
            Assert.Throws<ArgumentException>(() => new Schedule(input));
        }

        [Theory]
        [InlineData("every")]
        [InlineData("after")]
        [InlineData("until")]
        [InlineData("every 0:30,until")]
        public void missing_arg_for_spec(string input)
        {
            Assert.Throws<ArgumentException>(() => new Schedule(input));
        }

        [Theory]
        [InlineData("every ")]
        [InlineData("every 0")]
        [InlineData("every a")]
        [InlineData("every 0:")]
        [InlineData("every a:")]
        [InlineData("every :0")]
        [InlineData("every :a")]
        [InlineData("every :")]
        [InlineData("every ::")]
        [InlineData("every 30:a")]
        [InlineData("every a:30")]
        public void unparseable_timespan(string input)
        {
            Assert.Throws<FormatException>(() => new Schedule(input));
        }
    }
}