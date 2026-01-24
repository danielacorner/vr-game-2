using UnityEngine;
using UnityEngine.UI;

namespace VRDungeonCrawler.Debugging
{
    /// <summary>
    /// Debug button to teleport to Dungeon1 scene for testing
    /// </summary>
    public class DebugDungeonTeleport : MonoBehaviour
    {
        [Header("Scene to Load")]
        [Tooltip("Name of the dungeon scene to load")]
        public string dungeonSceneName = "Dungeon1";

        [Header("References")]
        public Button button;

        void Start()
        {
            if (button == null)
            {
                button = GetComponentInChildren<Button>();
            }

            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
                UnityEngine.Debug.Log($"[DebugDungeonTeleport] Button listener added for scene: {dungeonSceneName}");
            }
            else
            {
                UnityEngine.Debug.LogError("[DebugDungeonTeleport] No button found!");
            }
        }

        void OnButtonClick()
        {
            UnityEngine.Debug.Log($"[DebugDungeonTeleport] Button clicked! Loading scene: {dungeonSceneName}");
            LoadDungeonScene();
        }

        void LoadDungeonScene()
        {
            // Use the same method as PortalMenu - check if BootstrapManager exists
            if (VRDungeonCrawler.Core.BootstrapManager.Instance != null)
            {
                UnityEngine.Debug.Log($"[DebugDungeonTeleport] Using BootstrapManager to load: {dungeonSceneName}");
                VRDungeonCrawler.Core.BootstrapManager.Instance.LoadContentScene(dungeonSceneName);
            }
            else
            {
                UnityEngine.Debug.LogError("[DebugDungeonTeleport] BootstrapManager not found! Cannot load scene.");
            }
        }

        void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClick);
            }
        }
    }
}
