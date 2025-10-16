using Core.Utilities;

namespace Region.BoundsInEditor
{
    public class SectorBounds: LocationBounds
    {
        public override void CreateMeshBounds()
        {
#if UNITY_EDITOR
            myMeshFilter.mesh = MeshUtils.CreateOneBigCubeCollider(myLocation.GetAllBounds(), transform);
#endif
        }
    }
}