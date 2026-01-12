using UnityEngine;
using VRDungeonCrawler.Core;
using VRDungeonCrawler.UI;

namespace VRDungeonCrawler.Entities
{
    /// <summary>
    /// Portal entity that shows game mode menu when player enters
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class Portal : MonoBehaviour
    {
        [Header("Portal Settings")]
        [Tooltip("Radius for trigger detection")]
        public float triggerRadius = 2.5f; // Increased from 1.5f

        [Tooltip("Game mode menu prefab")]
        public GameObject gameModeMenuPrefab;

        [Header("Debug")]
        [Tooltip("Show debug logs for collision detection")]
        public bool showDebugLogs = true;

        [Tooltip("Visualize trigger zone")]
        public bool showTriggerVisualization = true;

        [Header("Visual Effects")]
        [Tooltip("Particle system for portal effect")]
        public ParticleSystem portalParticles;

        [Tooltip("Rotating rings")]
        public Transform outerRing;
        public Transform middleRing;
        public Transform innerRing;

        [Header("Rotation Speeds")]
        public float outerRingSpeed = 0.5f;
        public float middleRingSpeed = -0.8f;
        public float innerRingSpeed = 1.2f;

        private SphereCollider triggerCollider;
        private bool hasTriggered = false;
        private GameObject currentMenuInstance;
        private bool playerInTrigger = false;
        private float triggerHoldTime = 0f;
        private const float TRIGGER_HOLD_DURATION = 1.0f; // Hold for 1 second to trigger

        private void Awake()
        {
            // Setup trigger collider
            triggerCollider = GetComponent<SphereCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.radius = triggerRadius;
        }

        private void Update()
        {
            // Rotate rings
            if (outerRing != null)
                outerRing.Rotate(Vector3.forward, outerRingSpeed * Time.deltaTime * 30f);

            if (middleRing != null)
                middleRing.Rotate(Vector3.forward, middleRingSpeed * Time.deltaTime * 30f);

            if (innerRing != null)
                innerRing.Rotate(Vector3.forward, innerRingSpeed * Time.deltaTime * 30f);

            // Handle trigger hold time
            if (playerInTrigger && !hasTriggered)
            {
                triggerHoldTime += Time.deltaTime;

                if (triggerHoldTime >= TRIGGER_HOLD_DURATION)
                {
                    if (showDebugLogs)
                        Debug.Log($"[Portal] Trigger hold time reached: {triggerHoldTime:F1}s");
                    TriggerPortal();
                }
                else if (showDebugLogs && Time.frameCount % 30 == 0) // Log every 30 frames
                {
                    Debug.Log($"[Portal] Player in trigger: {triggerHoldTime:F1}s / {TRIGGER_HOLD_DURATION:F1}s");
                }
            }
        }

        private void OnDrawGizmos()
        {
            // Always show trigger zone in editor
            if (showTriggerVisualization)
            {
                Gizmos.color = playerInTrigger ? Color.green : Color.cyan;
                Gizmos.DrawWireSphere(transform.position, triggerRadius);

                // Show hold progress
                if (playerInTrigger && !hasTriggered)
                {
                    float progress = triggerHoldTime / TRIGGER_HOLD_DURATION;
                    Gizmos.color = Color.Lerp(Color.yellow, Color.green, progress);
                    Gizmos.DrawWireSphere(transform.position, triggerRadius * 0.8f);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered) return;

            bool isPlayer = IsPlayerCollider(other);

            if (isPlayer)
            {
                playerInTrigger = true;
                triggerHoldTime = 0f;

                if (showDebugLogs)
                    Debug.Log($"[Portal] Player entered trigger zone! Collider: {other.name}, Tag: {other.tag}, Root: {other.transform.root.name}");
            }
            else if (showDebugLogs)
            {
                Debug.Log($"[Portal] Non-player collider entered: {other.name}, Tag: {other.tag}, Root: {other.transform.root.name}");
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (hasTriggered) return;

            bool isPlayer = IsPlayerCollider(other);

            if (isPlayer && !playerInTrigger)
            {
                // In case OnTriggerEnter was missed
                playerInTrigger = true;
                triggerHoldTime = 0f;

                if (showDebugLogs)
                    Debug.Log($"[Portal] Player detected in trigger (via Stay)");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            bool isPlayer = IsPlayerCollider(other);

            if (isPlayer)
            {
                playerInTrigger = false;
                triggerHoldTime = 0f;

                if (showDebugLogs)
                    Debug.Log("[Portal] Player exited trigger zone");
            }
        }

        /// <summary>
        /// Checks if the collider belongs to the player
        /// </summary>
        private bool IsPlayerCollider(Collider other)
        {
            // Check tags
            if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
                return true;

            // Check names
            if (other.name.Contains("XR Origin") || other.name.Contains("CharacterController"))
                return true;

            // Check root transform
            if (other.transform.root.name.Contains("XR Origin"))
                return true;

            // Check for CharacterController component
            if (other.GetComponent<CharacterController>() != null)
                return true;

            return false;
        }

        private void TriggerPortal()
        {
            hasTriggered = true;

            // Flash effect
            if (portalParticles != null)
            {
                var emission = portalParticles.emission;
                emission.rateOverTime = 100f; // Burst of particles
            }

            // Show game mode menu
            ShowGameModeMenu();
        }

        private void ShowGameModeMenu()
        {
            if (gameModeMenuPrefab == null)
            {
                Debug.LogError("[Portal] Game mode menu prefab not assigned! Loading dungeon directly...");
                if (GameManager.Instance != null)
                    GameManager.Instance.EnterDungeon();
                return;
            }

            // Instantiate menu
            currentMenuInstance = Instantiate(gameModeMenuPrefab);

            // Get menu component and show it
            GameModeMenu menu = currentMenuInstance.GetComponent<GameModeMenu>();
            if (menu != null)
            {
                menu.ShowMenu();
                Debug.Log("[Portal] Game mode menu opened");
            }
            else
            {
                Debug.LogError("[Portal] GameModeMenu component not found on prefab!");
                Destroy(currentMenuInstance);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize trigger radius in editor
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }
    }
}
