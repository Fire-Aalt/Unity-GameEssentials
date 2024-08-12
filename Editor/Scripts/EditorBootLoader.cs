using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Eflatun.SceneReference;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEditor;

namespace RenderDream.GameEssentials
{
    [InitializeOnLoad]
    public static class EditorBootLoader
    {
        static EditorBootLoader()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    SaveScenesDataBeforePlayMode();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    TearDownAfterPlayMode();
                    break;
            }
        }

        private static void SaveScenesDataBeforePlayMode()
        {
            ScenesDataSO scenesData = ScenesDataSO.Instance;
            SceneReference bootLoaderScene = scenesData.bootLoaderScene;

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            // Get SceneGroup
            var openedScenes = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                openedScenes.Add(SceneManager.GetSceneAt(i).path);
            }
            int firstSceneGroupIndex = GetFirstSceneGroupIndex(scenesData.sceneGroups, openedScenes);

            EditorScenesDataWrapper.SetOpenedScenes(openedScenes.ToArray());
            EditorScenesDataWrapper.SetFirstSceneGroupIndex(firstSceneGroupIndex);

            // Force return if unknown SceneGroup
            if (!bootLoaderScene.LoadedScene.IsValid() && firstSceneGroupIndex == -1)
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
            if (firstSceneGroupIndex != -1)
            {
                SceneReference mainScene = scenesData.sceneGroups[firstSceneGroupIndex].MainScene.Reference;
                EditorSceneManager.MoveSceneAfter(mainScene.LoadedScene, topScene);
            }
        }

        private static void TearDownAfterPlayMode()
        {
            SceneReference bootLoaderScene = ScenesDataSO.Instance.bootLoaderScene;

            // Close _BootLoader scene if it was not opened
            var openedScenes = EditorScenesDataWrapper.GetOpenedScenes();
            if (string.IsNullOrEmpty(openedScenes.FirstOrDefault(p => p == bootLoaderScene.Path)))
            {
                EditorSceneManager.CloseScene(bootLoaderScene.LoadedScene, true);
            }
        }

        private static int GetFirstSceneGroupIndex(SceneGroup[] sceneGroups, List<string> openedScenes)
        {
            for (int i = 0; i < openedScenes.Count; i++)
            {
                int sceneGroupIndex = GetSceneGroupIndex(sceneGroups, openedScenes[i]);
                if (sceneGroupIndex != -1)
                {
                    return sceneGroupIndex;
                }
            }
            return -1;
        }

        private static int GetSceneGroupIndex(SceneGroup[] sceneGroups, string scenePath)
        {
            for (int i = 0; i < sceneGroups.Length; i++)
            {
                if (scenePath == sceneGroups[i].MainScene.Reference.Path)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
