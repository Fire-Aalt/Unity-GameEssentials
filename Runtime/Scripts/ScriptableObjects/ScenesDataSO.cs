using Eflatun.SceneReference;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RenderDream.UnityManager
{
    [CreateAssetMenu(menuName = "Data/Scenes Data")]
    public class ScenesDataSO : ScriptableObject
    {
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
    }
}