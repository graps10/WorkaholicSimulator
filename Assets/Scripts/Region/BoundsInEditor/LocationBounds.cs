using Core.Utilities;
using UnityEngine;

namespace Region.BoundsInEditor
{
    public class LocationBounds : BoundsSceneElement
    {
        public override void CreateMeshBounds()
        {
#if UNITY_EDITOR
            myMeshFilter.mesh = MeshUtils.CreateOneBigCubeCollider(
                myLocation.GetAllBounds(), 
                transform
            );
#endif
        }
    }
}