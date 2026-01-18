using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Prevents player from entering the portal's boundary sphere.
    /// Pushes player back if they get too close to portal center.
    /// </summary>
    public class PortalBoundary : MonoBehaviour
    {
        [SerializeField] private float boundaryRadius = 2.4f;
        [SerializeField] private float pushBackDistance = 2.6f;
        [SerializeField] private bool showDebug = true;

        private Transform player;
        private CharacterController playerController;

        void Start()
        {
            // Find player
            GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
            if (xrOrigin != null)
            {
                player = xrOrigin.transform;
                playerController = xrOrigin.GetComponent<CharacterController>();

                if (showDebug)
                    Debug.Log($"[PortalBoundary] Found player, enforcing {boundaryRadius}m boundary");
            }
            else
            {
                Debug.LogError("[PortalBoundary] Could not find XR Origin!");
                enabled = false;
            }
        }

        void Update()
        {
            if (player == null) return;

            // Check distance from portal center (2D distance, ignore Y)
            Vector3 portalPos = transform.position;
            Vector3 playerPos = player.position;

            Vector2 portalXZ = new Vector2(portalPos.x, portalPos.z);
            Vector2 playerXZ = new Vector2(playerPos.x, playerPos.z);

            float distance = Vector2.Distance(portalXZ, playerXZ);

            // If player is inside boundary, push them out
            if (distance < boundaryRadius)
            {
                // Calculate push direction (away from portal)
                Vector2 pushDirection = (playerXZ - portalXZ).normalized;
                Vector2 targetXZ = portalXZ + pushDirection * pushBackDistance;

                // Teleport player to safe position
                Vector3 safePosition = new Vector3(targetXZ.x, playerPos.y, targetXZ.y);

                if (playerController != null)
                {
                    // Disable controller briefly to allow teleport
                    playerController.enabled = false;
                    player.position = safePosition;
                    playerController.enabled = true;
                }
                else
                {
                    player.position = safePosition;
                }

                if (showDebug)
                    Debug.Log($"[PortalBoundary] Pushed player back from {distance:F2}m to {pushBackDistance}m");
            }
        }
    }
}
