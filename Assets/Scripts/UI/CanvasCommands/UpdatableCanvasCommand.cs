using Core.Interfaces;
using Core.PlayerSystem;
using UI.CanvasReceivers;

namespace UI.CanvasCommands
{
    public abstract class UpdatableCanvasCommand : CanvasCommand, IUpdatable
    {
        public abstract void OnUpdate();

        public override void Initialize(CanvasReceiver receiver)
        {
            base.Initialize(receiver);
            
            if (Player.Instance != null)
                Player.Instance.RegisterUpdatable(this);
        }

        public override void Dispose()
        {
            if (!IsDisposed && Player.Instance != null)
                Player.Instance.UnregisterUpdatable(this);
            
            base.Dispose();
        }
    }
}