using Core;
using Core.Utilities;
using UnityEngine;

namespace Components.CameraSystem
{
    public static class CameraManager
    {
        private const string Cameras_Parent_Name = "Cameras";
            
        private static Camera mainCamera;
        private static string currentCameraPath;

        private static SceneConfig currentSceneConfig;

        public static void Initialize()
        {
            SceneConfig sceneConfig = SceneManager.CurrentSceneConfig;
            if (sceneConfig == null)
            {
                Debug.LogError("SceneConfig is null. Cannot initialize CameraManager.");
                return;
            }
            
            currentSceneConfig = sceneConfig;
        }

        public static void SpawnCamera() => SetCameraBySceneIndex(GameObject.Find(Cameras_Parent_Name).transform);
            
        private static void SetCameraBySceneIndex(Transform transform)
        {
            if (currentSceneConfig == null)
            {
                Debug.LogError("SceneConfig is not initialized. Call Initialize first.");
                return;
            }
            
            SetCurrentCamera(transform);
        }
        
        public static void SetCurrentCamera(Transform transform, Transform target = null)
        {
            var cameraPath = GetCameraTypeForScene();

            if (mainCamera != null && currentCameraPath == cameraPath)
            {
                Debug.LogWarning("Camera of the requested type is already active.");
                return;
            }

            DestroyAllChildCameras(transform);
            SpawnCameraByPath(cameraPath, transform);

            currentCameraPath = cameraPath;
        }
        
        public static bool IsBoundsInCameraView(Bounds bounds)
        {
            if (mainCamera == null)
                return false;

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
            return GeometryUtility.TestPlanesAABB(planes, bounds);
        }
        
        private static string GetCameraTypeForScene() 
            => currentSceneConfig?.CameraPath;
        
        private static void DestroyAllChildCameras(Transform transform)
        {
            foreach (Transform child in transform)
                if (child.GetComponent<UnityEngine.Camera>() != null)
                    Object.Destroy(child.gameObject);
            
            mainCamera = null;
        }


        private static void SpawnCameraByPath(string path, Transform transform) 
            => mainCamera = UtilsProvider.SearchComponentInObject<UnityEngine.Camera>(
                UtilsProvider.LoadAndInstantiate(path, transform.position, null, transform)
            );
    }
}