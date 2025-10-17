using System;
using System.Collections.Generic;
using Core;
using Transition;
using UnityEngine;

namespace UI.Canvas
{
    public abstract class CanvasScreen : MonoBehaviour, IDisposable
    {
        public static CanvasScreen ActiveCanvasScreen { get; set; }
        public static event Action<CanvasScreen, bool> OnScreenToggled;
        
        private static List<CanvasScreen> existingCanvasScreens = new();
        
        [SerializeField] protected UnityEngine.Canvas canvasReference;
        
        protected bool IsInitialized;
        

        #region Initialization and Lifecycle
        
        public abstract void Initialize();
        
        public static void LoadCanvasScreensForCurrentScene(SceneConfig sceneConfig, RectTransform canvasScreenLayer)
        {
            List<string> paths = LoadCanvasScreensForScene(sceneConfig);
            DestroyCreatedCanvas();
            foreach (var pathToCanvasScreen in paths)
            {
                var prefab = Resources.Load<GameObject>(pathToCanvasScreen);
                if (prefab == null) Debug.LogError($"Failed to load prefab");
                if (canvasScreenLayer == null) Debug.LogError("Failed to find a valid parent layer.");
                CreateAndConfigureCanvasScreen(canvasScreenLayer, prefab);
            }
        }
        
        private static void DestroyCreatedCanvas()
        {
            foreach (var canvas in existingCanvasScreens)
                if (canvas != null)
                    Destroy(canvas.gameObject);
            
            existingCanvasScreens.Clear();
        }
        
        public virtual void Dispose() => RemoveListenersFromCanvasScreenButtons();
        
        #endregion

        #region Screen Management
        
        public static bool TrySwitchActiveScreen(CanvasScreen screenToSet)
        {
            if (screenToSet == null)
            {
                Debug.LogError("Screen == null");
                return false;
            }
            if (screenToSet == ActiveCanvasScreen)
            {
                Debug.LogError("Same screen " + ActiveCanvasScreen.gameObject.name);
                return false;
            }
            
            SwitchActiveScreen(screenToSet);
            return true;
        }

        public static bool TrySwitchActiveScreenByType<TScreen>() where TScreen : CanvasScreen
        {
            foreach (var canvasScreen in existingCanvasScreens)
            {
                if (canvasScreen is not TScreen) continue;
                SwitchActiveScreen(canvasScreen);
                return true;
            }
            
            Debug.LogError("Not found Canvas Screen");
            return false;
        }

        public static void ActivateCreatedCanvas()
        {
            foreach (var canvas in existingCanvasScreens)
                canvas.gameObject.SetActive(true);
        }

        private static void SwitchActiveScreen(CanvasScreen screenToSet)
        {
            CanvasScreen oldScreen = ActiveCanvasScreen;
            if (oldScreen != null)
                oldScreen.SwitchActive(false);

            screenToSet.SwitchActive(true);
            ActiveCanvasScreen = screenToSet;
        }

        private void SwitchActive(bool enable)
        {
            if (canvasReference != null)
                canvasReference.enabled = enable;

            OnScreenToggled?.Invoke(this, enable);
        }
        
        #endregion

        #region Canvas Creation and Configuration
        
        private static void CreateAndConfigureCanvasScreen(RectTransform canvasScreenLayer, GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, canvasScreenLayer);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;
            if (SceneManager.CurrentLoadMode == TransitionManager.LoadMode.Fade)
                instance.SetActive(false);
            
            existingCanvasScreens.Add(instance.GetComponent<CanvasScreen>());
        }

        private static List<string> LoadCanvasScreensForScene(SceneConfig sceneConfig)
        {
            List<string> prefabPaths = new List<string>();
            if (sceneConfig != null)
                foreach (var path in sceneConfig.PrefabCanvasScreensPath)
                    prefabPaths.Add(path);
            
            else Debug.LogError("Error loads prefabs paths");
            
            return prefabPaths;
        }
        
        #endregion
        
        protected abstract void AddListenersToCanvasScreenButtons();
        protected abstract void RemoveListenersFromCanvasScreenButtons();
    }
}