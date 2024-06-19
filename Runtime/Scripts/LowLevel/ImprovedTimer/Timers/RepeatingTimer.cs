using System;
using UnityEngine;

namespace RenderDream.GameEssentials
{
    public class RepeatingTimer : Timer
    {
        public Action OnIntervalDone = delegate { };
        public float intervalDuration;

        public override float Progress => Mathf.Clamp(CurrentTime / intervalDuration, 0, 1);

        public RepeatingTimer(float intervalDuration = 0, bool isManual = false, bool useUnscaledTime = false) : base(isManual, useUnscaledTime) 
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
