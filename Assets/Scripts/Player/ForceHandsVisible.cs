using UnityEngine;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Forces hands to be visible by ensuring all renderers are enabled
    /// Add this to XR Origin or hand GameObjects as a temporary fix
    /// </summary>
    public class ForceHandsVisible : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("[ForceHandsVisible] ========================================");
            Debug.Log("[ForceHandsVisible] FORCING HANDS VISIBLE");
            Debug.Log("[ForceHandsVisible] ========================================");

            // Find hands
            GameObject[] hands = new GameObject[]
            {
                GameObject.Find("PolytopiaHand_L"),
                GameObject.Find("PolytopiaHand_R")
            };

            foreach (GameObject hand in hands)
            {
                if (hand == null) continue;

                Debug.Log($"[ForceHandsVisible] Found hand: {hand.name}");
                Debug.Log($"[ForceHandsVisible]   Active: {hand.activeSelf} / {hand.activeInHierarchy}");

                // Force active
                hand.SetActive(true);

                // Enable all renderers
                MeshRenderer[] renderers = hand.GetComponentsInChildren<MeshRenderer>(true);
                Debug.Log($"[ForceHandsVisible]   Found {renderers.Length} renderers");

                foreach (MeshRenderer mr in renderers)
                {
                    Debug.Log($"[ForceHandsVisible]     - {mr.gameObject.name}: enabled={mr.enabled}");
                    mr.enabled = true;
                    mr.gameObject.SetActive(true);
                }

                Debug.Log($"[ForceHandsVisible]   ✓ Forced {hand.name} visible");
            }

            Debug.Log("[ForceHandsVisible] ========================================");
            Debug.Log("[ForceHandsVisible] DONE");
            Debug.Log("[ForceHandsVisible] ========================================");
        }
    }
}
