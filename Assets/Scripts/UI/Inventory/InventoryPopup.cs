using System;
using Core.ObjectPool;
using Core.SaveSystem;
using Core.Utilities;
using Entities.Databases;
using Entities.Molds;
using UI.Popup;
using UnityEngine;

namespace UI.Inventory
{
    public class InventoryPopup : PopupController
    {
        private const string Scriptable_Pool_Info_Path = "ScriptableObjects/ObjectPool/UI/InventoryPopupPoolInfo";
        
        [Header("References")]
        [SerializeField] private Transform gridContainer;
        [SerializeField] private InventorySlotView slotPrefab;
        [SerializeField] private FurnitureDatabase database;

        public static void Create(Action showAction = null, Action closeAction = null, 
            Vector2Int? overrideSize = null)
        {
            if (CanvasManager.Instance == null) return;
            
            var parent = CanvasManager.Instance.PopupCanvas;
            AssetUtils.TryLoadAsset(Scriptable_Pool_Info_Path, out PopupPrefabPoolInfo);
            var popup = ObjectPooler.TakePooledGameObject(PopupPrefabPoolInfo, parent).GetComponent<InventoryPopup>();
            
            var popupRect = popup.transform as RectTransform;
            if (popupRect == null)
            {
                popup.ReturnToPool();
                return;
            }
            
            popupRect.SetParent(parent, false);
            popupRect.anchoredPosition = Vector2.zero;
            
            popup.Initialize(showAction, closeAction, overrideSize);
        }
        
        private void Initialize(Action onShow, Action onClose = null, Vector2Int? overrideSize = null)
        {
            isDisposed = false;
            OnShowAction = onShow;
            OnCloseAction = onClose;
            
            InitializeRectTransform(overrideSize);
            RefreshInventory();
            
            OnShowAction?.Invoke();
        }

        private void RefreshInventory()
        {
            foreach (Transform child in gridContainer)
                Destroy(child.gameObject);

            var ownedItems = SaveManager.Progress.Inventory.GetOwnedItems();

            foreach (var kvp in ownedItems)
            {
                string id = kvp.Key;
                int count = kvp.Value;

                FurnitureMold mold = database.GetMoldById(id);

                if (mold != null)
                {
                    var slot = Instantiate(slotPrefab, gridContainer);
                    slot.Initialize(mold, count, OnItemSelected);
                }
            }
        }

        private void OnItemSelected(FurnitureMold mold)
        {
            Debug.Log($"Selected item for placement: {mold.name}");
            ReturnToPool();
        }
        
        public override void ReturnToPool()
        {
            if(isDisposed)
                return;

            isDisposed = true;
            
            // Callback for when we close popup
            if (OnCloseAction != null)
            {
                OnCloseAction.Invoke();
                OnCloseAction = null;
            }
            
            base.ReturnToPool();
        }
    }
}
