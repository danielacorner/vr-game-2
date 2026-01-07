using UnityEngine;
using UnityEditor;

public class ApplyTerrainGrass : MonoBehaviour
{
    [ContextMenu("Apply Dark Grass to Terrain")]
    public void ApplyGrassTexture()
    {
        Terrain terrain = GameObject.Find("Terrain")?.GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("[ApplyTerrainGrass] Terrain not found!");
            return;
        }

        // Create a simple dark grass colored texture
        Texture2D grassTexture = new Texture2D(128, 128);
        Color darkGrass = new Color(0.12f, 0.18f, 0.1f, 1f);
        
        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                // Add slight variation for texture
                float variation = Random.Range(-0.02f, 0.02f);
                Color pixelColor = new Color(
                    darkGrass.r + variation,
                    darkGrass.g + variation,
                    darkGrass.b + variation,
                    1f
                );
                grassTexture.SetPixel(x, y, pixelColor);
            }
        }
        grassTexture.Apply();

        // Save texture as asset
        string texturePath = "Assets/Materials/DarkGrassTexture.png";
        byte[] bytes = grassTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(texturePath, bytes);
        AssetDatabase.ImportAsset(texturePath);

        // Create terrain layer
        TerrainLayer terrainLayer = new TerrainLayer();
        terrainLayer.diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        terrainLayer.tileSize = new Vector2(15, 15);

        string layerPath = "Assets/Materials/DarkGrassLayer.terrainlayer";
        AssetDatabase.CreateAsset(terrainLayer, layerPath);
        AssetDatabase.SaveAssets();

        // Apply to terrain
        TerrainLayer[] layers = new TerrainLayer[] { terrainLayer };
        terrain.terrainData.terrainLayers = layers;

        Debug.Log("[ApplyTerrainGrass] ✓ Dark grass texture applied to terrain!");
    }
}