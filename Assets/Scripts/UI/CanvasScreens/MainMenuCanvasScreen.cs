using System;
using Core;
using Core.Enums;
using Transition;
using UI.Popup;
using UnityEngine;

namespace UI.CanvasScreens
{
    public class MainMenuCanvasScreen : CanvasScreen
    {
        [SerializeField] private CustomButtonController devOnlySceneButton;
        [SerializeField] private CustomButtonController testRegionButton;
        [SerializeField] private CustomButtonController settingsButton;
        [SerializeField] private CustomButtonController exitButton;
        
        private void Start() => Initialize();

        private void OnDestroy() => Dispose();

        public override void Initialize()
        {
            if(IsInitialized)return;
            
            AddListenersToCanvasScreenButtons();
            
#if UNITY_EDITOR == FALSE
            if(playDevSceneButton != null)
                playDevSceneButton.gameObject.SetActive(false);
#endif
            
            IsInitialized = true;

            TrySwitchActiveScreen(this);
        }
        private static void ContinueGame(Action callback)
        {
            /*if (!SaveManager.ContinueGame())
                SaveSelectorManager.Instance.ShowBodySaveSelectorFirstStart(callback);

            else callback?.Invoke();*/
            
            callback?.Invoke();
        }
        
        private static void LoadDevelopersScene() 
            => SceneManager.LoadScene((int)UnityScenes.developerOnlyScene, TransitionManager.LoadMode.Fade);

        // private void ShowWelcomePopup() 

        private void CreateExitPopup()
            => DialoguePopup.Create("Do you really want to exit game?", Exit, parent: transform);

        private void Exit() 
            => Application.Quit();

        private void LoadTestRegion()
        {
            SceneManager.LoadScene((int)UnityScenes.testRegion, TransitionManager.LoadMode.Fade);
            // SceneManager.OnNewSceneLoaded_AnimationFinished_ActionList += ShowWelcomePopup;
        }

        protected override void AddListenersToCanvasScreenButtons()
        {
#if UNITY_EDITOR
            if (devOnlySceneButton != null)
                devOnlySceneButton.onClick.AddListener(() => ContinueGame(callback: LoadDevelopersScene));
#else
            playDevSceneButton.gameObject.SetActive(false);
#endif

            if (settingsButton != null)
                settingsButton.onClick.AddListener(() => TrySwitchActiveScreenByType<MainSettingsCanvasScreen>());
            
            if(exitButton != null)
                exitButton.onClick.AddListener(CreateExitPopup);

            if(testRegionButton != null)
                testRegionButton.onClick.AddListener(() => ContinueGame(callback: LoadTestRegion));
        }

        protected override void RemoveListenersFromCanvasScreenButtons()
        {
            if(devOnlySceneButton != null)
                devOnlySceneButton.onClick.RemoveAllListeners();
            
            if(testRegionButton != null)
                testRegionButton.onClick.RemoveAllListeners();
            
            if(settingsButton != null)
                settingsButton.onClick.RemoveAllListeners();
            
            if(exitButton != null)
                exitButton.onClick.RemoveAllListeners();

            if (testRegionButton != null)
                testRegionButton.onClick.RemoveAllListeners();
        }
    }
}