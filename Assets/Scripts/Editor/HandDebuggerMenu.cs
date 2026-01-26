using UnityEngine;
using UnityEditor;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Menu items for adding hand visibility debugger to scene
    /// </summary>
    public static class HandDebuggerMenu
    {
        [MenuItem("Tools/VR Dungeon Crawler/Debug/Add Hand Visibility Debugger", priority = 100)]
        public static void AddHandDebugger()
        {
            // Find or create debugger object
            GameObject debuggerObj = GameObject.Find("HandVisibilityDebugger");

            if (debuggerObj == null)
            {
                debuggerObj = new GameObject("HandVisibilityDebugger");
                Debug.Log("[HandDebuggerMenu] Created HandVisibilityDebugger GameObject");
            }

            // Add component if not already present
            VRDungeonCrawler.Diagnostics.HandVisibilityDebugger debugger = debuggerObj.GetComponent<VRDungeonCrawler.Diagnostics.HandVisibilityDebugger>();
            if (debugger == null)
            {
                debugger = debuggerObj.AddComponent<VRDungeonCrawler.Diagnostics.HandVisibilityDebugger>();
                Debug.Log("[HandDebuggerMenu] ✓ Added HandVisibilityDebugger component");
            }
            else
            {
                Debug.Log("[HandDebuggerMenu] HandVisibilityDebugger already exists");
            }

            // Select the object
            Selection.activeGameObject = debuggerObj;

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            EditorUtility.DisplayDialog(
                "Hand Debugger Added",
                "✓ HandVisibilityDebugger added to scene\n\n" +
                "Enter Play mode or build to device to see diagnostic logs.\n\n" +
                "The debugger will log detailed hand state every 2 seconds.",
                "OK"
            );
        }

        [MenuItem("Tools/VR Dungeon Crawler/Debug/Remove Hand Visibility Debugger", priority = 101)]
        public static void RemoveHandDebugger()
        {
            GameObject debuggerObj = GameObject.Find("HandVisibilityDebugger");

            if (debuggerObj != null)
            {
                Object.DestroyImmediate(debuggerObj);
                Debug.Log("[HandDebuggerMenu] ✓ Removed HandVisibilityDebugger");

                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
                );
            }
            else
            {
                Debug.LogWarning("[HandDebuggerMenu] No HandVisibilityDebugger found in scene");
            }
        }
    }
}
