using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Eflatun.SceneReference;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

namespace RenderDream.GameEssentials
{
    public static class EditorBootLoader
    {
        private static ScenesDataSO _scenesData;
        private static EditorScenesSO _editorScenesData;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void InitializeEditor()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            _scenesData = ScenesDataSO.Instance;
            _editorScenesData = EditorScenesSO.Instance;

            SceneReference bootLoaderScene = _scenesData.bootLoaderScene;
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

                    _editorScenesData.openedScenes = new List<string>();
                    for (int i = 0; i < SceneManager.sceneCount; i++)
                    {
                        _editorScenesData.openedScenes.Add(SceneManager.GetSceneAt(i).path);
                    }
                    _editorScenesData.firstSceneDependencies = GetFirstSceneDependencies();

                    Scene topScene = SceneManager.GetSceneAt(0);
                    if (!bootLoaderScene.LoadedScene.IsValid())
                    {
                        EditorSceneManager.OpenScene(bootLoaderScene.Path, OpenSceneMode.Additive);
                        EditorSceneManager.MoveSceneBefore(bootLoaderScene.LoadedScene, topScene);
                    }
                    else if (topScene != bootLoaderScene.LoadedScene)
                    {
                        EditorSceneManager.MoveSceneBefore(bootLoaderScene.LoadedScene, topScene);
                    }

                    SceneReference mainScene = _editorScenesData.firstSceneDependencies.mainScene;
                    if (_editorScenesData.firstSceneDependencies != null)
                    {
                        EditorSceneManager.MoveSceneAfter(mainScene.LoadedScene, topScene);
                    }
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    bool wasBootLoaderOpened = false;
                    foreach (string openedScene in _editorScenesData.openedScenes)
                    {
                        if (openedScene == bootLoaderScene.Path)
                        {
                            wasBootLoaderOpened = true;
                        }
                    }
                    if (!wasBootLoaderOpened)
                    {
                        EditorSceneManager.CloseScene(bootLoaderScene.LoadedScene, true);
                    }
                    break;
            }
        }

        private static SceneDependencies GetFirstSceneDependencies()
        {
            SceneDependencies sceneDependencies = null;
            List<string> openedScenesInEditor = _editorScenesData.openedScenes;

            for (int i = 0; i < openedScenesInEditor.Count; i++)
            {
                sceneDependencies = HasDependencies(openedScenesInEditor[i]);
                if (sceneDependencies != null)
                {
                    break;
                }
            }
            return sceneDependencies;
        }

        public static SceneDependencies HasDependencies(string scenePath)
        {
            var sceneDependenciesArray = _scenesData.sceneDependencies;
            for (int i = 0; i < sceneDependenciesArray.Length; i++)
            {
                if (scenePath == sceneDependenciesArray[i].mainScene.Path)
                {
                    return sceneDependenciesArray[i];
                }
            }
            return null;
        }
#endif
    }
}
