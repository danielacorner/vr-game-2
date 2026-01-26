using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Super simple pit creator - just creates visible objects at fixed positions
    /// Easy to see and debug
    /// </summary>
    public static class SimpleMonsterPitCreator
    {
        [MenuItem("Tools/VR Dungeon Crawler/SIMPLE: Create Visible Pit Objects", priority = 140)]
        public static void CreateSimplePit()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name != "HomeArea")
            {
                EditorUtility.DisplayDialog("Wrong Scene", "Please open HomeArea scene first.", "OK");
                return;
            }

            Debug.Log("========================================");
            Debug.Log("[SimplePit] Starting pit creation...");

            // Fixed position at origin for testing
            Vector3 pitCenter = new Vector3(0f, 0f, 0f);

            // Find monster spawner if it exists
            GameObject spawner = GameObject.Find("MonsterSpawner");
            if (spawner != null)
            {
                pitCenter = spawner.transform.position;
                Debug.Log($"[SimplePit] Found MonsterSpawner at {pitCenter}");
            }
            else
            {
                Debug.LogWarning("[SimplePit] No MonsterSpawner found, using origin (0,0,0)");
            }

            // Clear any existing pit objects
            GameObject existingPit = GameObject.Find("PIT_OBJECTS");
            if (existingPit != null)
            {
                Object.DestroyImmediate(existingPit);
                Debug.Log("[SimplePit] Cleared existing pit objects");
            }

            // Create parent container
            GameObject pitContainer = new GameObject("PIT_OBJECTS");
            pitContainer.transform.position = pitCenter;

            // 1. Create GIANT yellow sphere at center - can't miss it!
            GameObject centerMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            centerMarker.name = "CENTER_YELLOW_SPHERE";
            centerMarker.transform.SetParent(pitContainer.transform);
            centerMarker.transform.position = pitCenter + Vector3.up * 2f; // 2m up in air
            centerMarker.transform.localScale = Vector3.one * 2f; // 2m diameter sphere!

            MeshRenderer centerRenderer = centerMarker.GetComponent<MeshRenderer>();
            Material yellowMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            yellowMat.color = Color.yellow;
            yellowMat.EnableKeyword("_EMISSION");
            yellowMat.SetColor("_EmissionColor", Color.yellow * 3f);
            centerRenderer.material = yellowMat;
            Object.DestroyImmediate(centerMarker.GetComponent<Collider>());

            Debug.Log($"[SimplePit] Created GIANT yellow sphere at {centerMarker.transform.position}");

            // 2. Create red cubes in a circle around center (rim markers)
            float rimRadius = 6f; // 12m diameter
            int rimSegments = 16;

            for (int i = 0; i < rimSegments; i++)
            {
                float angle = (i / (float)rimSegments) * 360f * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * rimRadius;
                float z = Mathf.Sin(angle) * rimRadius;

                GameObject rimCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rimCube.name = $"RIM_RED_CUBE_{i}";
                rimCube.transform.SetParent(pitContainer.transform);
                rimCube.transform.position = pitCenter + new Vector3(x, 0.5f, z); // 0.5m up
                rimCube.transform.localScale = Vector3.one * 1f; // 1m cubes

                MeshRenderer rimRenderer = rimCube.GetComponent<MeshRenderer>();
                Material redMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                redMat.color = Color.red;
                redMat.EnableKeyword("_EMISSION");
                redMat.SetColor("_EmissionColor", Color.red * 2f);
                rimRenderer.material = redMat;
                Object.DestroyImmediate(rimCube.GetComponent<Collider>());
            }

            Debug.Log($"[SimplePit] Created {rimSegments} RED cubes in rim circle");

            // 3. Create blue cubes at pit floor (3m below)
            float pitDepth = 3f;
            GameObject floorContainer = new GameObject("FLOOR_BLUE_CUBES");
            floorContainer.transform.SetParent(pitContainer.transform);
            floorContainer.transform.position = pitCenter - Vector3.up * pitDepth;

            for (int i = -2; i <= 2; i++)
            {
                for (int j = -2; j <= 2; j++)
                {
                    GameObject floorCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    floorCube.name = $"FLOOR_BLUE_CUBE_{i}_{j}";
                    floorCube.transform.SetParent(floorContainer.transform);
                    floorCube.transform.localPosition = new Vector3(i * 1.5f, 0f, j * 1.5f);
                    floorCube.transform.localScale = Vector3.one * 0.5f;

                    MeshRenderer floorRenderer = floorCube.GetComponent<MeshRenderer>();
                    Material blueMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    blueMat.color = Color.blue;
                    blueMat.EnableKeyword("_EMISSION");
                    blueMat.SetColor("_EmissionColor", Color.blue * 2f);
                    floorRenderer.material = blueMat;
                    Object.DestroyImmediate(floorCube.GetComponent<Collider>());
                }
            }

            Debug.Log($"[SimplePit] Created BLUE cubes on floor at {floorContainer.transform.position}");

            // 4. Create green sphere at spawner location
            if (spawner != null)
            {
                GameObject spawnerMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spawnerMarker.name = "SPAWNER_GREEN_MARKER";
                spawnerMarker.transform.SetParent(pitContainer.transform);
                spawnerMarker.transform.position = spawner.transform.position + Vector3.up * 0.5f;
                spawnerMarker.transform.localScale = Vector3.one * 1f;

                MeshRenderer spawnerRenderer = spawnerMarker.GetComponent<MeshRenderer>();
                Material greenMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                greenMat.color = Color.green;
                greenMat.EnableKeyword("_EMISSION");
                greenMat.SetColor("_EmissionColor", Color.green * 2f);
                spawnerRenderer.material = greenMat;
                Object.DestroyImmediate(spawnerMarker.GetComponent<Collider>());

                Debug.Log($"[SimplePit] Created GREEN sphere at spawner location {spawnerMarker.transform.position}");
            }

            // Select the pit container so you can see it in hierarchy
            Selection.activeGameObject = pitContainer;

            // Mark scene dirty
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("========================================");
            Debug.Log("[SimplePit] ✓✓✓ DONE! ✓✓✓");
            Debug.Log($"[SimplePit] Look for:");
            Debug.Log($"  - GIANT YELLOW SPHERE at {pitCenter + Vector3.up * 2f}");
            Debug.Log($"  - RED CUBES in circle (rim)");
            Debug.Log($"  - BLUE CUBES 3m below ground (floor)");
            Debug.Log($"  - GREEN SPHERE at spawner");
            Debug.Log($"[SimplePit] Check Unity Hierarchy for 'PIT_OBJECTS'");
            Debug.Log("========================================");

            EditorUtility.DisplayDialog(
                "Pit Objects Created!",
                "Created visible marker objects:\n\n" +
                "✓ GIANT YELLOW sphere (2m up in air)\n" +
                "✓ RED cubes (rim circle)\n" +
                "✓ BLUE cubes (floor, 3m down)\n" +
                "✓ GREEN sphere (spawner location)\n\n" +
                "Look in Scene view and Hierarchy for 'PIT_OBJECTS'.\n\n" +
                "If you can't see them, check:\n" +
                "1. Is HomeArea scene open?\n" +
                "2. Look in Hierarchy for 'PIT_OBJECTS'\n" +
                "3. Select it and press 'F' to focus camera",
                "OK"
            );
        }
    }
}
