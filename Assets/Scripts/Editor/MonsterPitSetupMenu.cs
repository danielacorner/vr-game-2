using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Editor menu to add MonsterPitSetup to HomeArea scene
    /// </summary>
    public static class MonsterPitSetupMenu
    {
        [MenuItem("Tools/VR Dungeon Crawler/Setup Monster Pit in HomeArea", priority = 200)]
        public static void SetupMonsterPit()
        {
            // Check if HomeArea scene is loaded
            Scene homeArea = SceneManager.GetSceneByName("HomeArea");
            if (!homeArea.isLoaded)
            {
                EditorUtility.DisplayDialog(
                    "Scene Not Loaded",
                    "Please open the HomeArea scene first, then run this command again.",
                    "OK"
                );
                return;
            }

            // Find or create Environment GameObject
            GameObject envObj = GameObject.Find("Environment");
            if (envObj == null)
            {
                envObj = new GameObject("Environment");
                Debug.Log("[MonsterPitSetup] Created Environment GameObject");
            }

            // Check if MonsterPitSetup already exists
            Environment.MonsterPitSetup existingSetup = envObj.GetComponent<Environment.MonsterPitSetup>();
            if (existingSetup != null)
            {
                Debug.Log("[MonsterPitSetup] MonsterPitSetup already exists");
                EditorUtility.DisplayDialog(
                    "Already Exists",
                    "MonsterPitSetup is already in the scene.\n\nIf the pit isn't showing, try:\n1. Delete the Environment GameObject\n2. Run this command again\n3. Enter Play mode to see the pit created",
                    "OK"
                );
                return;
            }

            // Add MonsterPitSetup component
            Environment.MonsterPitSetup pitSetup = envObj.AddComponent<Environment.MonsterPitSetup>();
            pitSetup.pitDiameter = 12f;
            pitSetup.pitDepth = 1.5f;
            pitSetup.createVisibleRim = true;
            pitSetup.createDebugMarker = true;
            pitSetup.showDebug = true;

            Debug.Log("[MonsterPitSetup] ✓ Added MonsterPitSetup component");

            // Mark scene dirty
            EditorSceneManager.MarkSceneDirty(homeArea);

            // Select the object
            Selection.activeGameObject = envObj;

            EditorUtility.DisplayDialog(
                "Monster Pit Setup Added",
                "✓ MonsterPitSetup added to HomeArea scene!\n\n" +
                "Next steps:\n" +
                "1. Save the scene (Ctrl/Cmd+S)\n" +
                "2. Enter Play mode in Unity to see the pit created\n" +
                "3. Build to Quest 3 to test in VR\n\n" +
                "The pit will be:\n" +
                "- 12m diameter (3x spawner width)\n" +
                "- 1.5m deep (requires double-jump)\n" +
                "- Visible stone rim around edge\n" +
                "- Yellow marker at center",
                "OK"
            );
        }

        [MenuItem("Tools/VR Dungeon Crawler/Remove Monster Pit Setup", priority = 201)]
        public static void RemoveMonsterPit()
        {
            GameObject envObj = GameObject.Find("Environment");
            if (envObj != null)
            {
                Environment.MonsterPitSetup pitSetup = envObj.GetComponent<Environment.MonsterPitSetup>();
                if (pitSetup != null)
                {
                    Object.DestroyImmediate(pitSetup);
                    Debug.Log("[MonsterPitSetup] Removed MonsterPitSetup component");

                    Scene homeArea = SceneManager.GetSceneByName("HomeArea");
                    if (homeArea.isLoaded)
                    {
                        EditorSceneManager.MarkSceneDirty(homeArea);
                    }

                    EditorUtility.DisplayDialog(
                        "Monster Pit Setup Removed",
                        "MonsterPitSetup component has been removed.\n\nNote: The pit modifications to the terrain are permanent until you reload the scene.",
                        "OK"
                    );
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Not Found",
                        "No MonsterPitSetup component found in the scene.",
                        "OK"
                    );
                }
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Not Found",
                    "No Environment GameObject found in the scene.",
                    "OK"
                );
            }
        }
    }
}
