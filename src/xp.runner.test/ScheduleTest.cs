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

        private DateTime At(DateTime date, string time)
        {
            var t = time.Split(':');
            return date.AddHours(Convert.ToInt32(t[0])).AddMinutes(Convert.ToInt32(t[1]));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(255)]
        public void run(int exitcode)
        {
            Assert.Equal(exitcode, new Schedule("").Run(() => exitcode));
        }

        [Fact]
        public void run_passes_on_exceptions()
        {
            Assert.Throws<InvalidOperationException>(() => new Schedule("").Run(() => {
                throw new InvalidOperationException("Expected");
            }));
        }

        [Fact]
        public void continues_true_before_first_run()
        {
            Assert.Equal(true, new Schedule("").Continue());
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
        public void continues_immediately_by_default()
        {
            var schedule = new Schedule("");
            schedule.Run(() => 0);

            var delayed = TimeSpan.Zero;
            schedule.Continue(delay => delayed = delay);
            Assert.Equal(TimeSpan.Zero, delayed);
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
        public void delay_when_running_every()
        {
            var schedule = new Schedule("every 1:00");
            schedule.Run(() => 0, DateTime.Now, runtime);

            var delayed = TimeSpan.Zero;
            schedule.Continue(delay => delayed = delay);
            Assert.Equal(30.0, delayed.TotalSeconds, PRECISION);
        }

        [Fact]
        public void delay_when_running_after()
        {
            var schedule = new Schedule("after 1:00");
            schedule.Run(() => 0, DateTime.Now, runtime);

            var delayed = TimeSpan.Zero;
            schedule.Continue(delay => delayed = delay);
            Assert.Equal(60.0, delayed.TotalSeconds, PRECISION);
        }

        [Theory]
        [InlineData("every 1:00")]
        [InlineData("after 1:00")]
        [InlineData("immediately")]
        public void no_initial_delay_for(string input)
        {
            var schedule = new Schedule(input);

            var delayed = TimeSpan.Zero;
            schedule.Continue(delay => delayed = delay);
            Assert.Equal(0.0, delayed.TotalSeconds, PRECISION);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 0, 1)]
        [InlineData(0, 30, 0)]
        [InlineData(3, 42, 1)]
        [InlineData(16, 0, 0)]
        [InlineData(23, 59, 59)]
        public void initial_delay_running_at(int hour, int minute, int second)
        {
            var schedule = new Schedule("at " + hour + ":" + minute + ":" + second, DateTime.Today);

            var delayed = TimeSpan.Zero;
            schedule.Continue(delay => delayed = delay);
            Assert.Equal(hour * 3600.0 + minute * 60.0 + second, delayed.TotalSeconds, PRECISION);
        }

        [Theory]
        [InlineData(12, 0, 0)]
        [InlineData(16, 42, 35)]
        [InlineData(23, 59, 59)]
        public void initial_delay_on_same_day(int hour, int minute, int second)
        {
            var schedule = new Schedule("at " + hour + ":" + minute + ":" + second, At(DateTime.Today, "12:00"));

            var delayed = TimeSpan.Zero;
            schedule.Continue(delay => delayed = delay);
            Assert.Equal((hour - 12) * 3600.0 + minute * 60.0 + second, delayed.TotalSeconds, PRECISION);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(3, 42, 1)]
        [InlineData(11, 59, 59)]
        public void initial_delay_until_next_day(int hour, int minute, int second)
        {
            var schedule = new Schedule("at " + hour + ":" + minute + ":" + second, At(DateTime.Today, "12:00"));

            var delayed = TimeSpan.Zero;
            schedule.Continue(delay => delayed = delay);
            Assert.Equal((hour + 12) * 3600.0 + minute * 60.0 + second, delayed.TotalSeconds, PRECISION);
        }

        [Fact]
        public void starts_at_first_time_in_future()
        {
            var schedule = new Schedule("at 03:00 06:30 09:15", At(DateTime.Today, "04:00"));
            var delayed = TimeSpan.Zero;

            schedule.Continue(delay => delayed = delay);
            Assert.Equal(TimeSpan.FromHours(2.5), delayed);
        }

        [Fact]
        public void running_at_list_of_times()
        {
            var schedule = new Schedule("at 03:00 06:30 09:15", DateTime.Today);
            var delayed = TimeSpan.Zero;

            schedule.Continue(delay => delayed = delay);
            Assert.Equal(TimeSpan.FromHours(3.0), delayed);

            schedule.Run(() => 0, At(DateTime.Today, "03:00"), TimeSpan.FromMinutes(15));
            schedule.Continue(delay => delayed = delay);
            Assert.Equal(TimeSpan.FromHours(3.25), delayed);

            schedule.Run(() => 0, At(DateTime.Today, "06:30"), TimeSpan.Zero);
            schedule.Continue(delay => delayed = delay);
            Assert.Equal(TimeSpan.FromHours(2.75), delayed);

            schedule.Run(() => 0, At(DateTime.Today, "09:15"), TimeSpan.FromMinutes(15));
            schedule.Continue(delay => delayed = delay);
            Assert.Equal(TimeSpan.FromHours(17.50), delayed);

            schedule.Run(() => 0, At(DateTime.Today.AddDays(1), "03:00"), TimeSpan.FromMinutes(30));
            schedule.Continue(delay => delayed = delay);
            Assert.Equal(TimeSpan.FromHours(3.0), delayed);
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
        [InlineData("ät 3:00")]
        public void unparseable_spec(string input)
        {
            Assert.Throws<ArgumentException>(() => new Schedule(input));
        }

        [Theory]
        [InlineData("every")]
        [InlineData("after")]
        [InlineData("until")]
        [InlineData("at")]
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