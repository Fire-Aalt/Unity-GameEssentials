using UnityEngine;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using System;

namespace RenderDream.GameEssentials
{
    public static class BootLoader
    {
        private static ScenesDataSO scenesData;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async UniTaskVoid OnAfterSceneLoad()
        {
            scenesData = ScenesDataSO.Instance;
            scenesData.IndexSceneGroups();

            if (scenesData == null)
            {
                Debug.LogError($"{typeof(ScenesDataSO)} was not found." +
                    $" Please, ensure you have a ScriptableObject.asset called 'ScenesData' in the root of any folder called 'Resources'" +
                    $" for the {typeof(BootLoader)} to work properly");
                return;
            }

            SceneGroup firstSceneGroup;
            SceneReference bootLoaderScene = scenesData.bootLoaderScene;
            
            if (bootLoaderScene.LoadedScene.IsValid())
            {
                firstSceneGroup = GetFirstSceneGroup();
                if (firstSceneGroup == null)
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
            await SceneLoader.Current.LoadSceneGroup(firstSceneGroup.Index, false, SceneTransition.TransitionOut);
        }

        private static SceneGroup GetFirstSceneGroup()
        {
            SceneGroup sceneGroup = null;
#if UNITY_EDITOR
            sceneGroup = EditorScenesSO.Instance.firstSceneGroup;
#endif
            sceneGroup ??= ScenesDataSO.Instance.sceneGroups[0];
            
            return sceneGroup;
        }
    }
}
