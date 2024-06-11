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
        [SerializeField] protected float initializeSceneGroupDuration = 1f;
        [SerializeField] protected float transitionDuration = 0.5f;

        [Title("Loading Bar")]
        [SerializeField] protected bool loadingBar;
        [SerializeField, ShowIf("loadingBar")] protected Image loadingBarImage;
        [SerializeField, ShowIf("loadingBar")] protected float fillSpeed = 5f;

        protected float targetProgress;

        protected virtual void Awake()
        {
            SetCanvasGroupActive(true);
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

            await LerpCanvasGroupAlpha(transitionDuration, true);
        }

        public virtual async UniTask TransitionOut()
        {
            transitionOutPlayer.PlayFeedbacks();

            await UniTask.WaitForSeconds(initializeSceneGroupDuration, ignoreTimeScale: true);
            await LerpCanvasGroupAlpha(transitionDuration, false);
        }

        protected virtual async UniTask LerpCanvasGroupAlpha(float duration, bool setActive)
        {
            float startAlpha = setActive ? 0f : 1f;
            float endAlpha = setActive ? 1f : 0f;

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                loadingCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                await UniTask.Yield();
            }

            SetCanvasGroupActive(setActive);
        }

        private void SetCanvasGroupActive(bool isActive)
        {
            loadingCanvasGroup.alpha = isActive ? 1f : 0f;
            loadingCanvasGroup.blocksRaycasts = isActive;
        }

    }
}
