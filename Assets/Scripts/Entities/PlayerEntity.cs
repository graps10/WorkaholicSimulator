using Core.Extensions;
using Core.Interfaces;
using Core.PlayerSystem;
using Core.SaveSystem;
using Core.Utilities;
using Entities.Molds;
using UnityEngine;

namespace Entities
{
    public class PlayerEntity : Entity, IUpdatable
    {
        [SerializeField] private PlayerInputHandler inputHandler;
        public PlayerInputHandler InputHandler
        {
            get
            {
                if (inputHandler == null)
                    inputHandler = GetComponent<PlayerInputHandler>();
                return inputHandler;
            }
        }
        
        public override void LoadEntity(Mold entityMold)
        {
            base.LoadEntity(entityMold);
             Initialize(entityMold as PlayerMold);
        }

        private void Initialize(PlayerMold mold)
        {
            if (inputHandler == null) 
                inputHandler = GetComponent<PlayerInputHandler>();
            
            Player.Instance.RegisterUpdatable(this);
            SafePoseTracker.StartTracking(transform);
            
            UtilsProvider.WaitAndRun(() =>
            {
                var savedTransform = SaveManager.Progress.PlayerTransformData;
                if (savedTransform.TryGetPlayerPose(out Pose pose))
                    transform.ApplyPose(pose);

                Physics.SyncTransforms();

            }, true);
        }

        public void OnUpdate() { }

        public override void ReturnToPool()
        {
            SafePoseTracker.StopTracking();
            Player.Instance.UnregisterUpdatable(this);
            
            base.ReturnToPool();
        }
    }
}
