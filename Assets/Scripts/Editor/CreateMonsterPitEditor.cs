using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Editor script to create monster pit in HomeArea terrain RIGHT NOW in the editor
    /// This modifies the actual terrain asset, so the pit is permanent until you undo
    /// </summary>
    public static class CreateMonsterPitEditor
    {
        [MenuItem("Tools/VR Dungeon Crawler/Create Monster Pit NOW (Modifies Terrain)", priority = 150)]
        public static void CreatePitNow()
        {
            // Check if HomeArea scene is loaded
            Scene homeArea = SceneManager.GetSceneByName("HomeArea");
            if (!homeArea.isLoaded)
            {
                EditorUtility.DisplayDialog(
                    "Scene Not Loaded",
                    "Please open the HomeArea scene first.",
                    "OK"
                );
                return;
            }

            // Find monster spawner
            GameObject spawner = GameObject.Find("MonsterSpawner");
            if (spawner == null)
            {
                EditorUtility.DisplayDialog(
                    "Monster Spawner Not Found",
                    "Could not find MonsterSpawner in the scene.\n\nPlease make sure HomeArea scene has a MonsterSpawner GameObject.",
                    "OK"
                );
                return;
            }

            Vector3 spawnerPos = spawner.transform.position;
            Debug.Log($"[CreateMonsterPit] Found MonsterSpawner at {spawnerPos}");

            // Find terrain
            Terrain terrain = Object.FindFirstObjectByType<Terrain>();
            if (terrain == null)
            {
                bool createMesh = EditorUtility.DisplayDialog(
                    "No Terrain Found",
                    "No Unity Terrain found in scene.\n\nWould you like to create a visible mesh pit instead?",
                    "Yes, Create Mesh Pit",
                    "Cancel"
                );

                if (createMesh)
                {
                    CreateMeshPit(spawnerPos);
                }
                return;
            }

            // Confirm before modifying terrain
            bool confirm = EditorUtility.DisplayDialog(
                "Modify Terrain",
                $"This will create a 12m diameter, 1.5m deep pit in the terrain at position {spawnerPos}.\n\n" +
                "This modifies the terrain permanently. You can undo with Ctrl/Cmd+Z.\n\n" +
                "Continue?",
                "Yes, Create Pit",
                "Cancel"
            );

            if (!confirm) return;

            // Create the pit
            CreateTerrainPit(terrain, spawnerPos);

            // Create visual markers
            CreatePitMarkers(spawnerPos);

            // Create pit floor indicators (red cubes to show depth)
            CreatePitFloorIndicators(spawnerPos, 3f);

            // Lower spawner deep into pit
            spawner.transform.position = new Vector3(spawnerPos.x, spawnerPos.y - 2.4f, spawnerPos.z);

            // Mark scene dirty
            EditorSceneManager.MarkSceneDirty(homeArea);

            // Show success dialog
            EditorUtility.DisplayDialog(
                "Monster Pit Created!",
                "✓ Pit created successfully!\n\n" +
                "You should now see:\n" +
                "- A depression in the terrain\n" +
                "- Yellow sphere at pit center\n" +
                "- Gray stone rim around edge\n" +
                "- Monster spawner lowered into pit\n\n" +
                "Save the scene (Ctrl/Cmd+S) to keep these changes.",
                "OK"
            );

            Debug.Log("[CreateMonsterPit] ✓ Pit created successfully!");
        }

        static void CreateTerrainPit(Terrain terrain, Vector3 centerPos)
        {
            float pitDiameter = 12f;
            float pitDepth = 3f; // Much deeper - 3 meters!
            float edgeSmoothness = 1.5f; // Steeper edges

            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;
            Vector3 terrainSize = terrainData.size;

            int heightmapWidth = terrainData.heightmapResolution;
            int heightmapHeight = terrainData.heightmapResolution;

            Debug.Log($"[CreateMonsterPit] Terrain size: {terrainSize}");
            Debug.Log($"[CreateMonsterPit] Heightmap resolution: {heightmapWidth}x{heightmapHeight}");

            // Get current heights
            float[,] heights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);

            // Convert world position to terrain coordinates
            Vector3 relativePos = centerPos - terrainPos;
            float normalizedX = relativePos.x / terrainSize.x;
            float normalizedZ = relativePos.z / terrainSize.z;

            int centerX = Mathf.RoundToInt(normalizedX * heightmapWidth);
            int centerZ = Mathf.RoundToInt(normalizedZ * heightmapHeight);

            Debug.Log($"[CreateMonsterPit] Pit center in heightmap: ({centerX}, {centerZ})");

            // Calculate pit radius and depth
            float pitRadiusWorld = pitDiameter / 2f;
            float normalizedDepth = pitDepth / terrainSize.y;

            Debug.Log($"[CreateMonsterPit] Pit radius: {pitRadiusWorld}m, normalized depth: {normalizedDepth}");

            int modifiedPixels = 0;

            // Modify heights in circular area
            for (int z = 0; z < heightmapHeight; z++)
            {
                for (int x = 0; x < heightmapWidth; x++)
                {
                    // Calculate distance from pit center in world units
                    float worldX = (x / (float)heightmapWidth) * terrainSize.x + terrainPos.x;
                    float worldZ = (z / (float)heightmapHeight) * terrainSize.z + terrainPos.z;

                    float dx = worldX - centerPos.x;
                    float dz = worldZ - centerPos.z;
                    float distanceWorld = Mathf.Sqrt(dx * dx + dz * dz);

                    if (distanceWorld <= pitRadiusWorld)
                    {
                        // Calculate depth with smooth falloff
                        float normalizedDistance = distanceWorld / pitRadiusWorld; // 0 at center, 1 at edge
                        float falloff = Mathf.Pow(1f - normalizedDistance, edgeSmoothness); // Smooth curve

                        // Lower the terrain
                        float currentHeight = heights[z, x];
                        float depthAtThisPoint = normalizedDepth * falloff;
                        heights[z, x] = Mathf.Max(0f, currentHeight - depthAtThisPoint);

                        modifiedPixels++;
                    }
                }
            }

            // Apply modified heights
            terrainData.SetHeights(0, 0, heights);

            Debug.Log($"[CreateMonsterPit] ✓ Modified {modifiedPixels} terrain pixels");
        }

        static void CreateMeshPit(Vector3 centerPos)
        {
            float pitDiameter = 12f;
            float pitDepth = 1.5f;

            // Create visible pit cylinder
            GameObject pitObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pitObj.name = "MonsterPit_Mesh";
            pitObj.transform.position = centerPos - Vector3.up * (pitDepth / 2f);
            pitObj.transform.localScale = new Vector3(pitDiameter, pitDepth, pitDiameter);

            // Make it brown/dirt colored
            MeshRenderer renderer = pitObj.GetComponent<MeshRenderer>();
            Material pitMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            pitMat.color = new Color(0.3f, 0.2f, 0.1f); // Dark brown
            renderer.material = pitMat;

            Debug.Log("[CreateMonsterPit] ✓ Created mesh pit");

            CreatePitMarkers(centerPos);
        }

        static void CreatePitMarkers(Vector3 centerPos)
        {
            float pitDiameter = 12f;

            // Create yellow marker at center
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "PitCenterMarker_YELLOW";
            marker.transform.position = centerPos + Vector3.up * 0.5f;
            marker.transform.localScale = Vector3.one * 0.5f;

            MeshRenderer markerRenderer = marker.GetComponent<MeshRenderer>();
            Material markerMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            markerMat.color = Color.yellow;
            markerMat.EnableKeyword("_EMISSION");
            markerMat.SetColor("_EmissionColor", Color.yellow * 2f);
            markerRenderer.material = markerMat;

            Object.DestroyImmediate(marker.GetComponent<Collider>()); // Remove collider

            Debug.Log($"[CreateMonsterPit] ✓ Created yellow marker at {centerPos}");

            // Create stone rim
            GameObject rim = new GameObject("PitRim_GRAY_STONES");
            rim.transform.position = centerPos;

            int segments = 32;
            float radius = pitDiameter / 2f;

            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * 360f * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                GameObject rimSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rimSegment.name = $"RimStone_{i}";
                rimSegment.transform.SetParent(rim.transform);
                rimSegment.transform.localPosition = new Vector3(x, 0.1f, z);
                rimSegment.transform.localScale = new Vector3(0.4f, 0.3f, 1f);
                rimSegment.transform.LookAt(centerPos + Vector3.up * 0.1f);

                // Gray stone material
                MeshRenderer renderer = rimSegment.GetComponent<MeshRenderer>();
                Material stoneMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                stoneMat.color = new Color(0.5f, 0.5f, 0.5f); // Gray
                renderer.material = stoneMat;

                Object.DestroyImmediate(rimSegment.GetComponent<Collider>()); // Remove collider
            }

            Debug.Log($"[CreateMonsterPit] ✓ Created stone rim with {segments} segments");
        }

        static void CreatePitFloorIndicators(Vector3 centerPos, float pitDepth)
        {
            // Create several red cubes on the pit floor to clearly show the depth
            GameObject floorMarkers = new GameObject("PitFloorIndicators_RED");
            floorMarkers.transform.position = centerPos - Vector3.up * pitDepth;

            // Create a grid of red cubes on the floor
            for (int i = -2; i <= 2; i++)
            {
                for (int j = -2; j <= 2; j++)
                {
                    if (i == 0 && j == 0) continue; // Skip center (where spawner is)

                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = $"FloorMarker_{i}_{j}";
                    cube.transform.SetParent(floorMarkers.transform);
                    cube.transform.localPosition = new Vector3(i * 1.5f, 0f, j * 1.5f);
                    cube.transform.localScale = Vector3.one * 0.3f;

                    // Bright red material
                    MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
                    Material redMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    redMat.color = Color.red;
                    redMat.EnableKeyword("_EMISSION");
                    redMat.SetColor("_EmissionColor", Color.red * 2f);
                    renderer.material = redMat;

                    Object.DestroyImmediate(cube.GetComponent<Collider>());
                }
            }

            Debug.Log($"[CreateMonsterPit] ✓ Created red floor indicators at depth {pitDepth}m");
        }
    }
}
