using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Makes the XR Origin (player) persist across scene loads
    /// Attach this to the XR Origin GameObject
    /// </summary>
    public class PersistentPlayer : MonoBehaviour
    {
        private static PersistentPlayer instance;
        private bool isLoadingScene = false;

        void Awake()
        {
            Debug.Log($"[PersistentPlayer] Awake called on {gameObject.name}, instance={(instance == null ? "null" : instance.gameObject.name)}");

            // Singleton pattern - only one player can exist
            if (instance != null && instance != this)
            {
                Debug.LogWarning($"[PersistentPlayer] ✗ Destroying duplicate XR Origin: {gameObject.name} (keeping {instance.gameObject.name})");
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log($"[PersistentPlayer] ✓ XR Origin will persist across scenes: {gameObject.name}");

            // Subscribe to scene loaded events
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (isLoadingScene)
            {
                Debug.Log($"[PersistentPlayer] Scene loaded: {scene.name}");
                StartCoroutine(PositionAtSpawnPoint(scene));
                isLoadingScene = false;
            }
        }

        public void PrepareForSceneLoad()
        {
            isLoadingScene = true;
            Debug.Log("[PersistentPlayer] Preparing for scene load");
        }

        IEnumerator PositionAtSpawnPoint(Scene scene)
        {
            // Wait one frame for scene objects to initialize
            yield return null;

            // Find spawn point GameObject
            GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
            if (spawnPoint == null)
                spawnPoint = GameObject.Find("SpawnPoint");

            CharacterController characterController = GetComponent<CharacterController>();

            // Disable CharacterController temporarily
            if (characterController != null)
            {
                characterController.enabled = false;
                Debug.Log("[PersistentPlayer] Disabled CharacterController for spawn");
            }

            Vector3 spawnPosition;
            Quaternion spawnRotation;

            if (spawnPoint != null)
            {
                spawnPosition = spawnPoint.transform.position;
                spawnRotation = spawnPoint.transform.rotation;
                Debug.Log($"[PersistentPlayer] Using spawn point at: {spawnPosition}");
            }
            else
            {
                // Default spawn position well above ground
                spawnPosition = new Vector3(0, 2f, 0);
                spawnRotation = Quaternion.identity;
                Debug.LogWarning($"[PersistentPlayer] No spawn point found in {scene.name}, using default: {spawnPosition}");
            }

            // Set position and rotation
            transform.position = spawnPosition;
            transform.rotation = spawnRotation;

            Debug.Log($"[PersistentPlayer] Positioned at: {spawnPosition}");

            // Wait two frames for physics to settle
            yield return null;
            yield return null;

            // Re-enable CharacterController
            if (characterController != null)
            {
                characterController.enabled = true;
                Debug.Log("[PersistentPlayer] Re-enabled CharacterController");
            }

            // Log camera status
            Camera mainCamera = Camera.main;
            Transform cameraOffset = null;

            if (mainCamera != null)
            {
                cameraOffset = mainCamera.transform.parent;

                Debug.Log($"[PersistentPlayer] ✓ Camera world: {mainCamera.transform.position}, local: {mainCamera.transform.localPosition}");

                if (cameraOffset != null)
                {
                    Debug.Log($"[PersistentPlayer] ✓ Camera Offset world: {cameraOffset.position}, local: {cameraOffset.localPosition}");

                    // Check if Camera Offset local position got reset
                    if (Mathf.Abs(cameraOffset.localPosition.y) < 0.1f)
                    {
                        Debug.LogError($"[PersistentPlayer] ✗ Camera Offset local Y is {cameraOffset.localPosition.y}, should be ~1.36! Fixing...");
                        cameraOffset.localPosition = new Vector3(0, 1.36f, 0);
                        Debug.Log($"[PersistentPlayer] Fixed Camera Offset local position to: {cameraOffset.localPosition}");
                    }
                }
                else
                {
                    Debug.LogError("[PersistentPlayer] ✗ Camera Offset not found!");
                }

                var trackedPoseDriver = mainCamera.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                if (trackedPoseDriver != null && trackedPoseDriver.enabled)
                {
                    Debug.Log("[PersistentPlayer] ✓ TrackedPoseDriver enabled");
                }
                else
                {
                    Debug.LogError("[PersistentPlayer] ✗ TrackedPoseDriver issue!");
                }
            }
            else
            {
                Debug.LogError("[PersistentPlayer] ✗ Main Camera not found!");
            }

            Debug.Log($"[PersistentPlayer] Spawn complete at: {transform.position}");

            // Wait 2 more seconds and check if Camera Offset is still correct
            yield return new WaitForSeconds(2f);

            if (mainCamera != null && cameraOffset != null)
            {
                Debug.Log($"[PersistentPlayer] Final check (2s later) - Camera Offset local: {cameraOffset.localPosition}, world: {cameraOffset.position}");

                if (Mathf.Abs(cameraOffset.localPosition.y) < 0.1f)
                {
                    Debug.LogError($"[PersistentPlayer] ✗✗✗ Camera Offset was RESET AGAIN to {cameraOffset.localPosition.y}! Something is overriding it!");
                }
                else
                {
                    Debug.Log($"[PersistentPlayer] ✓ Camera Offset still correct at Y={cameraOffset.localPosition.y:F2}");
                }
            }
        }
    }
}
