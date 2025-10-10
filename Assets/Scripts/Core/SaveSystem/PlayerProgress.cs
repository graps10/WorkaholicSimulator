using System;

namespace Core.SaveSystem
{
    [Serializable]
    public class PlayerProgress
    {
        private PlayerTransformData _playerTransformData;
        
        private bool _isFirstLaunch;

        public PlayerProgress()
        {
            _playerTransformData = new PlayerTransformData();
            _isFirstLaunch = true;
        }
    }
}