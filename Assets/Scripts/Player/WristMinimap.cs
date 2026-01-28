using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Simple wrist-mounted minimap that shows dungeon layout
    /// Appears on left wrist when looking at it
    /// </summary>
    public class WristMinimap : MonoBehaviour
    {
        [Header("Minimap Settings")]
        [Tooltip("Minimap size in world units")]
        public float minimapSize = 0.1f;

        [Header("Position Settings - Try different values!")]
        [Tooltip("Position offset from left hand (local space). X=left/right, Y=up(back of hand)/down(wrist), Z=forward(knuckles)/back(palm). Use -Y for wrist!")]
        public Vector3 leftWristOffset = new Vector3(0f, -0.1f, 0f);

        [Tooltip("Position offset from right hand (local space). X=left/right, Y=up(back of hand)/down(wrist), Z=forward(knuckles)/back(palm). Use -Y for wrist!")]
        public Vector3 rightWristOffset = new Vector3(0f, -0.1f, 0f);

        [Header("Quick Test")]
        [Tooltip("Test: Place minimap far from hand to see if position updates work")]
        public bool testMode = false;

        [Tooltip("Test distance from hand")]
        public float testDistance = 0.3f;

        [Tooltip("Alternative: Use negative Y to position below hand")]
        public bool useAlternativePositioning = false;

        [Tooltip("If alternative positioning: offset along hand's 'down' direction")]
        public float wristDistanceDown = 0.08f;

        [Header("Rotation Settings")]
        [Tooltip("Rotation offset for LEFT hand minimap (Euler angles). Z=-90 = 90° counterclockwise")]
        public Vector3 leftMinimapRotationOffset = new Vector3(0f, 0f, -90f);

        [Tooltip("Rotation offset for RIGHT hand minimap (Euler angles). Z=90 = 90° clockwise")]
        public Vector3 rightMinimapRotationOffset = new Vector3(0f, 0f, 90f);

        [Tooltip("Distance from camera to show minimap")]
        [Range(0.1f, 1f)]
        public float activationDistance = 0.4f;

        [Tooltip("Update interval in seconds")]
        public float updateInterval = 0.1f;

        [Header("Colors")]
        public Color backgroundColor = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        public Color roomColor = new Color(0.3f, 0.3f, 0.35f);
        public Color playerDotColor = Color.cyan;
        public Color visitedRoomColor = new Color(0.25f, 0.25f, 0.3f);

        [Header("Debug")]
        public bool showDebug = false;
        public bool alwaysVisible = false; // For testing
        public bool aggressiveLogging = false; // Log EVERY frame

        // Internal state
        private static WristMinimap instance;
        private GameObject minimapRoot;
        private Canvas minimapCanvas;
        private GameObject minimapContent;
        private Transform leftHand;
        private Transform rightHand;
        private Transform headCamera;
        private float nextUpdateTime;
        private List<GameObject> roomVisuals = new List<GameObject>();
        private GameObject playerDot;
        private bool isVisible = false;
        private Transform activeWrist;

        void Awake()
        {
            // Singleton pattern - only one minimap should exist
            if (instance != null && instance != this)
            {
                Debug.LogWarning($"[WristMinimap] Found existing instance. Copying new offset values from {gameObject.name} to {instance.gameObject.name}");

                // Copy the NEW offset values to the existing instance (in case scene has updated values)
                instance.leftWristOffset = this.leftWristOffset;
                instance.rightWristOffset = this.rightWristOffset;
                instance.leftMinimapRotationOffset = this.leftMinimapRotationOffset;
                instance.rightMinimapRotationOffset = this.rightMinimapRotationOffset;
                instance.minimapSize = this.minimapSize;
                instance.alwaysVisible = this.alwaysVisible;
                instance.aggressiveLogging = this.aggressiveLogging;

                Debug.Log($"[WristMinimap] Updated offsets: Left={instance.leftWristOffset}, Right={instance.rightWristOffset}");
                Debug.Log($"[WristMinimap] Updated rotations: Left={instance.leftMinimapRotationOffset}, Right={instance.rightMinimapRotationOffset}");

                Destroy(gameObject);
                return;
            }

            instance = this;

            // Make minimap persist across scene changes
            DontDestroyOnLoad(gameObject);
            Debug.Log("[WristMinimap] Minimap set to persist across scenes");

            // Subscribe to scene changes to re-find hands
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            Debug.Log($"[WristMinimap] Scene loaded: {scene.name}. Re-finding hands...");

            // Clear old references
            leftHand = null;
            rightHand = null;
            headCamera = null;
            activeWrist = null;

            // Re-initialize after scene change
            StartCoroutine(DelayedInitialization());
        }

        void Start()
        {
            Debug.Log("[WristMinimap] Start() called - beginning delayed initialization...");
            Debug.Log($"[WristMinimap] Current offset values: Left={leftWristOffset}, Right={rightWristOffset}");
            StartCoroutine(DelayedInitialization());
        }

        System.Collections.IEnumerator DelayedInitialization()
        {
            // Wait for VR to fully initialize
            yield return new WaitForSeconds(2f);

            Debug.Log("[WristMinimap] Delayed initialization starting...");

            // Find camera
            for (int attempt = 0; attempt < 5; attempt++)
            {
                headCamera = Camera.main?.transform;
                if (headCamera != null)
                {
                    Debug.Log($"[WristMinimap] Found camera: {headCamera.name}");
                    break;
                }
                Debug.LogWarning($"[WristMinimap] Camera not found (attempt {attempt + 1}/5), retrying...");
                yield return new WaitForSeconds(0.5f);
            }

            if (headCamera == null)
            {
                Debug.LogError("[WristMinimap] No main camera found after 5 attempts! Minimap disabled.");
                enabled = false;
                yield break;
            }

            // Find hand controllers with retries
            for (int attempt = 0; attempt < 5; attempt++)
            {
                FindHandControllers();

                if (leftHand != null || rightHand != null)
                {
                    Debug.Log($"[WristMinimap] Found hands - Left: {leftHand?.name}, Right: {rightHand?.name}");
                    break;
                }

                Debug.LogWarning($"[WristMinimap] Hands not found (attempt {attempt + 1}/5), retrying...");
                yield return new WaitForSeconds(0.5f);
            }

            if (leftHand == null && rightHand == null)
            {
                Debug.LogError("[WristMinimap] No hand controllers found after 5 attempts! Minimap disabled.");
                Debug.LogError("[WristMinimap] Searched for: LeftHand, RightHand, Left, Right, LeftHandController, RightHandController");
                enabled = false;
                yield break;
            }

            CreateMinimapUI();

            Debug.Log($"[WristMinimap] Successfully initialized!");
            Debug.Log($"  LeftHand: {leftHand?.name}");
            Debug.Log($"  RightHand: {rightHand?.name}");
            Debug.Log($"  Camera: {headCamera.name}");
            Debug.LogWarning($"  LEFT OFFSET: {leftWristOffset} (should be (0, 0, -0.15) for wrist)");
            Debug.LogWarning($"  RIGHT OFFSET: {rightWristOffset} (should be (0, 0, -0.15) for wrist)");

            if (alwaysVisible)
            {
                ShowMinimap(leftHand ?? rightHand);
            }
        }

        void FindHandControllers()
        {
            // Method 1: Try to find actual controller/wrist base (not attach child)
            Unity.XR.CoreUtils.XROrigin xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
            {
                // Look for LeftHand/RightHand Controller (base transform, not attach child)
                Transform[] allTransforms = xrOrigin.GetComponentsInChildren<Transform>();

                foreach (Transform t in allTransforms)
                {
                    string name = t.name.ToLower();

                    // Prioritize controller base, NOT attach child
                    bool isLeftController = name.Contains("lefthand") && name.Contains("controller") && !name.Contains("attach");
                    bool isRightController = name.Contains("righthand") && name.Contains("controller") && !name.Contains("attach");

                    if (isLeftController && leftHand == null)
                    {
                        leftHand = t;
                        Debug.Log($"[WristMinimap] Found left controller BASE: {t.name} (path: {GetTransformPath(t)})");
                    }
                    else if (isRightController && rightHand == null)
                    {
                        rightHand = t;
                        Debug.Log($"[WristMinimap] Found right controller BASE: {t.name} (path: {GetTransformPath(t)})");
                    }
                }
            }

            // Method 2: Search all transforms if Method 1 failed
            if (leftHand == null || rightHand == null)
            {
                Debug.LogWarning("[WristMinimap] Controller base not found, searching all transforms...");
                Transform[] allTransforms = FindObjectsOfType<Transform>();
                foreach (Transform t in allTransforms)
                {
                    string name = t.name.ToLower();

                    // Avoid attach children
                    if (name.Contains("attach"))
                        continue;

                    if ((name.Contains("lefthand") || (name.Contains("left") && name.Contains("controller"))) && leftHand == null)
                    {
                        leftHand = t;
                        Debug.Log($"[WristMinimap] Found left hand: {t.name} (path: {GetTransformPath(t)})");
                    }
                    else if ((name.Contains("righthand") || (name.Contains("right") && name.Contains("controller"))) && rightHand == null)
                    {
                        rightHand = t;
                        Debug.Log($"[WristMinimap] Found right hand: {t.name} (path: {GetTransformPath(t)})");
                    }

                    if (leftHand != null && rightHand != null)
                        break;
                }
            }

            // Method 3: Last resort - use attach child but warn
            if (leftHand == null || rightHand == null)
            {
                Debug.LogWarning("[WristMinimap] Controller base not found, falling back to attach child (fingertip)...");
                if (xrOrigin != null)
                {
                    if (leftHand == null)
                        leftHand = FindInChildren(xrOrigin.transform, "left");
                    if (rightHand == null)
                        rightHand = FindInChildren(xrOrigin.transform, "right");
                }
            }
        }

        string GetTransformPath(Transform t)
        {
            string path = t.name;
            Transform parent = t.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        Transform FindInChildren(Transform parent, string searchTerm)
        {
            foreach (Transform child in parent.GetComponentsInChildren<Transform>())
            {
                if (child.name.ToLower().Contains(searchTerm))
                {
                    if (showDebug) Debug.Log($"[WristMinimap] Found {searchTerm}: {child.name}");
                    return child;
                }
            }
            return null;
        }

        void CreateMinimapUI()
        {
            // Clean up any existing WristMinimap canvases from old sessions
            GameObject[] oldMinimaps = GameObject.FindObjectsOfType<GameObject>().Where(go => go.name == "WristMinimap" && go != minimapRoot).ToArray();
            if (oldMinimaps.Length > 0)
            {
                Debug.Log($"[WristMinimap] Cleaning up {oldMinimaps.Length} old minimap canvas(es)");
                foreach (GameObject old in oldMinimaps)
                {
                    Destroy(old);
                }
            }

            // Create root object - NO PARENT so it can move freely in world space
            minimapRoot = new GameObject("WristMinimap");
            minimapRoot.transform.SetParent(null); // Ensure it's in world space, not parented

            // Create canvas
            minimapCanvas = minimapRoot.AddComponent<Canvas>();
            minimapCanvas.renderMode = RenderMode.WorldSpace;

            CanvasScaler scaler = minimapRoot.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100;

            RectTransform canvasRT = minimapRoot.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(400, 400);
            canvasRT.localScale = new Vector3(minimapSize / 400f, minimapSize / 400f, minimapSize / 400f);

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(minimapRoot.transform, false);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = backgroundColor;
            RectTransform bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            // Border
            GameObject border = new GameObject("Border");
            border.transform.SetParent(minimapRoot.transform, false);
            Image borderImage = border.AddComponent<Image>();
            borderImage.color = new Color(0.4f, 0.4f, 0.5f);
            RectTransform borderRT = border.GetComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.sizeDelta = new Vector2(10, 10); // Outline

            // Content area
            minimapContent = new GameObject("Content");
            minimapContent.transform.SetParent(minimapRoot.transform, false);
            RectTransform contentRT = minimapContent.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.1f, 0.1f);
            contentRT.anchorMax = new Vector2(0.9f, 0.9f);
            contentRT.sizeDelta = Vector2.zero;

            // Draw dungeon rooms
            DrawDungeonRooms();

            // Create player dot
            CreatePlayerDot();

            minimapRoot.SetActive(false);
        }

        void DrawDungeonRooms()
        {
            // Find dungeon generator
            Dungeon.DungeonGenerator dungeonGen = FindFirstObjectByType<Dungeon.DungeonGenerator>();
            if (dungeonGen == null)
            {
                if (showDebug) Debug.LogWarning("[WristMinimap] No dungeon generator found");
                return;
            }

            // Get rooms via reflection
            var roomsField = typeof(Dungeon.DungeonGenerator).GetField("allRooms",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (roomsField == null) return;

            List<Dungeon.DungeonRoom> rooms = roomsField.GetValue(dungeonGen) as List<Dungeon.DungeonRoom>;
            if (rooms == null || rooms.Count == 0) return;

            // Find bounds of dungeon
            float minX = float.MaxValue, maxX = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            foreach (var room in rooms)
            {
                float roomSize = room.sizeInGrids * 2f;
                minX = Mathf.Min(minX, room.worldPosition.x);
                maxX = Mathf.Max(maxX, room.worldPosition.x + roomSize);
                minZ = Mathf.Min(minZ, room.worldPosition.z);
                maxZ = Mathf.Max(maxZ, room.worldPosition.z + roomSize);
            }

            float dungeonWidth = maxX - minX;
            float dungeonHeight = maxZ - minZ;
            float dungeonCenterX = (minX + maxX) / 2f;
            float dungeonCenterZ = (minZ + maxZ) / 2f;

            float scale = 320f / Mathf.Max(dungeonWidth, dungeonHeight);

            // Draw each room
            foreach (var room in rooms)
            {
                GameObject roomObj = new GameObject($"Room_{room.gridPosition}");
                roomObj.transform.SetParent(minimapContent.transform, false);

                Image roomImage = roomObj.AddComponent<Image>();
                roomImage.color = roomColor;

                RectTransform roomRT = roomObj.GetComponent<RectTransform>();
                float roomSize = room.sizeInGrids * 2f * scale * 0.9f;
                roomRT.sizeDelta = new Vector2(roomSize, roomSize);

                // Position relative to dungeon center
                float x = (room.worldPosition.x - dungeonCenterX) * scale;
                float z = (room.worldPosition.z - dungeonCenterZ) * scale;
                roomRT.anchoredPosition = new Vector2(x, z);

                roomVisuals.Add(roomObj);
            }

            if (showDebug)
                Debug.Log($"[WristMinimap] Drew {rooms.Count} rooms on minimap");
        }

        void CreatePlayerDot()
        {
            playerDot = new GameObject("PlayerArrow");
            playerDot.transform.SetParent(minimapContent.transform, false);

            RectTransform dotRT = playerDot.AddComponent<RectTransform>();
            dotRT.sizeDelta = new Vector2(20, 20);
            dotRT.anchoredPosition = Vector2.zero;

            // Create arrow using a triangle
            GameObject arrow = new GameObject("ArrowTriangle");
            arrow.transform.SetParent(playerDot.transform, false);

            // Add Image component for the arrow
            Image arrowImage = arrow.AddComponent<Image>();
            arrowImage.color = playerDotColor;

            // Create triangle sprite for arrow pointing up (forward)
            // Unity's default sprite is a square, so we'll use a filled triangle
            RectTransform arrowRT = arrow.GetComponent<RectTransform>();
            arrowRT.anchorMin = Vector2.zero;
            arrowRT.anchorMax = Vector2.one;
            arrowRT.sizeDelta = Vector2.zero;

            // Create triangle mesh for the arrow
            CreateArrowTriangle(arrow);
        }

        void CreateArrowTriangle(GameObject arrowObj)
        {
            // Create a canvas triangle by using vertices
            // For UI, we'll create three small boxes to form a triangle shape
            // Better approach: use a polygon or create actual triangle

            // Create arrow using simple Image rotation
            // Arrow points UP (forward in minimap space)
            Image arrowImage = arrowObj.GetComponent<Image>();

            // Try to find or create a triangle sprite
            // For now, we'll create a diamond/arrow shape using transform
            GameObject tip = new GameObject("Tip");
            tip.transform.SetParent(arrowObj.transform, false);
            Image tipImage = tip.AddComponent<Image>();
            tipImage.color = playerDotColor;
            RectTransform tipRT = tip.GetComponent<RectTransform>();
            tipRT.sizeDelta = new Vector2(8, 12); // Narrow and tall
            tipRT.anchoredPosition = new Vector2(0, 4); // Top
            tipRT.rotation = Quaternion.Euler(0, 0, 0);

            GameObject base1 = new GameObject("BaseLeft");
            base1.transform.SetParent(arrowObj.transform, false);
            Image base1Image = base1.AddComponent<Image>();
            base1Image.color = playerDotColor;
            RectTransform base1RT = base1.GetComponent<RectTransform>();
            base1RT.sizeDelta = new Vector2(6, 6);
            base1RT.anchoredPosition = new Vector2(-3, -3);

            GameObject base2 = new GameObject("BaseRight");
            base2.transform.SetParent(arrowObj.transform, false);
            Image base2Image = base2.AddComponent<Image>();
            base2Image.color = playerDotColor;
            RectTransform base2RT = base2.GetComponent<RectTransform>();
            base2RT.sizeDelta = new Vector2(6, 6);
            base2RT.anchoredPosition = new Vector2(3, -3);

            // Destroy the parent image since we're building arrow from children
            Destroy(arrowImage);
        }

        void LateUpdate()
        {
            if (aggressiveLogging && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[WristMinimap] LateUpdate - Frame {Time.frameCount}");
                Debug.Log($"  headCamera: {headCamera?.name ?? "NULL"}");
                Debug.Log($"  leftHand: {leftHand?.name ?? "NULL"}");
                Debug.Log($"  rightHand: {rightHand?.name ?? "NULL"}");
                Debug.Log($"  activeWrist: {activeWrist?.name ?? "NULL"}");
                Debug.Log($"  isVisible: {isVisible}");
            }

            if (headCamera == null)
            {
                if (aggressiveLogging && Time.frameCount % 60 == 0)
                    Debug.LogWarning("[WristMinimap] LateUpdate EARLY EXIT - headCamera is NULL");
                return;
            }

            // Check wrist visibility
            Transform targetWrist = GetVisibleWrist();

            if (alwaysVisible)
            {
                if (!isVisible)
                    ShowMinimap(leftHand ?? rightHand);
                targetWrist = activeWrist ?? leftHand ?? rightHand;

                if (showDebug && Time.frameCount % 120 == 0)
                {
                    Debug.Log($"[WristMinimap] AlwaysVisible mode: activeWrist={activeWrist?.name}, targetWrist={targetWrist?.name}");
                }
            }

            if (targetWrist != null)
            {
                if (!isVisible || targetWrist != activeWrist)
                {
                    if (aggressiveLogging)
                        Debug.Log($"[WristMinimap] Showing minimap on {targetWrist.name}");
                    ShowMinimap(targetWrist);
                }
                // Update position every frame in LateUpdate for instant following
                UpdateMinimapPosition(targetWrist);
            }
            else if (isVisible && !alwaysVisible)
            {
                if (aggressiveLogging)
                    Debug.Log("[WristMinimap] Hiding minimap - no visible wrist");
                HideMinimap();
            }

            // Update player position every frame for smooth rotation
            UpdatePlayerPosition();
        }

        Transform GetVisibleWrist()
        {
            // Check both wrists, prioritize left
            if (IsWristVisible(leftHand))
                return leftHand;
            if (IsWristVisible(rightHand))
                return rightHand;
            return null;
        }

        bool IsWristVisible(Transform wrist)
        {
            if (wrist == null || headCamera == null) return false;

            float distance = Vector3.Distance(headCamera.position, wrist.position);
            if (distance > activationDistance) return false;

            // Check if looking at wrist (angle check)
            Vector3 toWrist = (wrist.position - headCamera.position).normalized;
            float angle = Vector3.Angle(headCamera.forward, toWrist);

            return angle < 70f;
        }

        void ShowMinimap(Transform wrist)
        {
            activeWrist = wrist;
            if (minimapRoot != null)
            {
                minimapRoot.SetActive(true);
                isVisible = true;
                if (showDebug)
                    Debug.Log($"[WristMinimap] Showing on {wrist.name}");
            }
        }

        void HideMinimap()
        {
            if (minimapRoot != null)
            {
                minimapRoot.SetActive(false);
                isVisible = false;
                activeWrist = null;
                if (showDebug)
                    Debug.Log("[WristMinimap] Hidden");
            }
        }

        void UpdateMinimapPosition(Transform wrist)
        {
            // AGGRESSIVE LOGGING: Log every call
            if (aggressiveLogging)
            {
                Debug.Log($"[WristMinimap] UpdateMinimapPosition CALLED - Frame {Time.frameCount}");
            }

            if (minimapRoot == null || wrist == null)
            {
                if (showDebug || aggressiveLogging)
                    Debug.LogWarning($"[WristMinimap] UpdateMinimapPosition failed: root={minimapRoot != null}, wrist={wrist != null}");
                return;
            }

            Vector3 oldPosition = minimapRoot.transform.position;
            Vector3 targetPosition;

            if (testMode)
            {
                // TEST MODE: Place very far from hand to verify position updates are working
                targetPosition = wrist.position + wrist.forward * testDistance;
                if (aggressiveLogging || (showDebug && Time.frameCount % 120 == 0))
                {
                    Debug.LogWarning($"[WristMinimap] TEST MODE ACTIVE - minimap at {testDistance}m in front of hand");
                }
            }
            else if (useAlternativePositioning)
            {
                // Alternative: Position along hand's down direction (toward wrist/elbow)
                targetPosition = wrist.position - wrist.up * wristDistanceDown;
                if (aggressiveLogging)
                {
                    Debug.Log($"[WristMinimap] ALT positioning: wrist.pos={wrist.position}, wrist.up={wrist.up}, distance={wristDistanceDown}");
                    Debug.Log($"[WristMinimap] Target = {targetPosition}");
                }
            }
            else
            {
                // Standard: Use local offset
                Vector3 offset = wrist == leftHand ? leftWristOffset : rightWristOffset;
                Vector3 worldOffset = wrist.TransformDirection(offset);
                targetPosition = wrist.position + worldOffset;

                if (aggressiveLogging)
                {
                    Debug.Log($"[WristMinimap] STANDARD positioning:");
                    Debug.Log($"  Wrist: {wrist.name}");
                    Debug.Log($"  Wrist Path: {GetTransformPath(wrist)}");
                    Debug.Log($"  Local offset: {offset}");
                    Debug.Log($"  World offset: {worldOffset}");
                    Debug.Log($"  Wrist position: {wrist.position}");
                    Debug.Log($"  Target position: {targetPosition}");
                    Debug.Log($"  === Controller Coordinate System ===");
                    Debug.Log($"  Controller Forward (+Z): {wrist.forward} (points toward: knuckles/fingers)");
                    Debug.Log($"  Controller Right (+X): {wrist.right}");
                    Debug.Log($"  Controller Up (+Y): {wrist.up} (points toward: back of hand)");
                    Debug.Log($"  Controller Back (-Z): {-wrist.forward} (points toward: palm)");
                    Debug.Log($"  Controller Down (-Y): {-wrist.up} (points toward: wrist/forearm)");
                    Debug.Log($"  === Offset Analysis ===");
                    Debug.Log($"  Offset X={offset.x} component: {wrist.right * offset.x}");
                    Debug.Log($"  Offset Y={offset.y} component: {wrist.up * offset.y}");
                    Debug.Log($"  Offset Z={offset.z} component: {wrist.forward * offset.z}");

                    // Check if this is the fingertip attach child
                    if (wrist.name.ToLower().Contains("attach"))
                    {
                        Debug.LogError("[WristMinimap] ⚠️ WARNING: Using Attach Child (fingertip) instead of controller base!");
                        Debug.LogError("  This will place minimap at fingers instead of wrist.");
                        Debug.LogError("  The FindHandControllers() method should find parent transform, not attach child.");
                    }
                }
            }

            // ACTUALLY SET THE POSITION
            minimapRoot.transform.position = targetPosition;

            if (aggressiveLogging)
            {
                Debug.Log($"[WristMinimap] Position SET:");
                Debug.Log($"  Old: {oldPosition}");
                Debug.Log($"  New: {minimapRoot.transform.position}");
                Debug.Log($"  Changed: {Vector3.Distance(oldPosition, minimapRoot.transform.position) > 0.001f}");
            }

            if (showDebug && Time.frameCount % 120 == 0) // Log every 120 frames
            {
                Debug.Log($"[WristMinimap] Position Update:");
                Debug.Log($"  Wrist: {wrist.name}");
                Debug.Log($"  Alternative Mode: {useAlternativePositioning}");
                if (useAlternativePositioning)
                {
                    Debug.Log($"  Distance down: {wristDistanceDown}");
                    Debug.Log($"  Wrist up vector: {wrist.up}");
                }
                else
                {
                    Vector3 offset = wrist == leftHand ? leftWristOffset : rightWristOffset;
                    Debug.Log($"  Offset (local): {offset}");
                    Debug.Log($"  Wrist forward: {wrist.forward}");
                    Debug.Log($"  Wrist up: {wrist.up}");
                    Debug.Log($"  Wrist right: {wrist.right}");
                }
                Debug.Log($"  Wrist pos: {wrist.position}");
                Debug.Log($"  Minimap pos: {targetPosition}");
                Debug.Log($"  Distance: {Vector3.Distance(wrist.position, targetPosition):F3}m");
            }

            // Rotate with wrist like a real watch (not facing camera)
            // The minimap should be parallel to the back of the hand
            // Wrist forward = along the arm, wrist up = back of hand
            Quaternion wristRotation = wrist.rotation;

            // Apply 90 degree rotation to make it lie flat on wrist (like a watch face)
            // The canvas needs to face up (along the wrist's "up" direction)
            Quaternion watchOrientation = wristRotation * Quaternion.Euler(90f, 0f, 0f);

            // Apply custom rotation offset based on which hand (for wristwatch-like orientation)
            Vector3 rotationOffset = (wrist == leftHand) ? leftMinimapRotationOffset : rightMinimapRotationOffset;
            minimapRoot.transform.rotation = watchOrientation * Quaternion.Euler(rotationOffset);

            if (aggressiveLogging && Time.frameCount % 120 == 0)
            {
                Debug.Log($"[WristMinimap] Rotation applied: hand={wrist.name}, offset={rotationOffset}, final={minimapRoot.transform.rotation.eulerAngles}");
            }
        }

        void UpdatePlayerPosition()
        {
            if (playerDot == null) return;

            RectTransform dotRT = playerDot.GetComponent<RectTransform>();

            // Find dungeon generator to get dungeon bounds
            Dungeon.DungeonGenerator dungeonGen = FindFirstObjectByType<Dungeon.DungeonGenerator>();
            if (dungeonGen == null)
            {
                // No dungeon, keep centered
                dotRT.anchoredPosition = Vector2.zero;
            }
            else
            {
                // Get rooms via reflection
                var roomsField = typeof(Dungeon.DungeonGenerator).GetField("allRooms",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (roomsField != null)
                {
                    List<Dungeon.DungeonRoom> rooms = roomsField.GetValue(dungeonGen) as List<Dungeon.DungeonRoom>;
                    if (rooms != null && rooms.Count > 0)
                    {
                        // Find dungeon bounds
                        float minX = float.MaxValue, maxX = float.MinValue;
                        float minZ = float.MaxValue, maxZ = float.MinValue;

                        foreach (var room in rooms)
                        {
                            float roomSize = room.sizeInGrids * 2f;
                            minX = Mathf.Min(minX, room.worldPosition.x);
                            maxX = Mathf.Max(maxX, room.worldPosition.x + roomSize);
                            minZ = Mathf.Min(minZ, room.worldPosition.z);
                            maxZ = Mathf.Max(maxZ, room.worldPosition.z + roomSize);
                        }

                        float dungeonWidth = maxX - minX;
                        float dungeonHeight = maxZ - minZ;
                        float dungeonCenterX = (minX + maxX) / 2f;
                        float dungeonCenterZ = (minZ + maxZ) / 2f;

                        // Get player position (this WristMinimap component is on the player)
                        Vector3 playerPos = transform.position;

                        // Calculate player position relative to dungeon center
                        float relativeX = playerPos.x - dungeonCenterX;
                        float relativeZ = playerPos.z - dungeonCenterZ;

                        // Scale to minimap coordinates (320 is the content size)
                        float scale = 320f / Mathf.Max(dungeonWidth, dungeonHeight);
                        float minimapX = relativeX * scale;
                        float minimapZ = relativeZ * scale;

                        // Update arrow position on minimap
                        dotRT.anchoredPosition = new Vector2(minimapX, minimapZ);
                    }
                    else
                    {
                        // No rooms, keep centered
                        dotRT.anchoredPosition = Vector2.zero;
                    }
                }
                else
                {
                    dotRT.anchoredPosition = Vector2.zero;
                }
            }

            // Rotate based on player facing direction
            float yaw = transform.eulerAngles.y;
            dotRT.localRotation = Quaternion.Euler(0, 0, -yaw);
        }

        void OnDrawGizmos()
        {
            if (!showDebug || !Application.isPlaying) return;

            if (leftHand != null)
            {
                // Hand position
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(leftHand.position, 0.02f);

                // Draw hand axes
                Gizmos.color = Color.red;
                Gizmos.DrawLine(leftHand.position, leftHand.position + leftHand.right * 0.05f); // X = red
                Gizmos.color = Color.green;
                Gizmos.DrawLine(leftHand.position, leftHand.position + leftHand.up * 0.05f); // Y = green
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(leftHand.position, leftHand.position + leftHand.forward * 0.05f); // Z = blue

                // Draw where the minimap SHOULD be on left wrist
                Vector3 targetPos;
                if (useAlternativePositioning)
                {
                    targetPos = leftHand.position - leftHand.up * wristDistanceDown;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(leftHand.position, targetPos);
                }
                else
                {
                    targetPos = leftHand.position + leftHand.TransformDirection(leftWristOffset);
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(leftHand.position, targetPos);
                }

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(targetPos, 0.04f);
                Gizmos.DrawWireCube(targetPos, Vector3.one * minimapSize);
            }

            if (rightHand != null)
            {
                // Hand position
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(rightHand.position, 0.02f);

                // Draw hand axes
                Gizmos.color = Color.red;
                Gizmos.DrawLine(rightHand.position, rightHand.position + rightHand.right * 0.05f); // X = red
                Gizmos.color = Color.green;
                Gizmos.DrawLine(rightHand.position, rightHand.position + rightHand.up * 0.05f); // Y = green
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(rightHand.position, rightHand.position + rightHand.forward * 0.05f); // Z = blue

                // Draw where the minimap SHOULD be on right wrist
                Vector3 targetPos;
                if (useAlternativePositioning)
                {
                    targetPos = rightHand.position - rightHand.up * wristDistanceDown;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(rightHand.position, targetPos);
                }
                else
                {
                    targetPos = rightHand.position + rightHand.TransformDirection(rightWristOffset);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(rightHand.position, targetPos);
                }

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(targetPos, 0.04f);
                Gizmos.DrawWireCube(targetPos, Vector3.one * minimapSize);
            }

            // Draw where the minimap ACTUALLY is
            if (minimapRoot != null && isVisible)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(minimapRoot.transform.position, Vector3.one * minimapSize * 1.2f);
                Gizmos.DrawWireSphere(minimapRoot.transform.position, 0.01f);
            }
        }

        [ContextMenu("Force Update Position")]
        void ForceUpdatePosition()
        {
            if (activeWrist != null)
            {
                Debug.Log($"[WristMinimap] Forcing position update on {activeWrist.name}");
                UpdateMinimapPosition(activeWrist);
            }
            else
            {
                Debug.LogWarning("[WristMinimap] No active wrist!");
            }
        }

        [ContextMenu("Print Debug Info")]
        void PrintDebugInfo()
        {
            Debug.Log("========================================");
            Debug.Log("=== WristMinimap Debug Info ===");
            Debug.Log("========================================");
            Debug.Log($"Left Hand: {leftHand?.name ?? "NULL"}");
            Debug.Log($"Right Hand: {rightHand?.name ?? "NULL"}");
            Debug.Log($"Active Wrist: {activeWrist?.name ?? "NULL"}");
            Debug.Log($"Head Camera: {headCamera?.name ?? "NULL"}");
            Debug.Log($"Minimap Root: {minimapRoot?.name ?? "NULL"}");
            Debug.Log($"Is Visible: {isVisible}");
            Debug.Log($"Always Visible: {alwaysVisible}");
            Debug.Log("--- Position Settings ---");
            Debug.Log($"Test Mode: {testMode}");
            Debug.Log($"Alternative Positioning: {useAlternativePositioning}");
            Debug.Log($"Left Offset: {leftWristOffset}");
            Debug.Log($"Right Offset: {rightWristOffset}");
            Debug.Log($"Wrist Distance Down: {wristDistanceDown}");
            Debug.Log($"Test Distance: {testDistance}");

            if (activeWrist != null && minimapRoot != null)
            {
                Debug.Log("--- Current State ---");
                Debug.Log($"Wrist Position: {activeWrist.position}");
                Debug.Log($"Minimap Position: {minimapRoot.transform.position}");
                Debug.Log($"Distance: {Vector3.Distance(activeWrist.position, minimapRoot.transform.position):F3}m");

                // Calculate what position SHOULD be
                Vector3 shouldBe;
                if (testMode)
                {
                    shouldBe = activeWrist.position + activeWrist.forward * testDistance;
                    Debug.Log($"Should Be (TEST): {shouldBe}");
                }
                else if (useAlternativePositioning)
                {
                    shouldBe = activeWrist.position - activeWrist.up * wristDistanceDown;
                    Debug.Log($"Should Be (ALT): {shouldBe}");
                }
                else
                {
                    Vector3 offset = activeWrist == leftHand ? leftWristOffset : rightWristOffset;
                    shouldBe = activeWrist.position + activeWrist.TransformDirection(offset);
                    Debug.Log($"Should Be (STANDARD): {shouldBe}");
                }
                Debug.Log($"Position Error: {Vector3.Distance(shouldBe, minimapRoot.transform.position):F3}m");
            }
            Debug.Log("========================================");
        }

        [ContextMenu("Reinitialize")]
        void Reinitialize()
        {
            Debug.Log("[WristMinimap] Manual reinitialization requested...");
            StopAllCoroutines();

            // Clean up existing
            if (minimapRoot != null)
                Destroy(minimapRoot);

            leftHand = null;
            rightHand = null;
            headCamera = null;
            activeWrist = null;
            isVisible = false;

            StartCoroutine(DelayedInitialization());
        }

        [ContextMenu("TEST: Move Minimap 1m Forward")]
        void TestMoveExtreme()
        {
            if (minimapRoot == null)
            {
                Debug.LogError("[WristMinimap] Cannot test - minimapRoot is NULL!");
                return;
            }

            if (activeWrist == null)
            {
                Debug.LogError("[WristMinimap] Cannot test - activeWrist is NULL!");
                return;
            }

            Vector3 extremePos = activeWrist.position + activeWrist.forward * 1.0f;
            Debug.LogWarning($"[WristMinimap] TEST: Moving minimap to extreme position 1m forward");
            Debug.LogWarning($"  From: {minimapRoot.transform.position}");
            Debug.LogWarning($"  To: {extremePos}");

            minimapRoot.transform.position = extremePos;

            Debug.LogWarning($"  Actual: {minimapRoot.transform.position}");
            Debug.LogWarning($"  Success: {Vector3.Distance(minimapRoot.transform.position, extremePos) < 0.01f}");
        }

        [ContextMenu("TEST: Enable Aggressive Logging")]
        void EnableAggressiveLogging()
        {
            aggressiveLogging = true;
            Debug.LogWarning("[WristMinimap] Aggressive logging ENABLED - will log every frame!");
        }

        [ContextMenu("TEST: Disable Aggressive Logging")]
        void DisableAggressiveLogging()
        {
            aggressiveLogging = false;
            Debug.Log("[WristMinimap] Aggressive logging disabled");
        }

        [ContextMenu("RESET: Fix Offset Values")]
        void ResetOffsetValues()
        {
            // Force reset to wrist position (negative Z goes backward from fingertip toward wrist)
            leftWristOffset = new Vector3(0f, 0f, -0.15f);
            rightWristOffset = new Vector3(0f, 0f, -0.15f);

            Debug.LogWarning("[WristMinimap] RESET OFFSETS:");
            Debug.LogWarning($"  Left offset: {leftWristOffset}");
            Debug.LogWarning($"  Right offset: {rightWristOffset}");
            Debug.LogWarning("  These values are now saved. Minimap should move to wrist position.");

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif

            // Force immediate update if minimap is visible
            if (activeWrist != null)
            {
                UpdateMinimapPosition(activeWrist);
            }
        }

        [ContextMenu("DEBUG: Show Current Settings")]
        void ShowCurrentSettings()
        {
            Debug.Log("========================================");
            Debug.Log("=== CURRENT WRIST MINIMAP SETTINGS ===");
            Debug.Log("========================================");
            Debug.LogWarning($"Left Wrist Offset: {leftWristOffset}");
            Debug.LogWarning($"Right Wrist Offset: {rightWristOffset}");
            Debug.Log($"Test Mode: {testMode}");
            Debug.Log($"Alternative Positioning: {useAlternativePositioning}");
            Debug.Log($"Always Visible: {alwaysVisible}");
            Debug.Log($"Aggressive Logging: {aggressiveLogging}");
            Debug.Log("========================================");
            Debug.Log("Expected values for WRIST position:");
            Debug.Log("  leftWristOffset = (0, 0, -0.15)");
            Debug.Log("  rightWristOffset = (0, 0, -0.15)");
            Debug.Log("========================================");
        }
    }
}
