using UI.CanvasReceivers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.CanvasCommands
{
    public class CrosshairCanvasCommand : CanvasCommand
    {
        public const string Path = "ScriptableObjects/ObjectPool/UI/CanvasCommands/CrosshairCanvasCommandPoolInfo";
        public override string CanvasCommandPath => Path;
        
        private static CrosshairCanvasCommand activeInstance;
        
        [Header("UI References")]
        [SerializeField] private Image crosshairImage;
        [SerializeField] private Color initialColor = Color.white;
        
        public override void Initialize(CanvasReceiver receiver)
        {
            base.Initialize(receiver);
            
            activeInstance = this;
            SetColor(initialColor);
        }

        public override void Dispose()
        {
            activeInstance = null;
            base.Dispose();
        }
        
        public static void SetCrosshairColor(Color color)
        {
            if (activeInstance != null && activeInstance.crosshairImage != null)
                activeInstance.crosshairImage.color = color;
        }
        
        public static void SetVisibility(bool visible)
        {
            if (activeInstance != null)
                activeInstance.gameObject.SetActive(visible);
        }

        private void SetColor(Color color)
        {
            if (crosshairImage != null) 
                crosshairImage.color = color;
        }
    }
}