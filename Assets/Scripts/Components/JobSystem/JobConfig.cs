using UnityEditor;
using UnityEngine;

namespace Components.JobSystem
{
    [CreateAssetMenu(fileName = "NewJobConfig", menuName = "Core/Jobs/Job Config")]
    public class JobConfig : ScriptableObject
    {
        [Header("General Settings")]
        public string JobID;
        public string JobTitle;
        [TextArea] public string Description;
        
        [Header("Economy")]
        public int BaseReward = 100;
        public bool PayPerTask = false;
        
        [Header("Constraints")]
        public bool HasTimeLimit;
        public float TimeLimitSeconds = 120f;
        
        [Header("Logic")]
        public JobBase JobLogicPrefab; 
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(JobID))
            {
                JobID = System.Guid.NewGuid().ToString();
                EditorUtility.SetDirty(this);
            }
        }
        
        [ContextMenu("Generate New ID")]
        private void GenerateId()
        {
            JobID = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
