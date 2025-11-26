using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.SaveSystem
{
    [Serializable]
    public class PlayerProgress
    {
        [ES3Serializable] private PlayerTransformData playerTransformData = new();
        [ES3Serializable] private Dictionary<string, Dictionary<string, Pose>> locationObjectsPathAndPose = new();
        
        [ES3Serializable] private bool isFirstLaunch = true;

        public bool TryGetLocationObjectPoses(string location, out Dictionary<string, Pose> dictionary)
        {
            if (!locationObjectsPathAndPose.ContainsKey(location))
            {
                dictionary = null;
                return false;
            }
            
            dictionary = locationObjectsPathAndPose[location];
            return true;
        }

        public void AddLocationObjectPoses(string location, out Dictionary<string, Pose> dictionary)
        {
            if (!locationObjectsPathAndPose.ContainsKey(location))
                locationObjectsPathAndPose.Add(location, new Dictionary<string, Pose>());
            
            dictionary = locationObjectsPathAndPose[location];
        }
    }
}