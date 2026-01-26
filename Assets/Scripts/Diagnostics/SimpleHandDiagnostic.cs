using UnityEngine;

namespace VRDungeonCrawler.Diagnostics
{
    /// <summary>
    /// Simple diagnostic that logs hand state on startup
    /// Add this to any GameObject in Bootstrap scene
    /// </summary>
    public class SimpleHandDiagnostic : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("================================================");
            Debug.Log("[SimpleHandDiagnostic] STARTING HAND DIAGNOSTICS");
            Debug.Log("================================================");

            // Find hands
            GameObject leftHand = GameObject.Find("PolytopiaHand_L");
            GameObject rightHand = GameObject.Find("PolytopiaHand_R");

            Debug.Log($"[SimpleHandDiagnostic] Left hand found: {leftHand != null}");
            Debug.Log($"[SimpleHandDiagnostic] Right hand found: {rightHand != null}");

            if (leftHand != null)
            {
                DiagnoseHand(leftHand, "LEFT");
            }

            if (rightHand != null)
            {
                DiagnoseHand(rightHand, "RIGHT");
            }

            Debug.Log("================================================");
            Debug.Log("[SimpleHandDiagnostic] DIAGNOSTICS COMPLETE");
            Debug.Log("================================================");
        }

        void DiagnoseHand(GameObject hand, string name)
        {
            Debug.Log($"--- {name} HAND DIAGNOSTICS ---");
            Debug.Log($"  GameObject active: {hand.activeSelf}");
            Debug.Log($"  ActiveInHierarchy: {hand.activeInHierarchy}");
            Debug.Log($"  Layer: {hand.layer}");
            Debug.Log($"  Position: {hand.transform.position}");
            Debug.Log($"  LocalScale: {hand.transform.localScale}");

            // Check parent hierarchy
            Transform current = hand.transform;
            int depth = 0;
            while (current != null && depth < 5)
            {
                Debug.Log($"  Hierarchy[{depth}]: {current.name} (active: {current.gameObject.activeSelf})");
                current = current.parent;
                depth++;
            }

            // Check all MeshRenderers
            MeshRenderer[] renderers = hand.GetComponentsInChildren<MeshRenderer>(true);
            Debug.Log($"  Total MeshRenderers: {renderers.Length}");

            foreach (MeshRenderer mr in renderers)
            {
                Debug.Log($"    - {mr.gameObject.name}:");
                Debug.Log($"        Enabled: {mr.enabled}");
                Debug.Log($"        GameObject active: {mr.gameObject.activeSelf}");
                Debug.Log($"        ActiveInHierarchy: {mr.gameObject.activeInHierarchy}");

                if (mr.sharedMaterial != null)
                {
                    Debug.Log($"        Material: {mr.sharedMaterial.name}");
                    Debug.Log($"        Shader: {mr.sharedMaterial.shader.name}");
                }
                else
                {
                    Debug.Log($"        Material: NULL!");
                }
            }

            // Check camera culling
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                int handLayer = hand.layer;
                bool isCulled = (mainCam.cullingMask & (1 << handLayer)) == 0;
                Debug.Log($"  Camera culls hand layer: {isCulled}");
                Debug.Log($"  Camera culling mask: {mainCam.cullingMask}");
            }
        }
    }
}
