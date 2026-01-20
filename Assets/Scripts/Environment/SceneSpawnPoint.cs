using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Marks a spawn point for the player when entering a scene
    /// Place this in each scene where you want to spawn the player
    /// </summary>
    public class SceneSpawnPoint : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [Tooltip("Height offset to ensure player is above ground")]
        public float heightOffset = 0f;

        [Header("Debug")]
        [Tooltip("Show spawn point in scene view")]
        public bool showGizmo = true;

        void Start()
        {
            // This component is now PASSIVE - it only serves as a marker for PortalMenu to find
            // The actual positioning is handled by PortalMenu's LoadSceneAndSpawn() coroutine
            // This prevents timing conflicts and CharacterController issues

            Debug.Log($"[SceneSpawnPoint] Spawn marker ready at {transform.position}, heightOffset: {heightOffset}m");
        }

        /// <summary>
        /// Gets the spawn position including height offset
        /// </summary>
        public Vector3 GetSpawnPosition()
        {
            return transform.position + Vector3.up * heightOffset;
        }

        void OnDrawGizmos()
        {
            if (!showGizmo) return;

            // Draw spawn point indicator
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            // Draw up arrow
            Gizmos.color = Color.cyan;
            Vector3 arrowStart = transform.position;
            Vector3 arrowEnd = transform.position + Vector3.up * 2f;
            Gizmos.DrawLine(arrowStart, arrowEnd);

            // Draw forward direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * 1.5f);
        }
    }
}
