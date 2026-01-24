using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace VRDungeonCrawler.Dungeon
{
    /// <summary>
    /// Production-ready procedural dungeon generation with diverse room types
    /// Creates fully-featured rooms starting from the entrance room's north wall
    /// Includes combat rooms, treasure rooms, shop room, and boss room with door
    /// </summary>
    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Dungeon Settings")]
        [Tooltip("Total number of rooms to generate (including special rooms)")]
        [Range(5, 20)]
        public int totalRoomCount = 12;

        [Tooltip("Room size in grid units (2m per grid)")]
        [Range(3, 8)]
        public int roomSizeInGrids = 5;

        [Header("Generation Settings")]
        [Tooltip("Random seed (0 = random)")]
        public int seed = 0;

        [Tooltip("Chance for room to branch (0-1)")]
        [Range(0f, 1f)]
        public float branchChance = 0.3f;

        [Tooltip("Max attempts to place a room")]
        public int maxPlacementAttempts = 20;

        [Header("Room Content")]
        [Tooltip("Min/max pillars per room")]
        public Vector2Int pillarCount = new Vector2Int(2, 6);

        [Tooltip("Torches per room")]
        [Range(2, 8)]
        public int torchesPerRoom = 4;

        [Header("Debug")]
        [Tooltip("Show debug information")]
        public bool showDebug = true;

        // Internal state
        private List<DungeonRoom> allRooms = new List<DungeonRoom>();
        private HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
        private DungeonRoom shopRoom;
        private DungeonRoom bossRoom;
        private System.Random rng;

        void Start()
        {
            GenerateDungeon();
        }

        public void GenerateDungeon()
        {
            if (showDebug)
            {
                Debug.Log("========================================");
                Debug.Log("[DungeonGenerator] Starting dungeon generation...");
                Debug.Log($"[DungeonGenerator] Target room count: {totalRoomCount}");
                Debug.Log("========================================");
            }

            // Initialize random number generator
            rng = seed != 0 ? new System.Random(seed) : new System.Random();

            // Clear previous dungeon
            ClearDungeon();

            // Generate room layout using branching algorithm
            GenerateRoomLayout();

            // Build actual room geometry
            BuildRoomGeometry();

            // Connect rooms with corridors
            ConnectRooms();

            // Add decorations and features
            DecorateRooms();

            if (showDebug)
            {
                Debug.Log("========================================");
                Debug.Log($"[DungeonGenerator] ✓ Generated {allRooms.Count} rooms successfully");
                Debug.Log($"[DungeonGenerator] Shop room at: {shopRoom?.gridPosition}");
                Debug.Log($"[DungeonGenerator] Boss room at: {bossRoom?.gridPosition}");
                Debug.Log("========================================");
            }
        }

        private void ClearDungeon()
        {
            foreach (var room in allRooms)
            {
                if (room.roomObject != null)
                    Destroy(room.roomObject);
            }

            allRooms.Clear();
            occupiedCells.Clear();
        }

        private void GenerateRoomLayout()
        {
            // Start from entrance room's north wall (0, 0) in grid space
            // Entrance room is at origin, first procedural room is north (Z+)
            Vector2Int startCell = new Vector2Int(0, roomSizeInGrids);

            // Create starting room connected to entrance
            CreateRoom(startCell, RoomType.Start);

            // Queue for breadth-first generation
            Queue<DungeonRoom> roomQueue = new Queue<DungeonRoom>();
            roomQueue.Enqueue(allRooms[0]);

            int roomsToGenerate = totalRoomCount - 3; // Reserve slots for start, shop, boss
            int normalRoomsCreated = 0;

            while (roomQueue.Count > 0 && normalRoomsCreated < roomsToGenerate)
            {
                DungeonRoom currentRoom = roomQueue.Dequeue();

                // Try to branch from this room
                Vector2Int[] directions = GetRandomDirections();

                foreach (var dir in directions)
                {
                    if (normalRoomsCreated >= roomsToGenerate)
                        break;

                    // Decide whether to branch
                    bool shouldBranch = (float)rng.NextDouble() < branchChance || roomQueue.Count == 0;

                    if (shouldBranch)
                    {
                        Vector2Int newCell = currentRoom.gridPosition + dir * roomSizeInGrids;

                        if (CanPlaceRoom(newCell))
                        {
                            RoomType type = DetermineRoomType(normalRoomsCreated, roomsToGenerate);
                            DungeonRoom newRoom = CreateRoom(newCell, type);
                            newRoom.connectedFrom = currentRoom;
                            roomQueue.Enqueue(newRoom);
                            normalRoomsCreated++;
                        }
                    }
                }
            }

            // Place shop room branching from a random room
            PlaceSpecialRoom(RoomType.Shop);

            // Place boss room at furthest point from start
            PlaceBossRoom();
        }

        private RoomType DetermineRoomType(int index, int total)
        {
            float progress = (float)index / total;

            // More challenging rooms as you progress
            if (progress < 0.3f)
                return RoomType.Combat;
            else if (progress < 0.6f)
                return (float)rng.NextDouble() < 0.7f ? RoomType.Combat : RoomType.Treasure;
            else
                return (float)rng.NextDouble() < 0.5f ? RoomType.Combat : RoomType.Puzzle;
        }

        private DungeonRoom CreateRoom(Vector2Int gridPos, RoomType type)
        {
            // Mark grid cells as occupied
            for (int x = 0; x < roomSizeInGrids; x++)
            {
                for (int y = 0; y < roomSizeInGrids; y++)
                {
                    occupiedCells.Add(new Vector2Int(gridPos.x + x, gridPos.y + y));
                }
            }

            Vector3 worldPos = new Vector3(
                gridPos.x * DungeonRoomBuilder.GRID_SIZE,
                0,
                gridPos.y * DungeonRoomBuilder.GRID_SIZE
            );

            DungeonRoom room = new DungeonRoom
            {
                gridPosition = gridPos,
                worldPosition = worldPos,
                roomType = type,
                sizeInGrids = roomSizeInGrids
            };

            allRooms.Add(room);

            if (type == RoomType.Shop)
                shopRoom = room;
            else if (type == RoomType.Boss)
                bossRoom = room;

            return room;
        }

        private bool CanPlaceRoom(Vector2Int gridPos)
        {
            // Check if all cells for this room are free
            for (int x = 0; x < roomSizeInGrids; x++)
            {
                for (int y = 0; y < roomSizeInGrids; y++)
                {
                    Vector2Int cell = new Vector2Int(gridPos.x + x, gridPos.y + y);
                    if (occupiedCells.Contains(cell))
                        return false;
                }
            }

            return true;
        }

        private Vector2Int[] GetRandomDirections()
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

            // Shuffle directions
            for (int i = 0; i < directions.Length; i++)
            {
                int randomIndex = rng.Next(i, directions.Length);
                Vector2Int temp = directions[i];
                directions[i] = directions[randomIndex];
                directions[randomIndex] = temp;
            }

            return directions;
        }

        private void PlaceSpecialRoom(RoomType type)
        {
            // Find a random non-special room to branch from
            List<DungeonRoom> candidateRooms = allRooms
                .Where(r => r.roomType != RoomType.Shop && r.roomType != RoomType.Boss)
                .ToList();

            if (candidateRooms.Count == 0)
                return;

            DungeonRoom branchFrom = candidateRooms[rng.Next(candidateRooms.Count)];

            // Try directions until we find a valid placement
            Vector2Int[] directions = GetRandomDirections();
            foreach (var dir in directions)
            {
                Vector2Int newCell = branchFrom.gridPosition + dir * roomSizeInGrids;

                if (CanPlaceRoom(newCell))
                {
                    DungeonRoom specialRoom = CreateRoom(newCell, type);
                    specialRoom.connectedFrom = branchFrom;
                    return;
                }
            }
        }

        private void PlaceBossRoom()
        {
            // Find furthest room from start
            DungeonRoom furthestRoom = allRooms[0];
            float maxDist = 0;

            foreach (var room in allRooms)
            {
                float dist = Vector2Int.Distance(Vector2Int.zero, room.gridPosition);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    furthestRoom = room;
                }
            }

            // Try to place boss room adjacent to furthest room
            Vector2Int[] directions = GetRandomDirections();
            foreach (var dir in directions)
            {
                Vector2Int newCell = furthestRoom.gridPosition + dir * roomSizeInGrids;

                if (CanPlaceRoom(newCell))
                {
                    DungeonRoom boss = CreateRoom(newCell, RoomType.Boss);
                    boss.connectedFrom = furthestRoom;
                    return;
                }
            }
        }

        private void BuildRoomGeometry()
        {
            foreach (var room in allRooms)
            {
                GameObject roomObj = DungeonRoomBuilder.CreateRoom(
                    $"{room.roomType}Room_{room.gridPosition}",
                    room.sizeInGrids,
                    room.sizeInGrids,
                    transform
                );

                roomObj.transform.position = room.worldPosition;
                room.roomObject = roomObj;

                // Add TeleportationArea to floor
                AddTeleportationToFloor(roomObj);

                // Add ceiling
                GameObject ceiling = DungeonRoomBuilder.CreateCeiling(room.sizeInGrids, room.sizeInGrids);
                ceiling.transform.SetParent(roomObj.transform);

                if (showDebug)
                    Debug.Log($"[DungeonGenerator] Built {room.roomType} room at {room.worldPosition}");
            }
        }

        private void AddTeleportationToFloor(GameObject roomObj)
        {
            // Find all floor tiles and add TeleportationArea
            Transform floorParent = roomObj.transform.Find("Floor");
            if (floorParent != null)
            {
                foreach (Transform floorTile in floorParent)
                {
                    var teleportArea = floorTile.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea>();
                    teleportArea.interactionLayers = UnityEngine.XR.Interaction.Toolkit.InteractionLayerMask.GetMask("Teleport");
                }
            }
        }

        private void ConnectRooms()
        {
            // Create doorways where rooms connect
            foreach (var room in allRooms)
            {
                if (room.connectedFrom != null)
                {
                    CreateDoorwayBetweenRooms(room.connectedFrom, room);
                }
            }
        }

        private void CreateDoorwayBetweenRooms(DungeonRoom roomA, DungeonRoom roomB)
        {
            // Determine which walls face each other
            Vector3 direction = (roomB.worldPosition - roomA.worldPosition).normalized;

            // Create doorway in roomA's wall facing roomB
            GameObject doorway = DungeonRoomBuilder.CreateDoorway();
            doorway.transform.SetParent(roomA.roomObject.transform);

            float halfSize = roomSizeInGrids * DungeonRoomBuilder.GRID_SIZE / 2f;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                // East-West corridor
                doorway.transform.localPosition = new Vector3(
                    direction.x > 0 ? halfSize : -halfSize,
                    0,
                    0
                );
                doorway.transform.localRotation = Quaternion.Euler(0, direction.x > 0 ? 90 : -90, 0);
            }
            else
            {
                // North-South corridor
                doorway.transform.localPosition = new Vector3(
                    0,
                    0,
                    direction.z > 0 ? halfSize : -halfSize
                );
                doorway.transform.localRotation = Quaternion.Euler(0, direction.z > 0 ? 0 : 180, 0);
            }

            // Remove wall section where doorway is
            RemoveWallSection(roomA.roomObject, doorway.transform.position, direction);
        }

        private void RemoveWallSection(GameObject roomObj, Vector3 doorwayPos, Vector3 direction)
        {
            Transform walls = roomObj.transform.Find("Walls");
            if (walls == null) return;

            // Find and destroy the wall segment at the doorway position
            string wallName = GetWallName(direction);
            Transform wall = walls.Find(wallName);

            if (wall != null)
            {
                // Create gap in wall by destroying it
                // In production, you'd split the wall into segments
                Destroy(wall.gameObject);
            }
        }

        private string GetWallName(Vector3 direction)
        {
            if (direction.z > 0.5f) return "NorthWall";
            if (direction.z < -0.5f) return "SouthWall";
            if (direction.x > 0.5f) return "EastWall";
            if (direction.x < -0.5f) return "WestWall";
            return "";
        }

        private void DecorateRooms()
        {
            foreach (var room in allRooms)
            {
                switch (room.roomType)
                {
                    case RoomType.Start:
                    case RoomType.Combat:
                        DecorateCombatRoom(room);
                        break;
                    case RoomType.Treasure:
                        DecorateTreasureRoom(room);
                        break;
                    case RoomType.Puzzle:
                        DecoratePuzzleRoom(room);
                        break;
                    case RoomType.Shop:
                        DecorateShopRoom(room);
                        break;
                    case RoomType.Boss:
                        DecorateBossRoom(room);
                        break;
                }
            }
        }

        private void DecorateCombatRoom(DungeonRoom room)
        {
            // Add pillars for cover
            int pillarCount = rng.Next(this.pillarCount.x, this.pillarCount.y + 1);
            float roomRadius = roomSizeInGrids * DungeonRoomBuilder.GRID_SIZE * 0.35f;

            for (int i = 0; i < pillarCount; i++)
            {
                Vector3 localPos = GetRandomPositionInRoom(roomRadius);
                GameObject pillar = DungeonRoomBuilder.CreatePillar();
                pillar.transform.SetParent(room.roomObject.transform);
                pillar.transform.localPosition = localPos;
            }

            // Add wall torches
            AddWallTorches(room);
        }

        private void DecorateTreasureRoom(DungeonRoom room)
        {
            // Central pedestal with chest
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(room.roomObject.transform);
            pedestal.transform.localPosition = Vector3.zero;
            pedestal.transform.localScale = new Vector3(1.5f, 0.8f, 1.5f);
            pedestal.GetComponent<Renderer>().material = DungeonRoomBuilder.CreatePolytopiaStone(DungeonRoomBuilder.STONE_ACCENT);

            // Chest placeholder (would be actual chest prefab in production)
            GameObject chest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chest.name = "TreasureChest";
            chest.transform.SetParent(room.roomObject.transform);
            chest.transform.localPosition = new Vector3(0, 1.2f, 0);
            chest.transform.localScale = new Vector3(1f, 0.8f, 1f);

            Material chestMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            chestMat.color = new Color(0.6f, 0.4f, 0.2f); // Brown wood
            chest.GetComponent<Renderer>().material = chestMat;

            // Corner pillars
            float cornerOffset = roomSizeInGrids * DungeonRoomBuilder.GRID_SIZE * 0.4f;
            Vector3[] corners = {
                new Vector3(-cornerOffset, 0, -cornerOffset),
                new Vector3(cornerOffset, 0, -cornerOffset),
                new Vector3(-cornerOffset, 0, cornerOffset),
                new Vector3(cornerOffset, 0, cornerOffset)
            };

            foreach (var corner in corners)
            {
                GameObject pillar = DungeonRoomBuilder.CreatePillar();
                pillar.transform.SetParent(room.roomObject.transform);
                pillar.transform.localPosition = corner;
            }

            AddWallTorches(room);
        }

        private void DecoratePuzzleRoom(DungeonRoom room)
        {
            // Create pressure plate puzzle elements
            int plateCount = rng.Next(3, 6);
            float roomRadius = roomSizeInGrids * DungeonRoomBuilder.GRID_SIZE * 0.3f;

            for (int i = 0; i < plateCount; i++)
            {
                Vector3 localPos = GetRandomPositionInRoom(roomRadius);
                localPos.y = 0.05f;

                GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                plate.name = $"PressurePlate_{i}";
                plate.transform.SetParent(room.roomObject.transform);
                plate.transform.localPosition = localPos;
                plate.transform.localScale = new Vector3(1f, 0.1f, 1f);

                Material plateMat = DungeonRoomBuilder.CreatePolytopiaStone(new Color(0.5f, 0.5f, 0.6f));
                plate.GetComponent<Renderer>().material = plateMat;
            }

            // Add some pillars as obstacles
            AddWallTorches(room);
        }

        private void DecorateShopRoom(DungeonRoom room)
        {
            // Central shop counter
            GameObject counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "ShopCounter";
            counter.transform.SetParent(room.roomObject.transform);
            counter.transform.localPosition = Vector3.zero;
            counter.transform.localScale = new Vector3(3f, 1.2f, 1.5f);

            Material counterMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            counterMat.color = new Color(0.4f, 0.3f, 0.2f);
            counter.GetComponent<Renderer>().material = counterMat;

            // Shop items on counter (placeholder cubes)
            for (int i = 0; i < 3; i++)
            {
                GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
                item.name = $"ShopItem_{i}";
                item.transform.SetParent(room.roomObject.transform);
                item.transform.localPosition = new Vector3((i - 1) * 1.2f, 1.5f, 0);
                item.transform.localScale = Vector3.one * 0.4f;

                // Random colored items
                Material itemMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                itemMat.color = new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
                itemMat.EnableKeyword("_EMISSION");
                itemMat.SetColor("_EmissionColor", itemMat.color);
                item.GetComponent<Renderer>().material = itemMat;
            }

            // Extra bright lighting for shop
            GameObject shopLight = new GameObject("ShopLight");
            shopLight.transform.SetParent(room.roomObject.transform);
            shopLight.transform.localPosition = new Vector3(0, DungeonRoomBuilder.WALL_HEIGHT * 0.8f, 0);

            Light light = shopLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Color.white;
            light.intensity = 5f;
            light.range = 15f;

            AddWallTorches(room);
        }

        private void DecorateBossRoom(DungeonRoom room)
        {
            // Boss room should be more dramatic
            // Add columns around perimeter
            float columnRadius = roomSizeInGrids * DungeonRoomBuilder.GRID_SIZE * 0.4f;
            int columnCount = 8;

            for (int i = 0; i < columnCount; i++)
            {
                float angle = (float)i / columnCount * Mathf.PI * 2f;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * columnRadius,
                    0,
                    Mathf.Sin(angle) * columnRadius
                );

                GameObject pillar = DungeonRoomBuilder.CreatePillar(DungeonRoomBuilder.WALL_HEIGHT * 1.2f);
                pillar.transform.SetParent(room.roomObject.transform);
                pillar.transform.localPosition = pos;
            }

            // Boss door at entrance
            if (room.connectedFrom != null)
            {
                Vector3 doorDirection = (room.connectedFrom.worldPosition - room.worldPosition).normalized;
                float halfSize = roomSizeInGrids * DungeonRoomBuilder.GRID_SIZE / 2f;

                Vector3 doorPos = new Vector3(
                    doorDirection.x * halfSize,
                    0,
                    doorDirection.z * halfSize
                );

                GameObject bossDoorObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bossDoorObj.name = "BossDoor";
                bossDoorObj.transform.SetParent(room.roomObject.transform);
                bossDoorObj.transform.localPosition = doorPos;
                bossDoorObj.transform.localScale = new Vector3(3f, 4f, 0.5f);

                // Red glowing locked door material
                Material doorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                doorMat.color = new Color(0.8f, 0.2f, 0.2f);
                doorMat.EnableKeyword("_EMISSION");
                doorMat.SetColor("_EmissionColor", Color.red * 0.5f);
                bossDoorObj.GetComponent<Renderer>().material = doorMat;

                // Add BossDoor component
                bossDoorObj.AddComponent<BossDoor>();
            }

            AddWallTorches(room, torchesPerRoom * 2); // Extra bright boss room
        }

        private void AddWallTorches(DungeonRoom room, int count = -1)
        {
            if (count == -1)
                count = torchesPerRoom;

            float wallDistance = roomSizeInGrids * DungeonRoomBuilder.GRID_SIZE * 0.48f;

            for (int i = 0; i < count; i++)
            {
                float angle = (float)i / count * Mathf.PI * 2f;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * wallDistance,
                    2.5f,
                    Mathf.Sin(angle) * wallDistance
                );

                GameObject torch = DungeonRoomBuilder.CreateWallTorch();
                torch.transform.SetParent(room.roomObject.transform);
                torch.transform.localPosition = pos;
                torch.transform.localRotation = Quaternion.LookRotation(-pos.normalized);
            }
        }

        private Vector3 GetRandomPositionInRoom(float radius)
        {
            float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
            float distance = (float)rng.NextDouble() * radius;

            return new Vector3(
                Mathf.Cos(angle) * distance,
                0,
                Mathf.Sin(angle) * distance
            );
        }

        private void OnDrawGizmos()
        {
            if (!showDebug || allRooms.Count == 0)
                return;

            foreach (var room in allRooms)
            {
                Color color = GetRoomTypeColor(room.roomType);
                Gizmos.color = color;

                float size = roomSizeInGrids * DungeonRoomBuilder.GRID_SIZE;
                Gizmos.DrawWireCube(room.worldPosition + new Vector3(size / 2f, 1f, size / 2f), new Vector3(size, 2f, size));

                // Draw connections
                if (room.connectedFrom != null)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(
                        room.worldPosition + Vector3.up,
                        room.connectedFrom.worldPosition + Vector3.up
                    );
                }
            }
        }

        private Color GetRoomTypeColor(RoomType type)
        {
            switch (type)
            {
                case RoomType.Start: return Color.green;
                case RoomType.Combat: return Color.white;
                case RoomType.Treasure: return Color.yellow;
                case RoomType.Puzzle: return Color.cyan;
                case RoomType.Shop: return new Color(1f, 0.5f, 0f); // Orange
                case RoomType.Boss: return Color.red;
                default: return Color.gray;
            }
        }
    }

    [System.Serializable]
    public class DungeonRoom
    {
        public Vector2Int gridPosition;
        public Vector3 worldPosition;
        public RoomType roomType;
        public GameObject roomObject;
        public int sizeInGrids;
        public DungeonRoom connectedFrom;
    }

    public enum RoomType
    {
        Start,      // First room after entrance
        Combat,     // Standard combat room with pillars
        Treasure,   // Contains treasure chest on pedestal
        Puzzle,     // Pressure plates or puzzle elements
        Shop,       // Shop with counter and items
        Boss        // Boss room with dramatic columns and boss door
    }
}
