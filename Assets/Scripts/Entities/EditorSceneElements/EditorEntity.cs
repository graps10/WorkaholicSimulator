using System.Collections;
using Entities.Constructors;
using Region;
using UnityEditor;
using UnityEngine;

namespace Entities.EditorSceneElements
{
    [ExecuteInEditMode]
    public sealed class EditorEntity : EditorSceneElement
    {
        private const float Check_Location_Delay = 3f;
        
        [SerializeField] private Location entityLocation;

        private void OnEnable()
        {
        
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                DestroyImmediate(gameObject);
                return;
            }
            
            EditorEntityConstructor.Instance.AddExistingElement(entityLocation, gameObject);

            StartCoroutine(CheckLocation());
#else
            Destroy(gameObject);
#endif
        }

#if UNITY_EDITOR

        public void Initialize(Location location)
        {
            entityLocation = location;
            
            if(entityLocation == null)
                DestroyImmediate(gameObject);
        }

        private IEnumerator CheckLocation()
        {
            yield return new WaitForSecondsRealtime(Check_Location_Delay);
            
            if(EditorApplication.isPlaying || EditorApplication.isCompiling || this == null || gameObject == null)
                yield break;
                
            if(entityLocation == null)
                DestroyImmediate(gameObject);
        }
#endif
    }
}