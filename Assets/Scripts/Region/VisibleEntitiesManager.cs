using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Components;
using Core;
using Core.Interfaces;
using Core.PlayerSystem;
using UnityEngine;

namespace Region
{
    public static class VisibleEntitiesManager
    {
        private static Dictionary<IMoving, MovingState> movingEntitiesDictionary = new();

        private static Coroutine updateVisibleObjects;
        
        private static float currentTime;
        private static float defaultTime = 10;
        private static Vector2 rangeOfTimer = new(-2, 0);

        public static void ClearMovingEntitiesDictionary()
        {
            movingEntitiesDictionary.Clear();
        }

        public static void StartUpdatingVisibleObjects()
        {
            if (updateVisibleObjects == null)
                updateVisibleObjects = Player.Instance.StartCoroutine(TimerOfUpdatingVisibleEntities());
        }

        private static IEnumerator TimerOfUpdatingVisibleEntities()
        {
            while (movingEntitiesDictionary.Count > 0)
            {
                currentTime -= Time.deltaTime;

                if (currentTime <= 0)
                    VisibleObjectsTick();
            
                yield return null;
            }
        
            updateVisibleObjects = null;
        }

        private static void VisibleObjectsTick()
        {
            currentTime = defaultTime + Random.Range(rangeOfTimer.x, rangeOfTimer.y);
            UpdateVisibleObjects(GetVisibleObjects());
        }
        
        private static void UpdateVisibleObjects(HashSet<IMoving> newVisibleObjects)
        {
            for (int index = 0; index < movingEntitiesDictionary.Keys.Count; index++)
            {
                IMoving movingEntity = movingEntitiesDictionary.ElementAt(index).Key;

                if (movingEntitiesDictionary[movingEntity] == MovingState.OutsidePlayerView)
                    movingEntitiesDictionary[movingEntity] = MovingState.EntitiesToUnload;

                if (newVisibleObjects.Contains(movingEntity) ||
                    movingEntitiesDictionary[movingEntity] != MovingState.InPlayerView) continue;
                
                movingEntity.IsVisible = false;

                if (movingEntity.IsOutOfSector && !movingEntity.IsSectorLoaded)
                    movingEntitiesDictionary[movingEntity] = MovingState.OutsidePlayerView;
            }

            foreach (var movingObject in newVisibleObjects)
            {
                movingObject.IsVisible = true;
                movingEntitiesDictionary[movingObject] = MovingState.InPlayerView;
            }

            RemoveInvisibleObjects();
        }

        private static void RemoveInvisibleObjects()
        {
            List<IMoving> unloadList = new();

            for (int index = 0; index < movingEntitiesDictionary.Keys.Count; index++)
            {
                IMoving unloadMovingObject = movingEntitiesDictionary.ElementAt(index).Key;

                if (movingEntitiesDictionary[unloadMovingObject] == MovingState.EntitiesToUnload)
                    unloadList.Add(unloadMovingObject);
            }

            foreach (var item in unloadList)
                item.UnloadIfOutOfBounds();
        }

        public static void AddActingObject(IMoving movingObject)
        {
            if (movingEntitiesDictionary.TryAdd(movingObject, MovingState.InPlayerView))
                StartUpdatingVisibleObjects();
        }

        public static void RemoveActingObject(IMoving moving) =>
            movingEntitiesDictionary.Remove(moving);

        private static HashSet<IMoving> GetVisibleObjects()
        {
            var visibleObjects = new HashSet<IMoving>();

            foreach (var entity in movingEntitiesDictionary.Keys)
                visibleObjects.Add(entity);

            return visibleObjects;
        }

        private enum MovingState
        {
            InPlayerView,
            EntitiesToUnload,
            OutsidePlayerView
        }
    }
}
