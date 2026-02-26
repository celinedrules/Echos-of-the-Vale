using System.Collections.Generic;
using Utilities;

namespace Managers
{
    public class TimerManager
    {
        public static TimerManager Instance { get; } = new();

        private readonly List<Timer> _timers = new();
        private bool _isPaused;
        
        public void Update(float deltaTime, float unscaledDeltaTime)
        {
            if (_isPaused) return;

            for (int i = _timers.Count - 1; i >= 0; i--)
            {
                Timer timer = _timers[i];
                float dt = timer.UseUnscaledTime ? unscaledDeltaTime : deltaTime;
                timer.Tick(dt);

                if (timer.IsCompleted)
                    _timers.RemoveAt(i);
            }
        }

        public Timer CreateTimer(float duration, System.Action onComplete = null, bool useUnscaledTime = false)
        {
            Timer timer = new Timer(duration, onComplete, useUnscaledTime);
            _timers.Add(timer);
            timer.Start();
            return timer;
        }

        public void PauseAll() => _isPaused = true;
        public void ResumeAll() => _isPaused = false;
        public void ClearAll() => _timers.Clear();
    }
}