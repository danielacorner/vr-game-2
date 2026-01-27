using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

        [Tooltip("Position offset from left hand (local space)")]
        public Vector3 leftWristOffset = new Vector3(0f, 0.0f, -0.48f);

        [Tooltip("Position offset from right hand (local space)")]
        public Vector3 rightWristOffset = new Vector3(0f, 0.0f, -0.12f);

        [Tooltip("Rotation offset for minimap (to align with wrist)")]
        public Vector3 minimapRotationOffset = new Vector3(0f, 0f, 0f);

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

        // Internal state
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

        void Start()
        {
            Debug.Log("[WristMinimap] Start() called - beginning delayed initialization...");
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
            Debug.Log($"  Left Offset: {leftWristOffset}");
            Debug.Log($"  Right Offset: {rightWristOffset}");

            if (alwaysVisible)
            {
                ShowMinimap(leftHand ?? rightHand);
            }
        }

        void FindHandControllers()
        {
            // Method 1: Search by common names
            Transform[] allTransforms = FindObjectsOfType<Transform>();
            foreach (Transform t in allTransforms)
            {
                string name = t.name.ToLower();
                if (name.Contains("lefthand") || (name.Contains("left") && name.Contains("controller")))
                {
                    if (leftHand == null)
                    {
                        leftHand = t;
                        if (showDebug) Debug.Log($"[WristMinimap] Found left hand: {t.name}");
                    }
                }
                else if (name.Contains("righthand") || (name.Contains("right") && name.Contains("controller")))
                {
                    if (rightHand == null)
                    {
                        rightHand = t;
                        if (showDebug) Debug.Log($"[WristMinimap] Found right hand: {t.name}");
                    }
                }

                if (leftHand != null && rightHand != null)
                    break;
            }

            // Method 2: Try to find under XR Origin
            if (leftHand == null || rightHand == null)
            {
                Unity.XR.CoreUtils.XROrigin xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
                if (xrOrigin != null)
                {
                    if (leftHand == null)
                        leftHand = FindInChildren(xrOrigin.transform, "left");
                    if (rightHand == null)
                        rightHand = FindInChildren(xrOrigin.transform, "right");
                }
            }
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
            playerDot = new GameObject("PlayerDot");
            playerDot.transform.SetParent(minimapContent.transform, false);

            Image dotImage = playerDot.AddComponent<Image>();
            dotImage.color = playerDotColor;

            RectTransform dotRT = playerDot.GetComponent<RectTransform>();
            dotRT.sizeDelta = new Vector2(15, 15);
            dotRT.anchoredPosition = Vector2.zero;

            // Make it round
            GameObject circle = new GameObject("Circle");
            circle.transform.SetParent(playerDot.transform, false);
            Image circleImage = circle.AddComponent<Image>();
            circleImage.color = playerDotColor;
            RectTransform circleRT = circle.GetComponent<RectTransform>();
            circleRT.anchorMin = Vector2.zero;
            circleRT.anchorMax = Vector2.one;
            circleRT.sizeDelta = Vector2.zero;
        }

        void LateUpdate()
        {
            if (headCamera == null) return;

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
                    ShowMinimap(targetWrist);
                }
                // Update position every frame in LateUpdate for instant following
                UpdateMinimapPosition(targetWrist);
            }
            else if (isVisible && !alwaysVisible)
            {
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
            if (minimapRoot == null || wrist == null)
            {
                if (showDebug)
                    Debug.LogWarning($"[WristMinimap] UpdateMinimapPosition failed: root={minimapRoot != null}, wrist={wrist != null}");
                return;
            }

            // Position relative to wrist (in wrist's local space)
            Vector3 offset = wrist == leftHand ? leftWristOffset : rightWristOffset;
            Vector3 worldOffset = wrist.TransformDirection(offset);
            Vector3 targetPosition = wrist.position + worldOffset;

            minimapRoot.transform.position = targetPosition;

            if (showDebug && Time.frameCount % 60 == 0) // Log every 60 frames
            {
                Debug.Log($"[WristMinimap] Position Update:");
                Debug.Log($"  Wrist: {wrist.name}");
                Debug.Log($"  Offset (local): {offset}");
                Debug.Log($"  Offset (world): {worldOffset}");
                Debug.Log($"  Wrist pos: {wrist.position}");
                Debug.Log($"  Minimap pos: {targetPosition}");
            }

            // Rotate with wrist like a real watch (not facing camera)
            // The minimap should be parallel to the back of the hand
            // Wrist forward = along the arm, wrist up = back of hand
            Quaternion wristRotation = wrist.rotation;

            // Apply 90 degree rotation to make it lie flat on wrist (like a watch face)
            // The canvas needs to face up (along the wrist's "up" direction)
            Quaternion watchOrientation = wristRotation * Quaternion.Euler(90f, 0f, 0f);

            // Apply any custom rotation offset
            minimapRoot.transform.rotation = watchOrientation * Quaternion.Euler(minimapRotationOffset);
        }

        void UpdatePlayerPosition()
        {
            if (playerDot == null) return;

            // Keep player dot centered (the minimap camera follows the player)
            RectTransform dotRT = playerDot.GetComponent<RectTransform>();
            dotRT.anchoredPosition = Vector2.zero;

            // Rotate based on player facing direction
            float yaw = transform.eulerAngles.y;
            dotRT.localRotation = Quaternion.Euler(0, 0, -yaw);
        }

        void OnDrawGizmos()
        {
            if (!showDebug || !Application.isPlaying) return;

            if (leftHand != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(leftHand.position, 0.05f);

                // Draw where the minimap SHOULD be on left wrist
                Vector3 targetPos = leftHand.position + leftHand.TransformDirection(leftWristOffset);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(targetPos, 0.03f);
                Gizmos.DrawLine(leftHand.position, targetPos);
            }

            if (rightHand != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(rightHand.position, 0.05f);

                // Draw where the minimap SHOULD be on right wrist
                Vector3 targetPos = rightHand.position + rightHand.TransformDirection(rightWristOffset);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(targetPos, 0.03f);
                Gizmos.DrawLine(rightHand.position, targetPos);
            }

            // Draw where the minimap ACTUALLY is
            if (minimapRoot != null && isVisible)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(minimapRoot.transform.position, Vector3.one * 0.02f);
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
            Debug.Log("=== WristMinimap Debug Info ===");
            Debug.Log($"Left Hand: {leftHand?.name ?? "NULL"}");
            Debug.Log($"Right Hand: {rightHand?.name ?? "NULL"}");
            Debug.Log($"Active Wrist: {activeWrist?.name ?? "NULL"}");
            Debug.Log($"Head Camera: {headCamera?.name ?? "NULL"}");
            Debug.Log($"Minimap Root: {minimapRoot?.name ?? "NULL"}");
            Debug.Log($"Is Visible: {isVisible}");
            Debug.Log($"Always Visible: {alwaysVisible}");
            Debug.Log($"Left Offset: {leftWristOffset}");
            Debug.Log($"Right Offset: {rightWristOffset}");

            if (activeWrist != null && minimapRoot != null)
            {
                Debug.Log($"Wrist Position: {activeWrist.position}");
                Debug.Log($"Minimap Position: {minimapRoot.transform.position}");
                Debug.Log($"Distance: {Vector3.Distance(activeWrist.position, minimapRoot.transform.position)}");
            }
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
    }
}
