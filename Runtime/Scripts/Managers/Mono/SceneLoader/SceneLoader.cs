using Cysharp.Threading.Tasks;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;

namespace RenderDream.GameEssentials
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        public static bool IsTransitioning { get; private set; }

        private ScenesDataSO _scenesData;

        protected override void Awake()
        {
            base.Awake();

            _scenesData = ScenesDataSO.Instance;
        }

        public async UniTaskVoid LoadSceneWithTransition(SceneType sceneType, bool reloadScenes = false)
        {
            // Start full transition
            IsTransitioning = true;
            EventBus<SaveGameEvent>.Raise(new SaveGameEvent());
            await SceneTransitionManager.Current.TransitionIn();

            await LoadScene(_scenesData.GetSceneDependencies(sceneType), reloadScenes);
        }

        public async UniTask LoadScene(SceneDependencies sceneDependencies, bool reloadScenes = false)
        {
            // Start shallow transition
            IsTransitioning = true;
            MMSoundManager.Current.FreeAllLoopingSounds();

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
                    if (reloadScenes || !sceneDependencies.IsSceneDependent(scene))
                    {
                        scenesToUnload.Add(scene);
                    }
                }
            }

            // Unload scenes
            for (int i = 0; i < scenesToUnload.Count; i++)
            {
                await SceneManager.UnloadSceneAsync(scenesToUnload[i]);
            }

            // Load dependent scenes
            var dependentScenes = sceneDependencies.DependentScenes;
            for (int i = 0; i < dependentScenes.Count; i++)
            {
                if (!dependentScenes[i].LoadedScene.IsValid())
                {
                    await SceneManager.LoadSceneAsync(dependentScenes[i].Path, LoadSceneMode.Additive);
                }
            }

            await InitializeScene(sceneDependencies);
        }

        private async UniTask InitializeScene(SceneDependencies sceneDependencies)
        {
            // Set active scene
            SceneManager.SetActiveScene(SceneManager.GetSceneByPath(sceneDependencies.mainScene.Path));
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
}