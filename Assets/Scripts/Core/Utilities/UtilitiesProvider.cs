using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
using System.Text;
using Entities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core.Utilities
{
    public static class UtilitiesProvider
    {
        #region Wait and run a function 

        public static Coroutine WaitAndRun(Action action, bool afterEndOfFrame = false, float timeInSeconds = 0.0f)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            if (Player.Instance == null)
                Debug.LogError("Can't run a coroutine inside editor, since Player.Instance works only in editor");

            return Player.Instance.StartCoroutine(WaitAndRunCoroutine(action, afterEndOfFrame, timeInSeconds));
        }
        
        private static IEnumerator WaitAndRunCoroutine(Action action, bool afterEndOfFrame, float timeInSeconds)
        {
            if (timeInSeconds > 0f)
                yield return new WaitForSeconds(timeInSeconds);

            if (afterEndOfFrame)
                yield return new WaitForEndOfFrame();

            action.Invoke();
        }

        #endregion
        
        public static GameObject LoadAndInstantiate(string assetPath, Vector3 positionToSet, Vector3? rotationToSet = null, Transform parent = null)
        {
            if (!assetPath.StartsWith("Prefabs/"))
                assetPath = "Prefabs/" + assetPath;

            // Try to load asset
            if (!AssetUtils.TryLoadAsset(assetPath, out GameObject loadedObject))
                return null;

            var instantiatedObject = Object.Instantiate(loadedObject, parent, false);

            // Set transform
            instantiatedObject.transform.position = positionToSet;
            if (rotationToSet.HasValue)
                instantiatedObject.transform.localRotation = Quaternion.Euler(rotationToSet.Value);

            return instantiatedObject;
        }

        #region Finding objects

        public static T SearchComponentInObject<T>(GameObject gameObject) where T : Component
        {
            T searchingComponent = null;

            searchingComponent = gameObject.GetComponent<T>();

            if (searchingComponent == null)
                searchingComponent = gameObject.GetComponentInParent<T>();

            if (searchingComponent == null)
                searchingComponent = gameObject.GetComponentInChildren<T>();

            return searchingComponent;
        }

        public static bool TrySearchComponentInObject<T>(GameObject gameObject, out T component) where T : Component
        {
            if (gameObject.TryGetComponent(out component)) return true;
            else
            {
                component = gameObject.GetComponentInChildren<T>(true);
                if (component != null) return true;

                component = gameObject.GetComponentInParent<T>(true);
            }
            return component != null;
        }

        public static Transform GetTransformFromPath(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                GameObject obj = GameObject.Find(path);
                if (obj)
                    return obj.transform;
            }
            return null;
        }

        public static string GetGameObjectPath(GameObject obj, int depth = 99)
        {
            var path = new StringBuilder(obj.name);
            Transform parent = obj.transform.parent;

            int index = 1;
            while (parent != null && index <= depth)
            {
                path.Insert(0, parent.name + "/");
                parent = parent.parent;
                index++;
            }
            return path.ToString();
        }

        #endregion

        #region UnityEvent

        public static void ForceAddListener(ref UnityEvent unityEvent, UnityAction action)
        {
            if (unityEvent == null)
                unityEvent = new UnityEvent();

            unityEvent.AddListener(action);
        }

        public static void ForceAddListener<T>(ref UnityEvent<T> unityEvent, UnityAction<T> action)
        {
            if (unityEvent == null)
                unityEvent = new UnityEvent<T>();

            unityEvent.AddListener(action);
        }

        #endregion

#if UNITY_EDITOR
        /// <param name="target">Target of the editor. Will be set dirty if field value changed.</param>
        public static void TransformPathField(string label, SerializedProperty transformPathProperty, Object target, Rect rect = default)
        {
            Transform elementTransform = GetTransformFromPath(transformPathProperty.stringValue);

            Transform newTransform;

            if (rect != default)
                newTransform = (Transform)EditorGUI.ObjectField(rect, label, elementTransform, typeof(Transform), true);
            else
                newTransform = (Transform)EditorGUILayout.ObjectField(label, elementTransform, typeof(Transform), true);

            if (newTransform != elementTransform)
            {
                if (newTransform != null)
                    transformPathProperty.stringValue = GetGameObjectPath(newTransform.gameObject);
                else
                    transformPathProperty.stringValue = null;
                
                if(target != null)
                    EditorUtility.SetDirty(target);
            }
        }
#endif

        //public static int GetIndex(this UnityLayers layer) => (int)Mathf.Log((int)layer, 2);

        public static bool IsConstantZero(this AnimationCurve curve)
        {
            if (curve == null || curve.length == 0) return true;

            foreach (var key in curve.keys)
            {
                if (key.value != 0f || key.inTangent != 0f || key.outTangent != 0f)
                {
                    return false;
                }
            }
            return true;
        }

        private static readonly List<string> transformsWithChangedState = new();

        public static void StateSetChanged(this Entity actor)
        {
            var transformPath = GetGameObjectPath(actor.transform.parent.gameObject);

            if (!transformsWithChangedState.Contains(transformPath)) 
                transformsWithChangedState.Add(transformPath);
        }

        public static void ToggleStateChanged(this Entity entity)
        {
            var transformPath = GetGameObjectPath(entity.transform.parent.gameObject);

            if (!transformsWithChangedState.Contains(transformPath))
                transformsWithChangedState.Add(transformPath);
            else
                transformsWithChangedState.Remove(transformPath);
        }

        public static bool IsStateChanged(this Entity entity)
        {
            var transformPath = GetGameObjectPath(entity.transform.parent.gameObject);
            return transformsWithChangedState.Contains(transformPath);
        }

        public static Vector2 Clamp(this Vector2 vector, Vector2 min, Vector2 max) 
            => Clamp((Vector3)vector, (Vector3)min, (Vector3)max);

        public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
        {
            if(vector.x > max.x) vector.x = max.x; 
            else if(vector.x < min.x) vector.x = min.x;

            if (vector.y > max.y) vector.y = max.y;
            else if (vector.y < min.y) vector.y = min.y;

            if (vector.z > max.z) vector.z = max.z;
            else if (vector.z < min.z) vector.z = min.z;

            return vector;
        }

        [Tooltip("Searching starting from parent object")]
        public static T GetComponentInParentWithDepth<T>(Transform transform, int depth) where T : MonoBehaviour
        {
            int currentDepth = 0;
            Transform currentParent = transform;
            T searchComponent = null;

            do
            {
                currentParent = transform.parent;

                if(currentParent == null)
                {
                    Debug.LogWarning("No parent and can't find");
                    return null;
                }

                searchComponent = currentParent.GetComponent<T>();
                currentDepth++;
            
            } while (searchComponent == null || currentDepth == depth);

            return searchComponent;
        }


        /*public static void PlayFromCamera(this Sound sound)
        {
            if (Camera.main == null) return;

            sound.SetPosition(Camera.main.transform.position + Camera.main.transform.forward);
            sound.Play();
        }*/
    }

    [Flags]
    public enum Axis
    {
        X = 1,
        Y = 2,
        Z = 4
    }
}