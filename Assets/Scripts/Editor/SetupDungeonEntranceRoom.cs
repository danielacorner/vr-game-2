using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using VRDungeonCrawler.Dungeon;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Sets up the entrance room for Dungeon1
    /// Creates Polytopia-style low-poly dungeon with Zelda aesthetic
    /// Run from menu: Tools/VR Dungeon Crawler/Build Dungeon Entrance Room
    /// </summary>
    public class SetupDungeonEntranceRoom : UnityEditor.Editor
    {
        [MenuItem("Tools/VR Dungeon Crawler/Build Dungeon Entrance Room")]
        public static void BuildEntranceRoom()
        {
            Debug.Log("========================================");
            Debug.Log("Building Dungeon Entrance Room");
            Debug.Log("Polytopia-style low-poly aesthetic");
            Debug.Log("Ancient Dungeon VR / Zelda-inspired");
            Debug.Log("========================================");

            // Create or find entrance room parent
            GameObject entranceRoomGO = GameObject.Find("EntranceRoom");
            if (entranceRoomGO == null)
            {
                entranceRoomGO = new GameObject("EntranceRoom");
                entranceRoomGO.transform.position = Vector3.zero;
                Debug.Log("✓ Created EntranceRoom GameObject");
            }
            else
            {
                Debug.Log("✓ Found existing EntranceRoom GameObject");
            }

            // Add entrance room component
            DungeonEntranceRoom entranceRoom = entranceRoomGO.GetComponent<DungeonEntranceRoom>();
            if (entranceRoom == null)
            {
                entranceRoom = entranceRoomGO.AddComponent<DungeonEntranceRoom>();
                Debug.Log("✓ Added DungeonEntranceRoom component");
            }

            // Configure room settings
            entranceRoom.roomWidth = 8;
            entranceRoom.roomLength = 8;
            entranceRoom.addPillars = true;
            entranceRoom.addTorches = true;
            entranceRoom.addDoorways = true;
            entranceRoom.addCeiling = true;
            entranceRoom.addAmbientLight = true;
            entranceRoom.ambientIntensity = 0.3f;
            entranceRoom.showDebug = true;

            // Build the room
            entranceRoom.BuildRoom();

            // Add player spawn point
            CreatePlayerSpawnPoint(entranceRoomGO.transform);

            // Setup lighting
            SetupDungeonLighting();

            EditorUtility.SetDirty(entranceRoomGO);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log("✓✓✓ Dungeon Entrance Room Complete!");
            Debug.Log("Style: Low-poly Polytopia aesthetic");
            Debug.Log("Theme: Ancient Dungeon VR / Zelda classic");
            Debug.Log("Room size: 8x8 grid units (16m x 16m)");
            Debug.Log("Features: Pillars, torches, doorways, ceiling");
            Debug.Log("========================================");
        }

        static void CreatePlayerSpawnPoint(Transform parent)
        {
            GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
            if (spawnPoint == null)
            {
                spawnPoint = new GameObject("PlayerSpawnPoint");
                spawnPoint.transform.SetParent(parent);
                spawnPoint.transform.localPosition = new Vector3(0f, 0f, -6f); // South side of room
                spawnPoint.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); // Facing north

                // Add a visual indicator (small cube)
                GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
                indicator.name = "SpawnIndicator";
                indicator.transform.SetParent(spawnPoint.transform);
                indicator.transform.localPosition = Vector3.up * 0.5f;
                indicator.transform.localScale = Vector3.one * 0.5f;

                Material indicatorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                indicatorMat.color = new Color(0f, 1f, 0f, 0.5f);
                indicatorMat.EnableKeyword("_EMISSION");
                indicatorMat.SetColor("_EmissionColor", Color.green);
                indicator.GetComponent<Renderer>().material = indicatorMat;

                // Remove collider (visual only)
                Object.DestroyImmediate(indicator.GetComponent<Collider>());

                Debug.Log("✓ Created player spawn point at entrance");
            }
        }

        static void SetupDungeonLighting()
        {
            // Set scene ambient lighting to dark
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.15f, 0.15f, 0.2f); // Dark blue ambient

            // Disable skybox
            RenderSettings.skybox = null;

            // Set fog for atmosphere
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.1f, 0.1f, 0.15f);
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.05f;

            Debug.Log("✓ Configured dungeon lighting (dark ambient, fog)");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Clear Dungeon Room")]
        public static void ClearDungeonRoom()
        {
            GameObject entranceRoom = GameObject.Find("EntranceRoom");
            if (entranceRoom != null)
            {
                Object.DestroyImmediate(entranceRoom);
                Debug.Log("✓ Cleared dungeon entrance room");
            }

            GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
            if (spawnPoint != null)
            {
                Object.DestroyImmediate(spawnPoint);
                Debug.Log("✓ Cleared player spawn point");
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
