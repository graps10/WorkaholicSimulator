using System;
using Core.ObjectPool;
using UnityEngine;

namespace UI.Popup
{
    public abstract class PopupController : PooledGameObject
    {
        protected static PrefabPoolInfo PopupPrefabPoolInfo;
            
        [SerializeField] protected Vector2Int defaultPopupSize;
        
        protected bool isDisposed;
        
        protected Action OnShowAction = null;
        protected Action OnCloseAction = null;
        
        protected void InitializeRectTransform(Vector2Int? overrideSize)
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = overrideSize ?? defaultPopupSize;
        }
    }
}