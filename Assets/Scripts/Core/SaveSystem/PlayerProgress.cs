using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.SaveSystem
{
    [Serializable]
    public class PlayerProgress
    {
        private PlayerTransformData _playerTransformData = new();
        private Dictionary<string, Dictionary<string, Pose>> _locationObjectsPathAndPose = new();
        
        private bool _isFirstLaunch = true;

        public bool TryGetLocationObjectPoses(string location, out Dictionary<string, Pose> dictionary)
        {
            if (!_locationObjectsPathAndPose.ContainsKey(location))
            {
                dictionary = null;
                return false;
            }
            
            dictionary = _locationObjectsPathAndPose[location];
            return true;
        }

        public void AddLocationObjectPoses(string location, out Dictionary<string, Pose> dictionary)
        {
            _locationObjectsPathAndPose.Add(location, new Dictionary<string, Pose>());
            dictionary = _locationObjectsPathAndPose[location];
        }
    }
}