using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Core.PlayerSystem;
using Entities.Molds;
using UnityEngine;

namespace Entities.Constructors
{
    public class AsyncObjectConstructor<TReturnT, TConfigT> : ObjectConstructor<TReturnT, TConfigT> where TReturnT : Entity where TConfigT : Mold
    {
        protected const int Entity_Load_Batch_Size = 2;
    
        protected static Queue<Action> entityLoadQueue = new();
        private static Coroutine loadCoroutine;

        public override void LoadImmediately<T>(TConfigT entityMold, Transform transform, out T result) => result = null;

        public void EnqueueEntityLoad(TConfigT entityMold, Transform parentToSet, 
            Action<TReturnT> onComplete = null, Func<bool> loadCondition = null)
        {
            entityLoadQueue.Enqueue(() =>
            {
                if (loadCondition!=null && !loadCondition.Invoke()) return;
                LoadImmediately(entityMold, parentToSet, out TReturnT entity);
                onComplete?.Invoke(entity);
            });

            StartLoadCoroutine();
        }

        protected void StartLoadCoroutine() 
            => loadCoroutine ??= Player.Instance.StartCoroutine(SpawnEntityBatches());

        private IEnumerator SpawnEntityBatches()
        {
            while (entityLoadQueue.Count > 0)
            {
                var createCount = entityLoadQueue.Count < Entity_Load_Batch_Size ? entityLoadQueue.Count : Entity_Load_Batch_Size;

                for (int i = 0; i < createCount; i++)
                    entityLoadQueue.Dequeue()?.Invoke();

                yield return new WaitForEndOfFrame();
            }

            loadCoroutine = null;
        }
    
        public void ClearEntityLoadQueue()
        {
            entityLoadQueue.Clear();
        
            if (loadCoroutine != null && Player.Instance != null)
            {
                Player.Instance.StopCoroutine(loadCoroutine);
                loadCoroutine = null;
            }
        }
    }
}