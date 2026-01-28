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

        [Tooltip("Length of entrance room (Z axis) - aligned with Start room at Z=5")]
        public float roomLength = 10f;

        [Header("Features")]
        [Tooltip("Add decorative pillars")]
        public bool addPillars = false;

        [Tooltip("Add torches for lighting")]
        public bool addTorches = false;

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
            floor.layer = LayerMask.NameToLayer("Default"); // Ensure teleportation works
            floor.transform.SetParent(transform);
            floor.transform.localPosition = new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(roomWidth, 1f, roomLength);

            Renderer floorRenderer = floor.GetComponent<Renderer>();
            if (floorRenderer != null)
            {
                // Create URP-compatible material - dark stone floor
                Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                floorMat.color = new Color(0.3f, 0.25f, 0.2f); // Dark brown stone
                floorRenderer.material = floorMat;
                Debug.Log("[DungeonEntranceRoom] Created stone floor");
            }

            // Add TeleportationArea component for valid teleportation
            var teleportArea = floor.AddComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea>();

            // Set interaction layer to match teleport interactors (bit 31 = Teleport layer)
            // This is required for XRRayInteractor to recognize this as a valid teleport target
            teleportArea.interactionLayers = UnityEngine.XR.Interaction.Toolkit.InteractionLayerMask.GetMask("Teleport");

            Debug.Log($"[DungeonEntranceRoom] Added TeleportationArea to floor with interaction layers: {teleportArea.interactionLayers}");

            // WALLS AND CEILING REMOVED FOR DEBUGGING
            // Create walls (3 sides only - north side completely open for dungeon entrance)
            // North side has NO WALL to allow passage into Start room at Z=5
            // CreateWallWithDoorway(new Vector3(0, 2f, roomLength/2), new Vector3(roomWidth, 4f, 1f), true); // North - REMOVED
            // CreateWall(new Vector3(0, 2f, -roomLength/2), new Vector3(roomWidth, 4f, 1f)); // South - REMOVED
            // CreateWall(new Vector3(-roomWidth/2, 2f, 0), new Vector3(1f, 4f, roomLength)); // West - REMOVED
            // CreateWall(new Vector3(roomWidth/2, 2f, 0), new Vector3(1f, 4f, roomLength)); // East - REMOVED

            // Create ceiling if requested - REMOVED FOR DEBUGGING
            // if (addCeiling)
            // {
            //     GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //     ceiling.name = "Ceiling";
            //     ceiling.transform.SetParent(transform);
            //     ceiling.transform.localPosition = new Vector3(0, 4.5f, 0);
            //     ceiling.transform.localScale = new Vector3(roomWidth, 1f, roomLength);
            //
            //     Renderer ceilingRenderer = ceiling.GetComponent<Renderer>();
            //     if (ceilingRenderer != null)
            //     {
            //         Material ceilingMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            //         ceilingMat.color = new Color(0.2f, 0.2f, 0.25f); // Dark blue-gray
            //         ceilingRenderer.material = ceilingMat;
            //     }
            // }

            // Add torches for lighting
            if (addTorches)
            {
                PlaceTorch(new Vector3(-roomWidth/2 + 2, 2.5f, roomLength/2 - 2));
                PlaceTorch(new Vector3(roomWidth/2 - 2, 2.5f, roomLength/2 - 2));
                PlaceTorch(new Vector3(-roomWidth/2 + 2, 2.5f, -roomLength/2 + 2));
                PlaceTorch(new Vector3(roomWidth/2 - 2, 2.5f, -roomLength/2 + 2));
            }

            // Add pillars if requested
            if (addPillars)
            {
                CreatePillar(new Vector3(-roomWidth/2 + 5, 0, roomLength/2 - 5));
                CreatePillar(new Vector3(roomWidth/2 - 5, 0, roomLength/2 - 5));
                CreatePillar(new Vector3(-roomWidth/2 + 5, 0, -roomLength/2 + 5));
                CreatePillar(new Vector3(roomWidth/2 - 5, 0, -roomLength/2 + 5));
            }

            // Add ambient light
            if (addAmbientLight)
            {
                GameObject ambientLight = new GameObject("AmbientLight");
                ambientLight.transform.SetParent(transform);
                ambientLight.transform.localPosition = new Vector3(0, 3f, 0);
                Light light = ambientLight.AddComponent<Light>();
                light.type = LightType.Point;
                light.intensity = ambientIntensity * 10f;
                light.range = 30f;
                light.color = new Color(1f, 0.9f, 0.7f); // Warm light
            }

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
            Debug.Log($"[DungeonEntranceRoom] ✓ Built entrance room");
            Debug.Log($"[DungeonEntranceRoom] Position: {transform.position}, Size: {roomWidth}x{roomLength}");
            Debug.Log($"[DungeonEntranceRoom] North side OPEN (no wall) at Z={transform.position.z + roomLength/2f}");
            Debug.Log($"[DungeonEntranceRoom] Connects to Start room south wall at Z=5");
            Debug.Log("========================================");
        }

        void CreateWall(Vector3 localPosition, Vector3 localScale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.SetParent(transform);
            wall.transform.localPosition = localPosition;
            wall.transform.localScale = localScale;

            Renderer wallRenderer = wall.GetComponent<Renderer>();
            if (wallRenderer != null)
            {
                Material wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                wallMat.color = new Color(0.35f, 0.35f, 0.4f); // Dark gray stone
                wallRenderer.material = wallMat;
            }
        }

        void CreateWallWithDoorway(Vector3 localPosition, Vector3 localScale, bool isNorthWall)
        {
            // Create wall segments on either side of doorway opening
            // Doorway width = 12m to fit Start room (X=-4 to X=+6, spans 10m but needs 2m margin)
            float doorwayWidth = 12f; // Width of doorway opening
            float wallWidth = localScale.x;
            float segmentWidth = (wallWidth - doorwayWidth) / 2f;

            if (segmentWidth > 0.5f)
            {
                // Left wall segment
                GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leftWall.name = "WallLeft";
                leftWall.transform.SetParent(transform);
                leftWall.transform.localPosition = localPosition + new Vector3(-doorwayWidth/2f - segmentWidth/2f, 0, 0);
                leftWall.transform.localScale = new Vector3(segmentWidth, localScale.y, localScale.z);

                Renderer leftRenderer = leftWall.GetComponent<Renderer>();
                if (leftRenderer != null)
                {
                    Material wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    wallMat.color = new Color(0.35f, 0.35f, 0.4f);
                    leftRenderer.material = wallMat;
                }

                // Right wall segment
                GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rightWall.name = "WallRight";
                rightWall.transform.SetParent(transform);
                rightWall.transform.localPosition = localPosition + new Vector3(doorwayWidth/2f + segmentWidth/2f, 0, 0);
                rightWall.transform.localScale = new Vector3(segmentWidth, localScale.y, localScale.z);

                Renderer rightRenderer = rightWall.GetComponent<Renderer>();
                if (rightRenderer != null)
                {
                    Material wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    wallMat.color = new Color(0.35f, 0.35f, 0.4f);
                    rightRenderer.material = wallMat;
                }
            }

            // Add doorway frame pillars
            float doorwayHeight = 3.5f;

            // Left pillar
            GameObject leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftPillar.name = "DoorwayPillarLeft";
            leftPillar.transform.SetParent(transform);
            leftPillar.transform.localPosition = localPosition + new Vector3(-doorwayWidth/2f, doorwayHeight/2f - 0.5f, 0);
            leftPillar.transform.localScale = new Vector3(0.4f, doorwayHeight, 0.4f);

            Renderer leftPillarRenderer = leftPillar.GetComponent<Renderer>();
            if (leftPillarRenderer != null)
            {
                Material pillarMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                pillarMat.color = new Color(0.5f, 0.48f, 0.52f); // Lighter stone for accent
                leftPillarRenderer.material = pillarMat;
            }

            // Right pillar
            GameObject rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightPillar.name = "DoorwayPillarRight";
            rightPillar.transform.SetParent(transform);
            rightPillar.transform.localPosition = localPosition + new Vector3(doorwayWidth/2f, doorwayHeight/2f - 0.5f, 0);
            rightPillar.transform.localScale = new Vector3(0.4f, doorwayHeight, 0.4f);

            Renderer rightPillarRenderer = rightPillar.GetComponent<Renderer>();
            if (rightPillarRenderer != null)
            {
                Material pillarMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                pillarMat.color = new Color(0.5f, 0.48f, 0.52f);
                rightPillarRenderer.material = pillarMat;
            }

            // Lintel (top piece)
            GameObject lintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lintel.name = "DoorwayLintel";
            lintel.transform.SetParent(transform);
            lintel.transform.localPosition = localPosition + new Vector3(0, doorwayHeight - 0.5f, 0);
            lintel.transform.localScale = new Vector3(doorwayWidth + 0.8f, 0.4f, 0.4f);

            Renderer lintelRenderer = lintel.GetComponent<Renderer>();
            if (lintelRenderer != null)
            {
                Material lintelMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                lintelMat.color = new Color(0.5f, 0.48f, 0.52f);
                lintelRenderer.material = lintelMat;
            }

            Debug.Log($"[DungeonEntranceRoom] Created doorway opening at {localPosition}");
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
                Material holderMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                holderMat.color = new Color(0.1f, 0.1f, 0.1f);
                holderRenderer.material = holderMat;
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
                Material flameMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                flameMat.color = new Color(1f, 0.6f, 0.2f);
                flameMat.EnableKeyword("_EMISSION");
                flameMat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.1f) * 2f);
                flameRenderer.material = flameMat;
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
                Material pillarMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                pillarMat.color = new Color(0.4f, 0.4f, 0.45f);
                pillarRenderer.material = pillarMat;
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
                Material capMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                capMat.color = new Color(0.35f, 0.35f, 0.4f);
                capRenderer.material = capMat;
            }
        }
    }
}
