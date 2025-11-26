using System;
using System.Collections;
using Core.Extensions;
using Core.PlayerSystem;
using Core.SaveSystem;
using Entities;
using Entities.Constructors;
using UnityEngine;

namespace Region
{
    public static class LocationExtension
    {
        #region Entity creation and unloading methods

        /// <param name="onEntitySpawned">Callback invoked when each individual entity is spawned</param>
        /// <param name="onAllPresetEntitiesSpawned">Callback invoked when all entities from the preset are spawned</param>
        
        /// <summary>
        /// Creates multiple entities asynchronously using the specified spawn preset.
        /// Entities are spawned in sequence with tracking of completion.
        /// </summary>
        public static void CreateEntitiesAsync(this Location location, EntitySpawnPreset entitySpawnPreset,
            LocationEntitiesList listToPopulate = null, Action<Entity> onEntitySpawned = null, Action onAllPresetEntitiesSpawned = null)
        {
            if(Player.Instance != null)
                Player.Instance.StartCoroutine(CreateEntitiesAsyncRoutine(location, entitySpawnPreset, listToPopulate,
                    onEntitySpawned, onAllPresetEntitiesSpawned));
        }
        
        /// <summary>
        /// Creates a single entity asynchronously at the specified transform position.
        /// </summary>
        public static void CreateEntityAsync(this Location location, Transform entityTransform,
            EntitySpawnPreset preset, LocationEntitiesList listToPopulate = null, Action<Entity> onEntitySpawned = null)
        {
            if(Player.Instance != null)
                Player.Instance.StartCoroutine(CreateEntityAsyncRoutine(location, entityTransform, preset, listToPopulate, onEntitySpawned));
        }
        
        public static void UnloadEntities(this Location location, LocationEntitiesList entitiesToUnload)
        {
            foreach (var entityToUnload in entitiesToUnload)
                UnloadEntity(entityToUnload);

            location.ClearOnSwitchLogicEvent();
        }

        private static void UnloadEntity(Entity entityToUnload)
        {
            if (entityToUnload == null)
                return;

            entityToUnload.ReturnToPool();
        }

        #endregion
        
        #region Entity creation coroutines
        
        /// <summary>
        /// Coroutine for asynchronous spawning of multiple entities with completion tracking.
        /// </summary>
        private static IEnumerator CreateEntitiesAsyncRoutine(this Location location, EntitySpawnPreset entitySpawnPreset,
            LocationEntitiesList listToPopulate = null, Action<Entity> onEntitySpawned = null, Action onAllPresetEntitiesSpawned = null)
        {
            int totalEntitiesToSpawn = 0;
            int entitiesSpawnedCount = 0;
            
            foreach (var entityTransform in entitySpawnPreset.transforms)
            {
                if (entityTransform == null) continue;
                if (entityTransform.gameObject.activeSelf == false
                    || entityTransform.childCount > 0 && entityTransform.GetChild(0).gameObject.activeSelf == false)
                    continue;

                totalEntitiesToSpawn++;
            }

            if (totalEntitiesToSpawn == 0) 
            {
                onAllPresetEntitiesSpawned?.Invoke();
                yield break;
            }
            
            Action<Entity> trackingOnComplete = (entity) => // To track the completion of an entity's spawn
            {
                onEntitySpawned?.Invoke(entity);
                entitiesSpawnedCount++;
            };
            
            foreach (var entityTransform in entitySpawnPreset.transforms)
            {
                if (entityTransform == null) continue;
                if (entityTransform.gameObject.activeSelf == false
                    || entityTransform.childCount > 0 && entityTransform.GetChild(0).gameObject.activeSelf == false)
                    continue;

                yield return CreateEntityAsyncRoutine(location, entityTransform, entitySpawnPreset, listToPopulate,
                    trackingOnComplete);
            }
            
            yield return new WaitUntil(() => entitiesSpawnedCount >= totalEntitiesToSpawn);
            onAllPresetEntitiesSpawned?.Invoke();
        }

        /// <summary>
        /// Coroutine for asynchronous spawning of a single entity.
        /// </summary>
        private static IEnumerator CreateEntityAsyncRoutine(this Location location, Transform entityTransform,
            EntitySpawnPreset preset, LocationEntitiesList listToPopulate = null, Action<Entity> onEntitySpawned = null)
        {
            if (!preset.AllowMultipleEntitiesInSingleParent &&
                location._entitiesDictionary.TryGetValue(entityTransform, out var existingEntity))
            {
                SetupEntity(existingEntity, location, entityTransform);
                listToPopulate?.Add(existingEntity);
                onEntitySpawned?.Invoke(existingEntity);
                yield break;
            }

            bool isEntityCreated = false;
            Action<Entity> completeCallback = (entity) =>
            {
                FinalizeEntityCreation(location, entity, entityTransform, preset, listToPopulate, onEntitySpawned,
                    ref isEntityCreated);
            };

            EntityConstructor.Instance.EnqueueEntityLoad(preset.mold, entityTransform,
                loadCondition: () => location.IsLoaded,
                onComplete: completeCallback);

            yield return new WaitUntil(() => isEntityCreated);
        }

        #endregion
        
        #region Utilities
        
        private static void SetupEntity(Entity entity, Location location, Transform spawnPointKey)
        {
            var spawnPointDisposeKey = spawnPointKey; 
            
            entity.OnDispose += () =>
            {
                if (location != null && location._entitiesDictionary != null)
                {
                    if (location._entitiesDictionary.ContainsKey(spawnPointDisposeKey))
                        location._entitiesDictionary.Remove(spawnPointDisposeKey);
                }
            }; 
        }
        
        private static void FinalizeEntityCreation(this Location location, Entity entity, Transform entityTransform, 
            EntitySpawnPreset preset, LocationEntitiesList listToPopulate, Action<Entity> onEntitySpawned, ref bool entityCreatedFlag)
        {
            location._entitiesDictionary[entityTransform] = entity;
            
            if (location._savedEntityPoses != null 
                && location._savedEntityPoses.TryGetValue(entityTransform.name, out var pose))
                entity.transform.ApplyPose(pose);

            entity.ToggleLogic(preset.EnableLogic);
            entity.SwitchGraphics(true);

            SetupEntity(entity, location, entityTransform);
            listToPopulate?.Add(entity);
    
            entityCreatedFlag = true;
            onEntitySpawned?.Invoke(entity);
        }
        
        #endregion
    }
}