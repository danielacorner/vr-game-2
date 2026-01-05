using UnityEngine;

namespace VRDungeonCrawler.Core
{
    /// <summary>
    /// Builds the home area scene at runtime or in editor
    /// Creates 10x10m room with walls, floor, ceiling, and furniture
    /// </summary>
    public class HomeAreaBuilder : MonoBehaviour
    {
        [Header("Room Settings")]
        public float roomSize = 10f;
        public float wallHeight = 3f;
        public float wallThickness = 0.5f;

        [Header("Materials")]
        public Material floorMaterial;
        public Material wallMaterial;
        public Material ceilingMaterial;

        [Header("Portal")]
        public GameObject portalPrefab;
        public Vector3 portalPosition = new Vector3(0, 1.2f, 4f);

        [Header("Player Spawn")]
        public Transform playerSpawnPoint;

        private void Start()
        {
            // If you want to build at runtime, uncomment:
            // BuildRoom();
        }

        [ContextMenu("Build Home Area Room")]
        public void BuildRoom()
        {
            Debug.Log("[HomeAreaBuilder] Building home area...");

            // Clear existing room objects
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Wall") || child.name.Contains("Floor") || child.name.Contains("Ceiling"))
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            CreateFloor();
            CreateCeiling();
            CreateWalls();
            CreateFurniture();
            CreatePortal();

            Debug.Log("[HomeAreaBuilder] Home area built successfully!");
        }

        private void CreateFloor()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(transform);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(roomSize / 10f, 1f, roomSize / 10f); // Plane is 10x10 by default

            if (floorMaterial != null)
                floor.GetComponent<Renderer>().material = floorMaterial;
            else
                floor.GetComponent<Renderer>().material.color = new Color(0.23f, 0.23f, 0.23f); // Dark gray
        }

        private void CreateCeiling()
        {
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ceiling.name = "Ceiling";
            ceiling.transform.SetParent(transform);
            ceiling.transform.localPosition = new Vector3(0, wallHeight, 0);
            ceiling.transform.localRotation = Quaternion.Euler(180, 0, 0);
            ceiling.transform.localScale = new Vector3(roomSize / 10f, 1f, roomSize / 10f);

            if (ceilingMaterial != null)
                ceiling.GetComponent<Renderer>().material = ceilingMaterial;
            else
                ceiling.GetComponent<Renderer>().material.color = new Color(0.16f, 0.16f, 0.16f); // Darker gray
        }

        private void CreateWalls()
        {
            float halfSize = roomSize / 2f;

            // North wall (back - where portal will be)
            CreateWall("Wall_North", new Vector3(0, wallHeight / 2f, halfSize), new Vector3(roomSize, wallHeight, wallThickness));

            // South wall (front - where player spawns)
            CreateWall("Wall_South", new Vector3(0, wallHeight / 2f, -halfSize), new Vector3(roomSize, wallHeight, wallThickness));

            // East wall (right)
            CreateWall("Wall_East", new Vector3(halfSize, wallHeight / 2f, 0), new Vector3(wallThickness, wallHeight, roomSize));

            // West wall (left)
            CreateWall("Wall_West", new Vector3(-halfSize, wallHeight / 2f, 0), new Vector3(wallThickness, wallHeight, roomSize));
        }

        private void CreateWall(string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(transform);
            wall.transform.localPosition = position;
            wall.transform.localScale = scale;

            if (wallMaterial != null)
                wall.GetComponent<Renderer>().material = wallMaterial;
            else
                wall.GetComponent<Renderer>().material.color = new Color(0.33f, 0.33f, 0.33f); // Gray
        }

        private void CreateFurniture()
        {
            // Simple table
            GameObject table = new GameObject("Table");
            table.transform.SetParent(transform);
            table.transform.localPosition = new Vector3(-2, 0, 0);

            // Table top
            GameObject tableTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tableTop.name = "TableTop";
            tableTop.transform.SetParent(table.transform);
            tableTop.transform.localPosition = new Vector3(0, 0.7f, 0);
            tableTop.transform.localScale = new Vector3(1.5f, 0.1f, 1f);
            tableTop.GetComponent<Renderer>().material.color = new Color(0.55f, 0.43f, 0.28f); // Wood color

            // Simple chest
            GameObject chest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chest.name = "Chest";
            chest.transform.SetParent(transform);
            chest.transform.localPosition = new Vector3(2, 0.25f, 2);
            chest.transform.localScale = new Vector3(0.8f, 0.5f, 0.6f);
            chest.GetComponent<Renderer>().material.color = new Color(0.4f, 0.26f, 0.13f); // Dark wood

            // Floating orb light
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "MagicOrb";
            orb.transform.SetParent(transform);
            orb.transform.localPosition = new Vector3(2.5f, 2f, -2f);
            orb.transform.localScale = Vector3.one * 0.4f;
            orb.GetComponent<Renderer>().material.color = new Color(1f, 0.67f, 0.27f); // Orange glow
            orb.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            orb.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(1f, 0.67f, 0.27f) * 2f);

            // Add point light to orb
            Light orbLight = orb.AddComponent<Light>();
            orbLight.type = LightType.Point;
            orbLight.color = new Color(1f, 0.67f, 0.27f);
            orbLight.intensity = 2f;
            orbLight.range = 5f;
        }

        private void CreatePortal()
        {
            if (portalPrefab != null)
            {
                GameObject portal = Instantiate(portalPrefab, transform);
                portal.name = "Portal";
                portal.transform.localPosition = portalPosition;
            }
            else
            {
                Debug.LogWarning("[HomeAreaBuilder] No portal prefab assigned. Create portal manually or assign prefab.");
            }
        }

        private void OnDrawGizmos()
        {
            // Visualize room bounds
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * wallHeight / 2f, new Vector3(roomSize, wallHeight, roomSize));

            // Visualize portal position
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + portalPosition, 0.5f);

            // Visualize player spawn
            if (playerSpawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(playerSpawnPoint.position, 0.3f);
                Gizmos.DrawLine(playerSpawnPoint.position, playerSpawnPoint.position + playerSpawnPoint.forward * 1f);
            }
        }
    }
}
