using System;
using System.Collections.Generic;
using Core.ObjectPool;
using Core.SaveSystem;
using Core.Utilities;
using Entities.Databases;
using Entities.Molds;
using UI.Popup;
using UI.Popup.Interfaces;
using UnityEngine;

namespace UI.FurnitureShop
{
    public class FurnitureShopPopup : PopupController, IPopupWithCloseButton
    {
        private const string Scriptable_Pool_Info_Path = "ScriptableObjects/ObjectPool/UI/FurnitureShopPopupPoolInfo";
        
        private const float Info_Popup_Display_Time = 2f;
        
        [Header("References")]
        [SerializeField] private Transform contentContainer;
        [SerializeField] private ShopItemView itemPrefab;
        [SerializeField] private FurnitureDatabase database;
        
        [field: SerializeField] public CustomButtonController CloseButton { get; private set; }
        
        public static void Create(Action showAction = null, Action closeAction = null, 
            Vector2Int? overrideSize = null)
        {
            if (CanvasManager.Instance == null) return;
            
            var parent = CanvasManager.Instance.PopupCanvas;
            AssetUtils.TryLoadAsset(Scriptable_Pool_Info_Path, out PopupPrefabPoolInfo);
            var popup = ObjectPooler.TakePooledGameObject(PopupPrefabPoolInfo, parent).GetComponent<FurnitureShopPopup>();
            
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

            PopulateShop();
            InitializeRectTransform(overrideSize);
            InitializeCloseButton();
            CanvasCommands.MoneyCanvasCommand.SetPermanentMode(true);
            
            OnShowAction?.Invoke();
        }

        private void PopulateShop()
        {
            foreach (Transform child in contentContainer)
                Destroy(child.gameObject);

            List<FurnitureMold> items = database.GetAllFurniture();

            foreach (var mold in items)
            {
                var itemView = Instantiate(itemPrefab, contentContainer);
                itemView.Initialize(mold, TryBuyItem);
            }
        }

        private void TryBuyItem(FurnitureMold mold)
        {
            var wallet = SaveManager.Progress.Wallet;

            if (wallet.TrySpendMoney(mold.Price))
            {
                SaveManager.Progress.Inventory.AddItem(mold.ID);
                // PlaySound();
                Debug.Log($"Bought {mold.name}!");
            }
            else
            {
                // PlaySound();
                InfoPopup.Create("Not enough money!", anchor: InfoPopup.PopupAnchor.Center, displayTime: Info_Popup_Display_Time);
            }
        }
        
        public void InitializeCloseButton() => CloseButton.onClick.AddListener(ReturnToPool);
        
        public override void ReturnToPool()
        {
            if(isDisposed)
                return;

            isDisposed = true;
            
            OnShowAction = null;
            if (OnCloseAction != null)
            {
                OnCloseAction.Invoke();
                OnCloseAction = null;
            }
            
            if (CloseButton != null)
                CloseButton.onClick.RemoveAllListeners();
            
            CanvasCommands.MoneyCanvasCommand.SetPermanentMode(false);
            base.ReturnToPool();
        }
    }
}
