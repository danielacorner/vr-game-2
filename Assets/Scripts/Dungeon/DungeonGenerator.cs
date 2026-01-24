using UnityEngine;
using System.Collections.Generic;

namespace VRDungeonCrawler.Dungeon
{
    /// <summary>
    /// Procedurally generates dungeon layout similar to Ancient Dungeon VR
    /// Creates rooms, corridors, and populates with chests, enemies, shop, and boss door
    /// </summary>
    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Dungeon Settings")]
        [Tooltip("Number of rooms to generate")]
        public int roomCount = 8;
        
        [Tooltip("Grid size for room placement")]
        public int gridSize = 10;
        
        [Tooltip("Size of each room (meters)")]
        public float roomSize = 10f;
        
        [Header("Room Prefabs")]
        [Tooltip("Starting room prefab")]
        public GameObject startRoomPrefab;
        
        [Tooltip("Normal room prefabs")]
        public GameObject[] roomPrefabs;
        
        [Tooltip("Shop room prefab")]
        public GameObject shopRoomPrefab;
        
        [Tooltip("Boss room prefab")]
        public GameObject bossRoomPrefab;
        
        [Tooltip("Corridor prefab")]
        public GameObject corridorPrefab;
        
        [Header("Spawnable Objects")]
        [Tooltip("Chest prefab")]
        public GameObject chestPrefab;
        
        [Tooltip("Enemy prefabs")]
        public GameObject[] enemyPrefabs;
        
        [Header("Generation Settings")]
        [Tooltip("Random seed (0 = random)")]
        public int seed = 0;
        
        [Tooltip("Min enemies per room")]
        public int minEnemiesPerRoom = 1;
        
        [Tooltip("Max enemies per room")]
        public int maxEnemiesPerRoom = 3;
        
        [Tooltip("Chance to spawn chest (0-1)")]
        public float chestSpawnChance = 0.3f;
        
        private List<DungeonRoom> generatedRooms = new List<DungeonRoom>();
        private HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
        private DungeonRoom startRoom;
        private DungeonRoom shopRoom;
        private DungeonRoom bossRoom;
        
        private void Start()
        {
            LoadPrefabsFromResources();
            GenerateDungeon();
        }

        /// <summary>
        /// Loads prefabs from Resources folder to populate arrays at runtime
        /// </summary>
        private void LoadPrefabsFromResources()
        {
            Debug.Log("[DungeonGenerator] Loading prefabs from Resources...");

            // Load room prefabs
            if (roomPrefabs == null || roomPrefabs.Length == 0)
            {
                GameObject normalRoom = Resources.Load<GameObject>("Dungeon/NormalRoom");
                if (normalRoom != null)
                {
                    roomPrefabs = new GameObject[] { normalRoom };
                    Debug.Log("[DungeonGenerator] Loaded NormalRoom from Resources");
                }
                else
                {
                    Debug.LogError("[DungeonGenerator] Failed to load NormalRoom from Resources/Dungeon/");
                }
            }

            // Load enemy prefabs
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                GameObject enemyBasic = Resources.Load<GameObject>("Dungeon/Enemy_Basic");
                GameObject enemyTough = Resources.Load<GameObject>("Dungeon/Enemy_Tough");

                if (enemyBasic != null && enemyTough != null)
                {
                    enemyPrefabs = new GameObject[] { enemyBasic, enemyTough };
                    Debug.Log("[DungeonGenerator] Loaded 2 enemy prefabs from Resources");
                }
                else
                {
                    Debug.LogError("[DungeonGenerator] Failed to load enemy prefabs from Resources/Dungeon/");
                }
            }

            Debug.Log($"[DungeonGenerator] Prefab loading complete. RoomPrefabs: {roomPrefabs?.Length ?? 0}, EnemyPrefabs: {enemyPrefabs?.Length ?? 0}");
        }
        
        public void GenerateDungeon()
        {
            Debug.Log("[DungeonGenerator] ========================================");
            Debug.Log("[DungeonGenerator] Starting dungeon generation...");
            Debug.Log($"[DungeonGenerator] Generator position: {transform.position}");
            Debug.Log("[DungeonGenerator] ========================================");

            // Set random seed
            if (seed != 0)
                Random.InitState(seed);
            else
                Random.InitState(System.DateTime.Now.Millisecond);

            // Clear previous dungeon
            ClearDungeon();

            // NOTE: Entrance room is static in the scene (not generated here)
            // Generate procedural dungeon starting from entrance exit

            // Generate layout starting from entrance exit
            GenerateRoomLayout();

            // Connect rooms with corridors
            ConnectRooms();

            // Populate rooms
            PopulateRooms();

            Debug.Log("[DungeonGenerator] ========================================");
            Debug.Log($"[DungeonGenerator] ✓ Generated {generatedRooms.Count} procedural rooms");
            Debug.Log("[DungeonGenerator] ========================================");
        }
        
        private void ClearDungeon()
        {
            foreach (var room in generatedRooms)
            {
                if (room.roomObject != null)
                    Destroy(room.roomObject);
            }

            generatedRooms.Clear();
            occupiedCells.Clear();
        }

        private void GenerateRoomLayout()
        {
            // Start procedural generation from in front of entrance (Z+2 grid cells forward)
            Vector2Int entranceExitCell = new Vector2Int(0, 2);
            startRoom = CreateRoom(entranceExitCell, RoomType.Start);

            // Generate normal rooms using random walk from entrance exit
            Vector2Int currentCell = entranceExitCell;
            int roomsGenerated = 1;
            
            while (roomsGenerated < roomCount - 2) // -2 for shop and boss
            {
                // Pick random direction
                Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                Vector2Int direction = directions[Random.Range(0, directions.Length)];
                Vector2Int nextCell = currentCell + direction;
                
                // Check if cell is free and within bounds
                if (!occupiedCells.Contains(nextCell) && 
                    Mathf.Abs(nextCell.x) < gridSize && 
                    Mathf.Abs(nextCell.y) < gridSize)
                {
                    CreateRoom(nextCell, RoomType.Normal);
                    currentCell = nextCell;
                    roomsGenerated++;
                }
            }
            
            // Add shop room (branching from a random normal room)
            DungeonRoom randomRoom = generatedRooms[Random.Range(1, generatedRooms.Count)];
            Vector2Int shopCell = FindEmptyNeighbor(randomRoom.gridPosition);
            if (shopCell != Vector2Int.zero || !occupiedCells.Contains(shopCell))
            {
                shopRoom = CreateRoom(shopCell, RoomType.Shop);
            }
            
            // Add boss room at the furthest point from start
            Vector2Int bossCell = FindFurthestCell();
            bossRoom = CreateRoom(bossCell, RoomType.Boss);
        }
        
        private DungeonRoom CreateRoom(Vector2Int gridPos, RoomType type)
        {
            GameObject prefab = null;

            switch (type)
            {
                case RoomType.Start:
                    prefab = startRoomPrefab;
                    break;
                case RoomType.Normal:
                    prefab = roomPrefabs.Length > 0 ? roomPrefabs[Random.Range(0, roomPrefabs.Length)] : null;
                    break;
                case RoomType.Shop:
                    prefab = shopRoomPrefab;
                    break;
                case RoomType.Boss:
                    prefab = bossRoomPrefab;
                    break;
            }

            // Fallback to NormalRoom if specific prefab is missing
            if (prefab == null && roomPrefabs.Length > 0)
            {
                prefab = roomPrefabs[0];
                Debug.LogWarning($"[DungeonGenerator] {type}Room prefab missing, using NormalRoom fallback");
            }

            Vector3 worldPos = new Vector3(gridPos.x * roomSize, 0, gridPos.y * roomSize);

            // FORCE empty GameObject - don't use prefabs (they have geometry that breaks head tracking debugging)
            GameObject roomObj = new GameObject($"Room_{type}");
            roomObj.transform.position = worldPos;
            roomObj.transform.SetParent(transform);
            roomObj.name = $"{type}Room_{gridPos}";

            // Add simple colored cube marker at room position for debugging
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "RoomMarker";
            marker.transform.SetParent(roomObj.transform);
            marker.transform.localPosition = new Vector3(0, 1.5f, 0); // Floating marker
            marker.transform.localScale = new Vector3(2f, 2f, 2f);

            // Color code by room type
            Renderer markerRenderer = marker.GetComponent<Renderer>();
            if (markerRenderer != null)
            {
                switch (type)
                {
                    case RoomType.Start:
                        markerRenderer.material.color = Color.green;
                        break;
                    case RoomType.Normal:
                        markerRenderer.material.color = Color.white;
                        break;
                    case RoomType.Shop:
                        markerRenderer.material.color = Color.yellow;
                        break;
                    case RoomType.Boss:
                        markerRenderer.material.color = Color.red;
                        break;
                }
            }

            Debug.Log($"[DungeonGenerator] Created {type} room at {worldPos} with {markerRenderer.material.color} marker");

            DungeonRoom room = new DungeonRoom
            {
                gridPosition = gridPos,
                worldPosition = worldPos,
                roomType = type,
                roomObject = roomObj
            };

            generatedRooms.Add(room);
            occupiedCells.Add(gridPos);

            return room;
        }
        
        private Vector2Int FindEmptyNeighbor(Vector2Int cell)
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            
            foreach (var dir in directions)
            {
                Vector2Int neighbor = cell + dir;
                if (!occupiedCells.Contains(neighbor) && 
                    Mathf.Abs(neighbor.x) < gridSize && 
                    Mathf.Abs(neighbor.y) < gridSize)
                {
                    return neighbor;
                }
            }
            
            return cell;
        }
        
        private Vector2Int FindFurthestCell()
        {
            Vector2Int furthest = Vector2Int.zero;
            float maxDist = 0;
            
            for (int x = -gridSize; x < gridSize; x++)
            {
                for (int y = -gridSize; y < gridSize; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    if (!occupiedCells.Contains(cell))
                    {
                        float dist = Vector2Int.Distance(Vector2Int.zero, cell);
                        if (dist > maxDist)
                        {
                            maxDist = dist;
                            furthest = cell;
                        }
                    }
                }
            }
            
            return furthest;
        }
        
        private void ConnectRooms()
        {
            // Simple corridor connection - connect each room to nearest unconnected room
            foreach (var room in generatedRooms)
            {
                DungeonRoom nearest = FindNearestRoom(room);
                if (nearest != null && corridorPrefab != null)
                {
                    CreateCorridor(room.worldPosition, nearest.worldPosition);
                }
            }
        }
        
        private DungeonRoom FindNearestRoom(DungeonRoom fromRoom)
        {
            DungeonRoom nearest = null;
            float minDist = float.MaxValue;
            
            foreach (var room in generatedRooms)
            {
                if (room == fromRoom) continue;
                
                float dist = Vector3.Distance(fromRoom.worldPosition, room.worldPosition);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = room;
                }
            }
            
            return nearest;
        }
        
        private void CreateCorridor(Vector3 from, Vector3 to)
        {
            Vector3 direction = (to - from).normalized;
            float distance = Vector3.Distance(from, to);
            Vector3 midpoint = (from + to) / 2f;
            
            GameObject corridor = Instantiate(corridorPrefab, midpoint, Quaternion.LookRotation(direction), transform);
            corridor.transform.localScale = new Vector3(2f, 3f, distance);
        }
        
        private void PopulateRooms()
        {
            foreach (var room in generatedRooms)
            {
                if (room.roomType == RoomType.Normal)
                {
                    // Spawn enemies
                    int enemyCount = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);
                    for (int i = 0; i < enemyCount; i++)
                    {
                        SpawnEnemy(room);
                    }
                    
                    // Chance to spawn chest
                    if (Random.value < chestSpawnChance)
                    {
                        SpawnChest(room);
                    }
                }
                else if (room.roomType == RoomType.Boss)
                {
                    // Spawn boss door
                    SpawnBossDoor(room);
                }
            }
        }
        
        private void SpawnEnemy(DungeonRoom room)
        {
            if (enemyPrefabs.Length == 0) return;
            
            Vector3 spawnPos = room.worldPosition + new Vector3(
                Random.Range(-roomSize * 0.3f, roomSize * 0.3f),
                0.5f,
                Random.Range(-roomSize * 0.3f, roomSize * 0.3f)
            );
            
            GameObject enemy = Instantiate(
                enemyPrefabs[Random.Range(0, enemyPrefabs.Length)],
                spawnPos,
                Quaternion.identity,
                room.roomObject.transform
            );
        }
        
        private void SpawnChest(DungeonRoom room)
        {
            if (chestPrefab == null) return;
            
            Vector3 spawnPos = room.worldPosition + new Vector3(
                Random.Range(-roomSize * 0.3f, roomSize * 0.3f),
                0f,
                Random.Range(-roomSize * 0.3f, roomSize * 0.3f)
            );
            
            Instantiate(chestPrefab, spawnPos, Quaternion.identity, room.roomObject.transform);
        }
        
        private void SpawnBossDoor(DungeonRoom room)
        {
            // Boss door spawned at room entrance
            Vector3 doorPos = room.worldPosition + new Vector3(0, 0, -roomSize * 0.4f);
            
            // Create simple door placeholder for now
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "BossDoor";
            door.transform.position = doorPos;
            door.transform.localScale = new Vector3(3f, 4f, 0.5f);
            door.transform.parent = room.roomObject.transform;
            
            // Add boss door component
            var bossDoor = door.AddComponent<BossDoor>();
        }

        private void AddDebugMarkers(GameObject roomObj)
        {
            // DESTROY EVERYTHING - completely empty space
            foreach (Transform child in roomObj.transform)
            {
                Destroy(child.gameObject);
            }

            // Disable room renderer
            Renderer roomRenderer = roomObj.GetComponent<Renderer>();
            if (roomRenderer != null)
            {
                roomRenderer.enabled = false;
            }

            // Create SOLID VISIBLE floor using CUBE (easier to debug than invisible plane)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "FLOOR_SOLID";
            floor.transform.SetParent(roomObj.transform);
            floor.transform.localPosition = new Vector3(0, -0.5f, 0); // Slightly below origin
            floor.transform.localScale = new Vector3(20f, 1f, 20f); // Large flat floor

            // Make floor VISIBLE for debugging (green color)
            Renderer floorRenderer = floor.GetComponent<Renderer>();
            if (floorRenderer != null && floorRenderer.material != null)
            {
                floorRenderer.material.color = Color.green;
            }

            // ONLY add ONE bright light - nothing else
            GameObject lightObj = new GameObject("MAIN_LIGHT");
            lightObj.transform.SetParent(roomObj.transform);
            lightObj.transform.localPosition = new Vector3(0, 10f, 0);

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.intensity = 20f; // VERY bright
            light.range = 50f;
            light.color = Color.white;

            Debug.Log($"[DungeonGenerator] Created VISIBLE GREEN FLOOR at Y=-0.5 + bright light");
        }

        private void OnDrawGizmos()
        {
            if (generatedRooms.Count == 0) return;
            
            foreach (var room in generatedRooms)
            {
                Color color = Color.white;
                switch (room.roomType)
                {
                    case RoomType.Start: color = Color.green; break;
                    case RoomType.Normal: color = Color.white; break;
                    case RoomType.Shop: color = Color.yellow; break;
                    case RoomType.Boss: color = Color.red; break;
                }
                
                Gizmos.color = color;
                Gizmos.DrawWireCube(room.worldPosition, Vector3.one * roomSize);
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
    }
    
    public enum RoomType
    {
        Start,
        Normal,
        Shop,
        Boss
    }
}
