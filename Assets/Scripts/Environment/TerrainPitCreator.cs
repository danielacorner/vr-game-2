using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Creates a pit in the terrain around the monster spawner
    /// Pit is 3x the spawner width, deep enough to require double-jump to escape
    /// </summary>
    public class TerrainPitCreator : MonoBehaviour
    {
        [Header("Pit Settings")]
        [Tooltip("Diameter of the pit in meters")]
        public float pitDiameter = 12f; // 3x spawner width (~4m spawner = 12m pit)

        [Tooltip("Depth of the pit in meters (1.2m = double-jump height)")]
        public float pitDepth = 1.2f;

        [Tooltip("Smoothness of pit edges (higher = smoother)")]
        public float edgeSmoothness = 3f;

        [Header("References")]
        [Tooltip("Terrain to modify (auto-finds if not set)")]
        public Terrain terrain;

        [Tooltip("Monster spawner GameObject (auto-finds if not set)")]
        public GameObject monsterSpawner;

        [Header("Debug")]
        public bool showDebug = true;

        void Start()
        {
            // Auto-find terrain if not assigned
            if (terrain == null)
            {
                terrain = FindFirstObjectByType<Terrain>();
            }

            // Auto-find monster spawner if not assigned
            if (monsterSpawner == null)
            {
                monsterSpawner = GameObject.Find("MonsterSpawner");
                if (monsterSpawner == null)
                {
                    // Try to find by component
                    var spawnerComponent = FindFirstObjectByType<AI.MonsterSpawner>();
                    if (spawnerComponent != null)
                    {
                        monsterSpawner = spawnerComponent.gameObject;
                    }
                }

                if (monsterSpawner == null)
                {
                    Debug.LogWarning("[TerrainPitCreator] Monster spawner not found, creating pit at (0, 0, 0)");
                }
            }

            if (terrain != null)
            {
                CreatePit();
            }
            else
            {
                Debug.LogWarning("[TerrainPitCreator] No Unity Terrain found - creating visual pit mesh instead");
                CreateMeshPit();
            }
        }

        void CreatePit()
        {
            if (terrain == null)
            {
                Debug.LogError("[TerrainPitCreator] Cannot create pit - terrain is null!");
                return;
            }

            TerrainData terrainData = terrain.terrainData;
            int heightmapWidth = terrainData.heightmapResolution;
            int heightmapHeight = terrainData.heightmapResolution;
            Vector3 terrainSize = terrainData.size;

            // Get spawner position (or use origin if not found)
            Vector3 spawnerWorldPos = monsterSpawner != null ? monsterSpawner.transform.position : Vector3.zero;

            if (showDebug)
            {
                Debug.Log($"[TerrainPitCreator] ========================================");
                Debug.Log($"[TerrainPitCreator] Creating pit at world position: {spawnerWorldPos}");
                Debug.Log($"[TerrainPitCreator] Terrain size: {terrainSize}");
                Debug.Log($"[TerrainPitCreator] Heightmap resolution: {heightmapWidth}x{heightmapHeight}");
                Debug.Log($"[TerrainPitCreator] Pit diameter: {pitDiameter}m, depth: {pitDepth}m");
            }

            // Get current heightmap
            float[,] heights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);

            // Calculate pit center in heightmap coordinates
            Vector3 terrainPos = terrain.transform.position;
            Vector3 relativePos = spawnerWorldPos - terrainPos;

            // Convert world position to heightmap coordinates (0-1 range)
            float normalizedX = relativePos.x / terrainSize.x;
            float normalizedZ = relativePos.z / terrainSize.z;

            // Convert to heightmap array indices
            int centerX = Mathf.RoundToInt(normalizedX * heightmapWidth);
            int centerZ = Mathf.RoundToInt(normalizedZ * heightmapHeight);

            if (showDebug)
            {
                Debug.Log($"[TerrainPitCreator] Pit center (heightmap coords): ({centerX}, {centerZ})");
            }

            // Calculate pit radius in heightmap units
            float pitRadiusWorld = pitDiameter / 2f;
            int pitRadiusX = Mathf.RoundToInt((pitRadiusWorld / terrainSize.x) * heightmapWidth);
            int pitRadiusZ = Mathf.RoundToInt((pitRadiusWorld / terrainSize.z) * heightmapHeight);

            // Calculate depth in normalized height units (0-1 range)
            float normalizedDepth = pitDepth / terrainSize.y;

            int modifiedPixels = 0;

            // Modify heights in circular area
            for (int z = 0; z < heightmapHeight; z++)
            {
                for (int x = 0; x < heightmapWidth; x++)
                {
                    // Calculate distance from pit center
                    int dx = x - centerX;
                    int dz = z - centerZ;
                    float distanceInPixels = Mathf.Sqrt(dx * dx + dz * dz);

                    // Convert to world units for comparison
                    float distanceX = (dx / (float)heightmapWidth) * terrainSize.x;
                    float distanceZ = (dz / (float)heightmapHeight) * terrainSize.z;
                    float distanceWorld = Mathf.Sqrt(distanceX * distanceX + distanceZ * distanceZ);

                    if (distanceWorld <= pitRadiusWorld)
                    {
                        // Inside pit - calculate depth based on distance from center
                        // Use smooth falloff: full depth at center, 0 at edge
                        float normalizedDistance = distanceWorld / pitRadiusWorld; // 0 at center, 1 at edge
                        float falloff = 1f - Mathf.Pow(normalizedDistance, edgeSmoothness); // Smooth curve

                        // Calculate new height (lower = deeper)
                        float currentHeight = heights[z, x];
                        float depthAtThisPoint = normalizedDepth * falloff;
                        heights[z, x] = Mathf.Max(0f, currentHeight - depthAtThisPoint);

                        modifiedPixels++;
                    }
                }
            }

            // Apply modified heights to terrain
            terrainData.SetHeights(0, 0, heights);

            if (showDebug)
            {
                Debug.Log($"[TerrainPitCreator] ✓ Pit created! Modified {modifiedPixels} heightmap pixels");
                Debug.Log($"[TerrainPitCreator] Pit radius: {pitRadiusWorld}m ({pitRadiusX}x{pitRadiusZ} pixels)");
                Debug.Log($"[TerrainPitCreator] ========================================");
            }
        }

        /// <summary>
        /// Creates a mesh-based pit when no Unity Terrain is available
        /// Creates a cylindrical depression in the ground
        /// </summary>
        void CreateMeshPit()
        {
            Vector3 spawnerWorldPos = monsterSpawner != null ? monsterSpawner.transform.position : Vector3.zero;

            if (showDebug)
                Debug.Log($"[TerrainPitCreator] Creating mesh pit at {spawnerWorldPos}");

            // Create a ring mesh for the pit walls
            GameObject pitWallsObj = new GameObject("PitWalls");
            pitWallsObj.transform.position = spawnerWorldPos;

            MeshFilter meshFilter = pitWallsObj.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = pitWallsObj.AddComponent<MeshRenderer>();

            // Create cylinder mesh for pit walls
            Mesh pitMesh = CreateCylinderMesh(pitDiameter / 2f, pitDepth, 32);
            meshFilter.mesh = pitMesh;

            // Create dark brown/dirt material
            Material pitMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            pitMaterial.color = new Color(0.3f, 0.2f, 0.1f); // Dark brown
            pitMaterial.SetFloat("_Smoothness", 0.1f);
            pitMaterial.SetFloat("_Metallic", 0f);
            meshRenderer.material = pitMaterial;

            // Add collider to the pit bottom
            GameObject pitFloorObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pitFloorObj.name = "PitFloor";
            pitFloorObj.transform.SetParent(pitWallsObj.transform);
            pitFloorObj.transform.localPosition = new Vector3(0f, -pitDepth, 0f);
            pitFloorObj.transform.localScale = new Vector3(pitDiameter, 0.1f, pitDiameter);

            // Make pit floor darker
            MeshRenderer floorRenderer = pitFloorObj.GetComponent<MeshRenderer>();
            Material floorMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            floorMaterial.color = new Color(0.2f, 0.15f, 0.08f); // Darker brown
            floorMaterial.SetFloat("_Smoothness", 0.2f);
            floorRenderer.material = floorMaterial;

            // Lower the spawner into the pit
            if (monsterSpawner != null)
            {
                Vector3 spawnerPos = monsterSpawner.transform.position;
                spawnerPos.y -= pitDepth * 0.5f; // Halfway down into pit
                monsterSpawner.transform.position = spawnerPos;
            }

            if (showDebug)
                Debug.Log($"[TerrainPitCreator] ✓ Mesh pit created with diameter {pitDiameter}m and depth {pitDepth}m");
        }

        /// <summary>
        /// Creates a hollow cylinder mesh for pit walls
        /// </summary>
        Mesh CreateCylinderMesh(float radius, float height, int segments)
        {
            Mesh mesh = new Mesh();
            mesh.name = "PitCylinder";

            // Create vertices (inner ring at top, outer ring at top, inner at bottom, outer at bottom)
            Vector3[] vertices = new Vector3[segments * 4];
            Vector2[] uvs = new Vector2[segments * 4];
            int[] triangles = new int[segments * 6 * 2]; // Outer wall + inner wall

            float angleStep = 360f / segments;
            float innerRadius = radius * 0.95f; // Slight inner ring for thickness

            for (int i = 0; i < segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle);
                float z = Mathf.Sin(angle);

                // Top outer
                vertices[i] = new Vector3(x * radius, 0f, z * radius);
                uvs[i] = new Vector2((float)i / segments, 1f);

                // Top inner
                vertices[i + segments] = new Vector3(x * innerRadius, 0f, z * innerRadius);
                uvs[i + segments] = new Vector2((float)i / segments, 0.9f);

                // Bottom outer
                vertices[i + segments * 2] = new Vector3(x * radius, -height, z * radius);
                uvs[i + segments * 2] = new Vector2((float)i / segments, 0f);

                // Bottom inner
                vertices[i + segments * 3] = new Vector3(x * innerRadius, -height, z * innerRadius);
                uvs[i + segments * 3] = new Vector2((float)i / segments, 0.1f);
            }

            // Build triangles for outer wall
            int triIndex = 0;
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;

                // Outer wall quad (2 triangles)
                triangles[triIndex++] = i;
                triangles[triIndex++] = i + segments * 2;
                triangles[triIndex++] = next;

                triangles[triIndex++] = next;
                triangles[triIndex++] = i + segments * 2;
                triangles[triIndex++] = next + segments * 2;

                // Inner wall quad (2 triangles, reversed winding)
                triangles[triIndex++] = i + segments;
                triangles[triIndex++] = next + segments;
                triangles[triIndex++] = i + segments * 3;

                triangles[triIndex++] = next + segments;
                triangles[triIndex++] = next + segments * 3;
                triangles[triIndex++] = i + segments * 3;
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        void OnDrawGizmosSelected()
        {
            if (monsterSpawner == null) return;

            // Draw pit area
            Gizmos.color = Color.red;
            Vector3 spawnerPos = monsterSpawner.transform.position;

            // Draw pit circle at ground level
            DrawCircle(spawnerPos, pitDiameter / 2f, Color.red);

            // Draw pit bottom circle
            DrawCircle(spawnerPos - Vector3.up * pitDepth, pitDiameter / 2f, Color.yellow);

            // Draw depth indicator
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(spawnerPos, spawnerPos - Vector3.up * pitDepth);
        }

        void DrawCircle(Vector3 center, float radius, Color color)
        {
            Gizmos.color = color;
            int segments = 32;
            float angleStep = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep * Mathf.Deg2Rad;
                float angle2 = ((i + 1) % segments) * angleStep * Mathf.Deg2Rad;

                Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0f, Mathf.Sin(angle1) * radius);
                Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0f, Mathf.Sin(angle2) * radius);

                Gizmos.DrawLine(point1, point2);
            }
        }
    }
}
