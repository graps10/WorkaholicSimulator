using System.Collections.Generic;
using UnityEngine;

namespace Components.JobSystem.Jobs.Cleaning
{
    public class CleaningJob : JobBase
    {
        [Header("Spawning")]
        [SerializeField] private DirtStain dirtPrefab;
        [SerializeField] private int dirtCount = 10;
        [SerializeField] private Vector2 areaSize = new(5, 5);

        private readonly List<DirtStain> _activeStains = new();
        private int _cleanedCount;

        protected override void OnJobStarted() => SpawnDirt();

        public override void OnJobUpdate()
        {
            for (int i = _activeStains.Count - 1; i >= 0; i--)
            {
                if (_activeStains[i].IsCleaned)
                {
                    _activeStains.RemoveAt(i);
                    _cleanedCount++;
                    
                    if (config.PayPerTask)
                        OnAddProgressReward?.Invoke(10); // TODO: add new job config
                }
            }

            if (_activeStains.Count == 0)
                FinishJob(true);
        }

        public override void OnJobEnded()
        {
            foreach (var stain in _activeStains)
            {
                // TODO: use object pool instead
                if (stain != null) Destroy(stain.gameObject);
            }
            
            _activeStains.Clear();
        }

        private void SpawnDirt()
        {
            Vector3 center = transform.position;
            
            for (int i = 0; i < dirtCount; i++)
            {
                Vector3 randomPos = center + new Vector3(
                    Random.Range(-areaSize.x, areaSize.x),
                    0.01f,
                    Random.Range(-areaSize.y, areaSize.y)
                );

                DirtStain newStain = Instantiate(dirtPrefab, randomPos, Quaternion.Euler(90, Random.Range(0, 360), 0));
                _activeStains.Add(newStain);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x * 2, 0.1f, areaSize.y * 2));
        }
    }
}