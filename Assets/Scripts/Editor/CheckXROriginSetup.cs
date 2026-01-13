using UnityEngine;
using UnityEditor;
using Unity.XR.CoreUtils;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Checks XR Origin setup and reports what's missing
    /// </summary>
    public static class CheckXROriginSetup
    {
        [MenuItem("Tools/VR Dungeon Crawler/Check XR Origin Setup", priority = 20)]
        public static void CheckSetup()
        {
            Debug.Log("========================================");
            Debug.Log("[CheckXROrigin] Checking XR Origin setup...");
            Debug.Log("========================================");

            XROrigin xrOrigin = Object.FindObjectOfType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("[CheckXROrigin] ❌ No XR Origin found in scene!");
                EditorUtility.DisplayDialog("XR Origin Missing!", "No XR Origin found in the scene!", "OK");
                return;
            }

            Debug.Log($"[CheckXROrigin] ✓ Found XR Origin: {xrOrigin.name}");

            // Check for controllers
            Transform leftController = xrOrigin.transform.Find("Camera Offset/Left Controller");
            Transform rightController = xrOrigin.transform.Find("Camera Offset/Right Controller");

            bool hasLeftController = leftController != null;
            bool hasRightController = rightController != null;

            Debug.Log($"[CheckXROrigin] Left Controller: {(hasLeftController ? "✓ Found" : "❌ Missing")}");
            Debug.Log($"[CheckXROrigin] Right Controller: {(hasRightController ? "✓ Found" : "❌ Missing")}");

            if (!hasLeftController || !hasRightController)
            {
                Debug.LogError("[CheckXROrigin] ❌ Controllers are missing from XR Origin!");
                Debug.LogError("[CheckXROrigin] The XR Origin might need to be replaced with a fresh prefab.");

                EditorUtility.DisplayDialog(
                    "Controllers Missing!",
                    "The Left and/or Right Controllers are missing from the XR Origin!\n\n" +
                    "This explains why your hands and spell menu aren't showing.\n\n" +
                    "The XR Origin needs to be replaced with the correct prefab.",
                    "OK"
                );
                return;
            }

            // Check for hand models
            CheckForComponent(leftController, "Hand Model", new string[] { "SimpleHandAnimator", "HandPoseController", "PolytopiaHandTracker" });
            CheckForComponent(rightController, "Hand Model", new string[] { "SimpleHandAnimator", "HandPoseController", "PolytopiaHandTracker" });

            // Check for spell menu
            CheckForComponent(xrOrigin.transform, "Spell Menu", new string[] { "SpellRadialMenu", "HalfLifeAlyxSpellMenu", "SpellRadialMenuUI" });

            Debug.Log("========================================");
            Debug.Log("[CheckXROrigin] Check complete!");
            Debug.Log("========================================");
        }

        private static void CheckForComponent(Transform parent, string componentName, string[] possibleTypes)
        {
            bool found = false;
            string foundType = "";

            foreach (string typeName in possibleTypes)
            {
                if (parent.GetComponentInChildren(System.Type.GetType($"VRDungeonCrawler.Player.{typeName}"), true) != null)
                {
                    found = true;
                    foundType = typeName;
                    break;
                }
            }

            if (found)
            {
                Debug.Log($"[CheckXROrigin] ✓ {componentName} found: {foundType}");
            }
            else
            {
                Debug.LogWarning($"[CheckXROrigin] ⚠️ {componentName} not found on {parent.name}");
            }
        }

        [MenuItem("Tools/VR Dungeon Crawler/List XR Origin Children", priority = 21)]
        public static void ListXROriginChildren()
        {
            XROrigin xrOrigin = Object.FindObjectOfType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("No XR Origin found!");
                return;
            }

            Debug.Log("========================================");
            Debug.Log("[XR Origin Hierarchy]");
            Debug.Log($"Root: {xrOrigin.name}");

            ListChildrenRecursive(xrOrigin.transform, 1);

            Debug.Log("========================================");
        }

        private static void ListChildrenRecursive(Transform parent, int depth)
        {
            string indent = new string(' ', depth * 2);

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Debug.Log($"{indent}├─ {child.name}");

                Component[] components = child.GetComponents<Component>();
                foreach (Component comp in components)
                {
                    if (comp != null && !(comp is Transform))
                    {
                        Debug.Log($"{indent}   └─ {comp.GetType().Name}");
                    }
                }

                if (depth < 3) // Limit depth to avoid spam
                {
                    ListChildrenRecursive(child, depth + 1);
                }
            }
        }
    }
}
