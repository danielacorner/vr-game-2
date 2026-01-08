using UnityEngine;
using UnityEditor;

public class TerrainGrassApplier
{
    [MenuItem("Tools/Apply Dark Grass to Terrain")]
    public static void ApplyDarkGrass()
    {
        Terrain terrain = GameObject.Find("Terrain")?.GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("[TerrainGrassApplier] Terrain not found!");
            return;
        }

        // Create a simple dark grass colored texture
        Texture2D grassTexture = new Texture2D(256, 256);
        Color darkGrass = new Color(0.12f, 0.18f, 0.1f, 1f);
        
        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                // Add slight variation for texture realism
                float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.05f;
                Color pixelColor = new Color(
                    Mathf.Clamp01(darkGrass.r + noise),
                    Mathf.Clamp01(darkGrass.g + noise + 0.02f),
                    Mathf.Clamp01(darkGrass.b + noise),
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
        
        // Wait for import
        AssetDatabase.Refresh();

        // Create terrain layer
        TerrainLayer terrainLayer = new TerrainLayer();
        Texture2D loadedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        
        if (loadedTexture == null)
        {
            Debug.LogError("[TerrainGrassApplier] Failed to load grass texture!");
            return;
        }

        terrainLayer.diffuseTexture = loadedTexture;
        terrainLayer.tileSize = new Vector2(15, 15);
        terrainLayer.tileOffset = Vector2.zero;

        // Set material properties for realistic dirt/grass (not reflective)
        terrainLayer.metallic = 0f; // Dirt/grass is not metallic
        terrainLayer.smoothness = 0.15f; // Rough surface like dirt/grass

        // Save terrain layer
        string layerPath = "Assets/Materials/DarkGrassLayer.terrainlayer";
        AssetDatabase.CreateAsset(terrainLayer, layerPath);
        AssetDatabase.SaveAssets();

        // Apply to terrain
        TerrainLayer[] layers = new TerrainLayer[] { terrainLayer };
        terrain.terrainData.terrainLayers = layers;

        // Mark terrain dirty for saving
        EditorUtility.SetDirty(terrain);
        EditorUtility.SetDirty(terrain.terrainData);

        Debug.Log("[TerrainGrassApplier] ✓ Dark grass texture applied successfully!");
        Debug.Log($"Texture saved at: {texturePath}");
        Debug.Log($"Terrain layer saved at: {layerPath}");
    }
}