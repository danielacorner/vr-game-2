using UnityEngine;
using UnityEditor;
using VRDungeonCrawler.Environment;
using UnityEditor.SceneManagement;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Sets up GPU instanced dense forest with grass ground
    /// Can handle 100+ trees with excellent performance!
    /// </summary>
    public static class SetupInstancedForest
    {
        [MenuItem("Tools/VR Dungeon Crawler/Setup Dense Instanced Forest", priority = 5)]
        public static void SetupForest()
        {
            Debug.Log("========================================");
            Debug.Log("[InstancedForest] Setting up GPU instanced forest...");
            Debug.Log("========================================");

            // 1. Fix ground material first
            FixGroundMaterial();

            // 2. Create instanced forest renderer
            GameObject forestObj = GameObject.Find("InstancedForestRenderer");
            if (forestObj != null)
            {
                Debug.LogWarning("[InstancedForest] InstancedForestRenderer already exists. Removing old one...");
                Object.DestroyImmediate(forestObj);
            }

            forestObj = new GameObject("InstancedForestRenderer");
            InstancedForestRenderer forest = forestObj.AddComponent<InstancedForestRenderer>();

            // Configure for dense, enclosed feeling
            forest.treesPerRing = 40;  // 40 trees per ring
            forest.ringCount = 3;       // 3 rings = 120 trees total!
            forest.innerRadius = 20f;
            forest.ringSpacing = 3f;
            forest.positionRandomness = 1.5f;

            forest.heightRange = new Vector2(12f, 16f);
            forest.trunkThickness = 0.7f;
            forest.foliageScale = 7f;

            forest.barkColor = new Color(0.12f, 0.08f, 0.06f);
            forest.leavesColor = new Color(0.08f, 0.22f, 0.08f);

            // DON'T auto-generate on Start - we'll generate now in editor
            forest.autoGenerate = false;

            // Generate the forest NOW (in editor mode)
            forest.GenerateForest();

            // 3. Add boundary markers
            CreateBoundaryMarkers();

            // 4. Setup fog
            SetupFog();

            Debug.Log("========================================");
            Debug.Log("[InstancedForest] ✓✓✓ Setup complete!");
            Debug.Log("[InstancedForest] 120 trees using only 2 draw calls!");
            Debug.Log("========================================");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog(
                "Dense Instanced Forest Created!",
                "✓ 120 GPU-instanced trees (only 2 draw calls!)\n" +
                "✓ Grass ground material (non-reflective)\n" +
                "✓ 8 boundary markers with lights\n" +
                "✓ Atmospheric fog\n\n" +
                "Performance: Excellent for Quest 3!\n\n" +
                "IMPORTANT: Save the scene before building!",
                "OK"
            );
        }

        private static void FixGroundMaterial()
        {
            Debug.Log("[InstancedForest] Fixing ground material...");

            // Find terrain
            Terrain terrain = Object.FindObjectOfType<Terrain>();
            if (terrain != null)
            {
                Material terrainMat = terrain.materialTemplate;
                if (terrainMat != null)
                {
                    // Make it grassy and non-reflective
                    terrainMat.color = new Color(0.2f, 0.35f, 0.18f);
                    terrainMat.SetFloat("_Smoothness", 0.1f);
                    terrainMat.SetFloat("_Metallic", 0f);

                    EditorUtility.SetDirty(terrain);
                    Debug.Log("[InstancedForest] ✓ Fixed terrain material");
                }
            }

            // Fix any ground planes
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("ground") || obj.name.ToLower().Contains("terrain"))
                {
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null && renderer.sharedMaterial != null)
                    {
                        Material mat = renderer.sharedMaterial;

                        // Remove metallic/reflective
                        if (mat.HasProperty("_Metallic"))
                            mat.SetFloat("_Metallic", 0f);
                        if (mat.HasProperty("_Smoothness"))
                            mat.SetFloat("_Smoothness", 0.15f);

                        // Make it grass colored
                        mat.color = new Color(0.18f, 0.32f, 0.16f);

                        EditorUtility.SetDirty(renderer);
                        Debug.Log($"[InstancedForest] ✓ Fixed {obj.name}");
                    }
                }
            }
        }

        private static void CreateBoundaryMarkers()
        {
            Debug.Log("[InstancedForest] Creating boundary markers...");

            // Remove old markers if they exist
            GameObject oldMarkers = GameObject.Find("BoundaryMarkers");
            if (oldMarkers != null)
            {
                Object.DestroyImmediate(oldMarkers);
            }

            GameObject container = new GameObject("BoundaryMarkers");
            float radius = 19f;
            int count = 8;
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep;
                Vector3 position = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    2.5f,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                );

                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                marker.name = $"Marker_{i}";
                marker.transform.SetParent(container.transform);
                marker.transform.position = position;
                marker.transform.localScale = new Vector3(0.5f, 2.5f, 0.5f);
                marker.isStatic = true;

                // Stone material
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.25f, 0.25f, 0.3f);
                mat.SetFloat("_Smoothness", 0.2f);
                marker.GetComponent<Renderer>().material = mat;

                // Static light (no animation!)
                GameObject lightObj = new GameObject("Light");
                lightObj.transform.SetParent(marker.transform);
                lightObj.transform.localPosition = Vector3.up * 3f;

                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(0.5f, 0.7f, 1.0f);
                light.intensity = 1.5f;
                light.range = 10f;
            }

            container.isStatic = true;
            Debug.Log("[InstancedForest] ✓ Created 8 boundary markers");
        }

        private static void SetupFog()
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.06f, 0.08f, 0.1f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.02f;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.1f, 0.12f, 0.14f);

            Debug.Log("[InstancedForest] ✓ Configured fog");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Remove Instanced Forest", priority = 6)]
        public static void RemoveForest()
        {
            GameObject forest = GameObject.Find("InstancedForestRenderer");
            if (forest != null)
            {
                Object.DestroyImmediate(forest);
                Debug.Log("Removed InstancedForestRenderer");
            }

            GameObject markers = GameObject.Find("BoundaryMarkers");
            if (markers != null)
            {
                Object.DestroyImmediate(markers);
                Debug.Log("Removed BoundaryMarkers");
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
