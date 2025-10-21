using UnityEngine;

namespace UI.CanvasReceivers
{
    public sealed class WarningCanvasReceiver : CanvasReceiver
    {
        public static WarningCanvasReceiver Instance => instance ??= new WarningCanvasReceiver();
        public override GameObject Canvas => CanvasManager.Instance?.CanvasCommandReceiversLayer?.gameObject;
        
        private static WarningCanvasReceiver instance;

        public override void Dispose()
        {
            base.Dispose();
            instance = null;
        }
    }
}