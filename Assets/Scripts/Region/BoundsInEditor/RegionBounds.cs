using Core.Utilities;

namespace Region.BoundsInEditor
{
    public class RegionBounds: BoundsSceneElement
    {
        public override void CreateMeshBounds()
        {
#if UNITY_EDITOR
            myMeshFilter.mesh = MeshUtils.CreateCombinedMeshFromBounds(myLocation.GetAllBounds(), transform);
#endif
        }
    }
}