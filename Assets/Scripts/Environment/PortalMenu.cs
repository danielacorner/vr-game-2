using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        public float activationDistance = 6f;

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
        public float menuOffsetDistance = 3.0f;

        [Tooltip("Height above ground for menu")]
        public float menuHeight = 2f;

        [Header("Debug")]
        [Tooltip("Show debug logs")]
        public bool showDebug = false;

        private bool isMenuActive = false;
        private Vector3 lastPlayerDirection = Vector3.forward;

        void Start()
        {
            // Find player if not set
            if (player == null)
            {
                // Try multiple possible names for XR Origin
                GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
                if (xrOrigin == null)
                {
                    xrOrigin = GameObject.Find("XR Origin");
                }

                if (xrOrigin != null)
                {
                    player = xrOrigin.transform;
                    if (showDebug)
                        Debug.Log($"[PortalMenu] Found player: {xrOrigin.name}");
                }
                else
                {
                    Debug.LogError("[PortalMenu] Could not find player (XR Origin)! Portal menu will not work.");
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
            if (player == null || menuCanvas == null) return;

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

            // Update menu position and rotation when active
            if (isMenuActive)
            {
                PositionMenuRelativeToPlayer();
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
                if (showDebug)
                    Debug.Log($"[PortalMenu] Loading scene: {dungeonSceneName}");

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

            // Calculate direction from portal to player (on XZ plane only)
            Vector3 portalToPlayer = player.position - transform.position;
            portalToPlayer.y = 0f;

            // Only update position if player has moved significantly
            if (portalToPlayer.sqrMagnitude < 0.01f) return;

            portalToPlayer.Normalize();

            // Position menu on the side of portal facing the player
            // Menu is offset from portal center by menuOffsetDistance
            Vector3 menuPosition = transform.position + (portalToPlayer * menuOffsetDistance);
            menuPosition.y = transform.position.y + menuHeight;

            menuCanvas.transform.position = menuPosition;

            // Rotate menu to face the player
            Vector3 directionToPlayer = player.position - menuCanvas.transform.position;
            directionToPlayer.y = 0f;

            if (directionToPlayer.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                menuCanvas.transform.rotation = targetRotation;
            }

            if (showDebug && Time.frameCount % 120 == 0) // Log every 120 frames
            {
                Debug.Log($"[PortalMenu] Menu positioned at {menuPosition}, facing player from direction {portalToPlayer}");
            }
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
