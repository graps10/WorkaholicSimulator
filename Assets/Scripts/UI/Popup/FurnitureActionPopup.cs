using System;
using Core.ObjectPool;
using Core.Utilities;
using Entities.Molds;
using Hypertonic.Modules.UltimateSockets.PlaceableItems;
using UI.Popup.Interfaces;
using UnityEngine;

namespace UI.Popup
{
    public class FurnitureActionPopup : PopupController, IPopupWithCloseButton
    {
        private const string Scriptable_Pool_Info_Path = "ScriptableObjects/ObjectPool/UI/FurnitureActionPopupPoolInfo"; 
        
        [Header("Buttons")]
        [SerializeField] private CustomButtonController rotateButton;
        [SerializeField] private CustomButtonController pickUpButton;
        
        [field: SerializeField] public CustomButtonController CloseButton { get; private set; }

        private PlaceableItem _targetItem;
        private FurnitureMold _sourceMold;
        private Action<PlaceableItem> _onRotate;
        private Action<PlaceableItem, FurnitureMold> _onPickUp;

        public static void Create(PlaceableItem item, FurnitureMold mold, Action<PlaceableItem> onRotate, Action<PlaceableItem, FurnitureMold> onPickUp)
        {
            if (CanvasManager.Instance == null) return;
            
            var parent = CanvasManager.Instance.PopupCanvas;
            AssetUtils.TryLoadAsset(Scriptable_Pool_Info_Path, out PopupPrefabPoolInfo);
            
            var popup = ObjectPooler.TakePooledGameObject(PopupPrefabPoolInfo, parent).GetComponent<FurnitureActionPopup>();
            
            var popupRect = popup.transform as RectTransform;
            if (popupRect == null)
            {
                popup.ReturnToPool();
                return;
            }
            
            popupRect.SetParent(parent, false);
            popupRect.anchoredPosition = Vector2.zero;
            
            popup.Initialize(item, mold, onRotate, onPickUp);
        }

        private void Initialize(PlaceableItem item, FurnitureMold mold, Action<PlaceableItem> onRotate, Action<PlaceableItem, FurnitureMold> onPickUp)
        {
            _targetItem = item;
            _sourceMold = mold;
            _onRotate = onRotate;
            _onPickUp = onPickUp;
            
            InitializeRectTransform(null);
            InitializeButtons();
            
            // Interaction logic pauses input, so cursor should be visible automatically via PopupController base logic
        }

        private void OnRotateClicked()
        {
            _onRotate?.Invoke(_targetItem);
            ReturnToPool();
        }

        private void OnPickUpClicked()
        {
            _onPickUp?.Invoke(_targetItem, _sourceMold);
            ReturnToPool();
        }

        private void InitializeButtons()
        {
            rotateButton.onClick.AddListener(OnRotateClicked);
            pickUpButton.onClick.AddListener(OnPickUpClicked);
            InitializeCloseButton();
        }
        
        public void InitializeCloseButton() => CloseButton.onClick.AddListener(ReturnToPool);

        public override void ReturnToPool()
        {
            _targetItem = null;
            _sourceMold = null;
            
            _onRotate = null;
            _onPickUp = null;
            
            rotateButton.onClick.RemoveAllListeners();
            pickUpButton.onClick.RemoveAllListeners();
            CloseButton.onClick.RemoveAllListeners();
            
            base.ReturnToPool();
        }
    }
}