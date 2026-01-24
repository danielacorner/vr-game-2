using UnityEngine;

namespace VRDungeonCrawler.Dungeon
{
    /// <summary>
    /// Creates a static entrance room where player spawns before entering procedural dungeon
    /// This is attached to a GameObject in the scene, not created at runtime
    /// </summary>
    public class DungeonEntranceRoom : MonoBehaviour
    {
        [Header("Room Dimensions")]
        [Tooltip("Width of entrance room (X axis)")]
        public float roomWidth = 30f;

        [Tooltip("Length of entrance room (Z axis)")]
        public float roomLength = 20f;

        [Header("Features")]
        [Tooltip("Add decorative pillars")]
        public bool addPillars = true;

        [Tooltip("Add torches for lighting")]
        public bool addTorches = true;

        [Tooltip("Add doorways")]
        public bool addDoorways = true;

        [Tooltip("Add ceiling")]
        public bool addCeiling = true;

        [Tooltip("Add ambient light")]
        public bool addAmbientLight = true;

        [Tooltip("Ambient light intensity")]
        public float ambientIntensity = 0.3f;

        [Header("Debug")]
        [Tooltip("Show debug logs")]
        public bool showDebug = true;

        void Start()
        {
            BuildRoom();
        }

        public void BuildRoom()
        {
            Debug.Log("========================================");
            Debug.Log("[DungeonEntranceRoom] Building entrance room...");
            Debug.Log($"[DungeonEntranceRoom] Transform position: {transform.position}");
            Debug.Log($"[DungeonEntranceRoom] Room size: {roomWidth}m x {roomLength}m");
            Debug.Log("========================================");

            // Clear any existing children EXCEPT PlayerSpawnPoint
            foreach (Transform child in transform)
            {
                // Preserve spawn points and important objects
                if (child.name.Contains("Spawn") || child.name.Contains("spawn"))
                {
                    Debug.Log($"[DungeonEntranceRoom] Preserving: {child.name} at {child.position}");
                    continue;
                }
                Debug.Log($"[DungeonEntranceRoom] Destroying: {child.name}");
                Destroy(child.gameObject);
            }

            // Create LARGE, OBVIOUS floor for debugging - BRIGHT BLUE
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "EntranceFloor";
            floor.transform.SetParent(transform);
            floor.transform.localPosition = new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(roomWidth, 1f, roomLength);

            Renderer floorRenderer = floor.GetComponent<Renderer>();
            if (floorRenderer != null)
            {
                // Create URP-compatible material
                Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                floorMat.color = Color.cyan;
                floorRenderer.material = floorMat;
                Debug.Log("[DungeonEntranceRoom] Created CYAN floor with URP/Lit shader");
            }

            // TEMPORARILY DISABLED: No walls or ceiling for debugging
            // Just create open floor space so player can see clearly
            Debug.Log("[DungeonEntranceRoom] Skipping walls/ceiling for debugging");

            // TEMPORARILY DISABLED: No torches or pillars for debugging
            Debug.Log("[DungeonEntranceRoom] Skipping torches and pillars for debugging");

            // Always add a VERY BRIGHT light so we can see
            GameObject ambientLight = new GameObject("AmbientLight");
            ambientLight.transform.SetParent(transform);
            ambientLight.transform.localPosition = new Vector3(0, 5f, 0);
            Light light = ambientLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.intensity = 50f; // VERY BRIGHT
            light.range = 100f; // VERY LARGE RANGE
            light.color = Color.white;
            Debug.Log("[DungeonEntranceRoom] Created VERY BRIGHT light for debugging");

            // Add a YELLOW sphere at spawn point for visual debugging
            Transform spawnPoint = transform.Find("PlayerSpawnPoint");
            if (spawnPoint != null)
            {
                GameObject spawnMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spawnMarker.name = "SpawnMarker_DEBUG";
                spawnMarker.transform.SetParent(spawnPoint);
                spawnMarker.transform.localPosition = Vector3.zero;
                spawnMarker.transform.localScale = Vector3.one * 0.5f;

                Renderer markerRenderer = spawnMarker.GetComponent<Renderer>();
                if (markerRenderer != null)
                {
                    // Create URP-compatible material with emission
                    Material markerMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    markerMat.color = Color.yellow;
                    markerMat.EnableKeyword("_EMISSION");
                    markerMat.SetColor("_EmissionColor", Color.yellow * 2f);
                    markerRenderer.material = markerMat;
                }

                // Remove collider
                Collider markerCollider = spawnMarker.GetComponent<Collider>();
                if (markerCollider != null)
                    Destroy(markerCollider);

                Debug.Log($"[DungeonEntranceRoom] Created YELLOW spawn marker at {spawnPoint.position}");
            }

            Debug.Log("========================================");
            Debug.Log($"[DungeonEntranceRoom] ✓ Built SIMPLE DEBUG entrance room");
            Debug.Log($"[DungeonEntranceRoom] Position: {transform.position}, Size: {roomWidth}x{roomLength}");
            Debug.Log("[DungeonEntranceRoom] Look for: CYAN floor + YELLOW spawn marker");
            Debug.Log("========================================");
        }

        void PlaceTorch(Vector3 localPosition)
        {
            GameObject torch = new GameObject($"Torch");
            torch.transform.SetParent(transform);
            torch.transform.localPosition = localPosition;

            // Holder
            GameObject holder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            holder.name = "Holder";
            holder.transform.SetParent(torch.transform);
            holder.transform.localPosition = Vector3.zero;
            holder.transform.localScale = new Vector3(0.15f, 0.5f, 0.15f);
            Renderer holderRenderer = holder.GetComponent<Renderer>();
            if (holderRenderer != null)
            {
                holderRenderer.material.color = new Color(0.1f, 0.1f, 0.1f);
            }

            // Flame
            GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flame.name = "Flame";
            flame.transform.SetParent(torch.transform);
            flame.transform.localPosition = new Vector3(0, 0.6f, 0);
            flame.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);
            Renderer flameRenderer = flame.GetComponent<Renderer>();
            if (flameRenderer != null)
            {
                flameRenderer.material.color = new Color(1f, 0.6f, 0.2f);
                flameRenderer.material.EnableKeyword("_EMISSION");
                flameRenderer.material.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.1f) * 2f);
            }

            // Light
            Light torchLight = torch.AddComponent<Light>();
            torchLight.type = LightType.Point;
            torchLight.intensity = 8f;
            torchLight.range = 12f;
            torchLight.color = new Color(1f, 0.7f, 0.4f);
        }

        void CreatePillar(Vector3 localPosition)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Pillar";
            pillar.transform.SetParent(transform);
            pillar.transform.localPosition = localPosition + new Vector3(0, 2f, 0);
            pillar.transform.localScale = new Vector3(0.8f, 4f, 0.8f);

            Renderer pillarRenderer = pillar.GetComponent<Renderer>();
            if (pillarRenderer != null)
            {
                pillarRenderer.material.color = new Color(0.4f, 0.4f, 0.45f);
            }

            // Cap
            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cap.name = "Cap";
            cap.transform.SetParent(pillar.transform);
            cap.transform.localPosition = new Vector3(0, 0.5f, 0);
            cap.transform.localScale = new Vector3(1.3f, 0.15f, 1.3f);
            Renderer capRenderer = cap.GetComponent<Renderer>();
            if (capRenderer != null)
            {
                capRenderer.material.color = new Color(0.35f, 0.35f, 0.4f);
            }
        }
    }
}
