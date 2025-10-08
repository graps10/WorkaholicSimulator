using Core.Utilities;
using System;
using System.Collections.Generic;
//using Entities.Constructors;
//using Components.Camera;
//using Regions;
using Transition;
//using UI.Canvas;
//using Core.SaveSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Core
{
    [ExecuteAlways]
    public class SceneManager : MonoBehaviour
    {
        #region Public Events and Properties
        public static event Action OnTransitionEnd;
        public static event Action OnSceneChangeTriggered_BeforeAnimation_Event;
        public static bool IsChangingPlaymode { get; private set; }
        public static bool IsTransitioning { get; private set; }
        public static AsyncOperation SceneLoadOperation { get; private set; }
        public static SceneConfig CurrentSceneConfig 
        {
            get
            {
                if (currentSceneConfig == null)
                {
                    currentSceneConfig = LoadSceneConfig(0);
                }
                return currentSceneConfig;
            }
            private set => currentSceneConfig = value; 
        }
        public static TransitionManager.LoadMode CurrentLoadMode;

        // public lists of actions amongst the private fields... Use events instead. Please never do this again...
        public static Action AlwaysOnBeforeNewSceneLoaded_ActionList;  
        public static Action OnBeforeNewSceneLoaded_ActionList;

        public static Action OnAfterEnterAnimationEnded_ActionList;

        public static Action AlwaysOnAfterNewSceneLoaded_ActionList;
        public static Action OnAfterNewSceneLoaded_ActionList;
        
        public static Action OnNewSceneLoaded_AnimationFinished_ActionList;
        #endregion

        #region Private Fields
        //private static CanvasManager canvasManager;
        private static bool cameraAndCanvasInitialized;
        private static SceneConfig currentSceneConfig;
        
        #endregion

        #region Unity Lifecycle Methods
#if UNITY_EDITOR
        private void OnEnable() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        
        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
            UninitializeCameraAndCanvas();
            UnsubscribeAsyncActorConstructors();
        }
        #endregion

        #region Editor Play Mode Handling
#if UNITY_EDITOR
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            IsChangingPlaymode = state is PlayModeStateChange.ExitingEditMode or PlayModeStateChange.ExitingPlayMode;
        }
#endif
        #endregion

        #region Scene Management Methods
        public static void LoadScene(int sceneIndex, TransitionManager.LoadMode loadMode, List<Action> postSceneLoadActions = null)
        {
            CurrentLoadMode = loadMode;
            CurrentSceneConfig = LoadSceneConfig(sceneIndex);
            InitializeCameraAndCanvas();
            SubscribeAsyncActorConstructors();
            if (SceneLoadOperation != null) return;

            SceneLoadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneIndex);
            SceneLoadOperation.allowSceneActivation = false;

            OnSceneChangeTriggered_BeforeAnimation_Event?.Invoke();

            if (loadMode == TransitionManager.LoadMode.None)
            {
                //RegionManager.Regions.Clear();
                SceneLoadOperation.allowSceneActivation = true;
                SceneLoadOperation = null; 
                postSceneLoadActions?.ForEach(action => action?.Invoke());
                OnTransitionEnd?.Invoke();
                return;
            }
            if (postSceneLoadActions == null)
            {
                postSceneLoadActions = new List<Action>();
            }
            postSceneLoadActions.Add(ExecuteAfterEndTransition);

            ExecuteBeforeTransitionAnimation();

            TransitionManager.StartTransition(
                loadMode,
                ExecuteAfterEnterAnimation,
                ExecuteAfterLoadingScene,
                postSceneLoadActions);
        }

        private static void ExecuteAfterLoadingScene()
        {
            SceneLoadOperation = null;
            OnTransitionEnd?.Invoke();

            AlwaysOnAfterNewSceneLoaded_ActionList?.Invoke();

            OnAfterNewSceneLoaded_ActionList?.Invoke();

            OnAfterNewSceneLoaded_ActionList = null;
        }

        private static void ExecuteAfterEndTransition()
        {
            SceneLoadOperation = null;
            OnTransitionEnd?.Invoke();

            OnNewSceneLoaded_AnimationFinished_ActionList?.Invoke();
            OnNewSceneLoaded_AnimationFinished_ActionList = null;

            IsTransitioning = false;
        }

        private static void ExecuteBeforeTransitionAnimation()
        {
            AlwaysOnBeforeNewSceneLoaded_ActionList?.Invoke();

            OnBeforeNewSceneLoaded_ActionList?.Invoke();
            OnBeforeNewSceneLoaded_ActionList = null;

            //CanvasManager.Instance.LoadingText.SetActive(true);
            //SaveManager.SaveProgress();

            IsTransitioning = true;
        }

        private static void ExecuteAfterEnterAnimation()
        {
            SceneLoadOperation.allowSceneActivation = true;

            UtilitiesProvider.WaitAndRun(() =>
            {

                OnAfterEnterAnimationEnded_ActionList?.Invoke();
                OnAfterEnterAnimationEnded_ActionList = null;

                //CanvasManager.Instance.LoadingText.SetActive(false);
            }, true);
        }

        private static void InitializeCameraAndCanvas()
        {
            if (cameraAndCanvasInitialized) return;
            //canvasManager = FindObjectOfType<CanvasManager>();

            //if (canvasManager == null) Debug.LogError("CanvasManager not found in the scene. Please add it to the scene.");

            //OnSceneChangeTriggered_BeforeAnimation_Event += CameraManager.Initialize;
            cameraAndCanvasInitialized = true;
        }
        
        public static void UninitializeCameraAndCanvas()
        {
            //OnSceneChangeTriggered_BeforeAnimation_Event -= CameraManager.Initialize;
            cameraAndCanvasInitialized = false;
        }
        
        private static void SubscribeAsyncActorConstructors()
        {
            //OnSceneChangeTriggered_BeforeAnimation_Event += EntityConstructor.Instance.ClearActorLoadQueue;
        }
        
        public static void UnsubscribeAsyncActorConstructors()
        {
            //OnSceneChangeTriggered_BeforeAnimation_Event -= EntityConstructor.Instance.ClearActorLoadQueue;
        }
        #endregion

        #region Scene Configuration Loading
        private static SceneConfig LoadSceneConfig(int sceneIndex)
        {
            /*SceneConfigsContainer sceneConfigs = JSONParser.Load<SceneConfigsContainer>("SceneConfig.json");

            if (sceneConfigs != null && sceneConfigs.SceneConfig != null)
            {
                foreach (var config in sceneConfigs.SceneConfig)
                {
                    if (config.SceneIndex == sceneIndex)
                    {
                        return config;
                    }
                }
            }
            Debug.LogWarning($"Scene with index {sceneIndex} not found.");*/
            return null;
        }
        #endregion
    }
}

[Serializable]
public class SceneConfigsContainer
{ 
    public SceneConfig[] SceneConfig;
}

[Serializable]
public class SceneConfig
{
    public string SceneName;
    public int SceneIndex;
    public List<string> PrefabCanvasScreensPath;
    public string CameraPath;
}