using UnityEngine;

namespace VRDungeonCrawler.Utils
{
    /// <summary>
    /// TRUE Polytopia-style hands: Super simple, blocky, stylized
    /// Generates articulated hand with separate bone GameObjects for finger tracking
    /// Much simpler than the old generator - pure Polytopia aesthetic!
    /// </summary>
    public static class PolytopiaHandGenerator
    {
        /// <summary>
        /// Creates a fully articulated Polytopia hand with finger bones
        /// Ready for XR hand tracking animation
        /// </summary>
        public static GameObject CreateArticulatedHand(Transform parent, bool isLeftHand)
        {
            // Root hand object
            GameObject handRoot = new GameObject(isLeftHand ? "PolytopiaHand_L" : "PolytopiaHand_R");
            handRoot.transform.SetParent(parent);
            handRoot.transform.localPosition = Vector3.zero;
            handRoot.transform.localRotation = Quaternion.identity; // No rotation - proper orientation!
            handRoot.transform.localScale = Vector3.one;

            float handMirror = isLeftHand ? 1f : -1f;

            // Create palm (single chunky box)
            GameObject palm = CreatePalmObject(handRoot.transform, handMirror);

            // Create 5 articulated fingers with more realistic positioning and angles

            // Thumb - opposable orientation for grasping
            // Rotated perpendicular to other fingers, positioned for opposition
            CreateArticulatedFinger(palm.transform, "Thumb", new Vector3(0.030f * handMirror, -0.010f, 0.005f),
                Quaternion.Euler(0, 60f * handMirror, 80f * handMirror), 2, 0.016f, 0.024f, handMirror);

            // Index - straight forward, slightly angled outward
            CreateArticulatedFinger(palm.transform, "Index", new Vector3(0.020f * handMirror, 0, 0.048f),
                Quaternion.Euler(0, 8f * handMirror, 0), 3, 0.011f, 0.032f, handMirror);

            // Middle - longest, straight forward
            CreateArticulatedFinger(palm.transform, "Middle", new Vector3(0.005f * handMirror, 0, 0.052f),
                Quaternion.identity, 3, 0.011f, 0.036f, handMirror);

            // Ring - slightly shorter, angled inward
            CreateArticulatedFinger(palm.transform, "Ring", new Vector3(-0.010f * handMirror, 0, 0.050f),
                Quaternion.Euler(0, -5f * handMirror, 0), 3, 0.010f, 0.033f, handMirror);

            // Pinky - shortest, angled inward more
            CreateArticulatedFinger(palm.transform, "Pinky", new Vector3(-0.025f * handMirror, 0, 0.044f),
                Quaternion.Euler(0, -10f * handMirror, 0), 3, 0.009f, 0.028f, handMirror);

            return handRoot;
        }

        private static GameObject CreatePalmObject(Transform parent, float handMirror)
        {
            GameObject palm = new GameObject("Palm");
            palm.transform.SetParent(parent);
            palm.transform.localPosition = Vector3.zero;
            palm.transform.localRotation = Quaternion.identity;

            MeshFilter mf = palm.AddComponent<MeshFilter>();
            MeshRenderer mr = palm.AddComponent<MeshRenderer>();

            // More realistic palm shape - wider at knuckles, narrower at wrist
            mf.mesh = CreateSimpleBox(new Vector3(0.05f * handMirror, 0.025f, 0.045f)); // Taller, wider
            mr.material = CreatePolytopiaHandMaterial();

            return palm;
        }

        private static void CreateArticulatedFinger(Transform parent, string fingerName, Vector3 basePos,
            Quaternion baseRotation, int segments, float thickness, float totalLength, float handMirror)
        {
            float segmentLength = totalLength / segments;
            Transform currentParent = parent;
            Vector3 currentLocalPos = basePos;

            for (int i = 0; i < segments; i++)
            {
                GameObject segment = new GameObject($"{fingerName}_Segment{i}");
                segment.transform.SetParent(currentParent);
                segment.transform.localPosition = currentLocalPos;
                segment.transform.localRotation = i == 0 ? baseRotation : Quaternion.identity;

                MeshFilter mf = segment.AddComponent<MeshFilter>();
                MeshRenderer mr = segment.AddComponent<MeshRenderer>();

                // Polytopia style: minimal taper, chunky segments
                float currentThickness = thickness * (1f - i * 0.03f);
                mf.mesh = CreateSimpleBox(new Vector3(currentThickness * Mathf.Abs(handMirror), currentThickness, segmentLength));
                mr.material = CreatePolytopiaHandMaterial();

                // Next segment attaches to the tip of current segment
                currentParent = segment.transform;
                currentLocalPos = new Vector3(0, 0, segmentLength);
            }
        }

        /// <summary>
        /// Creates a super simple box mesh - TRUE Polytopia style!
        /// Much simpler than the old generator - fewer vertices, cleaner
        /// </summary>
        private static Mesh CreateSimpleBox(Vector3 size)
        {
            Mesh mesh = new Mesh();
            mesh.name = "PolytopiaBox";

            Vector3 halfSize = size / 2f;

            // Simple box: 8 vertices, shared normals for FLAT SHADING EFFECT
            Vector3[] vertices = new Vector3[]
            {
                // Bottom 4
                new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
                new Vector3(halfSize.x, -halfSize.y, -halfSize.z),
                new Vector3(halfSize.x, -halfSize.y, halfSize.z),
                new Vector3(-halfSize.x, -halfSize.y, halfSize.z),
                // Top 4
                new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
                new Vector3(halfSize.x, halfSize.y, -halfSize.z),
                new Vector3(halfSize.x, halfSize.y, halfSize.z),
                new Vector3(-halfSize.x, halfSize.y, halfSize.z),
            };

            // 12 triangles (6 faces × 2 triangles)
            int[] triangles = new int[]
            {
                // Bottom
                0, 2, 1, 0, 3, 2,
                // Top
                4, 5, 6, 4, 6, 7,
                // Front
                3, 6, 2, 3, 7, 6,
                // Back
                0, 1, 5, 0, 5, 4,
                // Left
                0, 7, 3, 0, 4, 7,
                // Right
                1, 2, 6, 1, 6, 5,
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals(); // Unity handles flat shading automatically
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// Creates TRUE Polytopia material - super matte, vibrant color
        /// </summary>
        public static Material CreatePolytopiaHandMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

            // Polytopia-style peachy skin
            mat.color = new Color(1f, 0.82f, 0.68f);

            // SUPER MATTE - no shine at all!
            mat.SetFloat("_Smoothness", 0f);
            mat.SetFloat("_Metallic", 0f);

            return mat;
        }

        /// <summary>
        /// Get Polytopia color by index
        /// </summary>
        public static Color GetPolytopiaColor(int colorIndex)
        {
            Color[] colors = new Color[]
            {
                new Color(1f, 0.82f, 0.68f),     // 0: Peachy
                new Color(0.8f, 0.6f, 0.5f),     // 1: Tan
                new Color(0.6f, 0.45f, 0.35f),   // 2: Brown
                new Color(0.7f, 0.85f, 1f),      // 3: Ice blue
                new Color(1f, 0.7f, 0.75f),      // 4: Pink
                new Color(0.75f, 1f, 0.7f),      // 5: Mint
                new Color(1f, 0.9f, 0.6f),       // 6: Yellow
            };
            return colors[colorIndex % colors.Length];
        }
    }
}
