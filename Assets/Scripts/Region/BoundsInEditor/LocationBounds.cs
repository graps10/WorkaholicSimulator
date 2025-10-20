using Core.Utilities;
using UnityEngine;

namespace Region.BoundsInEditor
{
    public class LocationBounds : BoundsSceneElement
    {
        private static Vector3 meshDefaultScaleBounds = Vector3.zero;
        private static Vector3 meshDefaultOffsetBounds = Vector3.zero;
        
        [SerializeField] protected bool isMovingInEditor;
        
        public override void CreateMeshBounds()
        {
#if UNITY_EDITOR
            myMeshFilter.mesh = MeshUtils.CreateOneBigCubeCollider(
                myLocation.GetAllBounds(), 
                meshDefaultScaleBounds, 
                meshDefaultOffsetBounds, 
                transform
            );
#endif
        }

#if UNITY_EDITOR
        
        protected override void Update()
        {
            if (!isMovingInEditor)
                base.Update();
        }
        
        [ContextMenu("CreateFirstBounds")]
        protected void CreateFirstBoundsContextMenu()
        {
            CreateMeshBounds();
        }
        
#endif
    }
}