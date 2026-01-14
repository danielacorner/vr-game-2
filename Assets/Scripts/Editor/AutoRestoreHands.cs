using UnityEngine;
using UnityEditor;
using VRDungeonCrawler.Utils;
using VRDungeonCrawler.Player;
using Unity.XR.CoreUtils;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Auto-runs once to restore hands when this script compiles
    /// </summary>
    [InitializeOnLoad]
    public static class AutoRestoreHands
    {
        private const string PREF_KEY = "VRD_HandsRestored_v2";

        static AutoRestoreHands()
        {
            // DISABLED - Run manually via "Tools/VR Dungeon Crawler/Reset Auto-Restore Hands Flag"
            return;

            // Only run once - check flag and skip to avoid repeated dialogs
            // Use "Reset Auto-Restore Hands Flag" menu item if you need to re-run
            if (EditorPrefs.GetBool(PREF_KEY, false))
            {
                return;
            }

            // Delay execution until after scripts compile
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;

                Debug.Log("========================================");
                Debug.Log("[AutoRestoreHands] AUTO-RESTORING HANDS NOW!");
                Debug.Log("========================================");

                DoRestore();

                // Mark as done
                EditorPrefs.SetBool(PREF_KEY, true);
            };
        }

        private static void DoRestore()
        {
            // Find XR Origin
            XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("[AutoRestoreHands] No XR Origin found!");
                return;
            }

            Transform leftController = xrOrigin.transform.Find("Camera Offset/Left Controller");
            Transform rightController = xrOrigin.transform.Find("Camera Offset/Right Controller");

            if (leftController == null || rightController == null)
            {
                Debug.LogError("[AutoRestoreHands] Controllers not found!");
                return;
            }

            // Remove old hands if they exist
            Transform oldLeft = leftController.Find("PolytopiaHand_L");
            if (oldLeft != null) Object.DestroyImmediate(oldLeft.gameObject);

            Transform oldRight = rightController.Find("PolytopiaHand_R");
            if (oldRight != null) Object.DestroyImmediate(oldRight.gameObject);

            // Generate hands
            GameObject leftHand = PolytopiaHandGenerator.CreateArticulatedHand(leftController, true);
            GameObject rightHand = PolytopiaHandGenerator.CreateArticulatedHand(rightController, false);

            Debug.Log($"[AutoRestoreHands] ✓ Generated hands: {leftHand.name}, {rightHand.name}");

            // Add tracking components
            AddTracker(leftHand, true);
            AddTracker(rightHand, false);

            // Create spell menu
            CreateSpellMenu(rightController);

            Debug.Log("========================================");
            Debug.Log("[AutoRestoreHands] ✓✓✓ HANDS RESTORED!");
            Debug.Log("========================================");

            EditorUtility.DisplayDialog(
                "Hands Auto-Restored!",
                "✓ Left and right hand models\n" +
                "✓ Hand tracking components\n" +
                "✓ Spell menu on right controller\n\n" +
                "Test in VR now!",
                "OK"
            );

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
        }

        private static void AddTracker(GameObject hand, bool isLeft)
        {
            PolytopiaHandTracker tracker = hand.AddComponent<PolytopiaHandTracker>();
            tracker.isLeftHand = isLeft;

            Transform palm = hand.transform.Find("Palm");
            if (palm != null)
            {
                tracker.thumbRoot = palm.Find("Thumb_Segment0");
                tracker.indexRoot = palm.Find("Index_Segment0");
                tracker.middleRoot = palm.Find("Middle_Segment0");
                tracker.ringRoot = palm.Find("Ring_Segment0");
                tracker.pinkyRoot = palm.Find("Pinky_Segment0");

                Debug.Log($"[AutoRestoreHands] ✓ Wired {(isLeft ? "LEFT" : "RIGHT")} hand bones");
            }
        }

        private static void CreateSpellMenu(Transform controller)
        {
            // Remove old menu
            SpellRadialMenu old = controller.GetComponentInChildren<SpellRadialMenu>();
            if (old != null) Object.DestroyImmediate(old.gameObject);

            GameObject menuObj = new GameObject("SpellRadialMenu");
            menuObj.transform.SetParent(controller);
            menuObj.transform.localPosition = Vector3.zero;
            menuObj.transform.localRotation = Quaternion.identity;

            SpellRadialMenu menu = menuObj.AddComponent<SpellRadialMenu>();

            // Create canvas
            GameObject canvasObj = new GameObject("MenuCanvas");
            canvasObj.transform.SetParent(menuObj.transform);
            canvasObj.transform.localPosition = Vector3.zero;

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(400, 400);
            canvasRect.localScale = Vector3.one * 0.001f;

            // Container
            GameObject containerObj = new GameObject("MenuContainer");
            containerObj.transform.SetParent(canvasObj.transform);
            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.sizeDelta = Vector2.zero;

            menu.menuCanvas = canvasObj.transform;
            menu.menuContainer = containerRect;

            var actionController = controller.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
            if (actionController != null)
            {
                menu.controller = actionController;
            }

            canvasObj.SetActive(false);

            Debug.Log("[AutoRestoreHands] ✓ Created spell menu");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Reset Auto-Restore Hands Flag", priority = 40)]
        public static void ResetFlag()
        {
            EditorPrefs.DeleteKey(PREF_KEY);
            Debug.Log("[AutoRestoreHands] Flag reset - will auto-restore on next script compile");
        }
    }
}
