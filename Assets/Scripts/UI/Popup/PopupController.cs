using System;
using Core.ObjectPool;
using UnityEngine;

namespace UI.Popup
{
    public abstract class PopupController : PooledGameObject
    {
        protected abstract string ScriptablePoolInfoPath { get; }
        
        protected static PrefabPoolInfo popupPrefabPoolInfo;
            
        [SerializeField] protected Vector2Int defaultPopupSize;
        
        protected bool isDisposed;
        protected Action OnCloseAction = null;
        
        protected void InitializeRectTransform(Vector2Int? overrideSize)
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = overrideSize ?? defaultPopupSize;
        }
    }
}