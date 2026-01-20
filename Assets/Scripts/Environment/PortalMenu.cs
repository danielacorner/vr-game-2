using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using VRDungeonCrawler.Player;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Portal menu that appears when player is nearby
    /// Shows a "Travel to Dungeon" button and handles scene transitions
    /// Menu is positioned relative to portal (fixed rotation)
    /// </summary>
    public class PortalMenu : MonoBehaviour
    {
        [Header("Detection")]
        [Tooltip("Distance at which menu appears")]
        public float activationDistance = 20f;

        [Tooltip("Player transform (XR Origin)")]
        public Transform player;

        [Header("UI")]
        [Tooltip("Canvas to show/hide")]
        public Canvas menuCanvas;

        [Tooltip("Button that travels to dungeon")]
        private Button _travelButton;
        public Button travelButton
        {
            get { return _travelButton; }
            set
            {
                _travelButton = value;
                // Set up listener when button is assigned
                if (_travelButton != null && Application.isPlaying)
                {
                    SetupButtonListener();
                }
            }
        }

        [Header("Scene")]
        [Tooltip("Name of dungeon scene to load")]
        public string dungeonSceneName = "Dungeon";

        [Header("Positioning")]
        [Tooltip("Distance from portal center where menu appears (at portal edge)")]
        public float menuOffsetDistance = 2.4f;

        [Tooltip("Height offset from portal center for menu")]
        public float menuHeight = 0f;

        [Header("Debug")]
        [Tooltip("Show debug logs")]
        public bool showDebug = false;

        private bool isMenuActive = false;
        private Vector3 lastPlayerDirection = Vector3.forward;

        void Start()
        {
            Debug.Log("[PortalMenu] VERSION: Build 2026-01-18-v14 - Reduced text sizes + better spacing");

            // Find player if not set - use Main Camera for head position
            if (player == null)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    player = mainCam.transform;
                    if (showDebug)
                        Debug.Log($"[PortalMenu] Found player camera: {mainCam.name} at position {mainCam.transform.position}");
                }
                else
                {
                    Debug.LogError("[PortalMenu] Could not find Main Camera! Portal menu will not work.");
                    enabled = false;
                    return;
                }
            }

            // TEMPORARY: Show menu always for testing
            // Hide menu initially
            // if (menuCanvas != null)
            // {
            //     menuCanvas.gameObject.SetActive(false);
            //     isMenuActive = false;
            // }

            // Set up button callback if already assigned
            SetupButtonListener();

            if (showDebug)
                Debug.Log($"[PortalMenu] Initialized at {transform.position}, activation distance: {activationDistance}");
        }

        void SetupButtonListener()
        {
            if (_travelButton != null)
            {
                // Remove existing listeners to avoid duplicates
                _travelButton.onClick.RemoveAllListeners();
                _travelButton.onClick.AddListener(OnTravelButtonClicked);

                if (showDebug)
                    Debug.Log("[PortalMenu] Travel button listener added");
            }
        }

        void Update()
        {
            // SAFETY: Return early if canvas not created yet
            if (player == null || menuCanvas == null)
            {
                if (showDebug && Time.frameCount % 300 == 0)
                    Debug.Log("[PortalMenu] Waiting for menuCanvas or player to be assigned");
                return;
            }

            // Always update menu position to track player
            PositionMenuRelativeToPlayer();

            // Check distance to player
            float distance = Vector3.Distance(transform.position, player.position);
            bool shouldBeActive = distance <= activationDistance;

            if (showDebug && Time.frameCount % 60 == 0) // Log every 60 frames
            {
                Debug.Log($"[PortalMenu] Distance to player: {distance:F2}m, shouldBeActive: {shouldBeActive}, isMenuActive: {isMenuActive}");
            }

            // Show/hide menu based on distance
            if (shouldBeActive && !isMenuActive)
            {
                ShowMenu();
            }
            else if (!shouldBeActive && isMenuActive)
            {
                HideMenu();
            }
        }

        void ShowMenu()
        {
            if (menuCanvas != null)
            {
                menuCanvas.gameObject.SetActive(true);
                isMenuActive = true;

                if (showDebug)
                    Debug.Log($"[PortalMenu] Menu shown at position {menuCanvas.transform.position}, scale {menuCanvas.transform.localScale}, active: {menuCanvas.gameObject.activeSelf}");
            }
            else
            {
                Debug.LogError("[PortalMenu] Cannot show menu - menuCanvas is null!");
            }
        }

        void HideMenu()
        {
            if (menuCanvas != null)
            {
                menuCanvas.gameObject.SetActive(false);
                isMenuActive = false;

                if (showDebug)
                    Debug.Log("[PortalMenu] Menu hidden");
            }
        }

        void OnTravelButtonClicked()
        {
            if (showDebug)
                Debug.Log($"[PortalMenu] Travel button clicked! Loading scene: {dungeonSceneName}");

            // Load dungeon scene
            LoadDungeonScene();
        }

        void LoadDungeonScene()
        {
            // Check if scene exists in build settings
            if (Application.CanStreamedLevelBeLoaded(dungeonSceneName))
            {
                Debug.Log($"[PortalMenu] Loading scene: {dungeonSceneName}");

                // Tell PersistentPlayer to prepare for scene load
                GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
                if (xrOrigin != null)
                {
                    PersistentPlayer persistentPlayer = xrOrigin.GetComponent<PersistentPlayer>();
                    if (persistentPlayer != null)
                    {
                        persistentPlayer.PrepareForSceneLoad();
                    }
                    else
                    {
                        Debug.LogError("[PortalMenu] PersistentPlayer component not found on XR Origin!");
                    }
                }
                else
                {
                    Debug.LogError("[PortalMenu] XR Origin not found!");
                }

                // Load the scene - PersistentPlayer will handle positioning
                SceneManager.LoadScene(dungeonSceneName);
            }
            else
            {
                Debug.LogError($"[PortalMenu] Scene '{dungeonSceneName}' not found in build settings! Add it to File > Build Settings > Scenes in Build");
            }
        }

        void PositionMenuRelativeToPlayer()
        {
            if (menuCanvas == null || player == null) return;

            // Calculate direction from portal to player (XZ plane only for positioning)
            Vector3 portalToPlayer = player.position - transform.position;
            portalToPlayer.y = 0f; // Use only horizontal direction

            // Only update position if player has moved significantly
            if (portalToPlayer.sqrMagnitude < 0.01f) return;

            portalToPlayer.Normalize();

            // Position menu on the edge of portal sphere (XZ) but at player head height (Y)
            Vector3 menuPosition = transform.position + (portalToPlayer * menuOffsetDistance);
            menuPosition.y = player.position.y; // Always at player head height

            menuCanvas.transform.position = menuPosition;

            // Rotate menu to face the player (horizontal only - keep vertical like a signpost)
            // Canvas front faces -Z in its local space, so we need to look AWAY from player
            Vector3 directionFromPlayer = menuCanvas.transform.position - player.position;
            directionFromPlayer.y = 0f; // Zero out Y to keep menu vertical

            if (directionFromPlayer.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionFromPlayer);
                menuCanvas.transform.rotation = targetRotation;
            }

            if (showDebug && Time.frameCount % 120 == 0) // Log every 120 frames
            {
                Debug.Log($"[PortalMenu] Menu positioned at {menuPosition}, player at {player.position}");
            }
        }

        /// <summary>
        /// Returns the closest cardinal direction (N/S/E/W) to the given direction
        /// </summary>
        Vector3 GetClosestCardinalDirection(Vector3 direction)
        {
            // Normalize the input
            direction.Normalize();

            // Calculate dot products with each cardinal direction
            float dotNorth = Vector3.Dot(direction, Vector3.forward);   // +Z
            float dotSouth = Vector3.Dot(direction, Vector3.back);      // -Z
            float dotEast = Vector3.Dot(direction, Vector3.right);      // +X
            float dotWest = Vector3.Dot(direction, Vector3.left);       // -X

            // Find the maximum dot product (closest direction)
            float maxDot = Mathf.Max(dotNorth, dotSouth, dotEast, dotWest);

            if (maxDot == dotNorth) return Vector3.forward;
            if (maxDot == dotSouth) return Vector3.back;
            if (maxDot == dotEast) return Vector3.right;
            return Vector3.left;
        }

        void OnDrawGizmosSelected()
        {
            // Draw activation radius in editor
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, activationDistance);

            // Draw menu height
            Gizmos.color = Color.yellow;
            Vector3 menuHeightPos = transform.position + Vector3.up * menuHeight;
            Gizmos.DrawWireSphere(menuHeightPos, 0.2f);
        }
    }
}
