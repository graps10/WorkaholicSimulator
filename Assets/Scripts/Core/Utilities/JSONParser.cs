using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Utilities
{
    public static class JSONParser
    {
        public static void Save<T>(string nameOfJSONFile, T data) where T : new()
        {
            string savePath;

#if UNITY_EDITOR
         
            savePath = Application.dataPath + "/Resources/Configs/" + nameOfJSONFile; 
#else
            savePath = Path.Combine(Application.temporaryCachePath, nameOfJSONFile);
#endif
            
            string directory = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(savePath, json);

            Debug.Log($"Data saved to: {savePath}");
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh(); 
#endif
        }

        public static void SaveToPersistent<T>(string nameOfJSONFile, T data) where T : new()
        {
            string savePath = Path.Combine(Application.persistentDataPath, nameOfJSONFile);

            
            string directory = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(savePath, json);

            Debug.Log($"Data saved to persistent path: {savePath}");
        }

        public static T Load<T>(string nameOfJSONFile) where T : new()
        {
            string resourcePath = "Configs/" + Path.GetFileNameWithoutExtension(nameOfJSONFile);
            TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset == null)
            {
                Debug.LogError($"File not found in Resources at path: {resourcePath}. New object created.");
                return new T();
            }

            try
            {
                var convert = JsonConvert.DeserializeObject<T>(textAsset.text);
                if (convert == null)
                {
                    Debug.LogWarning("Deserialization returned null, creating new object.");
                    return new T();
                }
                return convert;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error deserializing {resourcePath}: {e.Message}");
                return new T();
            }
        }
    }
}