using System;
using System.Collections.Generic;
using System.Linq;
using Eflatun.SceneReference;
using Game;
using Sirenix.OdinInspector;
using Unity.Entities.Content;
using Unity.Scenes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Hash128 = Unity.Entities.Hash128;
using SceneReference = Eflatun.SceneReference.SceneReference;

namespace RenderDream.GameEssentials
{
    [Serializable]
    public class SubSceneReference
    {
        [SerializeField, ReadOnly] private SceneReference _reference;
        [SerializeField] private SubSceneLoadMode _loadMode;
        
        [SerializeField, HideInInspector] private Hash128 _hash;
        
        public SubSceneReference(SubScene subScene)
        {
            _reference = new SceneReference(subScene.SceneAsset);
            AddressableUtils.CreateAssetEntry(subScene.SceneAsset, "SubSceneReferences");
            _hash = AssetDatabase.GUIDFromAssetPath(_reference.Path);
        }

        public SubSceneLoadMode LoadMode => _loadMode;
        public Hash128 RuntimeHash => _hash;
        public SceneReference SceneReference => _reference;
        public bool IsSubSceneLoaded => _reference.LoadedScene.isLoaded;
    }

    public enum SubSceneLoadMode
    {
        BeforeSceneGroup,
        AfterSceneGroup
    }
    
    [Serializable]
    public class SceneGroup
    {
        public string GroupName = "New Scene Group";
        [OnValueChanged("SetDirty")] public SceneData MainScene;
        [OnValueChanged("SetDirty")] public List<SceneData> AdditiveScenes;
        
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
        
        public List<SubSceneReference> SubScenes
        {
            get
            {
                if (_isDirty)
                {
                    _subScenes = new List<SubSceneReference>();
                    foreach (var sceneData in Scenes)
                    {
                        _subScenes.AddRange(sceneData.subScenes);
                    }
                }
                return _subScenes;
            }
        }
        private List<SubSceneReference> _subScenes;
        
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

        public SceneData GetSceneData(Scene scene) 
        {
            return Scenes.FirstOrDefault(s => s.Reference.Path == scene.path);
        }

        public void Validate()
        {
            ValidateSubScenes();
        }
        
        [Button]
        private void ValidateSubScenes()
        {
            _isDirty = true;
            foreach (var sceneData in Scenes)
            {
                if (sceneData.Reference.State == SceneReferenceState.Unsafe)
                {
                    sceneData.subScenes.Clear();
                    continue;
                }

                Scene scene;
                var mustClose = false;
                if (sceneData.Reference.LoadedScene.isLoaded)
                {
                    scene = sceneData.Reference.LoadedScene;
                }
                else
                {
                    scene = EditorSceneManager.OpenScene(sceneData.Reference.Path, OpenSceneMode.Additive);
                    mustClose = true;
                }

                var added = new List<SubSceneReference>();
                foreach (var subScene in EditorSubSceneUtils.GetSubScenes(scene))
                {
                    var reference =
                        sceneData.subScenes.FirstOrDefault(r => r.SceneReference.Path == subScene.EditableScenePath);
                    if (reference == null)
                    {
                        reference = new SubSceneReference(subScene);
                        sceneData.subScenes.Add(reference);
                    }
                    added.Add(reference);
                }
                
                sceneData.subScenes.RemoveAll((r) => !added.Contains(r));

                if (mustClose)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private void SetDirty()
        {
            _isDirty = true;
            ValidateSubScenes();
        }
    }

    [Serializable]
    public class SceneData
    {
        public SceneReference Reference;
        public SceneType SceneType;
        
        public string Name => Reference.Name;
        public List<SubSceneReference> subScenes;
    }
    
    public enum SceneType { MainMenu, UserInterface, HUD, Cinematic, Environment, Tooling }
}