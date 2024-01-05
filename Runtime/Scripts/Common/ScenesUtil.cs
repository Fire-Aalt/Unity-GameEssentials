#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RenderDream.UnityManager
{
    public static class ScenesUtil
    {
        public static List<string> OpenedScenesInEditor { get; private set; }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        public static void InitializeEditor()
        {
            OpenedScenesInEditor = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                OpenedScenesInEditor.Add(SceneManager.GetSceneAt(i).name);
            }

            EditorSceneManager.sceneOpened += HandleSceneOpened;
            EditorSceneManager.sceneClosed += HandleSceneClosed;
        }

        static void HandleSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (mode == OpenSceneMode.Single)
            {
                OpenedScenesInEditor.Clear();
            }
            OpenedScenesInEditor.Add(scene.name);
        }

        static void HandleSceneClosed(Scene scene)
        {
            if (Application.isPlaying)
            {
                return;
            }

            OpenedScenesInEditor.Remove(scene.name);
        }
#endif
    }
}
