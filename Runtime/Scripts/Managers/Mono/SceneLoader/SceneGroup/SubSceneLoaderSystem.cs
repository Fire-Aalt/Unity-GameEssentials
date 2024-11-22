#if UNITY_ENTITIES
using System.Collections.Generic;
using Unity.Entities;
using Unity.Scenes;

namespace RenderDream.GameEssentials
{
    public partial class SubSceneLoaderSystem : SystemBase
    {
        private readonly HashSet<Hash128> _loadedSubScenes = new();
        
        protected override void OnUpdate()
        {
            foreach (var sceneReference in SystemAPI.Query<RefRO<SceneReference>>())
            {
                _loadedSubScenes.Add(sceneReference.ValueRO.SceneGUID);
            }
        }

        public void TryAddLoadRequest(Hash128 subSceneHash)
        {
            if (_loadedSubScenes.Contains(subSceneHash))
            {
                return;
            }
            
            var loadProgressEntity = SceneSystem.LoadSceneAsync(World.Unmanaged, subSceneHash);
            
            var dataEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(dataEntity, new SubSceneLoadRequest
            {
                Value = loadProgressEntity
            });
        }

        public bool AreAllRequestedSubScenesLoaded()
        {
            var requests = 0;
            foreach (var requestRO in SystemAPI.Query<RefRO<SubSceneLoadRequest>>())
            {
                if (SceneSystem.GetSceneStreamingState(World.Unmanaged, requestRO.ValueRO.Value) !=
                    SceneSystem.SceneStreamingState.LoadedSuccessfully)
                {
                    requests++;
                }
            }
            return requests == 0;
        }
    }
    
    public struct SubSceneLoadRequest : IComponentData
    {
        public Entity Value;
    }
}
#endif