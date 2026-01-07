using UnityEngine;

namespace VRDungeonCrawler.Core
{
    /// <summary>
    /// Builds the outdoor home area scene
    /// Creates 50x50m natural environment with terrain, moon, campfire, and portal area
    /// Note: Terrain and trees are best created manually in Unity Editor for performance
    /// This script provides helper methods for programmatic setup where applicable
    /// </summary>
    public class OutdoorHomeBuilder : MonoBehaviour
    {
        [Header("Environment Settings")]
        [Tooltip("Size of the outdoor area (50m recommended)")]
        public float areaSize = 50f;

        [Header("References")]
        [Tooltip("Terrain object (create manually in Editor)")]
        public Terrain terrain;

        [Tooltip("Moon directional light")]
        public Light moonLight;

        [Tooltip("Campfire GameObject")]
        public GameObject campfire;

        [Tooltip("Portal GameObject")]
        public GameObject portal;

        [Header("Portal Settings")]
        public Vector3 portalPosition = new Vector3(20f, 0f, 20f);

        [Header("Player Spawn")]
        public Transform playerSpawnPoint;

        private void Start()
        {
            // Setup is mostly done in Editor, but can validate here
            ValidateSetup();
        }

        [ContextMenu("Validate Outdoor Setup")]
        public void ValidateSetup()
        {
            Debug.Log("[OutdoorHomeBuilder] Validating outdoor home area setup...");

            int warnings = 0;

            if (terrain == null)
            {
                Debug.LogWarning("[OutdoorHomeBuilder] Terrain not assigned! Create Terrain in Editor: GameObject > 3D Object > Terrain");
                warnings++;
            }

            if (moonLight == null)
            {
                Debug.LogWarning("[OutdoorHomeBuilder] Moon light not assigned! Create directional light with MoonController component");
                warnings++;
            }

            if (campfire == null)
            {
                Debug.LogWarning("[OutdoorHomeBuilder] Campfire not assigned! Create campfire with CampfireController component");
                warnings++;
            }

            if (portal == null)
            {
                Debug.LogWarning("[OutdoorHomeBuilder] Portal not assigned! Assign existing portal GameObject");
                warnings++;
            }

            if (warnings == 0)
            {
                Debug.Log("[OutdoorHomeBuilder] ✓ All components properly assigned!");
            }
            else
            {
                Debug.LogWarning($"[OutdoorHomeBuilder] Found {warnings} missing components. See warnings above.");
            }
        }

        [ContextMenu("Position Portal at Designated Area")]
        public void PositionPortal()
        {
            if (portal != null)
            {
                portal.transform.position = portalPosition;
                Debug.Log($"[OutdoorHomeBuilder] Portal positioned at {portalPosition}");
            }
            else
            {
                Debug.LogError("[OutdoorHomeBuilder] Portal reference not set!");
            }
        }

        private void OnDrawGizmos()
        {
            // Visualize outdoor area bounds
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireCube(transform.position, new Vector3(areaSize, 5f, areaSize));

            // Visualize portal position
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(portalPosition, 1.5f);
            Gizmos.DrawLine(transform.position, portalPosition);

            // Visualize player spawn
            if (playerSpawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(playerSpawnPoint.position, 0.5f);
                Gizmos.DrawLine(playerSpawnPoint.position, playerSpawnPoint.position + playerSpawnPoint.forward * 2f);
            }

            // Visualize campfire position (center)
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawWireSphere(Vector3.zero, 1f);
        }
    }
}
