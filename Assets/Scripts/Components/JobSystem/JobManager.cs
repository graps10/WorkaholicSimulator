using System;
using Components.JobSystem.Configs;
using Components.JobSystem.Tools;
using Core.Interfaces;
using Core.PlayerSystem;
using Core.SaveSystem;
using Entities.Constructors;
using UnityEngine;

namespace Components.JobSystem
{
    public class JobManager : MonoBehaviour, IUpdatable
    {
        public static JobManager Instance { get; private set; }

        public bool IsWorking { get; private set; }
        public JobConfig CurrentJobConfig { get; private set; }

        private JobBase _currentJobLogic;
        private JobTool _currentTool;
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
            
            if(!Player.Instance.EntityGameObjectIsNull)
                Player.Instance.PlayerEntityGameObject.TeleportPlayer(config.JobTransform.position, config.JobTransform.rotation);
            
            if (config is JobToolConfig toolConfig && toolConfig.ToolPrefab != null)
            {
                if (toolConfig.ToolHolder == null)
                {
                    Debug.LogError("[JobManager] ToolHolder is not assigned!");
                }
                else
                {
                    _currentTool = Instantiate(toolConfig.ToolPrefab, toolConfig.ToolHolder);
                    // TODO: EntityConstructor.Instance.LoadEntity() / Add new jobTool entity
                    _currentTool.OnEquip();
                }
            }
            
            if (config.JobLogicPrefab != null)
            {
                // Temporary
                _currentJobLogic = Instantiate(config.JobLogicPrefab, transform);
                _currentJobLogic.Initialize(config, CompleteJob, FailJob, AddPartialReward);
            }
            else
            {
                Debug.LogError($"[JobManager] Job Logic Prefab is missing in config: {config.name}");
                EndJobProcess(false);
                return;
            }

            _timeRemaining = config.HasTimeLimit ? config.TimeLimitSeconds : 0;
            OnJobStarted?.Invoke(config);
            Debug.Log($"Job Started: {config.JobTitle}");
        }
        
        public void OnUpdate()
        {
            if (!IsWorking) return;
            
            if (_currentJobLogic != null) _currentJobLogic.OnJobUpdate();
            if (_currentTool != null) _currentTool.OnToolUpdate();
            
            if (CurrentJobConfig.HasTimeLimit)
            {
                _timeRemaining -= Time.deltaTime;
                OnTimerUpdated?.Invoke(_timeRemaining);
                if (_timeRemaining <= 0) FailJob();
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
                // Temporary
                _currentJobLogic.OnJobEnded();
                Destroy(_currentJobLogic.gameObject);
                _currentJobLogic = null;
            }
            
            // Cleanup Tool
            if (_currentTool != null)
            {
                _currentTool.OnUnequip();
                _currentTool.ReturnToPool();
                _currentTool = null;
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