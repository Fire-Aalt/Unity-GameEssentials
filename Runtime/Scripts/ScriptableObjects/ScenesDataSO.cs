using Eflatun.SceneReference;
using Sirenix.OdinInspector;
using UnityEngine;

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
        [Title("Scenes")]
        [OnValueChanged("IndexSceneGroups")] public SceneGroup[] sceneGroups;

        public void IndexSceneGroups()
        {
            for (int i = 0; i < sceneGroups.Length; i++)
            {
                sceneGroups[i].Index = i;
            }
        }
    }
}