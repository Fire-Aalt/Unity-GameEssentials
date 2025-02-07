#if UNITY_ENTITIES
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Scenes;

namespace KrasCore.Essentials
{
    public struct SubSceneLoaderSingleton : IComponentData, IDisposable
    {
        public NativeHashSet<Hash128> UnloadedSubScenes;

        public void Dispose()
        {
            UnloadedSubScenes.Dispose();
        }
    }
    
    [UpdateInGroup(typeof(SceneSystemGroup))]
    public partial class SubSceneLoaderSystem : SystemBase
    {
        private readonly Dictionary<Hash128, Entity> _requestEntities = new();

        protected override void OnCreate()
        {
            EntityManager.CreateSingleton(new SubSceneLoaderSingleton
            {
                UnloadedSubScenes = new NativeHashSet<Hash128>(4, Allocator.Persistent)
            });
        }

        protected override void OnDestroy()
        {
            SystemAPI.GetSingleton<SubSceneLoaderSingleton>().Dispose();
        }

        protected override void OnUpdate()
        {
            var unloadedSubScenes = SystemAPI.GetSingleton<SubSceneLoaderSingleton>().UnloadedSubScenes;
            
            unloadedSubScenes.Clear();
            foreach (var kvp in _requestEntities)
            {
                var request = EntityManager.GetComponentData<SubSceneLoadRequest>(kvp.Value);

                if (!EntityManager.Exists(request.Value))
                {
                    unloadedSubScenes.Add(kvp.Key);
                }
            }

            foreach (var hash in unloadedSubScenes)
            {
                EntityManager.DestroyEntity(_requestEntities[hash]);
                _requestEntities.Remove(hash);
            }
        }

        public void TryAddLoadRequest(Hash128 subSceneHash)
        {
            if (_requestEntities.ContainsKey(subSceneHash))
            {
                return;
            }
            
            var loadProgressEntity = SceneSystem.LoadSceneAsync(World.Unmanaged, subSceneHash);
            
            var dataEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(dataEntity, new SubSceneLoadRequest
            {
                Value = loadProgressEntity
            });
            
            _requestEntities.Add(subSceneHash, dataEntity);
        }

        public bool AreAllRequestedSubScenesLoaded()
        {
            var requests = 0;
            foreach (var requestRO in SystemAPI.Query<RefRO<SubSceneLoadRequest>>())
            {
                var state = SceneSystem.GetSceneStreamingState(World.Unmanaged, requestRO.ValueRO.Value);
                if (state != SceneSystem.SceneStreamingState.LoadedSuccessfully)
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