using UnityEngine;

namespace RenderDream.GameEssentials
{
    public class StopwatchTimer : Timer
    {
        public StopwatchTimer(bool useUnscaledTime = false) : base(0, useUnscaledTime) { }

        public override void Tick()
        {
            if (IsRunning)
            {
                CurrentTime += Time.deltaTime;
            }
        }

        public override bool IsFinished => false;
    }
}
