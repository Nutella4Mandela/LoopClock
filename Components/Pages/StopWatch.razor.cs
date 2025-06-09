using MudBlazor;
using System.Timers;

namespace Time_Zone.Components.Pages
{
    public partial class StopWatch
    {
        const string Default_Time = "00:00:00";
        string elapsedTime = Default_Time;
        System.Timers.Timer timer = new System.Timers.Timer(1);
        DateTime startTime = DateTime.Now;
        bool isRunning = false;
        TimeSpan pausedElapsedTime = TimeSpan.Zero;

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            DateTime currentTime = e.SignalTime;
            elapsedTime = $"{currentTime.Subtract(startTime)}".Substring(0,8);
            InvokeAsync(StateHasChanged);
        }

        void StartTimer()
        {
            startTime = DateTime.Now - pausedElapsedTime;
            timer = new System.Timers.Timer(1);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
            isRunning = true;
        }

        void PauseTimer()
        {
            isRunning = false;
            timer.Enabled = false;
            pausedElapsedTime = DateTime.Now - startTime;
        }
        void ResetTimer()
        {
            isRunning = false;
            timer.Enabled = false;
            pausedElapsedTime = TimeSpan.Zero;
            elapsedTime = Default_Time;
        }

        void OnTimerChanges()
        {
            if (!isRunning)
                StartTimer();
            else
                PauseTimer();
        }

        void OnTimerResets()
        {
            ResetTimer();
            isRunning = false;
        }
    }
}