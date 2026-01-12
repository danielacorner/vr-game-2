using UnityEngine;
using UnityEditor;
using VRDungeonCrawler.Entities;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Editor utility to assign GameModeMenu prefab to Portal
    /// </summary>
    public class AssignPortalPrefab : EditorWindow
    {
        [MenuItem("Tools/VR Dungeon Crawler/Assign Portal Prefab")]
        public static void AssignPrefab()
        {
            // Find the Portal in the scene
            Portal portal = FindObjectOfType<Portal>();

            if (portal == null)
            {
                Debug.LogError("[AssignPortalPrefab] No Portal found in scene!");
                return;
            }

            // Load the GameModeMenu prefab
            string prefabPath = "Assets/Prefabs/UI/GameModeMenu.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"[AssignPortalPrefab] Failed to load prefab at {prefabPath}");
                return;
            }

            // Assign the prefab
            portal.gameModeMenuPrefab = prefab;

            // Mark scene as dirty so it saves
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(portal.gameObject.scene);

            Debug.Log($"[AssignPortalPrefab] ✓ Successfully assigned {prefab.name} to Portal.gameModeMenuPrefab");
            Debug.Log($"[AssignPortalPrefab] ✓ Scene marked dirty. Remember to save!");

            // Also update trigger radius while we're at it
            if (portal.triggerRadius != 2.5f)
            {
                portal.triggerRadius = 2.5f;
                Debug.Log($"[AssignPortalPrefab] ✓ Updated triggerRadius to 2.5f");
            }
        }
    }
}
