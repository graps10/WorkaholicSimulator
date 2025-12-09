using Components.JobSystem.Configs;
using Core.Interfaces;
using UnityEngine;

namespace Components.JobSystem
{
    public class JobProvider : MonoBehaviour, IInteractable
    {
        [SerializeField] private JobConfig jobConfig;

        public void Interact()
        {
            if (JobManager.Instance == null) return;

            if (JobManager.Instance.IsWorking)
            {
                // TODO: add info popup warning
                Debug.Log("Player is already working!");
                return;
            }
            
            JobManager.Instance.StartJob(jobConfig);
        }
    }
}