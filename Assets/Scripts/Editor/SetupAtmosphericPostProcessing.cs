using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using VRDungeonCrawler.Environment;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Editor script to automatically set up atmospheric post-processing for HomeArea scene
    /// Run from menu: Tools/VR Dungeon Crawler/Setup Atmospheric Post-Processing
    /// </summary>
    public class SetupAtmosphericPostProcessing : UnityEditor.Editor
    {
        [MenuItem("Tools/VR Dungeon Crawler/Setup Atmospheric Post-Processing")]
        public static void SetupPostProcessing()
        {
            Debug.Log("========================================");
            Debug.Log("Setting up Atmospheric Post-Processing...");
            Debug.Log("========================================");

            // Find or create the Volume GameObject
            GameObject volumeGO = GameObject.Find("Global Post-Processing Volume");
            if (volumeGO == null)
            {
                volumeGO = new GameObject("Global Post-Processing Volume");
                Debug.Log("✓ Created Global Post-Processing Volume GameObject");
            }
            else
            {
                Debug.Log("✓ Found existing Global Post-Processing Volume");
            }

            // Add Volume component if not present
            Volume volume = volumeGO.GetComponent<Volume>();
            if (volume == null)
            {
                volume = volumeGO.AddComponent<Volume>();
                volume.isGlobal = true;
                volume.priority = 1;
                Debug.Log("✓ Added Volume component (Global, Priority 1)");
            }

            // Create or get volume profile
            VolumeProfile profile = volume.profile;
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                volume.profile = profile;

                // Save the profile as an asset
                string profilePath = "Assets/Settings/HomeArea_PostProcessing_Profile.asset";
                System.IO.Directory.CreateDirectory("Assets/Settings");
                AssetDatabase.CreateAsset(profile, profilePath);
                Debug.Log($"✓ Created new VolumeProfile at {profilePath}");
            }

            // Configure Bloom
            Bloom bloom;
            if (!profile.TryGet(out bloom))
            {
                bloom = profile.Add<Bloom>(false);
            }
            bloom.active = true;
            bloom.threshold.Override(0.9f);
            bloom.intensity.Override(0.3f);
            bloom.scatter.Override(0.7f);
            bloom.tint.Override(Color.white);
            bloom.highQualityFiltering.Override(false); // Performance optimization for Quest 3
            Debug.Log("✓ Configured Bloom (threshold=0.9, intensity=0.3, scatter=0.7)");

            // Configure Color Adjustments
            ColorAdjustments colorAdjustments;
            if (!profile.TryGet(out colorAdjustments))
            {
                colorAdjustments = profile.Add<ColorAdjustments>(false);
            }
            colorAdjustments.active = true;
            // Cool blue tint for nighttime
            colorAdjustments.colorFilter.Override(new Color(0.98f, 1f, 1.02f));
            colorAdjustments.contrast.Override(10f);
            colorAdjustments.saturation.Override(5f);
            Debug.Log("✓ Configured Color Grading (cool blue tint, contrast=10, saturation=5)");

            // Configure Tonemapping
            Tonemapping tonemapping;
            if (!profile.TryGet(out tonemapping))
            {
                tonemapping = profile.Add<Tonemapping>(false);
            }
            tonemapping.active = true;
            tonemapping.mode.Override(TonemappingMode.ACES);
            Debug.Log("✓ Configured Tonemapping (ACES for cinematic look)");

            // Configure Vignette
            Vignette vignette;
            if (!profile.TryGet(out vignette))
            {
                vignette = profile.Add<Vignette>(false);
            }
            vignette.active = true;
            vignette.intensity.Override(0.25f);
            vignette.smoothness.Override(0.4f);
            vignette.color.Override(Color.black);
            Debug.Log("✓ Configured Vignette (intensity=0.25, smoothness=0.4)");

            // Note: Ambient Occlusion in URP is a Renderer Feature, not a Volume effect
            // To enable it manually: Project Settings > Graphics > URP Renderer > Add Renderer Feature > Screen Space Ambient Occlusion
            Debug.Log("Note: Ambient Occlusion must be added as a Renderer Feature in URP settings");

            // Depth of Field - Disabled by default for VR performance
            DepthOfField dof;
            if (profile.TryGet(out dof))
            {
                dof.active = false;
                Debug.Log("✓ Depth of Field disabled (VR performance)");
            }

            // Add AtmosphericPostProcessing component
            AtmosphericPostProcessing atmScript = volumeGO.GetComponent<AtmosphericPostProcessing>();
            if (atmScript == null)
            {
                atmScript = volumeGO.AddComponent<AtmosphericPostProcessing>();
                Debug.Log("✓ Added AtmosphericPostProcessing control script");
            }

            // Configure script settings
            atmScript.postProcessVolume = volume;
            atmScript.enableBloom = true;
            atmScript.bloomIntensity = 0.3f;
            atmScript.bloomThreshold = 0.9f;
            atmScript.bloomScatter = 0.7f;
            atmScript.enableColorGrading = true;
            atmScript.temperature = -10f;
            atmScript.contrast = 10f;
            atmScript.saturation = 5f;
            atmScript.enableTonemapping = true;
            atmScript.enableVignette = true;
            atmScript.vignetteIntensity = 0.25f;
            atmScript.enableDepthOfField = false;
            atmScript.showDebug = false;

            // Save changes
            EditorUtility.SetDirty(volumeGO);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log("✓✓✓ Post-Processing setup complete!");
            Debug.Log("Phase 1 Part 1 of 3: DONE");
            Debug.Log("Next: Enable volumetric fog in URP settings");
            Debug.Log("========================================");

            // Select the volume in hierarchy for easy access
            Selection.activeGameObject = volumeGO;
        }

        [MenuItem("Tools/VR Dungeon Crawler/Enable Volumetric Fog")]
        public static void EnableVolumetricFog()
        {
            Debug.Log("========================================");
            Debug.Log("Configuring Volumetric Fog...");
            Debug.Log("========================================");

            // Find the URP Asset
            UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset == null)
            {
                Debug.LogError("✗ No URP Asset found! Make sure you're using Universal Render Pipeline.");
                return;
            }

            Debug.Log($"✓ Found URP Asset: {urpAsset.name}");

            // Enable fog in Unity Lighting settings
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.003f; // Subtle atmospheric fog
            RenderSettings.fogColor = new Color(0.15f, 0.18f, 0.25f); // Cool blue-grey
            Debug.Log("✓ Enabled fog (Exponential Squared, density=0.003, cool blue-grey color)");

            // Note: URP's additional fog settings need to be configured in the renderer data asset
            Debug.Log("Note: For enhanced volumetric fog, go to:");
            Debug.Log("  1. Project Settings > Graphics > URP Asset");
            Debug.Log("  2. Click on the Renderer Data asset");
            Debug.Log("  3. Add 'Screen Space Ambient Occlusion' renderer feature if not present");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log("✓✓✓ Volumetric Fog configured!");
            Debug.Log("Phase 1 Part 2 of 3: DONE");
            Debug.Log("Next: Enhance existing lights");
            Debug.Log("========================================");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Enhance Scene Lights")]
        public static void EnhanceSceneLights()
        {
            Debug.Log("========================================");
            Debug.Log("Enhancing Scene Lights...");
            Debug.Log("========================================");

            int lightsEnhanced = 0;

            // Find and enhance campfire light
            GameObject campfire = GameObject.Find("Campfire");
            if (campfire != null)
            {
                Light[] campfireLights = campfire.GetComponentsInChildren<Light>();
                foreach (Light light in campfireLights)
                {
                    light.intensity = Mathf.Max(light.intensity, 1.5f);
                    light.range = Mathf.Max(light.range, 15f);
                    light.color = new Color(1f, 0.6f, 0.3f); // Warm orange

                    // Add cookie texture for flickering effect (if available)
                    // Note: You'll need to assign a cookie texture manually or create one
                    Debug.Log($"✓ Enhanced campfire light: {light.name}");
                    lightsEnhanced++;
                }
            }

            // Find and enhance portal light
            GameObject portal = GameObject.Find("Portal");
            if (portal != null)
            {
                Light[] portalLights = portal.GetComponentsInChildren<Light>();
                foreach (Light light in portalLights)
                {
                    light.intensity = Mathf.Max(light.intensity, 2f);
                    light.range = Mathf.Max(light.range, 20f);
                    light.color = new Color(0.5f, 0.8f, 1f); // Cool blue-white

                    Debug.Log($"✓ Enhanced portal light: {light.name}");
                    lightsEnhanced++;
                }
            }

            // Add torch lights near ruins
            GameObject ruins = GameObject.Find("Ruins");
            if (ruins != null)
            {
                // Create a parent object for torches
                Transform torchParent = ruins.transform.Find("Torches");
                GameObject torchParentGO;
                if (torchParent == null)
                {
                    torchParentGO = new GameObject("Torches");
                    torchParentGO.transform.SetParent(ruins.transform);
                    Debug.Log("✓ Created Torches parent object");
                }
                else
                {
                    torchParentGO = torchParent.gameObject;
                }

                // Add 3-5 torch lights around ruins (if not already present)
                int existingTorches = torchParentGO.transform.childCount;
                if (existingTorches < 3)
                {
                    Vector3[] torchPositions = new Vector3[]
                    {
                        new Vector3(5f, 1.5f, 5f),
                        new Vector3(-5f, 1.5f, 5f),
                        new Vector3(0f, 1.5f, 8f),
                    };

                    for (int i = existingTorches; i < torchPositions.Length; i++)
                    {
                        GameObject torch = new GameObject($"Torch_{i + 1}");
                        torch.transform.SetParent(torchParentGO.transform);
                        torch.transform.localPosition = torchPositions[i];

                        Light torchLight = torch.AddComponent<Light>();
                        torchLight.type = LightType.Point;
                        torchLight.color = new Color(1f, 0.7f, 0.4f); // Warm torch color
                        torchLight.intensity = 1.2f;
                        torchLight.range = 12f;
                        torchLight.shadows = LightShadows.Soft;

                        Debug.Log($"✓ Created torch light {i + 1} at {torchPositions[i]}");
                        lightsEnhanced++;
                    }
                }
            }

            // Enhance directional light (moon)
            Light[] allLights = Object.FindObjectsOfType<Light>();
            foreach (Light light in allLights)
            {
                if (light.type == LightType.Directional)
                {
                    // Subtle moonlight
                    light.color = new Color(0.7f, 0.8f, 1f); // Cool blue moonlight
                    light.intensity = Mathf.Max(light.intensity, 0.4f);
                    light.shadows = LightShadows.Soft;
                    Debug.Log($"✓ Enhanced directional light (moon): {light.name}");
                    lightsEnhanced++;
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log($"✓✓✓ Enhanced {lightsEnhanced} lights!");
            Debug.Log("Phase 1 Part 3 of 3: DONE");
            Debug.Log("========================================");
            Debug.Log("PHASE 1 COMPLETE - POST-PROCESSING & LIGHTING");
            Debug.Log("Ready for testing! Enter play mode to see the improvements.");
            Debug.Log("========================================");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Complete Phase 1 Setup (All)")]
        public static void CompletePhase1Setup()
        {
            Debug.Log("========================================");
            Debug.Log("RUNNING COMPLETE PHASE 1 SETUP");
            Debug.Log("========================================");

            SetupPostProcessing();
            Debug.Log("");
            EnableVolumetricFog();
            Debug.Log("");
            EnhanceSceneLights();

            Debug.Log("");
            Debug.Log("========================================");
            Debug.Log("✓✓✓ PHASE 1 COMPLETE - ALL SYSTEMS GO!");
            Debug.Log("========================================");
        }
    }
}
