using UnityEngine;
using UnityEditor;

public class DeleteRuneStones
{
    [MenuItem("Tools/VR Dungeon Crawler/Delete RuneStones")]
    static void DeleteRunes()
    {
        GameObject portal = GameObject.Find("Portal");
        if (portal == null)
        {
            Debug.LogError("[DeleteRuneStones] Portal not found!");
            return;
        }

        int deleted = 0;
        Transform[] children = portal.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name.Contains("RuneStone"))
            {
                Debug.Log($"[DeleteRuneStones] Deleting {child.name}");
                Object.DestroyImmediate(child.gameObject);
                deleted++;
            }
        }

        Debug.Log($"[DeleteRuneStones] Deleted {deleted} RuneStone GameObjects");
        EditorUtility.SetDirty(portal);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
}
