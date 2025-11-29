using System;
using Core.ObjectPool;
using Core.Utilities;
using TMPro;
using UI.Popup.Interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Popup
{
    public class DialoguePopup : PopupController, IPopupWithBgPanel, IPopupWithMainText
    {
        private const string Scriptable_Pool_Info_Path = "ScriptableObjects/ObjectPool/UI/DialoguePopupPoolInfo";

        [SerializeField] private Button yesButton, noButton;
        
        public Image PanelBg { get;  set; }
        public TextMeshProUGUI MainText { get; set; }
        
        private Action _cancelAction;
        private Action _confirmAction;
        
        public static void Create(string text = "Are you sure?", Action confirmAction = null, Action cancelAction = null, 
            Vector2Int? overrideSize = null)
        {
            if (CanvasManager.Instance == null) return;
            
            var parent = CanvasManager.Instance.PopupCanvas;
            AssetUtils.TryLoadAsset(Scriptable_Pool_Info_Path, out PopupPrefabPoolInfo);
            var popup = ObjectPooler.TakePooledGameObject(PopupPrefabPoolInfo, parent).GetComponent<DialoguePopup>();
            
            var popupRect = popup.transform as RectTransform;
            if (popupRect == null)
            {
                popup.ReturnToPool();
                return;
            }
            popupRect.SetParent(parent, false);
            popupRect.anchoredPosition = Vector2.zero;
            
            popup.Initialize(text, confirmAction, cancelAction, overrideSize);
        }
        
        private void Initialize(string text, Action confirmAction, Action cancelAction = null, Vector2Int? overrideSize = null)
        {
            _cancelAction = cancelAction;
            _confirmAction = confirmAction;

            InitializeBgPanel();
            InitializeMainText(text);
            InitializeButton(yesButton, OnClickConfirm);
            InitializeButton(noButton, OnClickCancel);
            InitializeRectTransform(overrideSize);
            
            ReactToCancel.SubscribeToCancel(OnClickCancel);
            // restrict player movement
        }

        public void InitializeBgPanel() => PanelBg.color = ColorPaletteContainer.UI_Background;
        public void InitializeMainText(string text)
        {
            MainText.text = text;
            MainText.color = ColorPaletteContainer.UI_PureBlack;
        }
        
        private static void InitializeButton(Button button, UnityAction onClickAction)
        {
            button.onClick.AddListener(onClickAction);
            button.GetComponentInChildren<Image>().color = ColorPaletteContainer.UI_Highlight;
            button.GetComponentInChildren<TextMeshProUGUI>().color = ColorPaletteContainer.UI_PureBlack;
        }

        private void OnClickConfirm()
        {
            _confirmAction?.Invoke();
            ClosePopup();
        }

        private void OnClickCancel()
        {
            _cancelAction?.Invoke();
            ClosePopup();
        }

        private void ClosePopup()
        {
            OnCloseAction?.Invoke();
            ReturnToPool();
        }

        public override void ReturnToPool()
        {
            ReactToCancel.UnsubscribeFromCancel(OnClickCancel);
            
            // disable player movement restriction

            if (yesButton != null)
                yesButton.onClick.RemoveAllListeners();
            
            if(noButton != null)
                noButton.onClick.RemoveAllListeners();
            
            base.ReturnToPool();
        }
    }
}