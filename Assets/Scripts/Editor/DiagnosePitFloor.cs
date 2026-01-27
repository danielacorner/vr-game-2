using UnityEngine;
using UnityEditor;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Diagnostic tool to find what's creating the flat floor in the pit
    /// </summary>
    public static class DiagnosePitFloor
    {
        [MenuItem("Tools/VR Dungeon Crawler/Diagnose Pit Floor", priority = 135)]
        public static void DiagnoseFloor()
        {
            Debug.Log("========================================");
            Debug.Log("[DiagnosePit] Starting diagnosis...");

            Vector3 pitCenter = new Vector3(10f, 0f, 10f);

            // Find all objects near pit center
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            Debug.Log($"[DiagnosePit] Checking {allObjects.Length} objects...");

            int foundCount = 0;
            foreach (GameObject obj in allObjects)
            {
                Vector3 pos = obj.transform.position;
                float distanceXZ = Vector3.Distance(new Vector3(pos.x, 0f, pos.z), new Vector3(pitCenter.x, 0f, pitCenter.z));

                // Check if object is near pit (within 15m radius)
                if (distanceXZ < 15f)
                {
                    // Check for large flat objects (potential ground planes)
                    Vector3 scale = obj.transform.lossyScale;

                    if ((scale.x > 10f || scale.z > 10f) && scale.y < 2f)
                    {
                        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                        MeshCollider collider = obj.GetComponent<MeshCollider>();

                        if (renderer != null || collider != null)
                        {
                            foundCount++;
                            Debug.LogWarning($"[DiagnosePit] FOUND LARGE FLAT OBJECT: '{obj.name}' at {pos}, scale={scale}, hasRenderer={renderer != null}, hasCollider={collider != null}");
                        }
                    }
                }
            }

            // Check terrain info
            Terrain terrain = Object.FindFirstObjectByType<Terrain>();
            if (terrain != null)
            {
                TerrainData data = terrain.terrainData;
                Debug.Log($"[DiagnosePit] Terrain: pos={terrain.transform.position}, size={data.size}, heightmapRes={data.heightmapResolution}");

                // Sample heights around pit center
                Vector3 terrainPos = terrain.transform.position;
                Vector3 terrainSize = data.size;

                Vector3 relativePos = pitCenter - terrainPos;
                float normX = relativePos.x / terrainSize.x;
                float normZ = relativePos.z / terrainSize.z;

                if (normX >= 0f && normX <= 1f && normZ >= 0f && normZ <= 1f)
                {
                    float centerHeight = data.GetInterpolatedHeight(normX, normZ);
                    Debug.Log($"[DiagnosePit] Terrain height at pit center: {centerHeight:F3}m (normalized={data.GetHeight((int)(normX * data.heightmapResolution), (int)(normZ * data.heightmapResolution)):F3})");

                    // Check minimum height in entire heightmap
                    float[,] heights = data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);
                    float minHeight = float.MaxValue;
                    float maxHeight = float.MinValue;

                    for (int y = 0; y < data.heightmapResolution; y++)
                    {
                        for (int x = 0; x < data.heightmapResolution; x++)
                        {
                            float h = heights[y, x];
                            if (h < minHeight) minHeight = h;
                            if (h > maxHeight) maxHeight = h;
                        }
                    }

                    Debug.Log($"[DiagnosePit] Heightmap range: min={minHeight:F4} ({minHeight * data.size.y:F2}m), max={maxHeight:F4} ({maxHeight * data.size.y:F2}m)");
                }
            }

            Debug.Log("========================================");
            Debug.Log($"[DiagnosePit] Found {foundCount} large flat objects near pit");
            Debug.Log("[DiagnosePit] Check console for results!");
        }
    }
}
