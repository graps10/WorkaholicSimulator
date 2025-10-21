using UnityEngine;

namespace UI.CanvasReceivers
{
    public sealed class DebugCanvasReceiver : CanvasReceiver
    {
        public static DebugCanvasReceiver Instance => instance ??= new DebugCanvasReceiver();

        public override GameObject Canvas => CanvasManager.Instance?.DebugCanvas?.gameObject;

        private static DebugCanvasReceiver instance;

        public override void Dispose()
        {
            base.Dispose();
            instance = null;
        }
    }
}