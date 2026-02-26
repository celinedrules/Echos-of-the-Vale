using System;

namespace Utilities
{
    public class Timer
    {
        public float Duration { get; private set; }
        public float RemainingTime { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsCompleted => RemainingTime <= 0;
        public bool UseUnscaledTime { get; private set; }
        public bool IsCancelled { get; private set; }
        private Action _onComplete;

        public Timer(float duration, Action onComplete = null, bool useUnscaledTime = false)
        {
            Duration = duration;
            RemainingTime = duration;
            _onComplete = onComplete;
            UseUnscaledTime = useUnscaledTime;
        }

        public void Start() => IsRunning = true;
        public void Pause() => IsRunning = false;
        
        public void Cancel()
        {
            IsCancelled = true;
            IsRunning = false;
            _onComplete = null; 
        }
        
        public void Reset()
        {
            RemainingTime = Duration;
            IsRunning = false;
            IsCancelled = false;
        }
        
        public void Restart()
        {
            RemainingTime = Duration;
            IsCancelled = false;
            IsRunning = true;
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning || IsCancelled) return;

            RemainingTime -= deltaTime;
        
            if (RemainingTime <= 0)
            {
                RemainingTime = 0;
                IsRunning = false;
                _onComplete?.Invoke();
            }
        }
    }
}