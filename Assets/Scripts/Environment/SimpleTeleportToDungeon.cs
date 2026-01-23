using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Simple teleport to dungeon area - no scene loading needed
    /// Just moves player to a different location in the same scene
    /// </summary>
    public class SimpleTeleportToDungeon : MonoBehaviour
    {
        [Header("Teleport Target")]
        [Tooltip("Where to teleport the player")]
        public Transform dungeonSpawnPoint;

        [Header("References")]
        [Tooltip("Usually finds automatically")]
        public Transform xrOrigin;

        void Start()
        {
            if (xrOrigin == null)
            {
                GameObject xrOriginObj = GameObject.Find("XR Origin (XR Rig)");
                if (xrOriginObj != null)
                {
                    xrOrigin = xrOriginObj.transform;
                }
            }

            if (dungeonSpawnPoint == null)
            {
                Debug.LogError("[SimpleTeleportToDungeon] No dungeon spawn point assigned!");
            }
        }

        /// <summary>
        /// Call this from button click or portal interaction
        /// </summary>
        public void TeleportToDungeon()
        {
            if (xrOrigin == null || dungeonSpawnPoint == null)
            {
                Debug.LogError("[SimpleTeleportToDungeon] Missing references!");
                return;
            }

            Debug.Log($"[SimpleTeleportToDungeon] Teleporting from {xrOrigin.position} to {dungeonSpawnPoint.position}");

            // Disable CharacterController during teleport
            CharacterController cc = xrOrigin.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
            }

            // Simple instant teleport
            xrOrigin.position = dungeonSpawnPoint.position;
            xrOrigin.rotation = dungeonSpawnPoint.rotation;

            // Re-enable CharacterController
            if (cc != null)
            {
                cc.enabled = true;
            }

            Debug.Log($"[SimpleTeleportToDungeon] ✓ Teleported to dungeon!");
        }
    }
}
