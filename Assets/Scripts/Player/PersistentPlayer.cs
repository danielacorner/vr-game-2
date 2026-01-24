using UnityEngine;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Bootstrap scene component - XR Origin lives here and persists via scene architecture
    /// NOT via DontDestroyOnLoad (which is an anti-pattern for XR)
    /// Content scenes load/unload additively around the persistent XR Origin
    /// </summary>
    public class PersistentPlayer : MonoBehaviour
    {
        void Awake()
        {
            Debug.Log("[PersistentPlayer] ========================================");
            Debug.Log($"[PersistentPlayer] XR Origin initialized in Bootstrap scene");
            Debug.Log($"[PersistentPlayer] Position: {transform.position}");
            Debug.Log("[PersistentPlayer] ========================================");
            DontDestroyOnLoad(gameObject);
            // XR Origin stays in Bootstrap scene - no DontDestroyOnLoad needed
            // Content scenes (HomeArea, Dungeon1) load additively
            // This prevents head tracking issues from scene transitions
        }

        void OnDestroy()
        {
            Debug.Log($"[PersistentPlayer] XR Origin destroyed (should only happen on app quit)");
        }
    }
}
