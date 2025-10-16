using Core.Utilities;
using UnityEngine;

namespace Region.BoundsInEditor
{
    public class LocationBounds : BoundsSceneElement
    {
        [SerializeField] private bool isMovingInEditor;
        [SerializeField] private Vector3 scaleBounds = Vector3.one;
        [SerializeField] private Vector3 offsetPosition = Vector3.zero;
        
        public override void CreateMeshBounds()
        {
#if UNITY_EDITOR
            myMeshFilter.mesh = MeshUtils.CreateOneBigCubeCollider(
                myLocation.GetAllBounds(), 
                scaleBounds, 
                offsetPosition, 
                transform
            );
#endif
        }

#if UNITY_EDITOR
        [ContextMenu("CreateFirstBounds")]
        private void CreateFirstBoundsContextMenu()
        {
            CreateMeshBounds();
        }

        protected override void Update()
        {
            if (!isMovingInEditor)
                base.Update();
        }
#endif
    }
}