using Core;
using Core.Interfaces;
using Core.ObjectPool;
using UnityEngine;

namespace UI
{
    public abstract class CanvasCommand : PooledGameObject, IUpdatable
    {
        public static string CanvasCommandPath { get;  private set; } 
        protected CanvasReceiver Receiver { get; private set; }

        public bool IsDisposed { private set; get; } = false;
        public virtual bool DisposeBetweenScenes => true;

        public virtual void Initialize(CanvasReceiver receiver)
        {
            Receiver = receiver;
            Receiver.RegisterCanvasCommand(this);

            transform.SetParent(Receiver.Canvas.transform, false);
            transform.localScale = Vector3.one;
            
            Player.Instance.RegisterUpdatable(this);
            IsDisposed = false;
        }

        public abstract void OnUpdate();

        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            Receiver.UnregisterCanvasCommand(this);
            IsDisposed = true;
            
            if(Player.Instance != null)
                Player.Instance.UnregisterUpdatable(this);
            
            ObjectPooler.ReturnPooledObject(this);
        }
    }

}