using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Extensions;
using Core.PlayerSystem;
using UnityEngine;

namespace Core.Utilities
{
    public static class SafePoseTracker
    {
        private const float Default_Interval = 5;
        private const int Max_Queue_Size = 3;
        
        private static Transform trackedTransform;
        private static float interval;
        
        private static Queue<Pose> trackedPoses = new();
        private static Coroutine timerCoroutine;

        public static void StartTracking(Transform trackedTransform, float interval = Default_Interval)
        {
            if(Player.Instance == null || trackedTransform == null)
                return;
            
            SafePoseTracker.trackedTransform = trackedTransform;
            SafePoseTracker.interval = interval;
            
            trackedPoses.Clear();
            trackedPoses.Enqueue(trackedTransform.GetPose());
            
            timerCoroutine = Player.Instance.StartCoroutine(Timer());
        }

        public static Pose GetSafePose()
        {
            if(trackedTransform == null && trackedPoses.Count == 0) return default;
            
            if (trackedPoses.Count == 0)
            {
                if (trackedTransform != null) return trackedTransform.GetPose();
                return default;
            }
            
            return trackedPoses.Last();
        }

        private static IEnumerator Timer()
        {
            while (trackedTransform != null)
            {
                yield return new WaitForSeconds(interval);
                UpdatePoses();
            }
        }

        private static void UpdatePoses()
        {
            if(trackedPoses.Count == Max_Queue_Size)
                trackedPoses.Dequeue();

            trackedPoses.Enqueue(trackedTransform.GetPose());
        }

        public static void StopTracking()
        {
            if (Player.Instance == null)
                return;

            if (timerCoroutine != null)
                Player.Instance.StopCoroutine(timerCoroutine);
        }
    }
}


