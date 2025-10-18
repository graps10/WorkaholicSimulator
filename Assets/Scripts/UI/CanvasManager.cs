using System.Collections.Generic;
using Core;
using Transition;
using UI.Canvas;
using UnityEngine;

namespace UI
{
    public class CanvasManager : MonoBehaviour
    {
        public static CanvasManager Instance { get; private set; }
        
        private static bool allCanvasScreensWereLoaded;
        
        [field: SerializeField] public RectTransform CanvasCommandReceiversLayer { get; private set; }
        [field: SerializeField] public RectTransform DebugCanvas { get; private set; }
        [field: SerializeField] public RectTransform EntityCanvas { get; private set; }
        [field: SerializeField] public RectTransform PopupCanvas { get; private set; }
        [field: SerializeField] public  RectTransform CanvasScreensLayer { get; private set; }
        [field: SerializeField] public GameObject LoadingText { get; private set; }
        [field: SerializeField] public GameObject Background { get; private set; }

        private RectTransform _mainCanvas;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.OnSceneChangeTriggered_BeforeAnimation_Event += ClearCanvasCommands;
            SceneManager.AlwaysOnAfterNewSceneLoaded_ActionList += ToggleBackground;

            AbstractSceneTransitionScriptableObject.OnEnterCompleted += LoadAllCanvasScreens;
            AbstractSceneTransitionScriptableObject.OnEnterCompleted += CanvasScreen.ActivateCreatedCanvas;
        }

        private void Start()
        {
            if (!allCanvasScreensWereLoaded) 
                LoadAllCanvasScreens();
        }

        private void LoadAllCanvasScreens()
        {
            allCanvasScreensWereLoaded = true;
            SceneConfig sceneConfig = SceneManager.CurrentSceneConfig;
            CanvasScreen.LoadCanvasScreensForCurrentScene(sceneConfig, CanvasScreensLayer);
        }

        private void ClearCanvasCommands()
        {
            List<CanvasCommand> commandsToDispose = new();
            commandsToDispose.AddRange(CanvasCommandReceiversLayer.GetComponentsInChildren<CanvasCommand>());
            commandsToDispose.AddRange(DebugCanvas.GetComponentsInChildren<CanvasCommand>());

            foreach (var command in commandsToDispose)
                if(command.DisposeBetweenScenes) command.Dispose();
        }

        private void ToggleBackground()
        {
            Background.SetActive(SceneManager.CurrentSceneConfig.SceneIndex == 0);
        }
    }
}

  