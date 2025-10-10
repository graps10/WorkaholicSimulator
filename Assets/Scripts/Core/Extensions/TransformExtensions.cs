using UnityEngine;

namespace Core.Extensions
{
    public static class TransformExtensions
    {
        public static void ApplyPose(this Transform transform, Pose pose)
            => transform.SetPositionAndRotation(pose.position, pose.rotation);

        public static Pose GetPose(this Transform transform)
            => new Pose(transform.position, transform.rotation);
    }
}