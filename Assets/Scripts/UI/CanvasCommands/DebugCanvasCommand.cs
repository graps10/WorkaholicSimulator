using System;
using TMPro;
using UI.CanvasReceivers;

namespace UI.CanvasCommands
{
    public class DebugCanvasCommand : CanvasCommand
    {
        public override string CanvasCommandPath => "ScriptableObjects/ObjectPool/UI/CanvasCommands/DebugCanvasCommandPoolInfo";
        
        private Func<string> _valueGetter;
        
        private TextMeshProUGUI _text;
        private float _defaultFontSize;

        public void Setup(Func<string> valueGetter)
        {
            _valueGetter = valueGetter;
        }

        public override void Initialize(CanvasReceiver receiver)
        {
            base.Initialize(receiver);
            
            _text = GetComponent<TextMeshProUGUI>();
        }

        public override void OnUpdate() => _text.text = _valueGetter?.Invoke();

        public void SetTextFontSize(float updatedFontSize)
        {
            _defaultFontSize = _text.fontSize;
            _text.fontSize = updatedFontSize;
        }

        private void ResetTextSettings()
        {
            _text.text = string.Empty;
            _text.fontSize = _defaultFontSize > 0 ? _defaultFontSize : _text.fontSize;
        }
        
        public override void Dispose()
        {
            if(IsDisposed)
                return;

            ResetTextSettings();
            base.Dispose();
        }
    }
}