using System;

namespace RenderDream.GameEssentials
{
    public class RepeatingTimer : Timer
    {
        public Action OnIntervalDone = delegate { };
        public float intervalDuration;

        public RepeatingTimer(float intervalDuration = 0, bool useUnscaledTime = false) : base(0, useUnscaledTime) 
        {
            this.intervalDuration = intervalDuration;
        }

        public void Start(float intervalDuration)
        {
            this.intervalDuration = intervalDuration;
            Start();
        }

        public override void Tick()
        {
            if (IsRunning && CurrentTime < intervalDuration)
            {
                CurrentTime += DeltaTime;
            }

            if (IsRunning && CurrentTime >= intervalDuration)
            {
                CurrentTime -= intervalDuration;
                OnIntervalDone.Invoke();
            }
        }

        public override bool IsFinished => false;
    }
}
