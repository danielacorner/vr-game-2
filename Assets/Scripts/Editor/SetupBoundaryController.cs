using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using VRDungeonCrawler.Environment;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Sets up boundary controller to prevent player and animals from falling off edges
    /// Run from menu: Tools/VR Dungeon Crawler/Setup Boundary Controller
    /// </summary>
    public class SetupBoundaryController : UnityEditor.Editor
    {
        [MenuItem("Tools/VR Dungeon Crawler/Setup Boundary Controller")]
        public static void Setup()
        {
            Debug.Log("========================================");
            Debug.Log("Setting Up Boundary Controller");
            Debug.Log("========================================");

            // Create or find boundary controller
            GameObject boundaryGO = GameObject.Find("BoundaryController");
            if (boundaryGO == null)
            {
                boundaryGO = new GameObject("BoundaryController");
                boundaryGO.transform.position = Vector3.zero;
                Debug.Log("✓ Created BoundaryController GameObject");
            }

            // Add boundary controller component
            BoundaryController controller = boundaryGO.GetComponent<BoundaryController>();
            if (controller == null)
            {
                controller = boundaryGO.AddComponent<BoundaryController>();
                Debug.Log("✓ Added BoundaryController component");
            }

            // Configure settings for 50m×50m terrain
            controller.boundaryCenter = Vector3.zero;
            controller.boundaryRadius = 24f; // 50m diameter terrain = 25m radius, use 24m for safety margin
            controller.warningDistance = 2f;
            controller.pushBackStrength = 10f;
            controller.affectPlayer = true;
            controller.affectAnimals = true;
            controller.showWarning = true;
            controller.warningColor = new Color(1f, 0.5f, 0f, 0.3f);
            controller.showDebug = true;

            EditorUtility.SetDirty(boundaryGO);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log("✓✓✓ Boundary Controller Setup Complete!");
            Debug.Log("Players and animals will be prevented from falling off edges.");
            Debug.Log("Boundary Radius: 24m (for 50m terrain)");
            Debug.Log("Warning Zone: 2m from edge");
            Debug.Log("========================================");
        }
    }
}
