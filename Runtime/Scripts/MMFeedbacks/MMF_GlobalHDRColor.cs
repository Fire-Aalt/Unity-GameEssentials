using MoreMountains.Feedbacks;
using UnityEngine;

namespace RenderDream.GameEssentials
{
    [AddComponentMenu("")]
    // displays a help text in the inspector
    [FeedbackHelp("This feedback lets you change Global HDR color intensity")]
    // path to add the feedback in the Add Feedback dropdown
    [FeedbackPath("Game Essentials/Global HDR Color")]
    public class MMF_GlobalHDRColor : MMF_Feedback
    {
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
#endif

        [MMFInspectorGroup("Global HDR Color", true, 37, true)]

        [Header("Settings")]
        public HDRColorComponent.FinalIntensity FinalIntensity = HDRColorComponent.FinalIntensity.Zero;
        public float LerpDuration = 0.2f;

        public override float FeedbackDuration
        {
            get { return LerpDuration; }
            set { LerpDuration = value; }
        }

        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            if (!Active)
                return;

            HDRColorManager.ChangeColorIntensity(FinalIntensity, LerpDuration);
        }
    }
}
