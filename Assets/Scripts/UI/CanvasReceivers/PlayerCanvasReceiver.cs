using UnityEngine;

namespace UI.CanvasReceivers
{
    public sealed class PlayerCanvasReceiver : CanvasReceiver
    {
        public static PlayerCanvasReceiver Instance => instance ??= new PlayerCanvasReceiver();
        public override GameObject Canvas => CanvasManager.Instance?.CanvasCommandReceiversLayer?.gameObject;
        
        private static PlayerCanvasReceiver instance;

        public override void Dispose()
        {
            base.Dispose();
            instance = null;
        }
    }
}