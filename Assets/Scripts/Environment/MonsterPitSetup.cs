using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Simple dedicated script to create a visible pit around the monster spawner
    /// Add this to any GameObject in HomeArea scene (like Terrain or a new "Environment" object)
    /// </summary>
    public class MonsterPitSetup : MonoBehaviour
    {
        [Header("Pit Configuration")]
        [Tooltip("Diameter of pit (3x spawner width)")]
        public float pitDiameter = 12f;

        [Tooltip("Depth of pit (requires double-jump to escape)")]
        public float pitDepth = 1.5f;

        [Tooltip("Smoothness of pit edges")]
        public float edgeSmoothness = 2f;

        [Header("Visual Markers")]
        [Tooltip("Create visible rim around pit")]
        public bool createVisibleRim = true;

        [Tooltip("Create debug sphere at center")]
        public bool createDebugMarker = true;

        [Header("Debug")]
        public bool showDebug = true;

        void Start()
        {
            if (showDebug)
                Debug.Log("[MonsterPitSetup] ========================================");

            // Find monster spawner
            GameObject spawner = GameObject.Find("MonsterSpawner");
            if (spawner == null)
            {
                Debug.LogError("[MonsterPitSetup] MonsterSpawner not found! Cannot create pit.");
                return;
            }

            Vector3 spawnerPos = spawner.transform.position;

            if (showDebug)
                Debug.Log($"[MonsterPitSetup] Found MonsterSpawner at {spawnerPos}");

            // Find terrain
            Terrain terrain = FindFirstObjectByType<Terrain>();

            if (terrain != null)
            {
                CreateTerrainPit(terrain, spawnerPos);
            }
            else
            {
                Debug.LogWarning("[MonsterPitSetup] No Terrain found, creating mesh pit instead");
                CreateMeshPit(spawnerPos);
            }

            // Create visual markers
            if (createDebugMarker)
            {
                CreateDebugMarker(spawnerPos);
            }

            if (createVisibleRim)
            {
                CreatePitRim(spawnerPos);
            }

            // Lower spawner into pit
            spawner.transform.position = new Vector3(spawnerPos.x, spawnerPos.y - pitDepth * 0.8f, spawnerPos.z);

            if (showDebug)
            {
                Debug.Log($"[MonsterPitSetup] ✓ Pit created at {spawnerPos}");
                Debug.Log($"[MonsterPitSetup] Diameter: {pitDiameter}m, Depth: {pitDepth}m");
                Debug.Log("[MonsterPitSetup] ========================================");
            }
        }

        void CreateTerrainPit(Terrain terrain, Vector3 centerPos)
        {
            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;
            Vector3 terrainSize = terrainData.size;

            int heightmapWidth = terrainData.heightmapResolution;
            int heightmapHeight = terrainData.heightmapResolution;

            // Get current heights
            float[,] heights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);

            // Convert world position to terrain coordinates
            Vector3 relativePos = centerPos - terrainPos;
            float normalizedX = relativePos.x / terrainSize.x;
            float normalizedZ = relativePos.z / terrainSize.z;

            int centerX = Mathf.RoundToInt(normalizedX * heightmapWidth);
            int centerZ = Mathf.RoundToInt(normalizedZ * heightmapHeight);

            // Calculate pit radius in heightmap units
            float pitRadiusWorld = pitDiameter / 2f;
            float normalizedDepth = pitDepth / terrainSize.y;

            if (showDebug)
            {
                Debug.Log($"[MonsterPitSetup] Terrain size: {terrainSize}");
                Debug.Log($"[MonsterPitSetup] Heightmap resolution: {heightmapWidth}x{heightmapHeight}");
                Debug.Log($"[MonsterPitSetup] Pit center (heightmap): ({centerX}, {centerZ})");
                Debug.Log($"[MonsterPitSetup] Normalized depth: {normalizedDepth}");
            }

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

            if (showDebug)
                Debug.Log($"[MonsterPitSetup] ✓ Modified {modifiedPixels} terrain heightmap pixels");
        }

        void CreateMeshPit(Vector3 centerPos)
        {
            // Create visible pit walls and floor
            GameObject pitObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pitObj.name = "PitWall";
            pitObj.transform.position = centerPos - Vector3.up * (pitDepth / 2f);
            pitObj.transform.localScale = new Vector3(pitDiameter, pitDepth, pitDiameter);

            // Make it brown/dirt colored
            MeshRenderer renderer = pitObj.GetComponent<MeshRenderer>();
            Material pitMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            pitMat.color = new Color(0.3f, 0.2f, 0.1f); // Dark brown
            renderer.material = pitMat;

            // Remove top cap, keep only walls
            Destroy(pitObj.GetComponent<Collider>());

            if (showDebug)
                Debug.Log($"[MonsterPitSetup] ✓ Created mesh pit");
        }

        void CreatePitRim(Vector3 centerPos)
        {
            // Create a visible rim using a torus-like ring
            GameObject rim = new GameObject("PitRim");
            rim.transform.position = centerPos;

            // Create multiple cylinders in a ring
            int segments = 32;
            float radius = pitDiameter / 2f;

            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * 360f * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                GameObject rimSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rimSegment.name = $"RimSegment_{i}";
                rimSegment.transform.SetParent(rim.transform);
                rimSegment.transform.localPosition = new Vector3(x, 0.1f, z);
                rimSegment.transform.localScale = new Vector3(0.3f, 0.2f, 0.8f);
                rimSegment.transform.LookAt(centerPos);

                // Make it stone colored
                MeshRenderer renderer = rimSegment.GetComponent<MeshRenderer>();
                Material stoneMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                stoneMat.color = new Color(0.5f, 0.5f, 0.5f); // Gray stone
                renderer.material = stoneMat;

                // Remove collider
                Destroy(rimSegment.GetComponent<Collider>());
            }

            if (showDebug)
                Debug.Log($"[MonsterPitSetup] ✓ Created visible rim with {segments} segments");
        }

        void CreateDebugMarker(Vector3 centerPos)
        {
            // Create a visible sphere at pit center
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "PitCenterMarker";
            marker.transform.position = centerPos + Vector3.up * 0.5f;
            marker.transform.localScale = Vector3.one * 0.5f;

            MeshRenderer renderer = marker.GetComponent<MeshRenderer>();
            Material markerMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            markerMat.color = Color.yellow;
            markerMat.SetFloat("_Metallic", 0);
            markerMat.SetFloat("_Smoothness", 0.5f);
            renderer.material = markerMat;

            // Make it emissive for visibility
            markerMat.EnableKeyword("_EMISSION");
            markerMat.SetColor("_EmissionColor", Color.yellow * 0.5f);

            if (showDebug)
                Debug.Log($"[MonsterPitSetup] ✓ Created debug marker at center");
        }

        void OnDrawGizmosSelected()
        {
            // Draw pit visualization
            GameObject spawner = GameObject.Find("MonsterSpawner");
            if (spawner == null) return;

            Vector3 pos = spawner.transform.position;

            // Draw pit circle at ground level
            Gizmos.color = Color.red;
            DrawCircle(pos, pitDiameter / 2f);

            // Draw pit bottom
            Gizmos.color = Color.yellow;
            DrawCircle(pos - Vector3.up * pitDepth, pitDiameter / 2f);

            // Draw depth line
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pos, pos - Vector3.up * pitDepth);
        }

        void DrawCircle(Vector3 center, float radius)
        {
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
