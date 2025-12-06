using Core.SaveSystem;
using DG.Tweening;
using TMPro;
using UI.CanvasReceivers;
using UnityEngine;

namespace UI.CanvasCommands
{
    public class MoneyCanvasCommand : CanvasCommand
    {
        public const string Path = "ScriptableObjects/ObjectPool/UI/CanvasCommands/MoneyCanvasCommandPoolInfo";
        public override string CanvasCommandPath => Path;
        
        private const float Alpha_Visible = 1f;
        private const float Alpha_Hidden = 0f;
        private const float Punch_Scale_Strength = 0.1f;
        private const float Punch_Duration = 0.2f;
        private const int   Punch_Vibrato = 1;
        private const float Punch_Elasticity = 0f;
        private const float Color_Return_Duration = 0.5f;
        
        private static MoneyCanvasCommand activeInstance;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float displayDuration = 3.0f;
        [SerializeField] private float counterAnimDuration = 2f;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color gainColor = Color.green;
        [SerializeField] private Color spendColor = Color.red;
        
        private Tween _fadeTween;
        private Tween _counterTween;
        private Tween _colorTween;
        private Tween _hideTimerTween;
        
        private bool _isPermanentMode;
        private int _currentDisplayedValue;

        public override void Initialize(CanvasReceiver receiver)
        {
            base.Initialize(receiver);
            activeInstance = this;

            if (canvasGroup != null) canvasGroup.alpha = Alpha_Hidden;
            
            int realMoney = SaveManager.Progress.Wallet.CurrentMoney;
            _currentDisplayedValue = realMoney;
            UpdateText(_currentDisplayedValue);
            
            WalletData.OnMoneyChanged += HandleMoneyChanged;
        }

        public override void Dispose()
        {
            WalletData.OnMoneyChanged -= HandleMoneyChanged;
            
            activeInstance = null;
            KillTweens();
            base.Dispose();
        }
        
        public static void SetPermanentMode(bool active)
        {
            if (activeInstance != null)
                activeInstance.SetPermanentModeInternal(active);
        }

        #region Logic

        private void SetPermanentModeInternal(bool active)
        {
            _isPermanentMode = active;

            if (_isPermanentMode)
            {
                _hideTimerTween?.Kill();
                ShowCanvas(true);
            }
            else
                StartHideTimer();
        }

        private void HandleMoneyChanged(int newAmount)
        {
            _hideTimerTween?.Kill();
            
            ShowCanvas(false);
            
            AnimateCounter(newAmount);
        }

        private void AnimateCounter(int targetValue)
        {
            _counterTween?.Kill();
            _colorTween?.Kill();
            
            Color targetColor = targetValue > _currentDisplayedValue ? gainColor : spendColor;
            moneyText.color = targetColor;
            
            transform.DOPunchScale(Vector3.one * Punch_Scale_Strength, Punch_Duration, Punch_Vibrato, Punch_Elasticity);
            
            _counterTween = DOVirtual.Float(_currentDisplayedValue, targetValue, counterAnimDuration, (val) =>
            {
                _currentDisplayedValue = Mathf.RoundToInt(val);
                UpdateText(_currentDisplayedValue);
            }).OnComplete(() => 
            {
                _currentDisplayedValue = targetValue;
                UpdateText(_currentDisplayedValue);
                _colorTween = moneyText.DOColor(defaultColor, Color_Return_Duration);
                
                if (!_isPermanentMode)
                    StartHideTimer();
            });
        }

        private void StartHideTimer()
        {
            _hideTimerTween?.Kill();
            _hideTimerTween = DOVirtual.DelayedCall(displayDuration, CheckHide).SetId(this);
        }
        
        private void UpdateText(int value)
        {
            if (moneyText != null) 
                moneyText.text = $"{value}$";
        }

        private void ShowCanvas(bool instant)
        {
            if (canvasGroup.alpha < 1f || (_fadeTween != null && _fadeTween.IsPlaying()))
            {
                _fadeTween?.Kill();
                if (instant)
                    canvasGroup.alpha = Alpha_Visible;
                else
                    _fadeTween = canvasGroup.DOFade(Alpha_Visible, fadeDuration);
            }
        }

        private void CheckHide()
        {
            if (_isPermanentMode) return;

            _fadeTween?.Kill();
            _fadeTween = canvasGroup.DOFade(Alpha_Hidden, fadeDuration);
        }

        private void KillTweens()
        {
            _fadeTween?.Kill();
            _counterTween?.Kill();
            _colorTween?.Kill();
            _hideTimerTween?.Kill();
            
            DOTween.Kill(this);
        }

        #endregion
    }
}