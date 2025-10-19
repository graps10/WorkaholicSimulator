using UnityEngine;

namespace Entities.EditorSceneElements
{
    [ExecuteInEditMode]
    public class EditorSceneElement : MonoBehaviour
    {
#if UNITY_EDITOR
        private Vector3 _initialPosition;

        protected virtual void Start()
        {
            // Saving start position of object while creating or loading
            _initialPosition = transform.localPosition;
        }

        protected virtual void Update()
        {
            // If object move, keep him back on last position
            if (transform.localPosition != _initialPosition)
                transform.localPosition = _initialPosition;
        }
#endif
    }
}