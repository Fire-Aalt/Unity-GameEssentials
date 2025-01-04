using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using SceneReference = Eflatun.SceneReference.SceneReference;

namespace KrasCore.Essentials
{
    public class SceneGroupManager
    {
        public event Action<SceneData> OnScenePersisted = delegate { };
        public event Action<string> OnSceneLoaded = delegate { };
        public event Action<string> OnSceneUnloaded = delegate { };
        public event Action OnSceneGroupLoaded = delegate { };

        private readonly AsyncOperationHandleGroup _handleGroup = new(10);
        
        public SceneGroup ActiveSceneGroup { get; private set; }

        public async UniTask LoadScenes(SceneGroup group, IProgress<float> progress, bool reloadDupScenes, int loadDelay, CancellationToken token)
        {
            ActiveSceneGroup = group;
            var loadedScenes = new List<string>();

            // Set _BootLoader as active scene to unload everything else
            SceneReference bootLoader = ScenesDataSO.Instance.bootLoaderScene;
            SceneManager.SetActiveScene(bootLoader.LoadedScene);

            await UnloadScenes(reloadDupScenes, token);

            int sceneCount = SceneManager.sceneCount;
            for (var i = 0; i < sceneCount; i++)
            {
                loadedScenes.Add(SceneManager.GetSceneAt(i).name);
            }

#if UNITY_ENTITIES
            // Load SubScenes before any other scenes
            await LoadEntityScenes(group, SubSceneLoadMode.BeforeSceneGroup, token);
#endif

            var totalScenesToLoad = ActiveSceneGroup.Scenes.Count;
            var operationGroup = new AsyncOperationGroup(totalScenesToLoad);

            for (var i = 0; i < totalScenesToLoad; i++)
            {
                var sceneData = group.Scenes[i];
                if (reloadDupScenes == false && loadedScenes.Contains(sceneData.Name)) continue;
                
                if (sceneData.Reference.State == SceneReferenceState.Regular)
                {
                    var operation = SceneManager.LoadSceneAsync(sceneData.Reference.Path, LoadSceneMode.Additive);
                    operationGroup.Operations.Add(operation);

                    operation.completed += _ => HandleSceneLoaded(sceneData);
                }
                else if (sceneData.Reference.State == SceneReferenceState.Addressable)
                {
                    var sceneHandle = Addressables.LoadSceneAsync(sceneData.Reference.Path, LoadSceneMode.Additive);
                    _handleGroup.Handles.Add(sceneHandle);

                    sceneHandle.Completed += _ => HandleSceneLoaded(sceneData);
                }
            }
            
            if (loadDelay > 0)
            {
                await UniTask.Delay(loadDelay, cancellationToken: token);
            }

            // Wait until all AsyncOperations in the group are done
            while (!operationGroup.IsDone || !_handleGroup.IsDone)
            {
                progress?.Report((operationGroup.Progress + _handleGroup.Progress) / 2);
                await UniTask.Delay(1, true, cancellationToken: token);
            }

#if UNITY_ENTITIES
            await LoadEntityScenes(group, SubSceneLoadMode.AfterSceneGroup, token);
#endif

            OnSceneGroupLoaded.Invoke();
        }

#if UNITY_ENTITIES
        private async UniTask LoadEntityScenes(SceneGroup group, SubSceneLoadMode loadMode, CancellationToken token)
        {
            var subSceneLoaderSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SubSceneLoaderSystem>();
            foreach (var subSceneReference in group.SubScenes)
            {
                if (subSceneReference.LoadMode == loadMode)
                {
                    subSceneLoaderSystem.TryAddLoadRequest(subSceneReference.RuntimeHash);
                }
            }
            
            while (!subSceneLoaderSystem.AreAllRequestedSubScenesLoaded())
            {
                await UniTask.Delay(1, true, cancellationToken: token);
            }
        }
#endif

        private void HandleSceneLoaded(SceneData sceneData)
        {
            if (sceneData.Reference.Path == ActiveSceneGroup.MainScene.Reference.Path)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(ActiveSceneGroup.MainScene.Name));
            }
            OnSceneLoaded.Invoke(sceneData.Reference.Path);
        }

        private async UniTask UnloadScenes(bool unloadDupScenes, CancellationToken token)
        {
            var scenesToUnload = new List<string>();
            int sceneCount = SceneManager.sceneCount;

            for (var i = sceneCount - 1; i > 0; i--)
            {
                var sceneAt = SceneManager.GetSceneAt(i);
                if (!sceneAt.isLoaded) continue;

                var sceneName = sceneAt.path;
                if (ActiveSceneGroup.IsSceneInGroup(sceneAt) && !unloadDupScenes)
                {
                    var sceneData = ActiveSceneGroup.GetSceneData(sceneAt);
                    HandleSceneLoaded(sceneData);
                    OnScenePersisted.Invoke(sceneData);
                    continue;
                }
                
                if (sceneAt.isSubScene) continue;
                
                if (_handleGroup.Handles.Any(h => h.IsValid() && h.Result.Scene.path == sceneName)) continue;

                scenesToUnload.Add(sceneName);
            }

            // Create an AsyncOperationGroup
            var operationGroup = new AsyncOperationGroup(scenesToUnload.Count);

            foreach (var scene in scenesToUnload)
            {
                var operation = SceneManager.UnloadSceneAsync(scene);
                if (operation == null) continue;

                operationGroup.Operations.Add(operation);

                OnSceneUnloaded.Invoke(scene);
            }

            foreach (var handle in _handleGroup.Handles)
            {
                if (handle.IsValid())
                {
                    _ = Addressables.UnloadSceneAsync(handle);
                }
            }
            _handleGroup.Handles.Clear();

            // Wait until all AsyncOperations in the group are done
            while (!operationGroup.IsDone)
            {
                await UniTask.Delay(1, cancellationToken: token);
            }

            // Optional: UnloadUnusedAssets - unloads all unused assets from memory
            await Resources.UnloadUnusedAssets();
        }
    }

    public readonly struct AsyncOperationGroup
    {
        public readonly List<AsyncOperation> Operations;

        public float Progress => Operations.Count == 0 ? 0 : Operations.Average(o => o.progress);
        public bool IsDone => Operations.All(o => o.isDone);

        public AsyncOperationGroup(int initialCapacity)
        {
            Operations = new List<AsyncOperation>(initialCapacity);
        }
    }

    public readonly struct AsyncOperationHandleGroup
    {
        public readonly List<AsyncOperationHandle<SceneInstance>> Handles;

        public float Progress => Handles.Count == 0 ? 0 : Handles.Average(h => h.PercentComplete);
        public bool IsDone => Handles.Count == 0 || Handles.All(o => o.IsDone);

        public AsyncOperationHandleGroup(int initialCapacity)
        {
            Handles = new List<AsyncOperationHandle<SceneInstance>>(initialCapacity);
        }
    }
}
