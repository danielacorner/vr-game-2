using UnityEngine;
using Unity.XR.CoreUtils;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Detects when player falls off the edge in dungeons
    /// Respawns them at their last safe position and applies damage
    /// </summary>
    public class PlayerFallDetector : MonoBehaviour
    {
        [Header("Fall Detection")]
        [Tooltip("Y position below which player is considered to have fallen")]
        public float fallThreshold = -10f;

        [Tooltip("Damage applied when falling (0.5 = half a heart)")]
        public float fallDamage = 0.5f;

        [Tooltip("Height above ground to consider position 'safe'")]
        public float groundCheckDistance = 2f;

        [Tooltip("How often to update safe position (seconds)")]
        public float safePositionUpdateInterval = 0.5f;

        [Tooltip("Only enable fall detection in dungeon scenes")]
        public bool onlyInDungeon = true;

        [Header("Debug")]
        public bool showDebug = false;

        private XROrigin xrOrigin;
        private Vector3 lastSafePosition;
        private float nextSafePositionUpdate;
        private bool hasFallen = false;
        private bool isInDungeon = false;

        void Start()
        {
            // Find XR Origin
            xrOrigin = FindFirstObjectByType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("[PlayerFallDetector] XR Origin not found!");
                enabled = false;
                return;
            }

            // Set initial safe position
            lastSafePosition = xrOrigin.transform.position;
            nextSafePositionUpdate = Time.time;

            // Check if we're in a dungeon
            CheckIfInDungeon();

            if (showDebug)
                Debug.Log($"[PlayerFallDetector] Initialized. Fall threshold: {fallThreshold}, Safe position: {lastSafePosition}");
        }

        void Update()
        {
            if (xrOrigin == null)
                return;

            // Only check if in dungeon (if that setting is enabled)
            if (onlyInDungeon && !isInDungeon)
                return;

            Vector3 playerPosition = xrOrigin.transform.position;

            // Check if player has fallen below threshold
            if (playerPosition.y < fallThreshold && !hasFallen)
            {
                OnPlayerFell();
                return;
            }

            // Update safe position periodically if on ground
            if (Time.time >= nextSafePositionUpdate)
            {
                if (IsOnGround(playerPosition))
                {
                    lastSafePosition = playerPosition;
                    nextSafePositionUpdate = Time.time + safePositionUpdateInterval;

                    if (showDebug && Time.frameCount % 120 == 0)
                        Debug.Log($"[PlayerFallDetector] Updated safe position: {lastSafePosition}");
                }
            }
        }

        bool IsOnGround(Vector3 position)
        {
            // Cast a ray downward to check if player is on ground
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 0.5f, Vector3.down, out hit, groundCheckDistance))
            {
                return true;
            }
            return false;
        }

        void OnPlayerFell()
        {
            hasFallen = true;

            if (showDebug)
                Debug.Log($"[PlayerFallDetector] Player fell! Respawning at {lastSafePosition}");

            // Apply damage
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.TakeDamage(fallDamage, xrOrigin.transform.position);
                Debug.Log($"[PlayerFallDetector] Applied {fallDamage} fall damage");
            }

            // Respawn at last safe position
            RespawnPlayer();

            // Reset fall flag after a short delay
            Invoke(nameof(ResetFallFlag), 1f);
        }

        void RespawnPlayer()
        {
            if (xrOrigin == null)
                return;

            // Teleport player to last safe position
            xrOrigin.transform.position = lastSafePosition;

            if (showDebug)
                Debug.Log($"[PlayerFallDetector] Respawned player at {lastSafePosition}");
        }

        void ResetFallFlag()
        {
            hasFallen = false;
        }

        void CheckIfInDungeon()
        {
            // Check if we're in a dungeon by looking for dungeon generator
            Dungeon.DungeonGenerator dungeonGen = FindFirstObjectByType<Dungeon.DungeonGenerator>();
            isInDungeon = (dungeonGen != null);

            if (showDebug)
                Debug.Log($"[PlayerFallDetector] In dungeon: {isInDungeon}");
        }

        void OnEnable()
        {
            // Re-check dungeon status when enabled
            CheckIfInDungeon();
        }

        // Subscribe to scene changes to detect when entering/leaving dungeon
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            CheckIfInDungeon();
        }

        void Awake()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // Debug visualization
        void OnDrawGizmos()
        {
            if (!showDebug || xrOrigin == null)
                return;

            // Draw last safe position
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(lastSafePosition, 0.5f);
            Gizmos.DrawLine(lastSafePosition, lastSafePosition + Vector3.up * 2f);

            // Draw fall threshold plane
            Gizmos.color = Color.red;
            Vector3 playerPos = xrOrigin.transform.position;
            Vector3 thresholdPos = new Vector3(playerPos.x, fallThreshold, playerPos.z);
            Gizmos.DrawWireCube(thresholdPos, new Vector3(10f, 0.1f, 10f));
        }
    }
}
