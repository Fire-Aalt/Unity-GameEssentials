using UnityEngine;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;

namespace RenderDream.GameEssentials
{
    public static class BootLoader
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async UniTaskVoid OnAfterSceneLoad()
        {
            if (ScenesDataSO.Instance == null)
            {
                Debug.LogError($"{typeof(ScenesDataSO)} was not found." +
                    $" Please, ensure you have a ScriptableObject.asset called 'ScenesData' in the root of any folder called 'Resources'" +
                    $" for the {typeof(BootLoader)} to work properly");
                return;
            }

            SceneDependencies firstSceneDependencies;
            SceneReference bootLoaderScene = ScenesDataSO.Instance.bootLoaderScene;
            
            if (bootLoaderScene.LoadedScene.IsValid())
            {
                firstSceneDependencies = GetFirstSceneDependencies();
                if (firstSceneDependencies == null)
                {
                    Debug.LogError($"No MainMenu Scene was found in {ScenesDataSO.Instance}." +
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
            await SceneLoader.Current.LoadScene(firstSceneDependencies, reloadScenes: false);
        }

        private static SceneDependencies GetFirstSceneDependencies()
        {
            SceneDependencies sceneDependencies = null;
#if UNITY_EDITOR
            sceneDependencies = EditorScenesSO.Instance.firstSceneDependencies;
#endif
            sceneDependencies ??= ScenesDataSO.Instance.GetSceneDependencies(SceneType.MainMenu);
            
            return sceneDependencies;
        }
    }
}
