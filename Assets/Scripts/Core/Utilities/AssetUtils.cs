using System.IO;
using UnityEditor;
using UnityEngine;

namespace Core.Utilities
{
    internal static class AssetUtils
    {
        private static T LoadAsset<T>(string assetPath) where T : Object
        {
            var loadedObject = Resources.Load<T>(assetPath);
            
            if (loadedObject == null)
                throw new FileNotFoundException("No file found - please check the configuration at path: " + assetPath);
            
            return loadedObject;
        }

        internal static bool TryLoadAsset<T>(string assetPath, out T loadedObject) where T : Object
        {
            loadedObject = LoadAsset<T>(assetPath);

            return loadedObject != null;
        }
        
#if UNITY_EDITOR
        
        public static bool TryLoadUnityAsset<T>(string path, out T asset) where T : Object
        {
            asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset == null)
            {
                Debug.LogError("No file found - please check the configuration at path: " + path);
                return false;
            }

            return true;
        }
        
#endif
    }
}