using Microsoft.Extensions.Logging;
using System;
using System.Timers;

namespace TranscriptsProcessor.Services
{
    public class Scheduler
    {
        //public Scheduler(ILogger logger)
        //{
        //    Logger = logger;
        //}

        public void Start()
        {
            TimeSpan timeToGo = GetNextMidnight() - DateTime.Now;
            if (timeToGo < TimeSpan.Zero)
            {
                timeToGo += TimeSpan.FromDays(1); // next day if it's already past midnight
            }

            timer = new Timer(timeToGo.TotalMilliseconds);
            timer.Elapsed += (sender, args) => TimerElapsed(sender, args, timer);
            timer.AutoReset = false; // Ensure the timer runs only once
            timer.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e, Timer timer)
        {
            PerformScheduledTask();

            // Reset the timer to fire again in 24 hours
            timer.Interval = 86400000; // 24 hours in milliseconds
            timer.Start();
        }

        private DateTime GetNextMidnight()
        {
            return DateTime.Today.AddDays(1);
        }

        private void PerformScheduledTask()
        {
            Console.WriteLine("Performing scheduled task at " + DateTime.Now);
            var service = new Service(Logger);
            service.Run("");
        }

        private Timer timer;
        private readonly ILogger Logger;
    }
}