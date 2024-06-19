using System;
using UnityEngine;

namespace RenderDream.GameEssentials
{
    public abstract class Timer : IDisposable
    {
        public float CurrentTime { get; protected set; }
        public bool IsRunning { get; private set; }

        protected float initialTime;
        protected bool manual;
        protected bool unscaledTime;

        protected float DeltaTime
        {
            get
            {
                if (unscaledTime)
                    return Time.unscaledDeltaTime;
                return Time.deltaTime;
            }
        }

        public virtual float Progress => Mathf.Clamp(CurrentTime / initialTime, 0, 1);

        public Action OnTimerStart = delegate { };
        public Action OnTimerStop = delegate { };

        protected Timer(bool isManual, bool useUnscaledTime)
        {
            manual = isManual;
            unscaledTime = useUnscaledTime;
        }

        public virtual void Start()
        {
            CurrentTime = initialTime;
            if (!IsRunning)
            {
                IsRunning = true;
                if (!manual)
                {
                    TimerManager.RegisterTimer(this);
                }
                OnTimerStart.Invoke();
            }
        }

        public virtual void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                if (!manual)
                {
                    TimerManager.DeregisterTimer(this);
                }
                OnTimerStop.Invoke();
            }
        }

        public abstract void Tick();
        public abstract bool IsFinished { get; }

        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;

        public virtual void Reset() => CurrentTime = initialTime;

        public virtual void Reset(float newTime)
        {
            initialTime = newTime;
            Reset();
        }

        bool disposed;

        ~Timer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Call Dispose to ensure deregistration of the timer from the TimerManager
        /// when the consumer is done with the timer or being destroyed
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing && !manual)
            {
                TimerManager.DeregisterTimer(this);
            }

            disposed = true;
        }
    }
}
