//using Core.SceneControl;
using UnityEngine;


namespace Core.Utilities
{
    public static class BoundsUtilities
    {
        private static Color boundsLineColor = Color.yellow;
        
        public static Bounds GetValidBounds(GameObject gameObject)
        {
            if (UtilitiesProvider.TrySearchComponentInObject(gameObject, out Renderer renderer))
                return renderer.bounds;
            else if (UtilitiesProvider.TrySearchComponentInObject(gameObject, out Collider objectCollider))
                return objectCollider.bounds;
            else if (gameObject.transform.childCount == 0)
                return new Bounds(gameObject.transform.position, Vector3.one);
            else return default;
        }

        #if UNITY_EDITOR
        public static void DrawBounds(Bounds b, float delay = 0)
        {
            //if(SceneBaker.IsBaking) return;

            // bottom
            var p1 = new Vector3(b.min.x, b.min.y, b.min.z);
            var p2 = new Vector3(b.max.x, b.min.y, b.min.z);
            var p3 = new Vector3(b.max.x, b.min.y, b.max.z);
            var p4 = new Vector3(b.min.x, b.min.y, b.max.z);

            Debug.DrawLine(p1, p2, boundsLineColor, delay);
            Debug.DrawLine(p2, p3, boundsLineColor, delay);
            Debug.DrawLine(p3, p4, boundsLineColor, delay);
            Debug.DrawLine(p4, p1, boundsLineColor, delay);

            // top
            var p5 = new Vector3(b.min.x, b.max.y, b.min.z);
            var p6 = new Vector3(b.max.x, b.max.y, b.min.z);
            var p7 = new Vector3(b.max.x, b.max.y, b.max.z);
            var p8 = new Vector3(b.min.x, b.max.y, b.max.z);

            Debug.DrawLine(p5, p6, boundsLineColor, delay);
            Debug.DrawLine(p6, p7, boundsLineColor, delay);
            Debug.DrawLine(p7, p8, boundsLineColor, delay);
            Debug.DrawLine(p8, p5, boundsLineColor, delay);

            // sides
            Debug.DrawLine(p1, p5, boundsLineColor, delay);
            Debug.DrawLine(p2, p6, boundsLineColor, delay);
            Debug.DrawLine(p3, p7, boundsLineColor, delay);
            Debug.DrawLine(p4, p8, boundsLineColor, delay);
        }
        #endif
    }
}
