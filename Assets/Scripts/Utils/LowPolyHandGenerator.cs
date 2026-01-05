using UnityEngine;

namespace VRDungeonCrawler.Utils
{
    /// <summary>
    /// Generates low-poly hand geometry with flat shading
    /// Simple blocky hand with palm and 5 fingers
    /// </summary>
    public static class LowPolyHandGenerator
    {
        /// <summary>
        /// Creates a low-poly hand mesh (left or right)
        /// </summary>
        public static Mesh CreateHandMesh(bool isLeftHand)
        {
            Mesh mesh = new Mesh();
            mesh.name = isLeftHand ? "LeftHand" : "RightHand";

            // We'll build the hand from boxes:
            // 1. Palm (flat rectangle)
            // 2. 5 fingers (3 segments each, progressively smaller)
            // 3. Thumb (2 segments, at angle)

            var vertices = new System.Collections.Generic.List<Vector3>();
            var triangles = new System.Collections.Generic.List<int>();
            var normals = new System.Collections.Generic.List<Vector3>();

            // POLYTOPIA STYLE: Chunky, square palm
            float handMirror = isLeftHand ? -1f : 1f;
            AddBox(vertices, triangles, normals,
                Vector3.zero,
                new Vector3(0.05f * handMirror, 0.05f, 0.018f), // More square, thicker
                Quaternion.identity);

            // Finger positions (relative to palm top center)
            Vector3 palmTop = new Vector3(0, 0.05f, 0);

            // POLYTOPIA STYLE: Stubby, chunky fingers with minimal length variation
            // Index finger - chunky
            Vector3 indexBase = palmTop + new Vector3(-0.024f * handMirror, 0, 0);
            AddFinger(vertices, triangles, normals, indexBase, handMirror, 0.032f, 0.015f);

            // Middle finger - slightly longer
            Vector3 middleBase = palmTop + new Vector3(-0.008f * handMirror, 0, 0);
            AddFinger(vertices, triangles, normals, middleBase, handMirror, 0.035f, 0.015f);

            // Ring finger - chunky
            Vector3 ringBase = palmTop + new Vector3(0.008f * handMirror, 0, 0);
            AddFinger(vertices, triangles, normals, ringBase, handMirror, 0.032f, 0.014f);

            // Pinky finger - still stubby but slightly smaller
            Vector3 pinkyBase = palmTop + new Vector3(0.024f * handMirror, 0, 0);
            AddFinger(vertices, triangles, normals, pinkyBase, handMirror, 0.028f, 0.013f);

            // Thumb - very chunky, Polytopia-style
            Vector3 thumbBase = new Vector3(-0.042f * handMirror, -0.015f, 0.014f);
            Quaternion thumbRotation = Quaternion.Euler(0, 0, -45f * handMirror);
            AddThumb(vertices, triangles, normals, thumbBase, handMirror, thumbRotation);

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
            mesh.RecalculateBounds();

            return mesh;
        }

        private static void AddFinger(System.Collections.Generic.List<Vector3> vertices,
                                      System.Collections.Generic.List<int> triangles,
                                      System.Collections.Generic.List<Vector3> normals,
                                      Vector3 basePos, float handMirror, float length, float thickness)
        {
            // 3 segments per finger
            float segmentLength = length / 3f;
            Vector3 currentPos = basePos;

            for (int i = 0; i < 3; i++)
            {
                // POLYTOPIA STYLE: Minimal taper - fingers stay chunky!
                float currentThickness = thickness * (1f - i * 0.04f); // Very slight taper
                Vector3 size = new Vector3(currentThickness * handMirror, segmentLength, currentThickness);

                AddBox(vertices, triangles, normals,
                    currentPos + new Vector3(0, segmentLength / 2f, 0),
                    size,
                    Quaternion.identity);

                currentPos += new Vector3(0, segmentLength, 0);
            }
        }

        private static void AddThumb(System.Collections.Generic.List<Vector3> vertices,
                                     System.Collections.Generic.List<int> triangles,
                                     System.Collections.Generic.List<Vector3> normals,
                                     Vector3 basePos, float handMirror, Quaternion rotation)
        {
            // POLYTOPIA STYLE: Extra chunky thumb, shorter segments
            float segmentLength = 0.024f; // Shorter = stubbier
            float thickness = 0.018f; // Extra thick

            for (int i = 0; i < 2; i++)
            {
                // Almost no taper - thumb stays thick!
                float currentThickness = thickness * (1f - i * 0.03f);
                Vector3 offset = rotation * new Vector3(0, segmentLength / 2f + i * segmentLength, 0);
                Vector3 size = new Vector3(currentThickness * handMirror, segmentLength, currentThickness);

                AddBox(vertices, triangles, normals,
                    basePos + offset,
                    size,
                    rotation);
            }
        }

        /// <summary>
        /// Adds a box to the mesh (8 vertices, 12 triangles)
        /// For flat shading, we duplicate vertices at each face
        /// </summary>
        private static void AddBox(System.Collections.Generic.List<Vector3> vertices,
                                   System.Collections.Generic.List<int> triangles,
                                   System.Collections.Generic.List<Vector3> normals,
                                   Vector3 center, Vector3 size, Quaternion rotation)
        {
            // For flat shading, each face needs unique vertices
            // 6 faces × 4 vertices = 24 vertices per box

            Vector3 halfSize = size / 2f;
            int startIndex = vertices.Count;

            // Front face (+Z)
            AddQuad(vertices, triangles, normals, center, rotation,
                new Vector3(-halfSize.x, -halfSize.y, halfSize.z),
                new Vector3(halfSize.x, -halfSize.y, halfSize.z),
                new Vector3(halfSize.x, halfSize.y, halfSize.z),
                new Vector3(-halfSize.x, halfSize.y, halfSize.z),
                Vector3.forward);

            // Back face (-Z)
            AddQuad(vertices, triangles, normals, center, rotation,
                new Vector3(halfSize.x, -halfSize.y, -halfSize.z),
                new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
                new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
                new Vector3(halfSize.x, halfSize.y, -halfSize.z),
                Vector3.back);

            // Top face (+Y)
            AddQuad(vertices, triangles, normals, center, rotation,
                new Vector3(-halfSize.x, halfSize.y, halfSize.z),
                new Vector3(halfSize.x, halfSize.y, halfSize.z),
                new Vector3(halfSize.x, halfSize.y, -halfSize.z),
                new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
                Vector3.up);

            // Bottom face (-Y)
            AddQuad(vertices, triangles, normals, center, rotation,
                new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
                new Vector3(halfSize.x, -halfSize.y, -halfSize.z),
                new Vector3(halfSize.x, -halfSize.y, halfSize.z),
                new Vector3(-halfSize.x, -halfSize.y, halfSize.z),
                Vector3.down);

            // Right face (+X)
            AddQuad(vertices, triangles, normals, center, rotation,
                new Vector3(halfSize.x, -halfSize.y, halfSize.z),
                new Vector3(halfSize.x, -halfSize.y, -halfSize.z),
                new Vector3(halfSize.x, halfSize.y, -halfSize.z),
                new Vector3(halfSize.x, halfSize.y, halfSize.z),
                Vector3.right);

            // Left face (-X)
            AddQuad(vertices, triangles, normals, center, rotation,
                new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
                new Vector3(-halfSize.x, -halfSize.y, halfSize.z),
                new Vector3(-halfSize.x, halfSize.y, halfSize.z),
                new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
                Vector3.left);
        }

        private static void AddQuad(System.Collections.Generic.List<Vector3> vertices,
                                    System.Collections.Generic.List<int> triangles,
                                    System.Collections.Generic.List<Vector3> normals,
                                    Vector3 center, Quaternion rotation,
                                    Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3,
                                    Vector3 normal)
        {
            int startIndex = vertices.Count;

            // Add 4 vertices
            vertices.Add(center + rotation * v0);
            vertices.Add(center + rotation * v1);
            vertices.Add(center + rotation * v2);
            vertices.Add(center + rotation * v3);

            // Add same normal for all 4 vertices (flat shading)
            Vector3 rotatedNormal = rotation * normal;
            normals.Add(rotatedNormal);
            normals.Add(rotatedNormal);
            normals.Add(rotatedNormal);
            normals.Add(rotatedNormal);

            // Add 2 triangles (6 indices)
            triangles.Add(startIndex + 0);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 1);

            triangles.Add(startIndex + 0);
            triangles.Add(startIndex + 3);
            triangles.Add(startIndex + 2);
        }

        /// <summary>
        /// Creates a material for the hand with flat shading
        /// POLYTOPIA STYLE: Matte finish, vibrant colors
        /// </summary>
        public static Material CreateHandMaterial(Color skinColor)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = skinColor;
            mat.SetFloat("_Smoothness", 0.1f); // Very matte for Polytopia look
            mat.SetFloat("_Metallic", 0f); // No metallic shine

            // Note: Unity automatically uses flat shading when normals are per-face
            // (which we create by duplicating vertices)

            return mat;
        }

        /// <summary>
        /// Polytopia-style color presets (pastel/vibrant team colors)
        /// </summary>
        public static Color GetPolytopiaColor(int colorIndex)
        {
            Color[] polytopiaColors = new Color[]
            {
                new Color(1f, 0.85f, 0.7f),      // 0: Peachy skin (default)
                new Color(0.8f, 0.6f, 0.5f),     // 1: Tan skin
                new Color(0.95f, 0.75f, 0.65f),  // 2: Light pink
                new Color(0.6f, 0.45f, 0.35f),   // 3: Brown skin
                new Color(0.7f, 0.85f, 1f),      // 4: Ice blue (tribe color)
                new Color(1f, 0.7f, 0.75f),      // 5: Soft pink (tribe color)
                new Color(0.75f, 1f, 0.7f),      // 6: Mint green (tribe color)
                new Color(1f, 0.9f, 0.6f),       // 7: Golden yellow (tribe color)
            };

            return polytopiaColors[colorIndex % polytopiaColors.Length];
        }
    }
}
