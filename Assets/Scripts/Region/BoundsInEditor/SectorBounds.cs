using Core.Utilities;

namespace Region.BoundsInEditor
{
    public class SectorBounds: BoundsSceneElement
    {
        public override void CreateMeshBounds()
        {
#if UNITY_EDITOR
            myMeshFilter.mesh = MeshUtils.CreateOneBigCubeCollider(myLocation.GetAllBounds(), transform);
#endif
        }
    }
}