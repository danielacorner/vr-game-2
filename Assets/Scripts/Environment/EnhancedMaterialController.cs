using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Controls enhanced PBR materials for terrain, ruins, and vegetation
    /// Manages procedural texture generation and material properties
    /// </summary>
    public class EnhancedMaterialController : MonoBehaviour
    {
        [Header("Terrain Settings")]
        [Tooltip("Enable enhanced terrain material")]
        public bool enhanceTerrainMaterial = true;

        [Range(0f, 2f)]
        [Tooltip("Normal map strength for terrain")]
        public float terrainNormalStrength = 1f;

        [Range(0f, 1f)]
        [Tooltip("Terrain roughness")]
        public float terrainRoughness = 0.8f;

        [Header("Ruins Settings")]
        [Tooltip("Enable enhanced ruins materials")]
        public bool enhanceRuinsMaterials = true;

        [Range(0f, 2f)]
        [Tooltip("Normal map strength for ruins")]
        public float ruinsNormalStrength = 1.5f;

        [Range(0f, 1f)]
        [Tooltip("Ruins roughness")]
        public float ruinsRoughness = 0.9f;

        [Header("Tree Settings")]
        [Tooltip("Enable enhanced tree materials")]
        public bool enhanceTreeMaterials = true;

        [Range(0f, 1f)]
        [Tooltip("Leaf translucency strength")]
        public float leafTranslucency = 0.3f;

        [Range(0f, 2f)]
        [Tooltip("Bark normal strength")]
        public float barkNormalStrength = 1.2f;

        private Material terrainMaterial;
        private Material[] ruinsMaterials;
        private Material[] treeMaterials;

        void Start()
        {
            ApplyEnhancements();
        }

        void ApplyEnhancements()
        {
            if (enhanceTerrainMaterial)
            {
                EnhanceTerrainMaterial();
            }

            if (enhanceRuinsMaterials)
            {
                EnhanceRuinsMaterials();
            }

            if (enhanceTreeMaterials)
            {
                EnhanceTreeMaterials();
            }
        }

        void EnhanceTerrainMaterial()
        {
            Terrain terrain = FindObjectOfType<Terrain>();
            if (terrain == null)
            {
                Debug.LogWarning("[EnhancedMaterialController] No terrain found");
                return;
            }

            // Get terrain material
            terrainMaterial = terrain.materialTemplate;
            if (terrainMaterial == null)
            {
                Debug.LogWarning("[EnhancedMaterialController] Terrain has no material");
                return;
            }

            // Generate procedural normal map
            Texture2D normalMap = GenerateProceduralNormalMap(512, 512, 0.5f);

            // Apply to terrain
            if (terrainMaterial.HasProperty("_BumpMap"))
            {
                terrainMaterial.SetTexture("_BumpMap", normalMap);
                terrainMaterial.SetFloat("_BumpScale", terrainNormalStrength);
            }

            if (terrainMaterial.HasProperty("_Smoothness"))
            {
                terrainMaterial.SetFloat("_Smoothness", 1f - terrainRoughness);
            }

            Debug.Log("[EnhancedMaterialController] Terrain material enhanced");
        }

        void EnhanceRuinsMaterials()
        {
            // Find all objects with "Ruin" in name
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("ruin") || obj.name.ToLower().Contains("stone"))
                {
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        foreach (Material mat in renderer.materials)
                        {
                            EnhanceRuinMaterial(mat);
                        }
                    }
                }
            }

            Debug.Log("[EnhancedMaterialController] Ruins materials enhanced");
        }

        void EnhanceRuinMaterial(Material mat)
        {
            // Generate stone normal map
            Texture2D normalMap = GenerateProceduralNormalMap(256, 256, 0.8f);

            if (mat.HasProperty("_BumpMap"))
            {
                mat.SetTexture("_BumpMap", normalMap);
                mat.SetFloat("_BumpScale", ruinsNormalStrength);
            }

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", 1f - ruinsRoughness);
            }

            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", 0f);
            }

            // Add weathering variation
            if (mat.HasProperty("_BaseColor"))
            {
                Color baseColor = mat.GetColor("_BaseColor");
                baseColor *= 0.7f; // Darken slightly for weathered look
                mat.SetColor("_BaseColor", baseColor);
            }
        }

        void EnhanceTreeMaterials()
        {
            // Find all trees
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("tree") ||
                    obj.name.ToLower().Contains("bark") ||
                    obj.name.ToLower().Contains("leaf") ||
                    obj.name.ToLower().Contains("leave"))
                {
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        foreach (Material mat in renderer.materials)
                        {
                            if (obj.name.ToLower().Contains("leaf") || obj.name.ToLower().Contains("leave"))
                            {
                                EnhanceLeafMaterial(mat);
                            }
                            else
                            {
                                EnhanceBarkMaterial(mat);
                            }
                        }
                    }
                }
            }

            Debug.Log("[EnhancedMaterialController] Tree materials enhanced");
        }

        void EnhanceBarkMaterial(Material mat)
        {
            // Generate bark normal map
            Texture2D normalMap = GenerateProceduralNormalMap(256, 256, 1.0f);

            if (mat.HasProperty("_BumpMap"))
            {
                mat.SetTexture("_BumpMap", normalMap);
                mat.SetFloat("_BumpScale", barkNormalStrength);
            }

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", 0.2f); // Rough bark
            }
        }

        void EnhanceLeafMaterial(Material mat)
        {
            // Enable translucency for leaves
            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", 0.3f);
            }

            // Add subsurface scattering approximation
            if (mat.HasProperty("_BaseColor"))
            {
                Color baseColor = mat.GetColor("_BaseColor");
                baseColor.a = 0.9f; // Slight transparency
                mat.SetColor("_BaseColor", baseColor);
            }

            // Enable alpha blending if shader supports it
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1); // Transparent
                mat.SetFloat("_Blend", 0); // Alpha blend
            }
        }

        Texture2D GenerateProceduralNormalMap(int width, int height, float strength)
        {
            Texture2D normalMap = new Texture2D(width, height, TextureFormat.RGB24, true);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Generate Perlin noise for height
                    float xCoord = (float)x / width * 10f;
                    float yCoord = (float)y / height * 10f;

                    float height_sample = Mathf.PerlinNoise(xCoord, yCoord);
                    float height_sample_dx = Mathf.PerlinNoise(xCoord + 0.01f, yCoord) - height_sample;
                    float height_sample_dy = Mathf.PerlinNoise(xCoord, yCoord + 0.01f) - height_sample;

                    // Convert height gradient to normal
                    Vector3 normal = new Vector3(-height_sample_dx * strength, -height_sample_dy * strength, 1f).normalized;

                    // Convert to normal map color (0-1 range)
                    Color normalColor = new Color(
                        normal.x * 0.5f + 0.5f,
                        normal.y * 0.5f + 0.5f,
                        normal.z * 0.5f + 0.5f
                    );

                    normalMap.SetPixel(x, y, normalColor);
                }
            }

            normalMap.Apply();
            return normalMap;
        }

        void OnValidate()
        {
            if (Application.isPlaying)
            {
                ApplyEnhancements();
            }
        }
    }
}
