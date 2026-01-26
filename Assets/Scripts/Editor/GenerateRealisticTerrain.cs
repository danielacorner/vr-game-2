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

            // Mark scene dirty
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("========================================");
            Debug.Log("[RealisticTerrain] ✓✓✓ DONE! ✓✓✓");
            Debug.Log("[RealisticTerrain] You should now see bumpy terrain with a visible pit!");
            Debug.Log("========================================");

            EditorUtility.DisplayDialog(
                "Terrain Generated!",
                "✓ Realistic bumpy terrain created\n" +
                "✓ Organic pit carved out\n\n" +
                "The terrain should now have visible height variation and a clear depression where the monster spawner is.\n\n" +
                "Save the scene to keep these changes!",
                "OK"
            );
        }

        static float[,] GeneratePerlinTerrain(TerrainData terrainData, int width, int height)
        {
            Debug.Log("[RealisticTerrain] Generating Perlin noise terrain...");

            float[,] heights = new float[height, width];

            // Perlin noise settings
            float scale = 20f; // Larger scale = gentler hills
            float heightMultiplier = 0.1f; // Max height variation (0.1 = 10% of terrain height)
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
            float pitDepth = 0.3f; // 30% of terrain height = deep pit
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
    }
}
