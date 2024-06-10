using Cysharp.Threading.Tasks;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace RenderDream.GameEssentials
{
    public class SceneTransitionManager : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] protected Canvas loadingCanvas;
        [SerializeField] protected Camera loadingCamera;

        [Title("MMF Players")]
        [SerializeField] protected MMF_Player transitionInPlayer;
        [SerializeField] protected MMF_Player transitionOutPlayer;

        [Title("Params")]
        [SerializeField] protected bool loadingBar;
        [SerializeField, ShowIf("loadingBar")] protected Image loadingBarImage;
        [SerializeField, ShowIf("loadingBar")] protected float fillSpeed = 0.5f;

        protected float targetProgress;

        protected virtual void Awake()
        {
            transitionInPlayer.Initialization();
            transitionOutPlayer.Initialization();
        }

        protected virtual void Update()
        {
            if (loadingBar && SceneLoader.IsLoading)
            {
                float currentFillAmount = loadingBarImage.fillAmount;
                float progressDifference = Mathf.Abs(currentFillAmount - targetProgress);

                float dynamicFillSpeed = progressDifference * fillSpeed;

                loadingBarImage.fillAmount = Mathf.Lerp(currentFillAmount, targetProgress, Time.deltaTime * dynamicFillSpeed);
            }
        }

        public virtual LoadingProgress InitializeProgressBar()
        {
            if (loadingBar)
            {
                loadingBarImage.fillAmount = 0f;
            }
            targetProgress = 1f;

            LoadingProgress progress = new();
            progress.Progressed += target => targetProgress = Mathf.Max(target, targetProgress);

            return progress;
        }

        public virtual void EnableLoadingCamera(bool enable)
        {
            loadingCamera.depth = enable ? 100 : -100;
            loadingCamera.enabled = enable;
        }

        public virtual async UniTask TransitionIn()
        {
            loadingBarImage.fillAmount = 0f;
            transitionInPlayer.PlayFeedbacks();

            await UniTask.WaitForSeconds(transitionInPlayer.TotalDuration, ignoreTimeScale: true);
        }

        public virtual async UniTask TransitionOut()
        {
            transitionOutPlayer.PlayFeedbacks();

            await UniTask.WaitForSeconds(transitionOutPlayer.TotalDuration, ignoreTimeScale: true);
        }
    }
}
