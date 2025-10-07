#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Core.Utilities;

namespace Core.ObjectPool
{
    [CustomEditor(typeof(PrefabPoolInfo))]
    public class PoolInfoEditor : Editor
    {
        private SerializedProperty _objectPathProperty;

        private void OnEnable()
        {
            string fieldName = GetBackingFieldName("ObjectPath");
            _objectPathProperty = serializedObject.FindProperty(fieldName);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            if (GUILayout.Button("Go to prefab"))
                OpenPrefab(_objectPathProperty.stringValue);

            serializedObject.ApplyModifiedProperties();
        }

        private void OpenPrefab(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("ObjectPath is not set.");
                return;
            }

            if (AssetUtils.TryLoadAsset(path, out GameObject prefab))
                AssetDatabase.OpenAsset(prefab);
            else
                Debug.LogError($"Prefab not found along path: {path}. Make sure the path is correct.");
        }

        private static string GetBackingFieldName(string name)
        {
            return $"<{name}>k__BackingField";
        }
    }
}
#endif