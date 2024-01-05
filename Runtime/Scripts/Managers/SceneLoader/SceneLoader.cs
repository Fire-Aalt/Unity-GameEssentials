using Cysharp.Threading.Tasks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RenderDream.GameEssentials
{
    public class SceneLoader : MMSingleton<SceneLoader>
    {
        [field: SerializeField, InlineEditor]
        public ScenesDataSO ScenesData { get; private set; }

        public static bool IsTransitioning { get; private set; }

        public async UniTask LoadSceneWithTransition(SceneType sceneType)
        {
            // Start full transition
            IsTransitioning = true;
            DataPersistenceManager.Current.SaveGame();
            await SceneTransitionManager.Current.TransitionIn();

            await LoadScene(ScenesData.GetSceneDependencies(sceneType));
        }

        public async UniTask LoadScene(SceneDependencies sceneDependencies)
        {
            // Start shallow transition
            IsTransitioning = true;
            MMSoundManager.Current.FreeAllLoopingSounds();
            //GameManager.Current.UpdateGameState(GameState.Transition);

            Scene bootLoader = ScenesData.bootLoaderScene.LoadedScene;
            SceneManager.SetActiveScene(bootLoader);
            SceneTransitionManager.Current.ChangeTransitionCameraState(isActive: true);

            // Unload current scenes except for _BootLoader
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene != bootLoader)
                {
                    AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(scene);
                    await UniTask.WaitUntil(() => asyncOperation.isDone);
                }
            }

            // Load main scene
            AsyncOperation mainSceneAsyncOperation = SceneManager.LoadSceneAsync(sceneDependencies.mainScene.Name, LoadSceneMode.Additive);
            await UniTask.WaitUntil(() => mainSceneAsyncOperation.isDone);

            // Load additive scenes
            foreach (var additiveScene in sceneDependencies.additiveScenes)
            {
                AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(additiveScene.Name, LoadSceneMode.Additive);
                await UniTask.WaitUntil(() => asyncOperation.isDone);
            }

            await InitializeScene(sceneDependencies);
        }

        private async UniTask InitializeScene(SceneDependencies sceneDependencies)
        {
            // Set active scene
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneDependencies.mainScene.Name));
            await UniTask.WaitForEndOfFrame(this);

            // Notify everyone
            EventBus<ScenesLoadedEvent>.Raise(new ScenesLoadedEvent {
                sceneDependencies = sceneDependencies
            });
            IsTransitioning = false;

            // End transition
            SceneTransitionManager.Current.ChangeTransitionCameraState(isActive: false);
            await SceneTransitionManager.Current.TransitionOut();
        }
    }

    public struct ScenesLoadedEvent : IEvent
    {
        public SceneDependencies sceneDependencies;
    }
}