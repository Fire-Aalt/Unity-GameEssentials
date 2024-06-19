using UnityEngine;

namespace RenderDream.GameEssentials
{
    public class StopwatchTimer : Timer
    {
        public StopwatchTimer(bool isManual = false, bool useUnscaledTime = false) : base(isManual, useUnscaledTime) { }

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
