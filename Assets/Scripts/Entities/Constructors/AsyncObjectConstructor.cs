using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Entities.Molds;
using UnityEngine;

namespace Entities.Constructors
{
    public class AsyncObjectConstructor<TReturnT, TConfigT> : ObjectConstructor<TReturnT, TConfigT> where TReturnT : Entity where TConfigT : Mold
    {
        protected const int Actor_Load_Batch_Size = 2;
    
        protected static Queue<Action> actorLoadQueue = new();
        private static Coroutine loadCoroutine;

        public override void LoadImmediately<T>(TConfigT entityMold, Transform transform, out T result) => result = null;

        public void EnqueueActorLoad(TConfigT actorMold, Transform parentToSet, 
            Action<TReturnT> onComplete = null, Func<bool> loadCondition = null)
        {
            actorLoadQueue.Enqueue(() =>
            {
                if (loadCondition!=null && !loadCondition.Invoke()) return;
                LoadImmediately(actorMold, parentToSet, out TReturnT actor);
                onComplete?.Invoke(actor);
            });

            StartLoadCoroutine();
        }

        protected void StartLoadCoroutine() 
            => loadCoroutine ??= Player.Instance.StartCoroutine(SpawnActorBatches());

        private IEnumerator SpawnActorBatches()
        {
            while (actorLoadQueue.Count > 0)
            {
                var createCount = actorLoadQueue.Count < Actor_Load_Batch_Size ? actorLoadQueue.Count : Actor_Load_Batch_Size;

                for (int i = 0; i < createCount; i++)
                    actorLoadQueue.Dequeue()?.Invoke();

                yield return new WaitForEndOfFrame();
            }

            loadCoroutine = null;
        }
    
        public void ClearActorLoadQueue()
        {
            actorLoadQueue.Clear();
        
            if (loadCoroutine != null && Player.Instance != null)
            {
                Player.Instance.StopCoroutine(loadCoroutine);
                loadCoroutine = null;
            }
        }
    }
}