using UnityEngine;

namespace VRDungeonCrawler.Dungeon
{
    /// <summary>
    /// Creates the entrance room for Dungeon1
    /// Medium-sized room with Polytopia-style low-poly aesthetic
    /// Zelda-inspired classic dungeon layout
    /// </summary>
    public class DungeonEntranceRoom : MonoBehaviour
    {
        [Header("Room Dimensions")]
        [Tooltip("Width in 2m grid units")]
        public int roomWidth = 8;

        [Tooltip("Length in 2m grid units")]
        public int roomLength = 8;

        [Header("Features")]
        [Tooltip("Add pillars in corners and middle")]
        public bool addPillars = true;

        [Tooltip("Add wall torches")]
        public bool addTorches = true;

        [Tooltip("Add entrance/exit doorways")]
        public bool addDoorways = true;

        [Tooltip("Add ceiling")]
        public bool addCeiling = true;

        [Header("Lighting")]
        [Tooltip("Add ambient light")]
        public bool addAmbientLight = true;

        [Tooltip("Ambient light intensity")]
        [Range(0f, 1f)]
        public float ambientIntensity = 0.3f;

        [Header("Debug")]
        public bool showDebug = true;

        public void BuildRoom()
        {
            // Clear existing children
            foreach (Transform child in transform)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }

            if (showDebug)
                Debug.Log("[DungeonEntranceRoom] Building entrance room...");

            // Create base room
            GameObject room = DungeonRoomBuilder.CreateRoom("EntranceRoom", roomWidth, roomLength, transform);

            // Add pillars
            if (addPillars)
            {
                CreatePillars(room.transform);
            }

            // Add torches
            if (addTorches)
            {
                CreateTorches(room.transform);
            }

            // Add doorways
            if (addDoorways)
            {
                CreateDoorways(room.transform);
            }

            // Add ceiling
            if (addCeiling)
            {
                GameObject ceiling = DungeonRoomBuilder.CreateCeiling(roomWidth, roomLength);
                ceiling.transform.SetParent(room.transform);
            }

            // Add ambient lighting
            if (addAmbientLight)
            {
                CreateAmbientLight(room.transform);
            }

            if (showDebug)
                Debug.Log("[DungeonEntranceRoom] ✓ Entrance room built successfully!");
        }

        void CreatePillars(Transform parent)
        {
            GameObject pillarsParent = new GameObject("Pillars");
            pillarsParent.transform.SetParent(parent);

            float halfWidth = roomWidth * DungeonRoomBuilder.GRID_SIZE / 2f;
            float halfLength = roomLength * DungeonRoomBuilder.GRID_SIZE / 2f;
            float inset = DungeonRoomBuilder.GRID_SIZE * 1.5f;

            // Corner pillars
            Vector3[] cornerPositions = new Vector3[]
            {
                new Vector3(-halfWidth + inset, 0f, -halfLength + inset), // SW
                new Vector3(halfWidth - inset, 0f, -halfLength + inset),  // SE
                new Vector3(-halfWidth + inset, 0f, halfLength - inset),  // NW
                new Vector3(halfWidth - inset, 0f, halfLength - inset)    // NE
            };

            for (int i = 0; i < cornerPositions.Length; i++)
            {
                GameObject pillar = DungeonRoomBuilder.CreatePillar();
                pillar.transform.SetParent(pillarsParent.transform);
                pillar.transform.localPosition = cornerPositions[i];
                pillar.name = $"CornerPillar_{i}";
            }

            // Center pillars (for larger rooms)
            if (roomWidth >= 6 && roomLength >= 6)
            {
                Vector3[] centerPositions = new Vector3[]
                {
                    new Vector3(-DungeonRoomBuilder.GRID_SIZE, 0f, 0f),
                    new Vector3(DungeonRoomBuilder.GRID_SIZE, 0f, 0f)
                };

                for (int i = 0; i < centerPositions.Length; i++)
                {
                    GameObject pillar = DungeonRoomBuilder.CreatePillar();
                    pillar.transform.SetParent(pillarsParent.transform);
                    pillar.transform.localPosition = centerPositions[i];
                    pillar.name = $"CenterPillar_{i}";
                }
            }

            if (showDebug)
                Debug.Log($"[DungeonEntranceRoom] Created {cornerPositions.Length} corner pillars");
        }

        void CreateTorches(Transform parent)
        {
            GameObject torchesParent = new GameObject("Torches");
            torchesParent.transform.SetParent(parent);

            float halfWidth = roomWidth * DungeonRoomBuilder.GRID_SIZE / 2f;
            float halfLength = roomLength * DungeonRoomBuilder.GRID_SIZE / 2f;
            float torchHeight = 2.5f;

            // North wall torches
            CreateWallTorchPair(torchesParent.transform,
                new Vector3(0f, torchHeight, halfLength - DungeonRoomBuilder.WALL_THICKNESS / 2f),
                180f, "North");

            // South wall torches
            CreateWallTorchPair(torchesParent.transform,
                new Vector3(0f, torchHeight, -halfLength + DungeonRoomBuilder.WALL_THICKNESS / 2f),
                0f, "South");

            // East wall torches
            CreateWallTorchPair(torchesParent.transform,
                new Vector3(halfWidth - DungeonRoomBuilder.WALL_THICKNESS / 2f, torchHeight, 0f),
                270f, "East");

            // West wall torches
            CreateWallTorchPair(torchesParent.transform,
                new Vector3(-halfWidth + DungeonRoomBuilder.WALL_THICKNESS / 2f, torchHeight, 0f),
                90f, "West");

            if (showDebug)
                Debug.Log("[DungeonEntranceRoom] Created wall torches");
        }

        void CreateWallTorchPair(Transform parent, Vector3 centerPos, float rotation, string wallName)
        {
            float spacing = DungeonRoomBuilder.GRID_SIZE * 2f;

            for (int i = -1; i <= 1; i += 2)
            {
                GameObject torch = DungeonRoomBuilder.CreateWallTorch();
                torch.transform.SetParent(parent);

                Vector3 offset = (rotation == 0f || rotation == 180f)
                    ? new Vector3(i * spacing, 0f, 0f)
                    : new Vector3(0f, 0f, i * spacing);

                torch.transform.localPosition = centerPos + offset;
                torch.transform.localRotation = Quaternion.Euler(0f, rotation, 0f);
                torch.name = $"{wallName}Torch_{(i > 0 ? "Right" : "Left")}";
            }
        }

        void CreateDoorways(Transform parent)
        {
            GameObject doorwaysParent = new GameObject("Doorways");
            doorwaysParent.transform.SetParent(parent);

            float halfLength = roomLength * DungeonRoomBuilder.GRID_SIZE / 2f;

            // South entrance (coming in)
            GameObject entranceDoorway = DungeonRoomBuilder.CreateDoorway();
            entranceDoorway.transform.SetParent(doorwaysParent.transform);
            entranceDoorway.transform.localPosition = new Vector3(0f, 0f, -halfLength);
            entranceDoorway.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            entranceDoorway.name = "EntranceDoorway_South";

            // North exit (going deeper)
            GameObject exitDoorway = DungeonRoomBuilder.CreateDoorway();
            exitDoorway.transform.SetParent(doorwaysParent.transform);
            exitDoorway.transform.localPosition = new Vector3(0f, 0f, halfLength);
            exitDoorway.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            exitDoorway.name = "ExitDoorway_North";

            if (showDebug)
                Debug.Log("[DungeonEntranceRoom] Created entrance and exit doorways");
        }

        void CreateAmbientLight(Transform parent)
        {
            GameObject lightObj = new GameObject("AmbientLight");
            lightObj.transform.SetParent(parent);
            lightObj.transform.localPosition = new Vector3(0f, DungeonRoomBuilder.WALL_HEIGHT / 2f, 0f);

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.6f, 0.65f, 0.75f); // Cool dungeon ambient
            light.intensity = ambientIntensity;
            light.range = roomWidth * DungeonRoomBuilder.GRID_SIZE;
            light.shadows = LightShadows.None;

            if (showDebug)
                Debug.Log("[DungeonEntranceRoom] Created ambient light");
        }
    }
}
