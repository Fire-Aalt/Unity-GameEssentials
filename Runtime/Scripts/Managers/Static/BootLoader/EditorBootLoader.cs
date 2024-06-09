using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Eflatun.SceneReference;
using UnityEngine;
using System.Linq;

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
                    SaveScenesDataBeforePlayMode(bootLoaderScene);
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    TearDownAfterPlayMode(bootLoaderScene);
                    break;
            }
        }

        private static void SaveScenesDataBeforePlayMode(SceneReference bootLoaderScene)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            // Get SceneDependencies
            _editorScenesData.openedScenes = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                _editorScenesData.openedScenes.Add(SceneManager.GetSceneAt(i).path);
            }
            _editorScenesData.firstSceneDependencies = GetFirstSceneDependencies();

            // Force return if unknown SceneDependencies
            if (!bootLoaderScene.LoadedScene.IsValid() && _editorScenesData.firstSceneDependencies == null)
            {
                return;
            }

            // Open and move _BootLoader scene as first
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

            // Move Main scene as second
            if (_editorScenesData.firstSceneDependencies != null)
            {
                SceneReference mainScene = _editorScenesData.firstSceneDependencies.mainScene;
                EditorSceneManager.MoveSceneAfter(mainScene.LoadedScene, topScene);
            }
        }

        private static void TearDownAfterPlayMode(SceneReference bootLoaderScene)
        {
            // Close _BootLoader scene if it was not opened
            if (string.IsNullOrEmpty(_editorScenesData.openedScenes.FirstOrDefault(p => p == bootLoaderScene.Path)))
            {
                EditorSceneManager.CloseScene(bootLoaderScene.LoadedScene, true);
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
