using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

public class CreateTestScene
{
    [MenuItem("Tools/Create Test Minimal Scene")]
    public static void CreateScene()
    {
        // Create new scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Add Directional Light
        GameObject light = new GameObject("Directional Light");
        Light lightComp = light.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Add Terrain
        GameObject terrainObj = Terrain.CreateTerrainGameObject(new TerrainData());
        terrainObj.name = "Terrain";
        Terrain terrain = terrainObj.GetComponent<Terrain>();
        TerrainData terrainData = terrain.terrainData;
        terrainData.size = new Vector3(50, 10, 50);
        terrainData.SetHeights(0, 0, new float[33, 33]); // Flat terrain

        // Add Spawn Point
        GameObject spawnPoint = new GameObject("PlayerSpawnPoint");
        spawnPoint.transform.position = new Vector3(25f, 2f, 25f); // Center of terrain, 2m up

        // Save scene
        EditorSceneManager.SaveScene(newScene, "Assets/Scenes/TestMinimal.unity");
        Debug.Log("✓ Created TestMinimal scene with terrain, light, and spawn point");
    }
}
#endif
