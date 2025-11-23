using TMPro;
using UnityEngine;

namespace UI.CanvasCommands
{
    public class InteractionCanvasCommand : CanvasCommand
    {
        public const string Path = "ScriptableObjects/ObjectPool/UI/CanvasCommands/InteractionCanvasCommandPoolInfo";

        public override string CanvasCommandPath => Path;

        [SerializeField] private TextMeshProUGUI interactionText;
        [SerializeField] private CanvasGroup canvasGroup;

        public override void Initialize(CanvasReceivers.CanvasReceiver receiver)
        {
            base.Initialize(receiver);

            if (canvasGroup != null) canvasGroup.alpha = 1f;
        }

        public override void OnUpdate() { }

        public void SetText(string text)
        {
            if (interactionText != null)
                interactionText.text = text;
        }
    }
}