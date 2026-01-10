using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using VRDungeonCrawler.Environment;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Phase 6: Enhanced Materials & Shaders
    /// Applies PBR materials with procedural normal maps
    /// Run from menu: Tools/VR Dungeon Crawler/Phase 6 - Setup Enhanced Materials
    /// </summary>
    public class SetupMaterialsPhase6 : UnityEditor.Editor
    {
        [MenuItem("Tools/VR Dungeon Crawler/Phase 6 - Setup Portal Distortion Shader")]
        public static void SetupPortalShader()
        {
            Debug.Log("========================================");
            Debug.Log("Phase 6: Setting Up Portal Distortion Shader");
            Debug.Log("========================================");

            // Find portal objects
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int portalsUpdated = 0;

            Shader portalShader = Shader.Find("VRDungeonCrawler/PortalDistortion");
            if (portalShader == null)
            {
                Debug.LogError("Portal distortion shader not found! Make sure PortalDistortion.shader is in Assets/Shaders/");
                return;
            }

            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("portal") || obj.name.ToLower().Contains("vortex"))
                {
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Material portalMaterial = new Material(portalShader);
                        portalMaterial.name = "PortalDistortion_Mat";

                        // Configure portal material
                        portalMaterial.SetFloat("_DistortionStrength", 0.1f);
                        portalMaterial.SetFloat("_DistortionSpeed", 1.0f);
                        portalMaterial.SetFloat("_FresnelPower", 3.0f);
                        portalMaterial.SetColor("_FresnelColor", new Color(0.5f, 0.8f, 1f, 1f));
                        portalMaterial.SetFloat("_EmissionIntensity", 2.0f);

                        renderer.material = portalMaterial;
                        portalsUpdated++;
                        Debug.Log($"✓ Applied portal shader to: {obj.name}");
                    }
                }
            }

            if (portalsUpdated == 0)
            {
                Debug.LogWarning("No portal objects found to apply shader to");
            }
            else
            {
                Debug.Log($"✓ Portal shader applied to {portalsUpdated} objects");
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log("✓✓✓ Portal distortion shader setup complete!");
            Debug.Log("========================================");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Phase 6 - Setup Enhanced Materials")]
        public static void SetupEnhancedMaterials()
        {
            Debug.Log("========================================");
            Debug.Log("Phase 6: Setting Up Enhanced Materials");
            Debug.Log("========================================");

            // Create or find material controller
            GameObject materialsParent = GameObject.Find("EnhancedMaterials");
            if (materialsParent == null)
            {
                materialsParent = new GameObject("EnhancedMaterials");
                materialsParent.transform.position = Vector3.zero;
                Debug.Log("✓ Created EnhancedMaterials parent");
            }

            // Add control script
            EnhancedMaterialController materialController = materialsParent.GetComponent<EnhancedMaterialController>();
            if (materialController == null)
            {
                materialController = materialsParent.AddComponent<EnhancedMaterialController>();
                Debug.Log("✓ Added EnhancedMaterialController component");
            }

            // Configure settings
            materialController.enhanceTerrainMaterial = true;
            materialController.terrainNormalStrength = 1f;
            materialController.terrainRoughness = 0.8f;

            materialController.enhanceRuinsMaterials = true;
            materialController.ruinsNormalStrength = 1.5f;
            materialController.ruinsRoughness = 0.9f;

            materialController.enhanceTreeMaterials = true;
            materialController.leafTranslucency = 0.3f;
            materialController.barkNormalStrength = 1.2f;

            EditorUtility.SetDirty(materialsParent);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log("✓✓✓ Enhanced materials setup complete!");
            Debug.Log("Note: Material enhancements will apply at runtime (when you play the scene)");
            Debug.Log("========================================");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Phase 6 - Upgrade Terrain Material")]
        public static void UpgradeTerrainMaterial()
        {
            Debug.Log("========================================");
            Debug.Log("Phase 6: Upgrading Terrain Material");
            Debug.Log("========================================");

            Terrain terrain = FindObjectOfType<Terrain>();
            if (terrain == null)
            {
                Debug.LogWarning("No terrain found in scene");
                return;
            }

            // Get or create terrain material
            Material terrainMat = terrain.materialTemplate;
            if (terrainMat == null)
            {
                terrainMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                terrainMat.name = "TerrainMaterial_Enhanced";
                terrain.materialTemplate = terrainMat;
                Debug.Log("✓ Created new terrain material");
            }

            // Configure PBR properties
            terrainMat.SetFloat("_Smoothness", 0.2f); // Rough ground
            terrainMat.SetFloat("_Metallic", 0f); // Non-metallic

            if (terrainMat.HasProperty("_BaseColor"))
            {
                // Darken slightly for more realistic look
                Color baseColor = terrainMat.GetColor("_BaseColor");
                baseColor *= 0.8f;
                terrainMat.SetColor("_BaseColor", baseColor);
            }

            Debug.Log("✓ Terrain material upgraded with PBR properties");
            Debug.Log("Note: Procedural normal maps will be applied at runtime");

            EditorUtility.SetDirty(terrain);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log("✓✓✓ Terrain material upgrade complete!");
            Debug.Log("========================================");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Phase 6 - Complete Setup")]
        public static void CompletePhase6Setup()
        {
            Debug.Log("========================================");
            Debug.Log("RUNNING COMPLETE PHASE 6 SETUP");
            Debug.Log("========================================");

            SetupPortalShader();
            Debug.Log("");
            SetupEnhancedMaterials();
            Debug.Log("");
            UpgradeTerrainMaterial();

            Debug.Log("");
            Debug.Log("========================================");
            Debug.Log("✓✓✓ PHASE 6 COMPLETE!");
            Debug.Log("Portal shader applied, PBR materials enhanced!");
            Debug.Log("Play the scene to see procedural normal maps applied at runtime.");
            Debug.Log("========================================");
        }
    }
}
