using Cysharp.Threading.Tasks;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RenderDream.UnityManager
{
    public class SceneTransitionManager : MMSingleton<SceneTransitionManager>
    {
        [field: SerializeField, Title("References")]
        public Camera TransitionCamera { get; private set; }

        [field: SerializeField, Title("MMF Players")]
        public MMF_Player TransitionInPlayer { get; private set; }
        [field: SerializeField]
        public MMF_Player TransitionOutPlayer { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            TransitionInPlayer.Initialization();
            TransitionOutPlayer.Initialization();
        }

        public void ChangeTransitionCameraState(bool isActive)
        {
            if (isActive)
            {
                TransitionCamera.depth = 100;
                TransitionCamera.enabled = true;
            }
            else
            {
                TransitionCamera.depth = -100;
                TransitionCamera.enabled = false;
            }
        }

        public async UniTask TransitionIn()
        {
            TransitionInPlayer.PlayFeedbacks();

            await UniTask.WaitForSeconds(TransitionInPlayer.TotalDuration, ignoreTimeScale: true);
        }

        public async UniTask TransitionOut()
        {
            TransitionOutPlayer.PlayFeedbacks();

            await UniTask.WaitForSeconds(TransitionOutPlayer.TotalDuration, ignoreTimeScale: true);
        }
    }
}
