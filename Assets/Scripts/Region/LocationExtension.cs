using System;
using System.Collections;
using Core;
using Core.Extensions;
using Core.PlayerSystem;
using Core.Utilities;
using Entities;
using Entities.Constructors;
using QuestSystem.Base;
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
            QuestEntitiesList listToPopulate = null, Action<Entity> onEntitySpawned = null, Action onAllPresetEntitiesSpawned = null)
        {
            if(Player.Instance != null)
                Player.Instance.StartCoroutine(CreateEntitiesAsyncRoutine(location, entitySpawnPreset, listToPopulate,
                    onEntitySpawned, onAllPresetEntitiesSpawned));
        }
        
        /// <summary>
        /// Creates a single entity asynchronously at the specified transform position.
        /// </summary>
        public static void CreateEntityAsync(this Location location, Transform entityTransform,
            EntitySpawnPreset preset, QuestEntitiesList listToPopulate = null, Action<Entity> onEntitySpawned = null)
        {
            if(Player.Instance != null)
                Player.Instance.StartCoroutine(CreateEntityAsyncRoutine(location, entityTransform, preset, listToPopulate, onEntitySpawned));
        }
        
        public static void UnloadEntities(this Location location, QuestEntitiesList entitiesToUnload)
        {
            foreach (var entityToUnload in entitiesToUnload)
                UnloadEntity(location, entityToUnload);

            location.ClearOnSwitchLogicEvent();
        }

        private static void UnloadEntity(this Location location, Entity entityToUnload)
        {
            if (entityToUnload == null)
                return;

            location.GetEntitiesDictionary().Remove(entityToUnload.transform.parent);

            UtilsProvider.WaitAndRun(() =>
            {
                if (entityToUnload != null && entityToUnload.gameObject.activeInHierarchy)
                    entityToUnload.ReturnToPool();
            }, true);
        }

        #endregion
        
        #region Entity creation coroutines
        
        /// <summary>
        /// Coroutine for asynchronous spawning of multiple entities with completion tracking.
        /// </summary>
        private static IEnumerator CreateEntitiesAsyncRoutine(this Location location, EntitySpawnPreset entitySpawnPreset,
            QuestEntitiesList listToPopulate = null, Action<Entity> onEntitySpawned = null, Action onAllPresetEntitiesSpawned = null)
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
            EntitySpawnPreset preset, QuestEntitiesList listToPopulate = null, Action<Entity> onEntitySpawned = null)
        {
            if (!preset.AllowMultipleEntitiesInSingleParent &&
                location.GetEntitiesDictionary().TryGetValue(entityTransform, out var existingEntity))
            {
                SetupEntity(existingEntity, preset, location);
                listToPopulate?.Add(existingEntity);
                onEntitySpawned?.Invoke(existingEntity);
                yield break;
            }

            bool isEntityCreated = false;
            Action<Entity> completeCallback = (entity) =>
            {
                FinalizeEntityCreation(location, entity, entityTransform, preset, listToPopulate, onEntitySpawned, ref isEntityCreated);
            };

            EntityConstructor.Instance.EnqueueEntityLoad(preset.mold, entityTransform,
                loadCondition: () => location.IsLoaded,
                onComplete: completeCallback);
            
            yield return new WaitUntil(() => isEntityCreated);
        }
        
        #endregion
        
        #region Utilities
        
        private static void SetupEntity(Entity entity, EntitySpawnPreset preset, Location location)
        {
            if (!preset.IsQuestEntity)
            {
                var entityTransformDispose = entity.transform;
                entity.OnDispose += () =>
                {
                    location.GetEntitiesDictionary()[entityTransformDispose] = null;
                }; 
            }
        }
        
        private static void FinalizeEntityCreation(this Location location, Entity entity, Transform entityTransform, 
            EntitySpawnPreset preset, QuestEntitiesList listToPopulate, Action<Entity> onEntitySpawned, ref bool entityCreatedFlag)
        {
            if (!preset.IsQuestEntity)
                location.GetEntitiesDictionary()[entityTransform] = entity;

            if (location.GetSavedEntityPoses() != null 
                && location.GetSavedEntityPoses().TryGetValue(entityTransform.name, out var pose))
                entity.transform.ApplyPose(pose);

            entity.ToggleLogic(preset.EnableLogic);
            entity.SwitchGraphics(true);

            SetupEntity(entity, preset, location);
            listToPopulate?.Add(entity);
    
            entityCreatedFlag = true;
            onEntitySpawned?.Invoke(entity);
        }

        public static void RemoveEntityFromList(this Location location, Entity entity)
        {
            location.GetEntitiesDictionary().Remove(entity.transform.parent);
        }

        public static bool IsPlayerInBounds(this Location location)
        {
            if(Player.Instance == null || Player.Instance.PlayerEntityGameObject == null) 
                return false;

            return RegionManager.IsPlayerInLocation(location);
        }
        
        #endregion
    }
}