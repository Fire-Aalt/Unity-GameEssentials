﻿using UnityEngine;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;

namespace RenderDream.GameEssentials
{
    public static class BootLoader
    {
        private static ScenesDataSO scenesData;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async UniTaskVoid OnAfterSceneLoad()
        {
            scenesData = ScenesDataSO.Instance;

            if (scenesData == null)
            {
                Debug.LogError($"{typeof(ScenesDataSO)} was not found." +
                    $" Please, ensure you have a ScriptableObject.asset called 'ScenesData' in the root of any folder called 'Resources'" +
                    $" for the {typeof(BootLoader)} to work properly");
                return;
            }

            int firstSceneGroupIndex;
            SceneReference bootLoaderScene = scenesData.bootLoaderScene;
            
            if (bootLoaderScene.LoadedScene.IsValid())
            {
                firstSceneGroupIndex = GetFirstSceneGroupIndex(scenesData.sceneGroups);
                if (firstSceneGroupIndex == -1)
                {
                    Debug.LogError($"No MainMenu Scene was found in {scenesData}." +
                        $" Please, ensure you have atleast one SceneDependency with type of {SceneType.MainMenu}");
                    return;
                }
            }
            else
            {
                Debug.LogWarning("Unknown dependencies found. BootLoader is inactive");
                return;
            }

            await UniTask.WaitUntil(() => SceneLoader.Current != null);
            await SceneLoader.Current.LoadSceneGroup(firstSceneGroupIndex, false, SceneTransition.TransitionOut);
        }

        private static int GetFirstSceneGroupIndex(SceneGroup[] sceneGroups)
        {
            int firstSceneGroupIndex = -1;

#if UNITY_EDITOR
            firstSceneGroupIndex = EditorScenesDataWrapper.GetFirstSceneGroupIndex();
#endif
            if (firstSceneGroupIndex == -1 && sceneGroups.Length > 0)
            {
                firstSceneGroupIndex = 0;
            }

            return firstSceneGroupIndex;
        }
    }
}
