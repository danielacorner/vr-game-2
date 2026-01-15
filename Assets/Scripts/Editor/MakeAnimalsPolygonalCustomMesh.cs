using UnityEngine;
using UnityEditor;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Converts animal mesh primitives to custom polygonal cubes
    /// Uses custom mesh assets instead of Unity primitives to avoid caching issues
    /// </summary>
    public static class MakeAnimalsPolygonalCustomMesh
    {
        [MenuItem("Tools/VR Dungeon Crawler/Make Animals Polygonal (Custom Mesh)")]
        public static void ConvertAnimalsToPolygonal()
        {
            // Create custom cube mesh asset
            Mesh cubeMesh = CreateCustomCubeMesh();
            string meshPath = "Assets/Meshes/PolygonalCube.asset";

            // Ensure Meshes folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Meshes"))
            {
                AssetDatabase.CreateFolder("Assets", "Meshes");
            }

            // Save the custom cube mesh as an asset
            AssetDatabase.CreateAsset(cubeMesh, meshPath);
            AssetDatabase.SaveAssets();

            // Load the saved mesh
            Mesh savedCubeMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);

            string[] animalPrefabPaths = new string[]
            {
                "Assets/Prefabs/Animals/Rabbit.prefab",
                "Assets/Prefabs/Animals/Squirrel.prefab",
                "Assets/Prefabs/Animals/Bird.prefab"
            };

            int convertedCount = 0;

            foreach (string prefabPath in animalPrefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    Debug.LogWarning($"[Polygonal] Prefab not found: {prefabPath}");
                    continue;
                }

                // Instantiate prefab for editing
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                // Find all MeshFilter components
                MeshFilter[] meshFilters = instance.GetComponentsInChildren<MeshFilter>();

                foreach (MeshFilter meshFilter in meshFilters)
                {
                    if (meshFilter.sharedMesh == null)
                        continue;

                    // Replace with custom cube mesh
                    meshFilter.sharedMesh = savedCubeMesh;

                    // Update colliders to BoxCollider
                    SphereCollider sphereCollider = meshFilter.GetComponent<SphereCollider>();
                    if (sphereCollider != null)
                    {
                        Vector3 center = sphereCollider.center;
                        float radius = sphereCollider.radius;
                        bool isTrigger = sphereCollider.isTrigger;

                        Object.DestroyImmediate(sphereCollider);

                        BoxCollider boxCollider = meshFilter.gameObject.AddComponent<BoxCollider>();
                        boxCollider.center = center;
                        boxCollider.size = Vector3.one * radius * 2f;
                        boxCollider.isTrigger = isTrigger;
                    }

                    CapsuleCollider capsuleCollider = meshFilter.GetComponent<CapsuleCollider>();
                    if (capsuleCollider != null)
                    {
                        Vector3 center = capsuleCollider.center;
                        float radius = capsuleCollider.radius;
                        float height = capsuleCollider.height;
                        bool isTrigger = capsuleCollider.isTrigger;

                        Object.DestroyImmediate(capsuleCollider);

                        BoxCollider boxCollider = meshFilter.gameObject.AddComponent<BoxCollider>();
                        boxCollider.center = center;
                        boxCollider.size = new Vector3(radius * 2f, height, radius * 2f);
                        boxCollider.isTrigger = isTrigger;
                    }

                    convertedCount++;
                }

                // Save changes back to prefab
                PrefabUtility.SaveAsPrefabAssetAndConnect(instance, prefabPath, InteractionMode.AutomatedAction);
                Object.DestroyImmediate(instance);

                Debug.Log($"[Polygonal] ✓ Converted {prefab.name} to polygonal style with custom mesh");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Animals Made Polygonal!",
                $"✓ Created custom cube mesh asset\n" +
                $"✓ Converted {convertedCount} mesh parts\n" +
                $"✓ Animals now use custom polygonal mesh",
                "OK"
            );
        }

        private static Mesh CreateCustomCubeMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "PolygonalCube";

            // Cube vertices (8 corners)
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, -0.5f), // 0
                new Vector3( 0.5f, -0.5f, -0.5f), // 1
                new Vector3( 0.5f,  0.5f, -0.5f), // 2
                new Vector3(-0.5f,  0.5f, -0.5f), // 3
                new Vector3(-0.5f, -0.5f,  0.5f), // 4
                new Vector3( 0.5f, -0.5f,  0.5f), // 5
                new Vector3( 0.5f,  0.5f,  0.5f), // 6
                new Vector3(-0.5f,  0.5f,  0.5f)  // 7
            };

            // Triangles (2 per face, 6 faces)
            int[] triangles = new int[]
            {
                // Front
                0, 2, 1, 0, 3, 2,
                // Back
                5, 7, 4, 5, 6, 7,
                // Left
                4, 3, 0, 4, 7, 3,
                // Right
                1, 6, 5, 1, 2, 6,
                // Top
                3, 6, 2, 3, 7, 6,
                // Bottom
                4, 1, 5, 4, 0, 1
            };

            // Normals (flat shaded - one normal per face)
            Vector3[] normals = new Vector3[]
            {
                Vector3.back, Vector3.back, Vector3.back, Vector3.back,
                Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
                Vector3.left, Vector3.left, Vector3.left, Vector3.left,
                Vector3.right, Vector3.right, Vector3.right, Vector3.right,
                Vector3.up, Vector3.up, Vector3.up, Vector3.up,
                Vector3.down, Vector3.down, Vector3.down, Vector3.down
            };

            // UVs
            Vector2[] uvs = new Vector2[vertices.Length];
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2(vertices[i].x, vertices[i].y);
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.uv = uvs;

            mesh.RecalculateBounds();

            return mesh;
        }
    }
}
