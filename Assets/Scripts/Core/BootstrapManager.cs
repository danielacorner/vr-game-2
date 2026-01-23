using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace VRDungeonCrawler.Core
{
    /// <summary>
    /// Bootstrap manager that keeps XR Origin persistent across scene transitions
    /// This scene never unloads - content scenes load/unload additively
    /// </summary>
    public class BootstrapManager : MonoBehaviour
    {
        public static BootstrapManager Instance { get; private set; }

        [Header("Initial Scene")]
        [Tooltip("Scene to load on start")]
        public string initialSceneName = "HomeArea";

        [Header("Fade Settings")]
        [Tooltip("Fade duration between scenes")]
        public float fadeDuration = 0.5f;

        private string currentContentScene;
        private bool isTransitioning = false;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            Debug.Log("[BootstrapManager] ========================================");
            Debug.Log("[BootstrapManager] Bootstrap scene loaded - XR Origin will persist");
            Debug.Log("[BootstrapManager] ========================================");

            // Load initial scene
            LoadContentScene(initialSceneName);
        }

        /// <summary>
        /// Load a new content scene, unloading the previous one
        /// </summary>
        public void LoadContentScene(string sceneName)
        {
            if (isTransitioning)
            {
                Debug.LogWarning($"[BootstrapManager] Already transitioning, ignoring load request for {sceneName}");
                return;
            }

            StartCoroutine(TransitionToScene(sceneName));
        }

        private IEnumerator TransitionToScene(string sceneName)
        {
            isTransitioning = true;

            Debug.Log($"[BootstrapManager] ========================================");
            Debug.Log($"[BootstrapManager] Transitioning to scene: {sceneName}");
            Debug.Log($"[BootstrapManager] ========================================");

            // Fade out (optional - you can add FadeCanvas later)
            yield return new WaitForSeconds(fadeDuration / 2f);

            // Load new scene additively
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (loadOp == null)
            {
                Debug.LogError($"[BootstrapManager] Failed to load scene: {sceneName}");
                isTransitioning = false;
                yield break;
            }

            loadOp.allowSceneActivation = true;

            // Wait for load
            while (!loadOp.isDone)
            {
                yield return null;
            }

            Debug.Log($"[BootstrapManager] ✓ Loaded scene: {sceneName}");

            // Set as active scene (for lighting, etc.)
            Scene newScene = SceneManager.GetSceneByName(sceneName);
            if (newScene.isLoaded)
            {
                SceneManager.SetActiveScene(newScene);
                Debug.Log($"[BootstrapManager] ✓ Set active scene: {sceneName}");

                // Move XR Origin to spawn point in new scene
                MovePlayerToSpawnPoint(newScene);
            }

            // Unload previous content scene
            if (!string.IsNullOrEmpty(currentContentScene) && currentContentScene != "Bootstrap")
            {
                Debug.Log($"[BootstrapManager] Unloading previous scene: {currentContentScene}");
                yield return SceneManager.UnloadSceneAsync(currentContentScene);
                Debug.Log($"[BootstrapManager] ✓ Unloaded: {currentContentScene}");
            }

            currentContentScene = sceneName;

            // Fade in (optional)
            yield return new WaitForSeconds(fadeDuration / 2f);

            isTransitioning = false;
            Debug.Log($"[BootstrapManager] ========================================");
            Debug.Log($"[BootstrapManager] ✓ Transition complete to {sceneName}");
            Debug.Log($"[BootstrapManager] ========================================");
        }

        /// <summary>
        /// Finds spawn point in the scene and moves XR Origin there
        /// </summary>
        private void MovePlayerToSpawnPoint(Scene scene)
        {
            // Find XR Origin
            GameObject xrOriginObj = GameObject.Find("XR Origin (XR Rig)");
            if (xrOriginObj == null)
            {
                Debug.LogWarning("[BootstrapManager] XR Origin not found, can't move to spawn point");
                return;
            }

            // Look for spawn point in the newly loaded scene
            GameObject[] rootObjects = scene.GetRootGameObjects();
            Transform spawnPoint = null;

            foreach (GameObject rootObj in rootObjects)
            {
                // Search for spawn point by name
                spawnPoint = FindSpawnPointRecursive(rootObj.transform);
                if (spawnPoint != null)
                    break;
            }

            if (spawnPoint != null)
            {
                // Disable CharacterController during teleport
                CharacterController cc = xrOriginObj.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false;
                }

                // Move XR Origin to spawn point
                xrOriginObj.transform.position = spawnPoint.position;
                xrOriginObj.transform.rotation = spawnPoint.rotation;

                // Re-enable CharacterController
                if (cc != null)
                {
                    cc.enabled = true;
                }

                Debug.Log($"[BootstrapManager] ✓ Moved player to spawn point: {spawnPoint.name} at {spawnPoint.position}");
            }
            else
            {
                Debug.Log($"[BootstrapManager] No spawn point found in {scene.name}, player stays at current position");
            }
        }

        /// <summary>
        /// Recursively search for spawn point GameObject
        /// </summary>
        private Transform FindSpawnPointRecursive(Transform parent)
        {
            // Check if this object is a spawn point
            string name = parent.name.ToLower();
            if (name.Contains("playerspawn") || name.Contains("dungeonspawn") || name == "spawnpoint")
            {
                return parent;
            }

            // Search children
            foreach (Transform child in parent)
            {
                Transform result = FindSpawnPointRecursive(child);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
