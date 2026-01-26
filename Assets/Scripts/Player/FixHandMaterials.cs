using UnityEngine;
using VRDungeonCrawler.Utils;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Fixes missing hand materials at runtime
    /// Add this to XR Origin in Bootstrap scene
    /// </summary>
    [DefaultExecutionOrder(-50)] // Run early
    public class FixHandMaterials : MonoBehaviour
    {
        void Awake()
        {
            Debug.Log("[FixHandMaterials] ========================================");
            Debug.Log("[FixHandMaterials] FIXING HAND MATERIALS");
            Debug.Log("[FixHandMaterials] ========================================");

            // Find hands
            GameObject[] hands = new GameObject[]
            {
                GameObject.Find("PolytopiaHand_L"),
                GameObject.Find("PolytopiaHand_R")
            };

            foreach (GameObject hand in hands)
            {
                if (hand == null) continue;

                Debug.Log($"[FixHandMaterials] Fixing materials for: {hand.name}");

                // Get all MeshRenderers
                MeshRenderer[] renderers = hand.GetComponentsInChildren<MeshRenderer>(true);
                Debug.Log($"[FixHandMaterials]   Found {renderers.Length} renderers");

                Material handMaterial = PolytopiaHandGenerator.CreatePolytopiaHandMaterial();
                Debug.Log($"[FixHandMaterials]   Created material: {handMaterial.name}");

                int fixedCount = 0;
                foreach (MeshRenderer mr in renderers)
                {
                    if (mr.sharedMaterial == null)
                    {
                        Debug.Log($"[FixHandMaterials]     Fixing {mr.gameObject.name} - material was NULL");
                        mr.sharedMaterial = handMaterial;
                        fixedCount++;
                    }
                    else
                    {
                        Debug.Log($"[FixHandMaterials]     {mr.gameObject.name} - already has material: {mr.sharedMaterial.name}");
                    }

                    // Ensure enabled
                    mr.enabled = true;
                }

                Debug.Log($"[FixHandMaterials]   ✓ Fixed {fixedCount} renderers for {hand.name}");
            }

            Debug.Log("[FixHandMaterials] ========================================");
            Debug.Log("[FixHandMaterials] DONE - Hands should now be visible!");
            Debug.Log("[FixHandMaterials] ========================================");
        }
    }
}
