using System;
using Core.Utilities;
using TMPro;
using UnityEngine;

namespace UI.CanvasCommands
{
    public class WarningCanvasCommand : UpdatableCanvasCommand
    {
        public const string Path = "ScriptableObjects/ObjectPool/UI/CanvasCommands/WarningCanvasCommandPoolInfo";
        public override string CanvasCommandPath => Path;

        [SerializeField] private TextMeshProUGUI warningText;
        [SerializeField] private TextMeshProUGUI warningTimerText;
        
        private string _textWarning;

        private Coroutine _timerCoroutine;
        
        private float _timer;
        private Action _onTimerRunOut;

        public void Setup(Action onTimerRunOut, int time, string textWarning)
        {
            _textWarning = textWarning;
            _onTimerRunOut = onTimerRunOut;
            _timer = time;
        }
        
        public override void OnUpdate()
        {
            if (_timer > 0)
            {
                _timer -= Time.deltaTime;
                warningText.text = _textWarning;
                warningTimerText.text = $"{_timer:F0} seconds left";

                if (ShouldInterruptTimer()) _timer = 0;
            }
            else
                CompleteTimer();
        }

        protected virtual bool ShouldInterruptTimer() => TeleportUtils.IsTeleportingWithAnimation;

        public void CompleteTimer()
        {
            _onTimerRunOut?.Invoke();
            base.Dispose();
        }
    }
}