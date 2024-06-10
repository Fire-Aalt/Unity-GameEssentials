using System;
using System.Collections.Generic;
using Eflatun.SceneReference;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

namespace RenderDream.GameEssentials
{
    [Serializable]
    public class SceneGroup
    {
        public string GroupName = "New Scene Group";
        [OnValueChanged("SetDirty")] public SceneData MainScene;
        [OnValueChanged("SetDirty")] public List<SceneData> AdditiveScenes;

        public int Index { get; set; }
        private bool _isDirty = true;

        public List<SceneData> Scenes
        {
            get
            {
                if (_isDirty)
                {
                    _scenes = new()
                    {
                        MainScene
                    };
                    _scenes.AddRange(AdditiveScenes);                    
                }
                return _scenes;
            }
        }
        private List<SceneData> _scenes;

        public bool IsSceneInGroup(Scene scene)
        {
            for (int i = 0; i < Scenes.Count; i++)
            {
                if (Scenes[i].Reference.Path == scene.path)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetDirty() => _isDirty = true;
    }

    [Serializable]
    public class SceneData
    {
        public SceneReference Reference;
        public string Name => Reference.Name;
        public SceneType SceneType;
    }

    public enum SceneType { MainMenu, UserInterface, HUD, Cinematic, Environment, Tooling }
}