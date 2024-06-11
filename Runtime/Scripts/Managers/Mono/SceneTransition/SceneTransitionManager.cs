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
        [SerializeField] protected CanvasGroup loadingCanvasGroup;
        [SerializeField] protected Camera loadingCamera;

        [Title("MMF Players")]
        [SerializeField] protected MMF_Player transitionInPlayer;
        [SerializeField] protected MMF_Player transitionOutPlayer;

        [Title("Transition Params")]
        [SerializeField] protected float extraWaitTime = 0.5f;
        [SerializeField] protected float transitionDuration = 1f;

        [Title("Loading Bar")]
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
            transitionInPlayer.PlayFeedbacks();

            await LerpAlpha(transitionDuration, 1f);
        }

        public virtual async UniTask TransitionOut()
        {
            transitionOutPlayer.PlayFeedbacks();

            await UniTask.WaitForSeconds(extraWaitTime, ignoreTimeScale: true);
            await LerpAlpha(transitionDuration, 0f);
        }

        protected virtual async UniTask LerpAlpha(float duration, float endAlpha)
        {
            float startAlpha = loadingCanvasGroup.alpha;

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                loadingCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                await UniTask.Yield();
            }

            loadingCanvasGroup.alpha = endAlpha;
        }
    }
}
