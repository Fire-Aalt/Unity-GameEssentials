using Eflatun.SceneReference;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RenderDream.GameEssentials
{
    [CreateAssetMenu(menuName = "Data/Scenes Data")]
    public class ScenesDataSO : ScriptableObject
    {
        private static readonly string _scenesDataPath = "ScenesData";
        public static ScenesDataSO Instance
        {
            get
            {
                if (_scenesData == null)
                {
                    _scenesData = Resources.Load<ScenesDataSO>(_scenesDataPath);
                }
                return _scenesData;
            }
        }
        private static ScenesDataSO _scenesData;

        [Title("_BootLoader")]
        public SceneReference bootLoaderScene;
        [Title("Scenes Data")]
        public SceneDependencies[] sceneDependencies;

        public SceneDependencies GetSceneDependencies(SceneType sceneType)
        {
            return sceneDependencies.FirstOrDefault(s => s.sceneType == sceneType);
        }
    }

    public enum SceneType
    {
        MainMenu,
        Gameplay
    }

    [Serializable]
    public class SceneDependencies
    {
        [Header("Type")]
        public SceneType sceneType;

        [Header("Scene References")]
        public SceneReference mainScene;
        public List<SceneReference> additiveScenes;

        public List<SceneReference> DependentScenes
        {
            get
            {
                List<SceneReference> mergedList = new()
                {
                    mainScene
                };
                mergedList.AddRange(additiveScenes);
                return mergedList;
            }
        }

        public bool IsSceneDependent(Scene scene)
        {
            if (mainScene.Path == scene.path)
            {
                return true;
            }
            foreach (var additiveScene in additiveScenes)
            {
                if (additiveScene.Path == scene.path)
                {
                    return true;
                }
            }
            return false;
        }
    }
}