using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Timers;

namespace ChronoLoop.Components.Pages
{
    public enum TimerType { Timer, Stopwatch, Clickcounter }
    public partial class Home
    {
        public string Name { get; set; } = "";
        public TimerType Type { get; set; } = TimerType.Timer;
        public bool IsRunning { get; set; } = false;
        public List<TimeSpan> Laps { get; set; } = new();
        
        List<Home> trackers = new();
        public TimeSpan Elapsed { get; set; } = TimeSpan.Zero;
        public TimeSpan CountdownDuration { get; set; } = TimeSpan.FromMinutes(1);
        public int ClickCount { get; set; } = 0;

        [JsonIgnore] public System.Timers.Timer Timer { get; set; }

        public DateTime? StartTime { get; set; } = null;

        public event Action TickOnMainThread;

        private bool showTypeButtons = false;

        [JsonIgnore] public TimeSpan LiveElapsed => IsRunning && StartTime != null ? Elapsed + (DateTime.Now - StartTime.Value) : Elapsed;
        [JsonIgnore] public string ElapsedTimeFormatted
        { 
            get 
            { 
                try { return LiveElapsed.ToString(@"hh\:mm\:ss"); }
                catch {return "00:00:00";}
            }
        }

        [JsonIgnore] public TimeSpan CurrentTime
        {
            get
            {
                var remaining = CountdownDuration - LiveElapsed;
                return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
            }
        }

        [JsonIgnore] public string CurrentTimeFormatted
        {
            get
            {
                try { return $"{(int)CurrentTime.TotalHours}:{CurrentTime.Minutes:D2}:{CurrentTime.Seconds:D2}"; }
                catch { return "00:00:00"; }
            }
        }

        [JsonIgnore] public double ProgressOffset
        {
            get
            {
                double radius = 88;
                double circumference = 2 * Math.PI * radius;
                if (CountdownDuration.TotalSeconds == 0) return circumference;

                double progress = CurrentTime.TotalSeconds / CountdownDuration.TotalSeconds;
                return circumference * (1 - progress);
            }
        }

        int? selectedIndex = null;

        async void Edit(int index)
        {
            selectedIndex = index;
            await Task.Delay(800);
            Navigation.NavigateTo($"/edit/{index}");
        }
        private void Tick()
        {
            TickOnMainThread?.Invoke();
        }

        public void InitTimer()
        {
            Timer = new System.Timers.Timer(1); // 1 second
            Timer.Elapsed += (s, e) => Tick();
            Timer.AutoReset = true;

            if (IsRunning) Timer.Start();
        }
        
        public void Start()
        {
            if (IsRunning) return;
            StartTime = DateTime.Now;
            Timer?.Start();
            IsRunning = true;
        }

        public void Pause()
        {
            if (!IsRunning) return;
            if (StartTime != null)
                Elapsed += DateTime.Now - StartTime.Value;
            Timer?.Stop();
            IsRunning = false;
        }

        public void Reset()
        {
            Elapsed = TimeSpan.Zero;
            StartTime = null;
            IsRunning = false;
            Timer?.Stop();
        }

        string GetEstimatedEndTime(Home tracker)
        {

            if (!tracker.IsRunning || CurrentTime <= TimeSpan.Zero)
                return "??/??/???? ??:?? ??";

            return DateTime.Now.Add(CurrentTime).ToString("g");
        }

        public string CountdownDurationFormatted => $"{(int)CountdownDuration.TotalHours}:{CountdownDuration.Minutes:D2}:{CountdownDuration.Seconds:D2}";

        // Might be temporary
        public string LiveElapsedFormatted => $"{(int)LiveElapsed.TotalHours}:{LiveElapsed.Minutes:D2}:{LiveElapsed.Seconds:D2}";

        Task Save() => Storage.SaveAsync(trackers);

        private void InitializeTracker(Home tracker)
        {
            tracker.InitTimer();
            tracker.TickOnMainThread += () => InvokeAsync(StateHasChanged);
        }
        protected override async Task OnInitializedAsync()
        {
            trackers = await Storage.LoadAsync();

            foreach (var t in trackers)
                InitializeTracker(t);
        }

        private string animationClass = string.Empty;
        private string rotatingClass = string.Empty;
        private string movingClass = string.Empty;
        private async Task ShowTypeOptions()
        {
            animationClass = "SlideIn";
            rotatingClass = "RotateF";
            movingClass = "moveLeft";
            showTypeButtons = true;
        }
        private async Task HideTypeOptions()
        {
            animationClass = "SlideOut";
            rotatingClass = "RotateB";
            movingClass = "moveRight";
            await Task.Delay(400);
            showTypeButtons = false;
        }


        async Task AddNewItem(TimerType type)
        {
            var tracker = new Home
            {
                Name = $"{type} {trackers.Count + 1}",
                Type = type
            };
            InitializeTracker(tracker);
            trackers.Add(tracker);
            await Save();
            showTypeButtons = false;
        }

        void IncrementClick(Home tracker)
        {
            tracker.ClickCount++;
        }

        async Task Toggle(Home tracker)
        {
            if (tracker.IsRunning) tracker.Pause();
            else tracker.Start();
            await Save();
        }

        async Task Reset(Home tracker)
        {
            tracker.Reset();
            await Save();
        }

        async Task Delete(Home tracker)
        {
            tracker.Timer?.Stop();
            trackers.Remove(tracker);
            await Save();
        }
    }
}
