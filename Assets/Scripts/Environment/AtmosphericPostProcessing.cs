using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Controls cinematic post-processing effects for atmospheric quality
    /// Manages URP Volume with bloom, color grading, tonemapping, vignette, DOF, and AO
    /// Optimized for Quest 3 VR performance
    /// </summary>
    public class AtmosphericPostProcessing : MonoBehaviour
    {
        [Header("Post-Processing Volume")]
        [Tooltip("Reference to the Volume component (auto-found if null)")]
        public Volume postProcessVolume;

        [Header("Bloom Settings")]
        [Tooltip("Enable bloom for moonlight and fire glow")]
        public bool enableBloom = true;

        [Range(0f, 2f)]
        [Tooltip("Bloom intensity (0.3 recommended for subtle glow)")]
        public float bloomIntensity = 0.3f;

        [Range(0f, 10f)]
        [Tooltip("Bloom threshold (0.9 = only bright lights)")]
        public float bloomThreshold = 0.9f;

        [Range(0f, 1f)]
        [Tooltip("Bloom scatter (0.7 = natural spread)")]
        public float bloomScatter = 0.7f;

        [Header("Color Grading")]
        [Tooltip("Enable color grading for atmospheric look")]
        public bool enableColorGrading = true;

        [Range(-100f, 100f)]
        [Tooltip("Temperature shift (-10 = cool/blue night)")]
        public float temperature = -10f;

        [Range(-100f, 100f)]
        [Tooltip("Contrast adjustment (10 = more dramatic)")]
        public float contrast = 10f;

        [Range(-100f, 100f)]
        [Tooltip("Saturation adjustment (5 = slightly more vivid)")]
        public float saturation = 5f;

        [Header("Tonemapping")]
        [Tooltip("Enable ACES tonemapping for cinematic look")]
        public bool enableTonemapping = true;

        [Header("Vignette")]
        [Tooltip("Enable vignette for edge darkening")]
        public bool enableVignette = true;

        [Range(0f, 1f)]
        [Tooltip("Vignette intensity (0.25 = subtle)")]
        public float vignetteIntensity = 0.25f;

        [Header("Depth of Field")]
        [Tooltip("Enable depth of field for background blur")]
        public bool enableDepthOfField = false; // Disabled by default for VR performance

        [Range(5f, 50f)]
        [Tooltip("Focus distance in meters")]
        public float focusDistance = 30f;

        [Range(0.1f, 10f)]
        [Tooltip("Aperture (f-stop) - lower = more blur")]
        public float aperture = 5.6f;

        // Note: Ambient Occlusion in URP is configured as a Renderer Feature, not a Volume effect
        // To enable AO, go to: Project Settings > Graphics > URP Renderer > Add Renderer Feature > Screen Space Ambient Occlusion

        [Header("Debug")]
        [Tooltip("Show debug logs")]
        public bool showDebug = false;

        // Cached volume overrides
        private Bloom bloom;
        private ColorAdjustments colorAdjustments;
        private Tonemapping tonemapping;
        private Vignette vignette;
        private DepthOfField depthOfField;

        void Start()
        {
            // Auto-find Volume if not assigned
            if (postProcessVolume == null)
            {
                postProcessVolume = GetComponent<Volume>();
                if (postProcessVolume == null)
                {
                    Debug.LogError("[AtmosphericPostProcessing] No Volume component found! Please add Volume component to this GameObject.");
                    return;
                }
            }

            // Ensure volume profile exists
            if (postProcessVolume.profile == null)
            {
                Debug.LogError("[AtmosphericPostProcessing] Volume has no profile assigned!");
                return;
            }

            // Get or add volume overrides
            InitializeVolumeOverrides();

            // Apply initial settings
            ApplyAllSettings();

            if (showDebug)
                Debug.Log("[AtmosphericPostProcessing] Post-processing initialized with cinematic settings");
        }

        void InitializeVolumeOverrides()
        {
            VolumeProfile profile = postProcessVolume.profile;

            // Bloom
            if (enableBloom)
            {
                if (!profile.TryGet(out bloom))
                {
                    bloom = profile.Add<Bloom>(false);
                }
            }

            // Color Adjustments
            if (enableColorGrading)
            {
                if (!profile.TryGet(out colorAdjustments))
                {
                    colorAdjustments = profile.Add<ColorAdjustments>(false);
                }
            }

            // Tonemapping
            if (enableTonemapping)
            {
                if (!profile.TryGet(out tonemapping))
                {
                    tonemapping = profile.Add<Tonemapping>(false);
                }
            }

            // Vignette
            if (enableVignette)
            {
                if (!profile.TryGet(out vignette))
                {
                    vignette = profile.Add<Vignette>(false);
                }
            }

            // Depth of Field
            if (enableDepthOfField)
            {
                if (!profile.TryGet(out depthOfField))
                {
                    depthOfField = profile.Add<DepthOfField>(false);
                }
            }
        }

        void ApplyAllSettings()
        {
            ApplyBloomSettings();
            ApplyColorGradingSettings();
            ApplyTonemappingSettings();
            ApplyVignetteSettings();
            ApplyDepthOfFieldSettings();
        }

        void ApplyBloomSettings()
        {
            if (bloom == null) return;

            bloom.active = enableBloom;
            bloom.intensity.Override(bloomIntensity);
            bloom.threshold.Override(bloomThreshold);
            bloom.scatter.Override(bloomScatter);

            if (showDebug)
                Debug.Log($"[AtmosphericPostProcessing] Bloom applied: intensity={bloomIntensity}, threshold={bloomThreshold}");
        }

        void ApplyColorGradingSettings()
        {
            if (colorAdjustments == null) return;

            colorAdjustments.active = enableColorGrading;
            colorAdjustments.colorFilter.Override(Color.white);

            // Temperature shift (cool blue for night)
            float tempShift = temperature / 100f; // Normalize to -1 to 1 range
            colorAdjustments.colorFilter.Override(new Color(1f - Mathf.Max(0, -tempShift) * 0.2f, 1f, 1f + Mathf.Max(0, -tempShift) * 0.2f));

            colorAdjustments.contrast.Override(contrast);
            colorAdjustments.saturation.Override(saturation);

            if (showDebug)
                Debug.Log($"[AtmosphericPostProcessing] Color grading applied: temp={temperature}, contrast={contrast}");
        }

        void ApplyTonemappingSettings()
        {
            if (tonemapping == null) return;

            tonemapping.active = enableTonemapping;
            tonemapping.mode.Override(TonemappingMode.ACES);

            if (showDebug)
                Debug.Log("[AtmosphericPostProcessing] Tonemapping: ACES mode enabled");
        }

        void ApplyVignetteSettings()
        {
            if (vignette == null) return;

            vignette.active = enableVignette;
            vignette.intensity.Override(vignetteIntensity);
            vignette.smoothness.Override(0.4f); // Soft edge

            if (showDebug)
                Debug.Log($"[AtmosphericPostProcessing] Vignette applied: intensity={vignetteIntensity}");
        }

        void ApplyDepthOfFieldSettings()
        {
            if (depthOfField == null) return;

            depthOfField.active = enableDepthOfField;

            if (enableDepthOfField)
            {
                depthOfField.mode.Override(DepthOfFieldMode.Gaussian); // Better for VR performance
                depthOfField.gaussianStart.Override(focusDistance * 0.8f);
                depthOfField.gaussianEnd.Override(focusDistance);
                depthOfField.gaussianMaxRadius.Override(1f); // Subtle blur for VR

                if (showDebug)
                    Debug.Log($"[AtmosphericPostProcessing] DOF applied: focus={focusDistance}m");
            }
        }


        void OnValidate()
        {
            // Apply settings in editor when values change
            if (Application.isPlaying && postProcessVolume != null && postProcessVolume.profile != null)
            {
                ApplyAllSettings();
            }
        }

        [ContextMenu("Toggle Depth of Field")]
        public void ToggleDepthOfField()
        {
            enableDepthOfField = !enableDepthOfField;
            ApplyDepthOfFieldSettings();
            Debug.Log($"[AtmosphericPostProcessing] Depth of Field: {(enableDepthOfField ? "ENABLED" : "DISABLED")}");
        }

        [ContextMenu("Apply Performance Mode (Minimal FX)")]
        public void ApplyPerformanceMode()
        {
            // Reduce settings for better performance
            enableDepthOfField = false;
            bloomIntensity = 0.2f;
            vignetteIntensity = 0.15f;

            ApplyAllSettings();
            Debug.Log("[AtmosphericPostProcessing] Performance mode applied - minimal effects");
        }

        [ContextMenu("Apply Quality Mode (Full FX)")]
        public void ApplyQualityMode()
        {
            // Full quality settings
            enableDepthOfField = false; // Keep disabled for VR comfort
            bloomIntensity = 0.3f;
            vignetteIntensity = 0.25f;

            ApplyAllSettings();
            Debug.Log("[AtmosphericPostProcessing] Quality mode applied - full effects (DOF off for VR)");
        }
    }
}
