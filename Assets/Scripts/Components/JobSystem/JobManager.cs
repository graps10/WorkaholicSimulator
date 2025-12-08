using System;
using Core.Interfaces;
using Core.PlayerSystem;
using Core.SaveSystem;
using UnityEngine;

namespace Components.JobSystem
{
    public class JobManager : MonoBehaviour, IUpdatable
    {
        public static JobManager Instance { get; private set; }

        public bool IsWorking { get; private set; }
        public JobConfig CurrentJobConfig { get; private set; }

        private JobBase _currentJobLogic;
        private float _timeRemaining;
        
        public event Action<JobConfig> OnJobStarted;
        public event Action<bool, int> OnJobEnded;
        public event Action<float> OnTimerUpdated;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            if (Player.Instance != null)
                Player.Instance.RegisterUpdatable(this);
        }

        private void OnDisable()
        {
            if (Player.Instance != null)
                Player.Instance.UnregisterUpdatable(this);
        }
        
        public void StartJob(JobConfig config)
        {
            if (IsWorking) return;

            CurrentJobConfig = config;
            IsWorking = true;
            _timeRemaining = config.HasTimeLimit ? config.TimeLimitSeconds : 0;
            
            if (config.JobLogicPrefab != null)
            {
                _currentJobLogic = Instantiate(config.JobLogicPrefab, transform);
                _currentJobLogic.Initialize(config, CompleteJob, FailJob, AddPartialReward);
            }
            else
            {
                Debug.LogError($"[JobManager] Job Logic Prefab is missing in config: {config.name}");
                EndJobProcess(false);
                return;
            }

            OnJobStarted?.Invoke(config);
            Debug.Log($"Job Started: {config.JobTitle}");
        }
        
        public void OnUpdate()
        {
            if (!IsWorking || _currentJobLogic == null) return;
            
            _currentJobLogic.OnJobUpdate();
            
            if (CurrentJobConfig.HasTimeLimit)
            {
                _timeRemaining -= Time.deltaTime;
                OnTimerUpdated?.Invoke(_timeRemaining);

                if (_timeRemaining <= 0)
                    FailJob();
            }
        }

        private void CompleteJob() => EndJobProcess(true);
        private void FailJob() => EndJobProcess(false);

        private void AddPartialReward(int amount)
        {
            SaveManager.Progress.Wallet.AddMoney(amount);
            Debug.Log($"Partial Reward: {amount}");
        }

        private void EndJobProcess(bool success)
        {
            if (!IsWorking) return;

            int finalReward = success && !CurrentJobConfig.PayPerTask ? CurrentJobConfig.BaseReward : 0;

            if (finalReward > 0)
            {
                SaveManager.Progress.Wallet.AddMoney(finalReward);
                Debug.Log($"Job Finished. Reward: {finalReward}");
            }

            // Cleanup Logic
            if (_currentJobLogic != null)
            {
                _currentJobLogic.OnJobEnded();
                Destroy(_currentJobLogic.gameObject);
                _currentJobLogic = null;
            }

            IsWorking = false;
            OnJobEnded?.Invoke(success, finalReward);
            CurrentJobConfig = null;
        }
        
        public void ForceStopJob()
        {
            if (IsWorking) FailJob();
        }
    }
}