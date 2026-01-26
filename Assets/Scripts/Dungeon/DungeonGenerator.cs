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
        [Range(5, 40)]
        public int totalRoomCount = 25;

        [Tooltip("Room size in grid units (2m per grid)")]
        [Range(3, 8)]
        public int roomSizeInGrids = 5;

        [Header("Generation Settings")]
        [Tooltip("Random seed (0 = random)")]
        public int seed = 0;

        [Tooltip("Chance for room to branch (0-1)")]
        [Range(0f, 1f)]
        public float branchChance = 0.6f;

        [Tooltip("Minimum branches per room")]
        [Range(1, 3)]
        public int minBranchesPerRoom = 1;

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
        public bool showDebug = false;

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
            // Start from entrance room's north wall
            // Center the first room with the entrance room doorway (entrance is 30m wide, centered at X=0)
            // Room is 10m wide (5 grids * 2m), so place corner at X=-4m (-2 grids) to better align with doorway
            Vector2Int startCell = new Vector2Int(-2, roomSizeInGrids);

            if (showDebug)
            {
                Debug.Log($"[DungeonGenerator] Starting generation with startCell: {startCell}");
                Debug.Log($"[DungeonGenerator] RoomSizeInGrids: {roomSizeInGrids}, GRID_SIZE: {DungeonRoomBuilder.GRID_SIZE}");
            }

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

                // Try to branch from this room - create multiple branches for complexity
                Vector2Int[] directions = GetRandomDirections();
                int branchesCreated = 0;
                int maxBranches = rng.Next(minBranchesPerRoom, 4); // 1-3 branches per room

                foreach (var dir in directions)
                {
                    if (normalRoomsCreated >= roomsToGenerate)
                        break;

                    // Decide whether to branch - more likely if we haven't created min branches yet
                    bool shouldBranch = branchesCreated < minBranchesPerRoom ||
                                       (branchesCreated < maxBranches && (float)rng.NextDouble() < branchChance) ||
                                       roomQueue.Count == 0;

                    if (shouldBranch)
                    {
                        Vector2Int newCell = currentRoom.gridPosition + dir * roomSizeInGrids;

                        if (showDebug)
                            Debug.Log($"[DungeonGenerator] Attempting to place room at {newCell} from {currentRoom.gridPosition} in direction {dir}");

                        // CRITICAL: Prevent rooms from going into entrance room area (gridPos.y must be >= roomSizeInGrids)
                        if (newCell.y < roomSizeInGrids)
                        {
                            if (showDebug)
                                Debug.Log($"[DungeonGenerator] ✗ BLOCKED: Room at {newCell} would overlap entrance area (Y={newCell.y} < {roomSizeInGrids})");
                            continue; // Skip this direction
                        }

                        if (CanPlaceRoom(newCell))
                        {
                            RoomType type = DetermineRoomType(normalRoomsCreated, roomsToGenerate);
                            DungeonRoom newRoom = CreateRoom(newCell, type);
                            newRoom.connectedFrom = currentRoom;
                            roomQueue.Enqueue(newRoom);
                            normalRoomsCreated++;
                            branchesCreated++;

                            if (showDebug)
                                Debug.Log($"[DungeonGenerator] ✓ SUCCESS: Created room {normalRoomsCreated}/{roomsToGenerate} of type {type} at {newCell}");
                        }
                        else if (showDebug)
                        {
                            Debug.Log($"[DungeonGenerator] ✗ FAILED: Cannot place room at {newCell} - position occupied");
                        }
                    }
                    else if (showDebug)
                    {
                        Debug.Log($"[DungeonGenerator] Skipped branching in direction {dir} (shouldBranch=false, branchesCreated={branchesCreated})");
                    }
                }

                if (showDebug)
                    Debug.Log($"[DungeonGenerator] Room at {currentRoom.gridPosition} created {branchesCreated} branches");
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
            if (showDebug)
                Debug.Log($"[DungeonGenerator] CreateRoom called: type={type}, gridPos={gridPos}");

            // Mark grid cells as occupied
            for (int x = 0; x < roomSizeInGrids; x++)
            {
                for (int y = 0; y < roomSizeInGrids; y++)
                {
                    Vector2Int cell = new Vector2Int(gridPos.x + x, gridPos.y + y);
                    occupiedCells.Add(cell);

                    if (showDebug && x == 0 && y == 0)
                        Debug.Log($"[DungeonGenerator] Marking cells {gridPos} through {new Vector2Int(gridPos.x + roomSizeInGrids - 1, gridPos.y + roomSizeInGrids - 1)} as occupied");
                }
            }

            Vector3 worldPos = new Vector3(
                gridPos.x * DungeonRoomBuilder.GRID_SIZE,
                0,
                gridPos.y * DungeonRoomBuilder.GRID_SIZE
            );

            if (showDebug)
                Debug.Log($"[DungeonGenerator] Room worldPos: {worldPos}");

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

            if (showDebug)
                Debug.Log($"[DungeonGenerator] Room created successfully. Total rooms: {allRooms.Count}");

            return room;
        }

        private bool CanPlaceRoom(Vector2Int gridPos)
        {
            // CRITICAL: Prevent rooms from overlapping with entrance room area
            // Entrance room occupies Z=-5 to Z=+5 (grid Y < 5)
            // Start room is at grid Y=5, so all dungeon rooms must be at Y >= 5
            if (gridPos.y < roomSizeInGrids)
            {
                if (showDebug)
                    Debug.Log($"[DungeonGenerator] Room at {gridPos} is INVALID - too close to entrance (gridPos.y={gridPos.y} < {roomSizeInGrids})");
                return false;
            }

            // Check if all cells for this room are free
            for (int x = 0; x < roomSizeInGrids; x++)
            {
                for (int y = 0; y < roomSizeInGrids; y++)
                {
                    Vector2Int cell = new Vector2Int(gridPos.x + x, gridPos.y + y);
                    if (occupiedCells.Contains(cell))
                    {
                        if (showDebug)
                            Debug.Log($"[DungeonGenerator] Cell {cell} is occupied (checking room at {gridPos})");
                        return false;
                    }
                }
            }

            if (showDebug)
                Debug.Log($"[DungeonGenerator] Room at {gridPos} is VALID (all {roomSizeInGrids}x{roomSizeInGrids} cells free)");

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
            {
                if (showDebug)
                    Debug.LogWarning($"[DungeonGenerator] No candidate rooms to place {type}!");
                return;
            }

            // Shuffle candidates to try multiple rooms
            for (int i = 0; i < candidateRooms.Count; i++)
            {
                int randomIndex = rng.Next(i, candidateRooms.Count);
                var temp = candidateRooms[i];
                candidateRooms[i] = candidateRooms[randomIndex];
                candidateRooms[randomIndex] = temp;
            }

            // Try multiple candidate rooms until we find valid placement
            foreach (var branchFrom in candidateRooms)
            {
                // Try all directions from this room
                Vector2Int[] directions = GetRandomDirections();
                foreach (var dir in directions)
                {
                    Vector2Int newCell = branchFrom.gridPosition + dir * roomSizeInGrids;

                    if (CanPlaceRoom(newCell))
                    {
                        DungeonRoom specialRoom = CreateRoom(newCell, type);
                        specialRoom.connectedFrom = branchFrom;

                        if (showDebug)
                            Debug.Log($"[DungeonGenerator] ✓ Placed {type} room at {newCell} connected to {branchFrom.roomType} at {branchFrom.gridPosition}");
                        return;
                    }
                }
            }

            // Failed to place after trying all candidates
            if (showDebug)
                Debug.LogWarning($"[DungeonGenerator] ✗ Failed to place {type} room - no valid positions found!");
        }

        private void PlaceBossRoom()
        {
            // Sort rooms by distance from start, furthest first
            List<DungeonRoom> candidateRooms = allRooms
                .Where(r => r.roomType != RoomType.Shop && r.roomType != RoomType.Boss)
                .OrderByDescending(r => Vector2Int.Distance(Vector2Int.zero, r.gridPosition))
                .ToList();

            if (candidateRooms.Count == 0)
            {
                if (showDebug)
                    Debug.LogWarning($"[DungeonGenerator] No candidate rooms to place Boss!");
                return;
            }

            // Try furthest rooms first
            foreach (var branchFrom in candidateRooms.Take(10)) // Try up to 10 furthest rooms
            {
                Vector2Int[] directions = GetRandomDirections();
                foreach (var dir in directions)
                {
                    Vector2Int newCell = branchFrom.gridPosition + dir * roomSizeInGrids;

                    if (CanPlaceRoom(newCell))
                    {
                        DungeonRoom boss = CreateRoom(newCell, RoomType.Boss);
                        boss.connectedFrom = branchFrom;

                        if (showDebug)
                            Debug.Log($"[DungeonGenerator] ✓ Placed Boss room at {newCell} connected to {branchFrom.roomType} at {branchFrom.gridPosition}");
                        return;
                    }
                }
            }

            // Failed to place boss room
            if (showDebug)
                Debug.LogWarning($"[DungeonGenerator] ✗ Failed to place Boss room - no valid positions found!");
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

                // Add LARGE DEBUG MARKER above each room
                if (showDebug)
                {
                    GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    marker.name = "DEBUG_MARKER";
                    marker.transform.SetParent(roomObj.transform);
                    marker.transform.localPosition = new Vector3(roomSizeInGrids * DungeonRoomBuilder.GRID_SIZE / 2f, 10f, roomSizeInGrids * DungeonRoomBuilder.GRID_SIZE / 2f);
                    marker.transform.localScale = Vector3.one * 3f;

                    Material markerMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    markerMat.color = GetRoomTypeColor(room.roomType);
                    markerMat.EnableKeyword("_EMISSION");
                    markerMat.SetColor("_EmissionColor", GetRoomTypeColor(room.roomType) * 2f);
                    marker.GetComponent<Renderer>().material = markerMat;

                    // Remove collider
                    Destroy(marker.GetComponent<Collider>());
                }

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
            Debug.Log($"[ConnectRooms] CALLED! Total rooms: {allRooms.Count}");

            // Create doorway in Start room's south wall to connect to entrance room
            if (showDebug)
                Debug.Log($"[DungeonGenerator] ConnectRooms: Looking for Start room among {allRooms.Count} rooms");

            DungeonRoom startRoom = allRooms.Find(r => r.roomType == RoomType.Start);
            if (startRoom != null)
            {
                Debug.Log($"[ConnectRooms] Found Start room, calling CreateEntranceDoorway...");
                if (showDebug)
                    Debug.Log($"[DungeonGenerator] Found Start room at {startRoom.gridPosition}, creating entrance doorway...");
                CreateEntranceDoorway(startRoom);
                if (showDebug)
                    Debug.Log($"[DungeonGenerator] Created entrance doorway in Start room at {startRoom.gridPosition}");
            }
            else
            {
                Debug.LogWarning($"[ConnectRooms] NO Start room found!");
                if (showDebug)
                    Debug.LogWarning($"[DungeonGenerator] WARNING: Could not find Start room! allRooms.Count = {allRooms.Count}");
            }

            // Create doorways where rooms connect
            int doorwayCount = 0;
            foreach (var room in allRooms)
            {
                if (room.connectedFrom != null)
                {
                    doorwayCount++;
                    CreateDoorwayBetweenRooms(room.connectedFrom, room);

                    if (showDebug)
                        Debug.Log($"[DungeonGenerator] Connected {room.roomType} at {room.gridPosition} to {room.connectedFrom.roomType} at {room.connectedFrom.gridPosition}");
                }
            }
            Debug.Log($"[ConnectRooms] Created {doorwayCount} doorways between rooms");

            // Also connect adjacent rooms that aren't parent-child (create loops)
            CreateAdditionalConnections();
            Debug.Log($"[ConnectRooms] Finished");
            // Force recompile
        }

        private void CreateAdditionalConnections()
        {
            // Find adjacent rooms and connect some of them to create loops
            int connectionsAdded = 0;
            int maxAdditionalConnections = allRooms.Count / 3; // Add connections to ~33% of rooms

            for (int i = 0; i < allRooms.Count && connectionsAdded < maxAdditionalConnections; i++)
            {
                var roomA = allRooms[i];

                // Check all four directions
                Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

                foreach (var dir in directions)
                {
                    Vector2Int adjacentCell = roomA.gridPosition + dir * roomSizeInGrids;

                    // Find if there's a room at this adjacent cell
                    var roomB = allRooms.Find(r => r.gridPosition == adjacentCell);

                    if (roomB != null && roomB != roomA.connectedFrom && roomA != roomB.connectedFrom)
                    {
                        // These rooms are adjacent but not directly connected - maybe add a connection
                        if ((float)rng.NextDouble() < 0.3f) // 30% chance to add extra connection
                        {
                            CreateDoorwayBetweenRooms(roomA, roomB);
                            connectionsAdded++;

                            if (showDebug)
                                Debug.Log($"[DungeonGenerator] Added loop connection between {roomA.gridPosition} and {roomB.gridPosition}");

                            break; // Only add one extra connection per room
                        }
                    }
                }
            }

            if (showDebug)
                Debug.Log($"[DungeonGenerator] Added {connectionsAdded} additional connections to create loops");
        }

        private void CreateEntranceDoorway(DungeonRoom startRoom)
        {
            Debug.Log($"[CreateEntranceDoorway] Creating entrance doorway for {startRoom.roomObject.name}");

            // Create doorway in south wall of Start room to connect to entrance room
            GameObject doorway = DungeonRoomBuilder.CreateDoorway();
            doorway.transform.SetParent(startRoom.roomObject.transform);

            float halfSize = roomSizeInGrids * DungeonRoomBuilder.GRID_SIZE / 2f;

            // Place doorway in center of south wall (facing entrance room at -Z)
            doorway.transform.localPosition = new Vector3(0, 0, -halfSize);
            doorway.transform.localRotation = Quaternion.Euler(0, 180, 0); // Face south

            // Remove south wall section where doorway is
            Vector3 southDirection = new Vector3(0, 0, -1);
            Debug.Log($"[CreateEntranceDoorway] Calling RemoveWallSection for south wall...");
            RemoveWallSection(startRoom.roomObject, doorway.transform.position, southDirection);
            Debug.Log($"[CreateEntranceDoorway] Finished");
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

            // Remove wall section where doorway is in BOTH rooms
            RemoveWallSection(roomA.roomObject, doorway.transform.position, direction);
            // Also remove the opposite wall in roomB (opposite direction)
            RemoveWallSection(roomB.roomObject, doorway.transform.position, -direction);

            Debug.Log($"[CreateDoorwayBetweenRooms] Created doorway between {roomA.roomType} at {roomA.gridPosition} and {roomB.roomType} at {roomB.gridPosition}");
        }

        private void RemoveWallSection(GameObject roomObj, Vector3 doorwayPos, Vector3 direction)
        {
            Transform walls = roomObj.transform.Find("Walls");
            if (walls == null)
            {
                Debug.LogWarning($"[RemoveWallSection] No Walls object found in {roomObj.name}");
                return;
            }

            // Find the wall at the doorway position
            string wallName = GetWallName(direction);
            Transform wall = walls.Find(wallName);

            Debug.Log($"[RemoveWallSection] Room: {roomObj.name}, Wall: {wallName}, Found: {wall != null}");

            if (wall != null)
            {
                // Get wall dimensions before destroying
                Vector3 wallScale = wall.localScale;
                Vector3 wallPosition = wall.localPosition;
                Quaternion wallRotation = wall.localRotation;

                Debug.Log($"[RemoveWallSection] Replacing {wallName} (size {wallScale}) with segmented wall at {wallPosition}");

                // Destroy the old solid wall
                DestroyImmediate(wall.gameObject);

                // Determine width and thickness based on wall orientation
                // North/South walls: width is X-axis, thickness is Z-axis
                // East/West walls: width is Z-axis, thickness is X-axis
                float wallWidth, wallThickness;
                if (wallName == "NorthWall" || wallName == "SouthWall")
                {
                    wallWidth = wallScale.x;
                    wallThickness = wallScale.z;
                }
                else // EastWall or WestWall
                {
                    wallWidth = wallScale.z;
                    wallThickness = wallScale.x;
                }

                Debug.Log($"[RemoveWallSection] Wall dimensions: width={wallWidth}, height={wallScale.y}, thickness={wallThickness}");

                // Create new segmented wall with doorway opening
                GameObject segmentedWall = DungeonRoomBuilder.CreateWallWithDoorway(
                    wallWidth,   // width (correct for wall orientation)
                    wallScale.y, // height
                    wallThickness, // thickness (correct for wall orientation)
                    2f,          // doorway width
                    3f,          // doorway height
                    wallName     // wall name
                );

                // Position the new segmented wall where the old wall was
                segmentedWall.transform.SetParent(walls);
                segmentedWall.transform.localPosition = wallPosition;
                segmentedWall.transform.localRotation = wallRotation;

                Debug.Log($"[RemoveWallSection] Created segmented {wallName} with {segmentedWall.transform.childCount} segments");
            }
            else
            {
                Debug.LogWarning($"[RemoveWallSection] Wall {wallName} not found in {roomObj.name}");
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
            // Add pillars for cover - mix of intact and broken
            int pillarCount = rng.Next(this.pillarCount.x, this.pillarCount.y + 1);
            float roomRadius = roomSizeInGrids * DungeonRoomBuilder.GRID_SIZE * 0.35f;

            for (int i = 0; i < pillarCount; i++)
            {
                Vector3 localPos = GetRandomPositionInRoom(roomRadius);

                // 30% chance for broken pillar
                GameObject pillar = (float)rng.NextDouble() < 0.3f
                    ? DungeonRoomBuilder.CreateBrokenPillar()
                    : DungeonRoomBuilder.CreatePillar();

                pillar.transform.SetParent(room.roomObject.transform);
                pillar.transform.localPosition = localPos;
            }

            // Add ancient dungeon decorations
            AddAncientDecorations(room, density: 0.7f);

            // Add wall torches
            AddWallTorches(room);

            // Spawn skeleton monsters (2-3 for combat rooms)
            int skeletonCount = rng.Next(2, 4);
            SpawnSkeletons(room, skeletonCount);
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

            // Add ancient decorations
            AddAncientDecorations(room, density: 0.5f);

            AddWallTorches(room);

            // Spawn skeleton guards (1-2 guarding the treasure)
            int skeletonCount = rng.Next(1, 3);
            SpawnSkeletons(room, skeletonCount);
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
            int pillarCount = rng.Next(2, 4);
            float pillarRadius = roomSizeInGrids * DungeonRoomBuilder.GRID_SIZE * 0.35f;
            for (int i = 0; i < pillarCount; i++)
            {
                Vector3 localPos = GetRandomPositionInRoom(pillarRadius);
                GameObject pillar = DungeonRoomBuilder.CreatePillar();
                pillar.transform.SetParent(room.roomObject.transform);
                pillar.transform.localPosition = localPos;
            }

            // Add ancient decorations
            AddAncientDecorations(room, density: 0.6f);

            AddWallTorches(room);

            // Spawn skeleton wanderers (0-1 for puzzle rooms - light defense)
            int skeletonCount = rng.Next(0, 2);
            SpawnSkeletons(room, skeletonCount);
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

            // Add light ancient decorations (shop should look more maintained)
            AddAncientDecorations(room, density: 0.3f);

            AddWallTorches(room);

            // Shop is a safe zone - no skeletons spawned
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

            // Add dramatic ancient decorations for boss atmosphere
            AddAncientDecorations(room, density: 0.8f);

            AddWallTorches(room, torchesPerRoom * 2); // Extra bright boss room

            // Spawn skeleton horde for boss room (2-4 skeletons - intense combat)
            int skeletonCount = rng.Next(2, 5);
            SpawnSkeletons(room, skeletonCount);
        }

        /// <summary>
        /// Adds ancient dungeon decorations - rubble, moss, cracks, bones, etc.
        /// </summary>
        private void AddAncientDecorations(DungeonRoom room, float density = 1f)
        {
            float roomRadius = roomSizeInGrids * DungeonRoomBuilder.GRID_SIZE * 0.4f;
            int decorationCount = (int)(10 * density);

            for (int i = 0; i < decorationCount; i++)
            {
                Vector3 localPos = GetRandomPositionInRoom(roomRadius);
                float roll = (float)rng.NextDouble();

                GameObject decoration = null;

                if (roll < 0.25f)
                {
                    // Rubble pile
                    decoration = DungeonRoomBuilder.CreateRubblePile(Random.Range(0.8f, 1.5f));
                    decoration.transform.localPosition = localPos;
                }
                else if (roll < 0.45f)
                {
                    // Moss patch
                    decoration = DungeonRoomBuilder.CreateMossPatch(Random.Range(0.4f, 0.8f));
                    decoration.transform.localPosition = localPos + new Vector3(0, 0.01f, 0);
                }
                else if (roll < 0.55f)
                {
                    // Floor cracks
                    decoration = DungeonRoomBuilder.CreateCrack(Random.Range(0.8f, 1.5f));
                    decoration.transform.localPosition = localPos + new Vector3(0, 0.01f, 0);
                }
                else if (roll < 0.65f)
                {
                    // Fallen column
                    decoration = DungeonRoomBuilder.CreateFallenColumn(Random.Range(1.5f, 2.5f));
                    decoration.transform.localPosition = localPos + new Vector3(0, 0.2f, 0);
                    decoration.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                }
                else if (roll < 0.75f)
                {
                    // Scattered bones
                    decoration = DungeonRoomBuilder.CreateBones();
                    decoration.transform.localPosition = localPos;
                }

                if (decoration != null)
                {
                    decoration.transform.SetParent(room.roomObject.transform);
                }
            }

            // Add wall decorations
            AddWallDecorations(room, (int)(6 * density));

            // Add ceiling decorations (vines)
            if ((float)rng.NextDouble() < 0.5f)
            {
                AddCeilingVines(room, (int)(3 * density));
            }
        }

        /// <summary>
        /// Adds cracks and damage to walls
        /// </summary>
        private void AddWallDecorations(DungeonRoom room, int count)
        {
            Transform walls = room.roomObject.transform.Find("Walls");
            if (walls == null) return;

            string[] wallNames = { "NorthWall", "SouthWall", "EastWall", "WestWall" };

            for (int i = 0; i < count; i++)
            {
                // Pick a random wall
                string wallName = wallNames[rng.Next(wallNames.Length)];
                Transform wall = walls.Find(wallName);

                if (wall != null)
                {
                    float roll = (float)rng.NextDouble();
                    GameObject decoration = null;

                    if (roll < 0.6f)
                    {
                        // Wall crack
                        decoration = DungeonRoomBuilder.CreateCrack(Random.Range(0.5f, 1.2f));
                    }
                    else if (roll < 0.85f)
                    {
                        // Moss patch on wall
                        decoration = DungeonRoomBuilder.CreateMossPatch(Random.Range(0.3f, 0.6f));
                    }
                    else
                    {
                        // Wall damage
                        decoration = DungeonRoomBuilder.CreateWallDamage(Random.Range(0.4f, 0.8f));
                    }

                    if (decoration != null)
                    {
                        decoration.transform.SetParent(wall);
                        decoration.transform.localPosition = new Vector3(
                            Random.Range(-3f, 3f),
                            Random.Range(0.5f, 3f),
                            wallName.Contains("East") || wallName.Contains("West") ? 0.21f : 0.21f
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Adds hanging vines from ceiling
        /// </summary>
        private void AddCeilingVines(DungeonRoom room, int count)
        {
            Transform ceiling = room.roomObject.transform.Find("Ceiling");
            if (ceiling == null) return;

            float roomRadius = roomSizeInGrids * DungeonRoomBuilder.GRID_SIZE * 0.3f;

            for (int i = 0; i < count; i++)
            {
                Vector3 localPos = GetRandomPositionInRoom(roomRadius);
                localPos.y = DungeonRoomBuilder.WALL_HEIGHT;

                GameObject vines = DungeonRoomBuilder.CreateHangingVines(Random.Range(1f, 2f));
                vines.transform.SetParent(room.roomObject.transform);
                vines.transform.localPosition = localPos;
            }
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

        // ==================== SKELETON MONSTER SPAWNING ====================

        /// <summary>
        /// Spawns skeleton monsters in a room using MonsterBuilder
        /// </summary>
        /// <param name="room">Room to spawn in</param>
        /// <param name="count">Number of skeletons to spawn</param>
        private void SpawnSkeletons(DungeonRoom room, int count)
        {
            if (count <= 0) return;

            float roomRadius = roomSizeInGrids * DungeonRoomBuilder.GRID_SIZE * 0.35f;

            for (int i = 0; i < count; i++)
            {
                Vector3 localPos = GetRandomPositionInRoom(roomRadius);

                // Floor tiles are 0.1 units tall, centered at Y=0, so top surface is at Y=0.05
                const float FLOOR_TOP = 0.05f;
                localPos.y = FLOOR_TOP;

                // Use MonsterBuilder to create skeleton (same as HomeArea spawner)
                GameObject skeleton = AI.MonsterBuilder.CreateSkeleton();
                skeleton.transform.SetParent(room.roomObject.transform);
                skeleton.transform.localPosition = localPos;
                skeleton.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                // Find ACTUAL lowest vertex in both room space and skeleton local space
                float lowestRoomY = float.MaxValue;
                float lowestSkeletonLocalY = float.MaxValue;
                MeshFilter[] meshFilters = skeleton.GetComponentsInChildren<MeshFilter>();

                foreach (MeshFilter mf in meshFilters)
                {
                    if (mf.mesh != null)
                    {
                        Vector3[] vertices = mf.mesh.vertices;
                        Transform meshTransform = mf.transform;

                        foreach (Vector3 localVert in vertices)
                        {
                            // Transform vertex to world space
                            Vector3 worldVert = meshTransform.TransformPoint(localVert);

                            // Then to room's local space
                            Vector3 roomLocalVert = room.roomObject.transform.InverseTransformPoint(worldVert);
                            if (roomLocalVert.y < lowestRoomY)
                            {
                                lowestRoomY = roomLocalVert.y;
                            }

                            // And to skeleton's local space for collider
                            Vector3 skeletonLocalVert = skeleton.transform.InverseTransformPoint(worldVert);
                            if (skeletonLocalVert.y < lowestSkeletonLocalY)
                            {
                                lowestSkeletonLocalY = skeletonLocalVert.y;
                            }
                        }
                    }
                }

                // Adjust Y so lowest vertex sits on floor top (Y=FLOOR_TOP in room space)
                if (lowestRoomY < FLOOR_TOP)
                {
                    localPos.y += (FLOOR_TOP - lowestRoomY);
                    skeleton.transform.localPosition = localPos;
                }

                // Calculate mesh bounds for collider sizing (after positioning)
                Bounds meshBounds = CalculateMonsterBounds(skeleton);

                // Adjust collider to ensure it reaches the ground
                // The physics collider should extend from the lowest vertex to the top
                float colliderHeight = meshBounds.size.y;
                float colliderCenterY = lowestSkeletonLocalY + (colliderHeight / 2f);
                Vector3 colliderCenter = new Vector3(meshBounds.center.x, colliderCenterY, meshBounds.center.z);

                // Add MonsterBase component
                AI.MonsterBase monsterBase = skeleton.AddComponent<AI.MonsterBase>();
                monsterBase.monsterType = AI.MonsterType.Skeleton;
                monsterBase.maxHP = 15;
                monsterBase.currentHP = 15;
                monsterBase.knockbackForce = 8f;
                monsterBase.showDebug = false;

                // Add MonsterAI component with aggro behavior
                AI.MonsterAI monsterAI = skeleton.AddComponent<AI.MonsterAI>();
                monsterAI.walkSpeed = 1.2f;
                monsterAI.chaseSpeed = 3.5f;
                monsterAI.aggroRange = 2f; // Detect player within 2m
                monsterAI.maxRoamDistance = 8f;
                monsterAI.showDebug = true; // ENABLE DEBUG

                // Add SkeletonAnimator for procedural animation
                AI.SkeletonAnimator animator = skeleton.AddComponent<AI.SkeletonAnimator>();
                animator.walkCycleSpeed = 4f;
                animator.legSwingAngle = 30f;
                animator.armSwingAngle = 20f;
                animator.showDebug = true; // ENABLE DEBUG

                // Add SkeletonAttack for melee combat
                AI.SkeletonAttack attack = skeleton.AddComponent<AI.SkeletonAttack>();
                attack.attackRange = 1.5f;
                attack.attackDamage = 1;
                attack.attackCooldown = 2f;
                attack.showDebug = true; // ENABLE DEBUG

                // Add SkeletonEyeEffect for fiery red eyes when aggro
                AI.SkeletonEyeEffect eyeEffect = skeleton.AddComponent<AI.SkeletonEyeEffect>();
                eyeEffect.enableFireParticles = true;
                eyeEffect.fireParticleCount = 10;
                eyeEffect.showDebug = true; // ENABLE DEBUG

                // Add Rigidbody
                Rigidbody rb = skeleton.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = skeleton.AddComponent<Rigidbody>();
                    rb.mass = 1f;
                    rb.linearDamping = 2f;
                    rb.angularDamping = 1f;
                    rb.useGravity = true;
                    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    rb.constraints = RigidbodyConstraints.FreezeRotation;
                }

                // Add colliders - make trigger collider tighter to match visible model
                // Trigger collider for spell detection - reduced to 60% of mesh bounds for tighter hitbox
                BoxCollider triggerCollider = skeleton.AddComponent<BoxCollider>();
                triggerCollider.isTrigger = true;
                triggerCollider.size = new Vector3(
                    meshBounds.size.x * 0.6f,  // Narrower width
                    meshBounds.size.y * 0.9f,  // Slightly shorter height
                    meshBounds.size.z * 0.6f   // Narrower depth
                );
                triggerCollider.center = colliderCenter;

                // Physics collider - small footprint only to block teleportation directly under skeleton
                // This prevents player from teleporting into the skeleton but allows close teleportation nearby
                BoxCollider physicsCollider = skeleton.AddComponent<BoxCollider>();
                physicsCollider.isTrigger = false;
                // Small footprint: width=0.3, height=0.3 (just feet area), depth=0.3
                physicsCollider.size = new Vector3(0.3f, 0.3f, 0.3f);
                // Position at the bottom (ground level in skeleton local space)
                physicsCollider.center = new Vector3(0f, lowestSkeletonLocalY + 0.15f, 0f);
            }

            if (showDebug)
                Debug.Log($"[DungeonGenerator] Spawned {count} skeletons in {room.roomType} room at {room.gridPosition}");
        }

        /// <summary>
        /// Calculate the bounds of all mesh renderers in a monster
        /// Returns the combined bounds in local space
        /// </summary>
        private Bounds CalculateMonsterBounds(GameObject obj)
        {
            MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();

            if (renderers.Length == 0)
            {
                return new Bounds(Vector3.zero, Vector3.one);
            }

            // Start with first renderer's bounds
            Bounds bounds = new Bounds(renderers[0].transform.localPosition, Vector3.zero);

            // Expand to include all renderers
            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer.GetComponent<MeshFilter>() != null)
                {
                    Mesh mesh = renderer.GetComponent<MeshFilter>().mesh;
                    if (mesh != null)
                    {
                        Bounds meshBounds = mesh.bounds;
                        Vector3[] corners = new Vector3[8];
                        corners[0] = renderer.transform.localPosition + renderer.transform.localRotation * (meshBounds.center + new Vector3(meshBounds.extents.x, meshBounds.extents.y, meshBounds.extents.z));
                        corners[1] = renderer.transform.localPosition + renderer.transform.localRotation * (meshBounds.center + new Vector3(-meshBounds.extents.x, meshBounds.extents.y, meshBounds.extents.z));
                        corners[2] = renderer.transform.localPosition + renderer.transform.localRotation * (meshBounds.center + new Vector3(meshBounds.extents.x, -meshBounds.extents.y, meshBounds.extents.z));
                        corners[3] = renderer.transform.localPosition + renderer.transform.localRotation * (meshBounds.center + new Vector3(-meshBounds.extents.x, -meshBounds.extents.y, meshBounds.extents.z));
                        corners[4] = renderer.transform.localPosition + renderer.transform.localRotation * (meshBounds.center + new Vector3(meshBounds.extents.x, meshBounds.extents.y, -meshBounds.extents.z));
                        corners[5] = renderer.transform.localPosition + renderer.transform.localRotation * (meshBounds.center + new Vector3(-meshBounds.extents.x, meshBounds.extents.y, -meshBounds.extents.z));
                        corners[6] = renderer.transform.localPosition + renderer.transform.localRotation * (meshBounds.center + new Vector3(meshBounds.extents.x, -meshBounds.extents.y, -meshBounds.extents.z));
                        corners[7] = renderer.transform.localPosition + renderer.transform.localRotation * (meshBounds.center + new Vector3(-meshBounds.extents.x, -meshBounds.extents.y, -meshBounds.extents.z));

                        foreach (Vector3 corner in corners)
                        {
                            bounds.Encapsulate(corner);
                        }
                    }
                }
            }

            return bounds;
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
