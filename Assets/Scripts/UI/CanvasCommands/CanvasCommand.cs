using Core.ObjectPool;
using UI.CanvasReceivers;
using UnityEngine;

namespace UI.CanvasCommands
{
    public abstract class CanvasCommand : PooledGameObject
    {
        public abstract string CanvasCommandPath { get; } 
        protected CanvasReceiver Receiver { get; private set; }

        public bool IsDisposed { private set; get; }
        public virtual bool DisposeBetweenScenes => true;

        public virtual void Initialize(CanvasReceiver receiver)
        {
            Receiver = receiver;
            Receiver.RegisterCanvasCommand(this);

            transform.SetParent(Receiver.Canvas.transform, false);
            transform.localScale = Vector3.one;
            
            IsDisposed = false;
        }

        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            Receiver.UnregisterCanvasCommand(this);
            IsDisposed = true;
            
            ObjectPooler.ReturnPooledObject(this);
        }
    }
}