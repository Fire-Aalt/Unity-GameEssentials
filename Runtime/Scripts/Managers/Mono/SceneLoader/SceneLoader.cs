using Cysharp.Threading.Tasks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace RenderDream.GameEssentials
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        public static bool IsTransitioning { get; private set; }

        private ScenesDataSO _scenesData;

        protected override void Awake()
        {
            base.Awake();

            _scenesData = BootLoader.ScenesData;
        }

        public async UniTask LoadSceneWithTransition(SceneType sceneType)
        {
            // Start full transition
            IsTransitioning = true;
            EventBus<SaveGameEvent>.Raise(new SaveGameEvent());
            await SceneTransitionManager.Current.TransitionIn();

            await LoadScene(_scenesData.GetSceneDependencies(sceneType));
        }

        public async UniTask LoadScene(SceneDependencies sceneDependencies)
        {
            // Start shallow transition
            IsTransitioning = true;
            MMSoundManager.Current.FreeAllLoopingSounds();
            //GameManager.Current.UpdateGameState(GameState.Transition);

            Scene bootLoader = _scenesData.bootLoaderScene.LoadedScene;
            SceneManager.SetActiveScene(bootLoader);
            SceneTransitionManager.Current.ChangeTransitionCameraState(isActive: true);

            // Find scenes to Unload (except for _BootLoader)
            List<Scene> scenesToUnload = new();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene != bootLoader)
                {
                    scenesToUnload.Add(scene);
                }
            }

            // Unload scenes
            for (int i = 0; i < scenesToUnload.Count; i++)
            {
                AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(scenesToUnload[i]);
                await UniTask.WaitUntil(() => asyncOperation.isDone);
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
            EventBus<LoadGameEvent>.Raise(new LoadGameEvent());
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

    public struct SaveGameEvent : IEvent { }
    public struct LoadGameEvent : IEvent { }
}