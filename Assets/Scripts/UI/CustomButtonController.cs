using System;
using Core.ObjectPool;
using MelenitasDev.SoundsGood;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CustomButtonController : Button, IDisposable
    {
        private PooledGameObject _pooledGameObject;
        private RectTransform _buttonDimensions;
    
        private Image _borderImage;
        private Image _capImage;
        private Image _iconImage;
    
        private TMP_Text _text;
        private Sound _clickSound;
        private bool _isInitialized;

        protected override void Start()
        {
            base.Start();

            if (_isInitialized)
                return;

            Initialize();
        }

        //Default initialize
        private void Initialize()
        {
            _isInitialized = true;

            _pooledGameObject = GetComponent<PooledGameObject>();
            _buttonDimensions = transform.GetChild(0).GetComponent<RectTransform>();
        
            _borderImage = transform.GetChild(0).GetComponent<Image>();
            _capImage = _borderImage.transform.GetChild(0).GetComponent<Image>();

            if (_capImage != null)
            {
                _text = _capImage.GetComponentInChildren<TMP_Text>(false);
            
                if (_text == null && _capImage.transform.childCount > 0)
                     _iconImage = _capImage.transform.GetChild(0)?.GetComponent<Image>();
            }
        
            _clickSound = new Sound(SFX.buttonClick);
            _clickSound.SetPosition(Camera.main?.transform.position ?? Vector3.zero);
            onClick.AddListener(() => _clickSound.Play());
        
            SetDefaultColors();
        }
    
        private void SetDefaultColors()
        {
            if (_borderImage) _borderImage.color = ColorPaletteContainer.UI_Highlight;
            if (_capImage) _capImage.color = ColorPaletteContainer.UI_Background;
            if (_text) _text.color = ColorPaletteContainer.UI_PureBlack;
            if (_iconImage) _iconImage.color = ColorPaletteContainer.UI_PureBlack;
        }
    
        private void SetBorderWidth(float targetWidth)
        {
            if (_buttonDimensions != null)
                _buttonDimensions.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        }

        ///initialize base values
        private void Initialize(string buttonText)
        {
            Initialize();
            SetText(buttonText);
        }

        //Expanded initialize
        private void Initialize(float buttonBorderWidth, string buttonText , Action actions)
        {
            Initialize(buttonText);
            SetBorderWidth(buttonBorderWidth);
        
            if (actions != null)
                onClick.AddListener(actions.Invoke);
        }

        public static CustomButtonController Create(ButtonMold buttonMold, Transform parent, PrefabPoolInfo _PrefabPoolInfo)
        {
            CustomButtonController customButtonController = ObjectPooler.TakePooledGameObject(_PrefabPoolInfo).GetComponent<CustomButtonController>();

            customButtonController.transform.SetParent(parent);
            customButtonController.transform.localScale = Vector3.one;

            customButtonController.Initialize(buttonMold.ButtonBorderWidth, buttonMold.Text , buttonMold.OnClick);

            return customButtonController;
        }

        public void SetText(string buttonText)
        {
            if(!_isInitialized)
                Initialize();
        
            _text.text = buttonText;
        }

        public void Dispose()
        {
            onClick.RemoveAllListeners();
            _pooledGameObject.ReturnToPool();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            onClick.RemoveAllListeners();
        }
        
        [Serializable]
        public struct ButtonMold
        {
            public string Text;
            public float ButtonBorderWidth;
            public Action OnClick;
        
            public ButtonMold(Action onClick, string text,  float? borderWidth)
            {
                Text = string.IsNullOrEmpty(text) ? "Default text" : text;

                ButtonBorderWidth = borderWidth ?? 0f;
                OnClick = onClick;
            }
        }
    }
}
