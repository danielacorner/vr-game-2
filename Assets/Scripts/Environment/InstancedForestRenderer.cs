using UnityEngine;
using System.Collections.Generic;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Renders a dense forest using GPU instancing - hundreds of trees with minimal performance cost
    /// Uses a single draw call per tree type instead of individual GameObjects
    /// </summary>
    public class InstancedForestRenderer : MonoBehaviour
    {
        [Header("Forest Settings")]
        [Tooltip("Number of trees in each ring")]
        public int treesPerRing = 40;

        [Tooltip("Number of concentric rings")]
        [Range(1, 5)]
        public int ringCount = 3;

        [Tooltip("Inner radius (clearing edge)")]
        public float innerRadius = 20f;

        [Tooltip("Spacing between rings")]
        public float ringSpacing = 3f;

        [Tooltip("Random position offset")]
        public float positionRandomness = 1.5f;

        [Header("Tree Appearance")]
        [Tooltip("Tree height range")]
        public Vector2 heightRange = new Vector2(10f, 15f);

        [Tooltip("Tree trunk thickness")]
        public float trunkThickness = 0.6f;

        [Tooltip("Foliage size multiplier")]
        public float foliageScale = 6f;

        [Header("Materials")]
        [Tooltip("Bark material (must have GPU Instancing enabled)")]
        public Material barkMaterial;

        [Tooltip("Leaves material (must have GPU Instancing enabled)")]
        public Material leavesMaterial;

        [Header("Colors")]
        public Color barkColor = new Color(0.15f, 0.1f, 0.08f);
        public Color leavesColor = new Color(0.1f, 0.25f, 0.1f);

        [Header("Runtime")]
        [Tooltip("Generate forest on Start (disable if setting up manually)")]
        public bool autoGenerate = false;

        [Header("Debug")]
        public bool showGizmos = true;

        // Instancing data
        private Mesh trunkMesh;
        private Mesh foliageMesh;
        private Matrix4x4[] trunkMatrices;
        private Matrix4x4[] foliageMatrices;
        private int totalTrees;
        private bool isGenerated = false;

        private void Start()
        {
            // Only auto-generate if explicitly enabled
            if (autoGenerate)
            {
                GenerateForest();
            }
        }

        public void GenerateForest()
        {
            if (isGenerated)
            {
                Debug.LogWarning("[InstancedForest] Forest already generated. Clear first if you want to regenerate.");
                return;
            }

            Debug.Log("[InstancedForest] Generating GPU instanced forest...");

            // Create meshes
            CreateMeshes();

            // Create materials if not assigned
            if (barkMaterial == null)
            {
                barkMaterial = CreateInstancedMaterial("BarkMaterial", barkColor);
            }
            if (leavesMaterial == null)
            {
                leavesMaterial = CreateInstancedMaterial("LeavesMaterial", leavesColor);
            }

            // Ensure GPU instancing is enabled
            if (barkMaterial != null) barkMaterial.enableInstancing = true;
            if (leavesMaterial != null) leavesMaterial.enableInstancing = true;

            // Generate tree positions and matrices
            GenerateTreeMatrices();

            isGenerated = true;

            Debug.Log($"[InstancedForest] ✓ Created {totalTrees} instanced trees!");
            Debug.Log($"[InstancedForest] Using only 2 draw calls total!");
        }

        private void CreateMeshes()
        {
            // Create cylinder mesh for trunks
            GameObject tempCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunkMesh = tempCylinder.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(tempCylinder);

            // Create sphere mesh for foliage
            GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliageMesh = tempSphere.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(tempSphere);
        }

        private Material CreateInstancedMaterial(string name, Color color)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = name;
            mat.color = color;
            mat.SetFloat("_Smoothness", name.Contains("Bark") ? 0.1f : 0.3f);
            mat.enableInstancing = true;
            return mat;
        }

        private void GenerateTreeMatrices()
        {
            List<Matrix4x4> trunkMatrixList = new List<Matrix4x4>();
            List<Matrix4x4> foliageMatrixList = new List<Matrix4x4>();

            totalTrees = 0;

            // Use a fixed random seed for consistent results
            Random.InitState(42);

            for (int ring = 0; ring < ringCount; ring++)
            {
                float radius = innerRadius + (ring * ringSpacing);
                float angleStep = 360f / treesPerRing;

                for (int i = 0; i < treesPerRing; i++)
                {
                    float angle = i * angleStep + Random.Range(-angleStep * 0.3f, angleStep * 0.3f);
                    float r = radius + Random.Range(-positionRandomness, positionRandomness);

                    Vector3 position = transform.position + new Vector3(
                        Mathf.Cos(angle * Mathf.Deg2Rad) * r,
                        0,
                        Mathf.Sin(angle * Mathf.Deg2Rad) * r
                    );

                    float height = Random.Range(heightRange.x, heightRange.y);
                    float rotation = Random.Range(0f, 360f);

                    // Trunk matrix
                    Vector3 trunkPos = position + Vector3.up * (height / 2f);
                    Quaternion trunkRot = Quaternion.Euler(0, rotation, 0);
                    Vector3 trunkScale = new Vector3(trunkThickness, height / 2f, trunkThickness);
                    trunkMatrixList.Add(Matrix4x4.TRS(trunkPos, trunkRot, trunkScale));

                    // Foliage matrix
                    Vector3 foliagePos = position + Vector3.up * (height * 0.75f);
                    Quaternion foliageRot = Quaternion.identity;
                    Vector3 foliageScaleVec = Vector3.one * (this.foliageScale + Random.Range(-1f, 1f));
                    foliageMatrixList.Add(Matrix4x4.TRS(foliagePos, foliageRot, foliageScaleVec));

                    totalTrees++;
                }
            }

            trunkMatrices = trunkMatrixList.ToArray();
            foliageMatrices = foliageMatrixList.ToArray();

            Debug.Log($"[InstancedForest] Generated {totalTrees} tree positions");
        }

        private void Update()
        {
            if (!isGenerated) return;
            if (trunkMatrices == null || foliageMatrices == null) return;
            if (barkMaterial == null || leavesMaterial == null) return;

            // Render all trunks in one draw call
            DrawInstances(trunkMesh, barkMaterial, trunkMatrices);

            // Render all foliage in one draw call
            DrawInstances(foliageMesh, leavesMaterial, foliageMatrices);
        }

        private void DrawInstances(Mesh mesh, Material material, Matrix4x4[] matrices)
        {
            if (mesh == null || material == null || matrices == null) return;

            // DrawMeshInstanced supports up to 1023 instances per call
            int batchSize = 1023;
            int batchCount = Mathf.CeilToInt((float)matrices.Length / batchSize);

            for (int batch = 0; batch < batchCount; batch++)
            {
                int start = batch * batchSize;
                int count = Mathf.Min(batchSize, matrices.Length - start);

                Matrix4x4[] batchMatrices = new Matrix4x4[count];
                System.Array.Copy(matrices, start, batchMatrices, 0, count);

                Graphics.DrawMeshInstanced(
                    mesh,
                    0,
                    material,
                    batchMatrices,
                    count,
                    null,
                    UnityEngine.Rendering.ShadowCastingMode.On,
                    true,
                    0,
                    null,
                    UnityEngine.Rendering.LightProbeUsage.BlendProbes
                );
            }
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            // Draw perimeter rings
            Gizmos.color = Color.green;
            for (int ring = 0; ring < ringCount; ring++)
            {
                float radius = innerRadius + (ring * ringSpacing);
                DrawCircle(transform.position, radius, 32);
            }
        }

        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prev = center + new Vector3(Mathf.Cos(0) * radius, 0, Mathf.Sin(0) * radius);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }

        public void ClearForest()
        {
            trunkMatrices = null;
            foliageMatrices = null;
            isGenerated = false;
            Debug.Log("[InstancedForest] Forest cleared");
        }
    }
}
