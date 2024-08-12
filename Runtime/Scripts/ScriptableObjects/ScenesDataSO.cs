using Eflatun.SceneReference;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RenderDream.GameEssentials
{
    [CreateAssetMenu(menuName = "Data/Scenes Data")]
    public class ScenesDataSO : ScriptableObject
    {
        private const string SCENES_DATA_PATH = "ScenesData";
        public static ScenesDataSO Instance
        {
            get
            {
                if (_scenesData == null)
                {
                    _scenesData = Resources.Load<ScenesDataSO>(SCENES_DATA_PATH);
                }
                return _scenesData;
            }
        }
        private static ScenesDataSO _scenesData;

        [Title("_BootLoader")]
        public SceneReference bootLoaderScene;
        [Title("Scenes")]
        [OnValueChanged("IndexSceneGroups")] public SceneGroup[] sceneGroups;
    }
}