using System.Collections.Generic;
using UnityEngine;

namespace Core.Utilities
{
    public static class MeshUtils
    {
        public static Bounds TransformBounds(Bounds localBounds, Transform transform)
        {
            Vector3 center = transform.TransformPoint(localBounds.center);
            Vector3 extents = localBounds.extents;
            Vector3 axisX = transform.TransformVector(extents.x, 0, 0);
            Vector3 axisY = transform.TransformVector(0, extents.y, 0);
            Vector3 axisZ = transform.TransformVector(0, 0, extents.z);

            extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
            extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
            extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

            return new Bounds(center, extents * 2);
        }

        #region Mesh Extensions

        public static bool IsValid(this Mesh mesh)
        {
            foreach (var vertex in mesh.vertices)
                if (vertex.IsNaN()) return false;

            return true;
        }

        #endregion
        
        #region Creating Meshes

        public static Mesh CreateCombinedMeshFromBounds(List<Bounds> boundsArray, Transform parent = null)
        {
            Mesh combinedMesh = new Mesh();
            //8 points are needed to create one cube and each object needs a separate cube
            Vector3[] vertices = new Vector3[boundsArray.Count * 8];

            //it connects elements from the vertex array and creates a triangle polygon.
            //Therefore, we need 36, each side of the cube consists of two triangles,
            //each triangle consists of three points, which comes out to 36 = 3 * 2 *6
            int[] triangles = new int[boundsArray.Count * 36];

            //a numbers that separates each Bounds so that we do not overwrite data already written
            int vertexOffset = 0;
            int triangleOffset = 0;

            foreach (Bounds bounds in boundsArray)
            {
                Vector3[] boundsVertices = GetVerticesFromBounds(bounds, parent);
                int[] boundsTriangles = GetTrianglesForBox(vertexOffset);
                for (int i = 0; i < boundsVertices.Length; i++)
                {
                    vertices[vertexOffset + i] = boundsVertices[i];
                }
                for (int i = 0; i < boundsTriangles.Length; i++)
                {
                    triangles[triangleOffset + i] = boundsTriangles[i];
                }
                vertexOffset += 8; // 8 vertices per bounds
                triangleOffset += 36; // 36 indices per bounds
            }

            combinedMesh.vertices = vertices;
            combinedMesh.triangles = triangles;
            combinedMesh.RecalculateNormals();

            return combinedMesh;
        }

        public static Mesh CreateOneBigCubeCollider(List<Bounds> boundsArray, Transform parent = null)
        {
            if (boundsArray == null || boundsArray.Count == 0)
            {
                Debug.LogWarning("boundsArray is null or empty. Returning empty mesh.");
                return new Mesh();
            }
            
            Mesh combinedMesh = new Mesh();
            int totalVertices = 8;
            int totalTriangles = 36;
            Vector3[] vertices = new Vector3[totalVertices];
            int[] triangles = new int[totalTriangles];
            int vertexOffset = 0;
            int triangleOffset = 0;

            List<float> minX = new();
            List<float> minY = new();
            List<float> minZ = new();

            List<float> maxX = new();
            List<float> maxY = new();
            List<float> maxZ = new();

            foreach (var item in boundsArray)
            {
                minX.Add(item.min.x);
                minY.Add(item.min.y);
                minZ.Add(item.min.z);

                maxX.Add(item.max.x);
                maxY.Add(item.max.y);
                maxZ.Add(item.max.z);
            }

            Vector3 minVector = new Vector3(MathUtils.FindBiggestValue(minX.ToArray()), 
                MathUtils.FindBiggestValue(minY.ToArray()), MathUtils.FindBiggestValue(minZ.ToArray()));

            Vector3 maxVector = new Vector3(MathUtils.FindSmallestValue(maxX.ToArray()), 
                MathUtils.FindSmallestValue(maxY.ToArray()), MathUtils.FindSmallestValue(maxZ.ToArray()));

            Vector3[] boundsVertices = GetVerticesFromBounds(minVector, maxVector, parent);

            int[] boundsTriangles = GetTrianglesForBox(vertexOffset);

            for (int i = 0; i < boundsVertices.Length; i++)
            {
                vertices[vertexOffset + i] = boundsVertices[i];
            }
            
            for (int i = 0; i < boundsTriangles.Length; i++)
            {
                triangles[triangleOffset + i] = boundsTriangles[i];
            }
            
            combinedMesh.vertices = vertices;
            combinedMesh.triangles = triangles;
            combinedMesh.RecalculateNormals();

            return combinedMesh;
        }

        public static Mesh CreateOneBigCubeCollider(List<Bounds> boundsArray, Vector3 scale, Vector3 offset, Transform parent = null)
        {
            if (boundsArray == null || boundsArray.Count == 0)
            {
                Debug.LogWarning("boundsArray is null or empty. Returning empty mesh.");
                return new Mesh();
            }
            
            Mesh combinedMesh = new Mesh();
            int totalVertices = 8;
            int totalTriangles = 36;
            Vector3[] vertices = new Vector3[totalVertices];
            int[] triangles = new int[totalTriangles];
            int vertexOffset = 0;
            int triangleOffset = 0;

            List<float> minX = new();
            List<float> minY = new();
            List<float> minZ = new();

            List<float> maxX = new();
            List<float> maxY = new();
            List<float> maxZ = new();

            foreach (var item in boundsArray)
            {
                minX.Add(item.min.x);
                minY.Add(item.min.y);
                minZ.Add(item.min.z);

                maxX.Add(item.max.x);
                maxY.Add(item.max.y);
                maxZ.Add(item.max.z);
            }

            Vector3 minVector = new Vector3(MathUtils.FindBiggestValue(minX.ToArray()), 
                MathUtils.FindBiggestValue(minY.ToArray()), MathUtils.FindBiggestValue(minZ.ToArray()));

            Vector3 maxVector = new Vector3(MathUtils.FindSmallestValue(maxX.ToArray()), 
                MathUtils.FindSmallestValue(maxY.ToArray()), MathUtils.FindSmallestValue(maxZ.ToArray()));

            minVector -= parent.position;
            maxVector -= parent.position;

            minVector.x *= scale.x;
            minVector.y *= scale.y;
            minVector.z *= scale.z;

            maxVector.x *= scale.x;
            maxVector.y *= scale.y;
            maxVector.z *= scale.z;

            minVector.x += offset.x;
            minVector.y += offset.y;
            minVector.z += offset.z;

            maxVector.x += offset.x;
            maxVector.y += offset.y;
            maxVector.z += offset.z;


            Vector3[] boundsVertices = GetVerticesFromBounds(minVector, maxVector);

            int[] boundsTriangles = GetTrianglesForBox(vertexOffset);

            for (int i = 0; i < boundsVertices.Length; i++)
            {
                vertices[vertexOffset + i] = boundsVertices[i];
            }
            
            for (int i = 0; i < boundsTriangles.Length; i++)
            {
                triangles[triangleOffset + i] = boundsTriangles[i];
            }
            
            combinedMesh.vertices = vertices;
            combinedMesh.triangles = triangles;
            combinedMesh.RecalculateNormals();

            return combinedMesh;
        }
        
        private static Vector3[] CreateVertices(Vector3 min, Vector3 max)
        {
            Vector3[] vertices = new Vector3[8];

            vertices[0] = new Vector3(min.x, min.y, min.z);
            vertices[1] = new Vector3(max.x, min.y, min.z);
            vertices[2] = new Vector3(max.x, max.y, min.z);
            vertices[3] = new Vector3(min.x, max.y, min.z);
            vertices[4] = new Vector3(min.x, min.y, max.z);
            vertices[5] = new Vector3(max.x, min.y, max.z);
            vertices[6] = new Vector3(max.x, max.y, max.z);
            vertices[7] = new Vector3(min.x, max.y, max.z);
            return vertices;

        }

        #endregion
        
        private static Vector3[] GetVerticesFromBounds(Bounds bounds, Transform parent = null)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            return GetVerticesFromBounds(min, max, parent);
        }

        private static Vector3[] GetVerticesFromBounds(Vector3 min, Vector3 max, Transform parent = null, float minimalSize = 1)
        {
            if (parent != null)
            {
                min -= parent.position;
                max -= parent.position; 
            }

            CheckVertexSize(ref min, ref max, minimalSize);

            return CreateVertices(min, max);
        }

        private static void CheckVertexSize(ref Vector3 vertexMin, ref Vector3 vertexMax, float minimalVertexSize = 1)
        {
            if (Mathf.Abs(vertexMin.x - vertexMax.x) < minimalVertexSize)
            {
                vertexMin.x -= minimalVertexSize / 2;
                vertexMax.x += minimalVertexSize / 2;
            }
            if (Mathf.Abs(vertexMin.y - vertexMax.y) < minimalVertexSize)
            {
                vertexMin.y -= minimalVertexSize / 2;
                vertexMax.y += minimalVertexSize / 2;
            }
            if (Mathf.Abs(vertexMin.z - vertexMax.z) < minimalVertexSize)
            {
                vertexMin.z -= minimalVertexSize / 2;
                vertexMax.z += minimalVertexSize / 2;
            }
        }
        
        private static int[] GetTrianglesForBox(int vertexOffset)
        {
            int[] triangles = new int[36];
            // Front
            triangles[0] = vertexOffset + 0; triangles[1] = vertexOffset + 2; triangles[2] = vertexOffset + 1;
            triangles[3] = vertexOffset + 0; triangles[4] = vertexOffset + 3; triangles[5] = vertexOffset + 2;
            // Back
            triangles[6] = vertexOffset + 4; triangles[7] = vertexOffset + 5; triangles[8] = vertexOffset + 6;
            triangles[9] = vertexOffset + 4; triangles[10] = vertexOffset + 6; triangles[11] = vertexOffset + 7;
            // Left
            triangles[12] = vertexOffset + 0; triangles[13] = vertexOffset + 7; triangles[14] = vertexOffset + 3;
            triangles[15] = vertexOffset + 0; triangles[16] = vertexOffset + 4; triangles[17] = vertexOffset + 7;
            // Right
            triangles[18] = vertexOffset + 1; triangles[19] = vertexOffset + 2; triangles[20] = vertexOffset + 6;
            triangles[21] = vertexOffset + 1; triangles[22] = vertexOffset + 6; triangles[23] = vertexOffset + 5;
            // Top
            triangles[24] = vertexOffset + 2; triangles[25] = vertexOffset + 3; triangles[26] = vertexOffset + 7;
            triangles[27] = vertexOffset + 2; triangles[28] = vertexOffset + 7; triangles[29] = vertexOffset + 6;
            // Bottom
            triangles[30] = vertexOffset + 0; triangles[31] = vertexOffset + 5; triangles[32] = vertexOffset + 4;
            triangles[33] = vertexOffset + 0; triangles[34] = vertexOffset + 1; triangles[35] = vertexOffset + 5;
            return triangles;
        }
    }
}