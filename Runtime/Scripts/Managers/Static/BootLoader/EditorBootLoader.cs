using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Eflatun.SceneReference;
using UnityEngine;
using System.Linq;
using System;


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

            // Get SceneGroup
            _editorScenesData.openedScenes = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                _editorScenesData.openedScenes.Add(SceneManager.GetSceneAt(i).path);
            }
            _editorScenesData.firstSceneGroup = GetFirstSceneGroup();
            _editorScenesData.firstSceneGroupIndex = Array.FindIndex(_scenesData.sceneGroups, g => g == _editorScenesData.firstSceneGroup); ;

            // Force return if unknown SceneGroup
            if (!bootLoaderScene.LoadedScene.IsValid() && _editorScenesData.firstSceneGroup == null)
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
            if (_editorScenesData.firstSceneGroup != null)
            {
                SceneReference mainScene = _editorScenesData.firstSceneGroup.MainScene.Reference;
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

        private static SceneGroup GetFirstSceneGroup()
        {
            SceneGroup sceneGroup = null;
            List<string> openedScenesInEditor = _editorScenesData.openedScenes;

            for (int i = 0; i < openedScenesInEditor.Count; i++)
            {
                sceneGroup = HasSceneGroup(openedScenesInEditor[i]);
                if (sceneGroup != null)
                {
                    break;
                }
            }
            return sceneGroup;
        }

        public static SceneGroup HasSceneGroup(string scenePath)
        {
            var sceneDependenciesArray = _scenesData.sceneGroups;
            for (int i = 0; i < sceneDependenciesArray.Length; i++)
            {
                if (scenePath == sceneDependenciesArray[i].MainScene.Reference.Path)
                {
                    return sceneDependenciesArray[i];
                }
            }
            return null;
        }
#endif
    }
}
