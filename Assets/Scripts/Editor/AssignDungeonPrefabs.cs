using UnityEngine;
using UnityEditor;
using VRDungeonCrawler.Dungeon;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Editor script to assign prefabs to DungeonGenerator
    /// </summary>
    public static class AssignDungeonPrefabs
    {
        [MenuItem("Tools/VR Dungeon Crawler/Assign Dungeon Prefabs")]
        public static void AssignPrefabs()
        {
            Debug.Log("========================================");
            Debug.Log("Assigning Dungeon Prefabs");
            Debug.Log("========================================");

            // Find DungeonGenerator in the scene
            DungeonGenerator generator = GameObject.FindFirstObjectByType<DungeonGenerator>();
            if (generator == null)
            {
                Debug.LogError("DungeonGenerator not found in scene!");
                return;
            }

            // Load prefabs from Assets
            GameObject normalRoom = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Dungeon/NormalRoom.prefab");
            GameObject enemyBasic = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Dungeon/Enemy_Basic.prefab");
            GameObject enemyTough = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Dungeon/Enemy_Tough.prefab");

            if (normalRoom == null)
            {
                Debug.LogError("Failed to load NormalRoom.prefab");
                return;
            }
            if (enemyBasic == null)
            {
                Debug.LogError("Failed to load Enemy_Basic.prefab");
                return;
            }
            if (enemyTough == null)
            {
                Debug.LogError("Failed to load Enemy_Tough.prefab");
                return;
            }

            // Assign arrays
            generator.roomPrefabs = new GameObject[] { normalRoom };
            generator.enemyPrefabs = new GameObject[] { enemyBasic, enemyTough };

            // Mark as dirty and save
            EditorUtility.SetDirty(generator);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log("Assigned roomPrefabs: " + generator.roomPrefabs.Length + " prefabs");
            Debug.Log("Assigned enemyPrefabs: " + generator.enemyPrefabs.Length + " prefabs");
            Debug.Log("========================================");
            Debug.Log("Dungeon prefabs assigned successfully!");
            Debug.Log("========================================");
        }
    }
}
