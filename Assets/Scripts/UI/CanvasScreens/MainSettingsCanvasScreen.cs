using Core;
using UI.SaveSystemUI;
using UnityEngine;

namespace UI.CanvasScreens
{
    public class MainSettingsCanvasScreen : CanvasScreen
    {
        [SerializeField] private CustomButtonController backButton;
        [SerializeField] private CustomButtonController switchProfileButton;

        private void Start() => Initialize();
        
        public override void Initialize()
        {
            if (IsInitialized)
                return;
            
            IsInitialized = true;

            SceneManager.OnBeforeNewSceneLoaded_ActionList += Dispose;
            OnScreenToggled += HandleScreenToggled;

            AddListenersToCanvasScreenButtons();
        }

        protected override void AddListenersToCanvasScreenButtons()
        {
            backButton.onClick.AddListener(SwitchToMainMenu);
            switchProfileButton.onClick.AddListener(() => SaveSelectorManager.Instance.ShowBodySaveSelector(true));
        }
        
        protected override void RemoveListenersFromCanvasScreenButtons()
        {
            backButton.onClick.RemoveAllListeners();
            switchProfileButton.onClick.RemoveAllListeners();
        }
        
        private static void SwitchToMainMenu() => TrySwitchActiveScreenByType<MainMenuCanvasScreen>();
        
        private void HandleScreenToggled(CanvasScreen screen, bool isActive)
        {
            if (screen != this) return;              
            
            if (isActive)
                ReactToCancel.SubscribeToCancel(SwitchToMainMenu);
            else
                ReactToCancel.UnsubscribeFromCancel(SwitchToMainMenu);
        }

        public override void Dispose()
        {
            ReactToCancel.UnsubscribeFromCancel(SwitchToMainMenu);
            
            SceneManager.OnBeforeNewSceneLoaded_ActionList -= Dispose;
            OnScreenToggled -= HandleScreenToggled;
            base.Dispose();
        }
    }
}