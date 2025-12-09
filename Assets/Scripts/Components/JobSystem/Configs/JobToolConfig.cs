using Components.JobSystem.Tools;
using UnityEngine;

namespace Components.JobSystem.Configs
{
    [CreateAssetMenu(fileName = "JobToolConfig", menuName = "Core/Jobs/Job Tool Config")]
    public class JobToolConfig : JobConfig
    {
        [Header("Tool Settings")]
        public Transform ToolHolder;
        public JobTool ToolPrefab;
    }
}