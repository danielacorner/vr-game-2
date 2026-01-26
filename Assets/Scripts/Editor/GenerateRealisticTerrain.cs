using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Generates realistic bumpy terrain using Perlin noise
    /// Then carves out an organic-looking pit around the monster spawner
    /// </summary>
    public static class GenerateRealisticTerrain
    {
        [MenuItem("Tools/VR Dungeon Crawler/Generate Realistic Terrain + Pit", priority = 130)]
        public static void GenerateTerrain()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name != "HomeArea")
            {
                EditorUtility.DisplayDialog("Wrong Scene", "Please open HomeArea scene first.", "OK");
                return;
            }

            // Find terrain
            Terrain terrain = Object.FindFirstObjectByType<Terrain>();
            if (terrain == null)
            {
                EditorUtility.DisplayDialog(
                    "No Terrain Found",
                    "No Unity Terrain found in scene.\n\nPlease create a terrain first:\nGameObject > 3D Object > Terrain",
                    "OK"
                );
                return;
            }

            bool confirm = EditorUtility.DisplayDialog(
                "Generate Realistic Terrain",
                "This will:\n" +
                "1. Generate bumpy terrain using Perlin noise\n" +
                "2. Create an organic pit around MonsterSpawner\n" +
                "3. Make the pit clearly visible\n\n" +
                "This will overwrite existing terrain heights.\n\n" +
                "Continue?",
                "Yes, Generate",
                "Cancel"
            );

            if (!confirm) return;

            Debug.Log("========================================");
            Debug.Log("[RealisticTerrain] Starting terrain generation...");

            TerrainData terrainData = terrain.terrainData;
            int width = terrainData.heightmapResolution;
            int height = terrainData.heightmapResolution;

            Debug.Log($"[RealisticTerrain] Terrain heightmap: {width}x{height}");

            // Reset terrain position to ground level
            terrain.transform.position = new Vector3(terrain.transform.position.x, 0f, terrain.transform.position.z);
            Debug.Log($"[RealisticTerrain] Terrain position reset to Y=0");

            // Find monster spawner
            GameObject spawner = GameObject.Find("MonsterSpawner");
            Vector3 pitCenter = spawner != null ? spawner.transform.position : Vector3.zero;

            if (spawner == null)
            {
                Debug.LogWarning("[RealisticTerrain] No MonsterSpawner found, pit at origin");
            }

            Debug.Log($"[RealisticTerrain] Pit center: {pitCenter}");

            // Generate terrain heights
            float[,] heights = GeneratePerlinTerrain(terrainData, width, height);

            // Carve pit
            heights = CarvePit(terrainData, heights, pitCenter, width, height);

            // Apply to terrain
            terrainData.SetHeights(0, 0, heights);

            Debug.Log("[RealisticTerrain] ✓ Terrain heights applied");

            // Fix terrain material (remove reflectiveness)
            FixTerrainMaterial(terrain);

            // Position MonsterSpawner at pit floor
            if (spawner != null)
            {
                PositionSpawnerAtPitFloor(spawner, terrain, pitCenter);
                CreateSpawnerVisualTower(spawner);
            }

            // Mark scene dirty
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("========================================");
            Debug.Log("[RealisticTerrain] ✓✓✓ DONE! ✓✓✓");
            Debug.Log("[RealisticTerrain] You should now see bumpy terrain with a visible pit!");
            Debug.Log("[RealisticTerrain] Monster spawner tower is positioned at the pit floor!");
            Debug.Log("========================================");

            EditorUtility.DisplayDialog(
                "Terrain Generated!",
                "✓ Realistic bumpy terrain created\n" +
                "✓ Organic pit carved out\n" +
                "✓ MonsterSpawner positioned at pit floor\n\n" +
                "The terrain should now have visible height variation and a clear depression with the monster spawner tower visible at the bottom of the pit.\n\n" +
                "Save the scene to keep these changes!",
                "OK"
            );
        }

        static float[,] GeneratePerlinTerrain(TerrainData terrainData, int width, int height)
        {
            Debug.Log("[RealisticTerrain] Generating Perlin noise terrain...");

            float[,] heights = new float[height, width];

            // Perlin noise settings
            float scale = 80f; // VERY large scale = very smooth, gentle rolling hills
            float heightMultiplier = 0.035f; // Minimal variation = very subtle bumps
            float offsetX = Random.Range(0f, 10000f); // Random seed
            float offsetY = Random.Range(0f, 10000f);

            // Base height - LOW so terrain sits near ground level
            float baseHeight = 0.12f; // Start at 12% up (allows small hills but keeps terrain low)

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Get Perlin noise value (0-1)
                    float xCoord = offsetX + (x / (float)width) * scale;
                    float yCoord = offsetY + (y / (float)height) * scale;

                    // Sample multiple octaves for more natural variation
                    float noise = 0f;
                    noise += Mathf.PerlinNoise(xCoord, yCoord) * 1.0f; // Base layer
                    noise += Mathf.PerlinNoise(xCoord * 2f, yCoord * 2f) * 0.5f; // Detail layer
                    noise += Mathf.PerlinNoise(xCoord * 4f, yCoord * 4f) * 0.25f; // Fine detail
                    noise /= 1.75f; // Normalize

                    // Apply to height (centered around base height)
                    heights[y, x] = baseHeight + (noise - 0.5f) * heightMultiplier;

                    // Clamp to valid range
                    heights[y, x] = Mathf.Clamp01(heights[y, x]);
                }
            }

            Debug.Log("[RealisticTerrain] ✓ Perlin terrain generated");
            return heights;
        }

        static float[,] CarvePit(TerrainData terrainData, float[,] heights, Vector3 pitCenter, int width, int height)
        {
            Debug.Log("[RealisticTerrain] Carving organic pit...");

            Vector3 terrainPos = Object.FindFirstObjectByType<Terrain>().transform.position;
            Vector3 terrainSize = terrainData.size;

            // Pit settings
            float pitDiameter = 12f;
            float pitDepth = 0.7f; // 70% of terrain height = VERY deep pit!
            float pitRadiusWorld = pitDiameter / 2f;

            // Convert pit center to terrain coordinates
            Vector3 relativePos = pitCenter - terrainPos;
            float normalizedX = relativePos.x / terrainSize.x;
            float normalizedZ = relativePos.z / terrainSize.z;

            int centerX = Mathf.RoundToInt(normalizedX * width);
            int centerZ = Mathf.RoundToInt(normalizedZ * height);

            Debug.Log($"[RealisticTerrain] Pit center in heightmap: ({centerX}, {centerZ})");

            // Add noise to pit shape for organic look
            float noiseScale = 5f;
            float noiseOffset = Random.Range(0f, 100f);

            int modifiedPixels = 0;

            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Calculate world position
                    float worldX = (x / (float)width) * terrainSize.x + terrainPos.x;
                    float worldZ = (z / (float)height) * terrainSize.z + terrainPos.z;

                    float dx = worldX - pitCenter.x;
                    float dz = worldZ - pitCenter.z;
                    float distanceWorld = Mathf.Sqrt(dx * dx + dz * dz);

                    // Add Perlin noise to radius for organic shape
                    float angle = Mathf.Atan2(dz, dx);
                    float noiseValue = Mathf.PerlinNoise(
                        noiseOffset + Mathf.Cos(angle) * noiseScale,
                        noiseOffset + Mathf.Sin(angle) * noiseScale
                    );

                    // Vary radius based on noise (±20%)
                    float radiusVariation = 0.8f + noiseValue * 0.4f;
                    float effectiveRadius = pitRadiusWorld * radiusVariation;

                    if (distanceWorld <= effectiveRadius)
                    {
                        // Calculate depth with smooth falloff
                        float normalizedDistance = distanceWorld / effectiveRadius;
                        float falloff = Mathf.Pow(1f - normalizedDistance, 1.5f); // Smooth bowl shape

                        // Add noise to depth for uneven pit floor
                        float depthNoise = Mathf.PerlinNoise(x * 0.1f + noiseOffset, z * 0.1f + noiseOffset);
                        float depthVariation = 0.8f + depthNoise * 0.4f;

                        // Calculate final depth
                        float depthAtThisPoint = pitDepth * falloff * depthVariation;

                        // Lower the terrain
                        heights[z, x] = Mathf.Max(0.05f, heights[z, x] - depthAtThisPoint);

                        modifiedPixels++;
                    }
                }
            }

            Debug.Log($"[RealisticTerrain] ✓ Modified {modifiedPixels} pixels for pit");
            return heights;
        }

        static void PositionSpawnerAtPitFloor(GameObject spawner, Terrain terrain, Vector3 pitCenter)
        {
            Debug.Log("[RealisticTerrain] Positioning MonsterSpawner at pit floor...");

            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;
            Vector3 terrainSize = terrainData.size;

            // Convert pit center to terrain coordinates
            Vector3 relativePos = pitCenter - terrainPos;
            float normalizedX = relativePos.x / terrainSize.x;
            float normalizedZ = relativePos.z / terrainSize.z;

            // Clamp to terrain bounds
            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedZ = Mathf.Clamp01(normalizedZ);

            // Sample the exact height at pit center (after pit carving)
            float normalizedHeight = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);

            // Convert to world height
            float worldHeight = normalizedHeight * terrainSize.y + terrainPos.y;

            // Position spawner at pit floor
            // Spawner's base platform is at Y=0 in local space, so we position it at the terrain height
            Vector3 spawnerPosition = new Vector3(pitCenter.x, worldHeight, pitCenter.z);
            spawner.transform.position = spawnerPosition;

            Debug.Log($"[RealisticTerrain] ✓ MonsterSpawner positioned at {spawnerPosition}");
            Debug.Log($"[RealisticTerrain]   Terrain height at pit center: {worldHeight:F2}m");
        }

        static void FixTerrainMaterial(Terrain terrain)
        {
            Debug.Log("[RealisticTerrain] Fixing terrain material (removing reflectiveness)...");

            // Get terrain material - create if doesn't exist
            Material terrainMat = terrain.materialTemplate;
            if (terrainMat == null)
            {
                terrainMat = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
                terrain.materialTemplate = terrainMat;
            }

            // Set smoothness to 0 (no reflectiveness) and metallic to 0
            terrainMat.SetFloat("_Smoothness", 0f);
            terrainMat.SetFloat("_Metallic", 0f);

            Debug.Log("[RealisticTerrain] ✓ Terrain material fixed (smoothness=0, metallic=0)");
        }

        static void CreateSpawnerVisualTower(GameObject spawner)
        {
            Debug.Log("[RealisticTerrain] Creating elaborate spawner tower visual...");

            // Clear any existing visual children
            for (int i = spawner.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(spawner.transform.GetChild(i).gameObject);
            }

            // Create tower container
            GameObject tower = new GameObject("SpawnerTower");
            tower.transform.SetParent(spawner.transform);
            tower.transform.localPosition = Vector3.zero;
            tower.transform.localRotation = Quaternion.identity;

            // Base platform (wide black disc)
            GameObject basePlatform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basePlatform.name = "BasePlatform";
            basePlatform.transform.SetParent(tower.transform);
            basePlatform.transform.localPosition = Vector3.up * 0.5f;
            basePlatform.transform.localScale = new Vector3(3f, 0.25f, 3f);

            Material blackMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            blackMat.color = new Color(0.1f, 0.1f, 0.1f);
            basePlatform.GetComponent<MeshRenderer>().material = blackMat;
            Object.DestroyImmediate(basePlatform.GetComponent<Collider>());

            // Central pillar (tall black cylinder)
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "CentralPillar";
            pillar.transform.SetParent(tower.transform);
            pillar.transform.localPosition = Vector3.up * 2.5f;
            pillar.transform.localScale = new Vector3(0.8f, 2f, 0.8f);
            pillar.GetComponent<MeshRenderer>().material = blackMat;
            Object.DestroyImmediate(pillar.GetComponent<Collider>());

            // Purple glowing crystal at top
            GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crystal.name = "GlowingCrystal";
            crystal.transform.SetParent(tower.transform);
            crystal.transform.localPosition = Vector3.up * 5.5f;
            crystal.transform.localScale = Vector3.one * 1.2f;

            Material purpleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            purpleMat.color = new Color(0.5f, 0f, 0.8f);
            purpleMat.EnableKeyword("_EMISSION");
            purpleMat.SetColor("_EmissionColor", new Color(0.8f, 0f, 1.2f) * 2f);
            crystal.GetComponent<MeshRenderer>().material = purpleMat;
            Object.DestroyImmediate(crystal.GetComponent<Collider>());

            // Four corner pillars (smaller black cubes)
            float cornerRadius = 2f;
            for (int i = 0; i < 4; i++)
            {
                float angle = (i * 90f) * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * cornerRadius;
                float z = Mathf.Sin(angle) * cornerRadius;

                GameObject cornerPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cornerPillar.name = $"CornerPillar_{i}";
                cornerPillar.transform.SetParent(tower.transform);
                cornerPillar.transform.localPosition = new Vector3(x, 1.5f, z);
                cornerPillar.transform.localScale = new Vector3(0.4f, 2f, 0.4f);
                cornerPillar.GetComponent<MeshRenderer>().material = blackMat;
                Object.DestroyImmediate(cornerPillar.GetComponent<Collider>());

                // Small purple orbs on corner pillars
                GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                orb.name = $"CornerOrb_{i}";
                orb.transform.SetParent(tower.transform);
                orb.transform.localPosition = new Vector3(x, 2.8f, z);
                orb.transform.localScale = Vector3.one * 0.4f;
                orb.GetComponent<MeshRenderer>().material = purpleMat;
                Object.DestroyImmediate(orb.GetComponent<Collider>());
            }

            // Pyramid cap (black)
            GameObject pyramid = new GameObject("PyramidCap");
            pyramid.transform.SetParent(tower.transform);
            pyramid.transform.localPosition = Vector3.up * 4.8f;

            // Create pyramid using 4 triangular faces
            for (int i = 0; i < 4; i++)
            {
                GameObject face = GameObject.CreatePrimitive(PrimitiveType.Cube);
                face.name = $"PyramidFace_{i}";
                face.transform.SetParent(pyramid.transform);
                face.transform.localRotation = Quaternion.Euler(45f, i * 90f, 0f);
                face.transform.localPosition = Vector3.zero;
                face.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);
                face.GetComponent<MeshRenderer>().material = blackMat;
                Object.DestroyImmediate(face.GetComponent<Collider>());
            }

            Debug.Log("[RealisticTerrain] ✓ Elaborate spawner tower created!");
        }
    }
}
