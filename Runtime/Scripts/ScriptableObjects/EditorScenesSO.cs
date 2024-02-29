using System.Collections.Generic;
using UnityEngine;

namespace RenderDream.GameEssentials
{
    public class EditorScenesSO : ScriptableObject
    {
        private static readonly string _editorScenesDataPath = "EditorScenesData";
        public static EditorScenesSO Instance
        {
            get
            {
                if (_editorScenesData == null)
                {
                    _editorScenesData = Resources.Load<EditorScenesSO>(_editorScenesDataPath);
                }
                return _editorScenesData;
            }
        }
        private static EditorScenesSO _editorScenesData;

        public List<string> openedScenes;
        public SceneDependencies firstSceneDependencies;

    }
}
