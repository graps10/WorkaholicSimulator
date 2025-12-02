using System;
using Core.InputSystem;
using Core.ObjectPool;
using Core.PlayerSystem;
using UnityEngine;

namespace UI.Popup
{
    public abstract class PopupController : PooledGameObject
    {
        protected static PrefabPoolInfo PopupPrefabPoolInfo;
            
        [SerializeField] protected Vector2Int defaultPopupSize;
        
        [Header("Interaction Settings")]
        [Tooltip("If true, cursor will be shown and player input disabled.")]
        [SerializeField] protected bool requiresInteraction = true;
        
        protected bool isDisposed;
        
        protected Action OnShowAction = null;
        protected Action OnCloseAction = null;
        
        protected virtual void OnEnable()
        {
            if (requiresInteraction)
            {
                /*CursorController.ToggleCursor(true);
                if (Player.Instance != null && Player.Instance.InputHandler != null)
                    Player.Instance.InputHandler.SetInputActive(false);*/
            }
        }
        
        protected void InitializeRectTransform(Vector2Int? overrideSize)
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = overrideSize ?? defaultPopupSize;
        }
        
        public override void ReturnToPool()
        {
            if (requiresInteraction)
            {
                /*CursorController.ToggleCursor(false);
                if (Player.Instance != null && Player.Instance.InputHandler != null)
                    Player.Instance.InputHandler.SetInputActive(true);*/
            }
            
            base.ReturnToPool();
        }
    }
}