using MoreMountains.Feedbacks;
using UnityEngine;

namespace RenderDream.GameEssentials
{
    [AddComponentMenu("")]
    // displays a help text in the inspector
    [FeedbackHelp("This feedback lets you change HDR color intensity")]
    // path to add the feedback in the Add Feedback dropdown
    [FeedbackPath("Game Essentials/HDR Color")]
    public class MMF_HDRColor : MMF_Feedback
    {
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
        public override bool EvaluateRequiresSetup() { return (Target == null); }
#endif

        [MMFInspectorGroup("HDR Color", true, 37, true)]
        [Header("HDR Color Controller")]
        public HDRColorComponent Target;

        [Header("Settings")]
        public TimescaleModes TimescaleMode;
        public HDRColorComponent.FinalIntensity FinalIntensity = HDRColorComponent.FinalIntensity.Zero;
        public float LerpDuration = 0.2f;

        public override float FeedbackDuration
        {
            get {
                if (TimescaleMode == TimescaleModes.Unscaled)
                    return LerpDuration;
                else
                    return ApplyTimeMultiplier(LerpDuration);
            }
            set { LerpDuration = value; }
        }

        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            if (!Active || Target == null)
                return;

            Target.LerpIntensityValue(TimescaleMode, FinalIntensity, LerpDuration).Forget();
        }
    }
}
