using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace RenderDream.GameEssentials
{
    public static class BootLoader
    {
        private const string SCENES_DATA_PATH = "ScenesData";
        public static ScenesDataSO ScenesData { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static async UniTask OnBeforeSceneLoad()
        {
            ScenesData = Resources.Load<ScenesDataSO>(SCENES_DATA_PATH);
            if (ScenesData == null) 
            {
                Debug.LogError($"{typeof(ScenesDataSO)} was not found." +
                    $" Please, ensure you have a ScriptableObject.asset called 'ScenesData' in the root of any folder called 'Resources'" +
                    $" for the {typeof(BootLoader)} to work properly");
                return;
            }

            SceneManager.LoadScene(ScenesData.bootLoaderScene.Path, LoadSceneMode.Single);
            EventBus<LoadGameEvent>.Raise(new LoadGameEvent());

            await UniTask.WaitUntil(() => SceneLoader.Current != null);

            SceneDependencies firstSceneDependencies = GetFirstSceneDependencies();
            if (firstSceneDependencies == null)
            {
                Debug.LogError($"No MainMenu Scene was found in {ScenesData}." +
                    $" Please, ensure you have atleast one SceneDependency with type of {SceneType.MainMenu}");
                return;
            }
            await SceneLoader.Current.LoadScene(firstSceneDependencies);
        }

        private static SceneDependencies GetFirstSceneDependencies()
        {
            SceneDependencies sceneDependencies = null;
            List<string> openedScenesInEditor = ScenesUtil.OpenedScenesInEditor;

            if (openedScenesInEditor != null)
            {
                for (int i = 0; i < openedScenesInEditor.Count; i++)
                {
                    sceneDependencies = HasDependencies(openedScenesInEditor[i]);
                    if (sceneDependencies != null)
                    {
                        break;
                    }
                }
            }
            sceneDependencies ??= ScenesData.GetSceneDependencies(SceneType.MainMenu);
            return sceneDependencies;
        }

        public static SceneDependencies HasDependencies(string sceneName)
        {
            var sceneDependenciesArray = ScenesData.sceneDependencies;
            for (int i = 0; i < sceneDependenciesArray.Length; i++)
            {
                if (sceneName == sceneDependenciesArray[i].mainScene.Name)
                {
                    return sceneDependenciesArray[i];
                }
            }
            return null;
        }
    }
}
