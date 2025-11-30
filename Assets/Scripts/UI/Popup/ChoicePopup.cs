using Core.ObjectPool;
using System.Collections.Generic;
using Core.Utilities;
using TMPro;
using UI.Popup.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class ChoicePopup : PopupController, IPopupWithBgPanel, IPopupWithMainText, IPopupWithCloseButton
    {
        private const string Scriptable_Pool_Info_Path = "ScriptableObjects/ObjectPool/UI/ChoicePopupPoolInfo";
        
        [SerializeField] private Transform buttonsContainerTransform;
        [SerializeField] private PrefabPoolInfo buttonPrefabPoolInfo;
        
        [field: SerializeField] public Image PanelBg { get; private set; }
        [field: SerializeField] public TextMeshProUGUI MainText { get; private set; }
        [field: SerializeField] public CustomButtonController CloseButton { get; private set; }
        
        private List<CustomButtonController> myButtons = new();
        
        public static void Create(string text, CustomButtonController.ButtonMold[] buttonMolds, Vector2Int? overrideSize = null)
        {
            if (CanvasManager.Instance == null) return;
            
            var parent = CanvasManager.Instance.PopupCanvas;
            AssetUtils.TryLoadAsset(Scriptable_Pool_Info_Path, out PopupPrefabPoolInfo);
            var popup = ObjectPooler.TakePooledGameObject(PopupPrefabPoolInfo, parent).GetComponent<ChoicePopup>();
            
            var popupRect = popup.transform as RectTransform;
            if (popupRect == null)
            {
                popup.ReturnToPool();
                return;
            }
            popupRect.SetParent(parent, false);
            popupRect.anchoredPosition = Vector2.zero;

            popup.Initialize(text, buttonMolds, overrideSize);
        }

        private void Initialize(string text, CustomButtonController.ButtonMold[] buttonMolds, Vector2Int? overrideSize = null)
        {
            for (int i = 0; i < buttonMolds.Length; i++)
            {
                var button = CustomButtonController.Create(buttonMolds[i], buttonsContainerTransform, buttonPrefabPoolInfo);
                button.onClick.AddListener(ReturnToPool);
                myButtons.Add(button);
            }
            
            InitializeBgPanel();
            InitializeMainText(text);
            InitializeCloseButton();
            InitializeRectTransform(overrideSize);
            
            ReactToCancel.SubscribeToCancel(ReturnToPool);
        }
        
        public void InitializeBgPanel() => PanelBg.color = ColorPaletteContainer.UI_Background;
        public void InitializeMainText(string text)
        {
            MainText.text = text;
            MainText.color = ColorPaletteContainer.UI_PureBlack;
        }
        public void InitializeCloseButton() => CloseButton.onClick.AddListener(ReturnToPool);
        
        public override void ReturnToPool()
        {
            ReactToCancel.UnsubscribeFromCancel(ReturnToPool);

            if (myButtons != null)
            {
                foreach (var button in myButtons)
                    button.Dispose();
                
                myButtons.Clear();
            }

            if (CloseButton != null)
                CloseButton.onClick.RemoveAllListeners();

            base.ReturnToPool();
        }
    }
}

