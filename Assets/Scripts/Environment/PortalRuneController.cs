using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Animates ancient runes around portal platform
    /// Provides mystical atmosphere for portal teleportation area
    /// Runes pulse with glowing emission for mysterious effect
    /// </summary>
    public class PortalRuneController : MonoBehaviour
    {
        [Header("Rune Settings")]
        [Tooltip("Material for the runes (should have emission enabled)")]
        public Material runeMaterial;

        [Tooltip("Speed of the glow pulsing effect")]
        [Range(0.5f, 5f)]
        public float glowSpeed = 2f;

        [Tooltip("Minimum emission intensity")]
        [Range(0f, 3f)]
        public float minGlow = 0.5f;

        [Tooltip("Maximum emission intensity")]
        [Range(0f, 3f)]
        public float maxGlow = 2.0f;

        [Tooltip("Base color for rune emission (cyan for portal theme)")]
        public Color runeColor = new Color(0f, 1f, 1f);

        [Header("Rune Objects")]
        [Tooltip("Array of rune renderers to animate")]
        public Renderer[] runeRenderers;

        private float glowTime;
        private Material[] runeMaterialInstances;

        void Start()
        {
            // Auto-find rune renderers if not assigned
            if (runeRenderers == null || runeRenderers.Length == 0)
            {
                runeRenderers = GetComponentsInChildren<Renderer>();
                Debug.Log($"[PortalRuneController] Auto-found {runeRenderers.Length} rune renderers");
            }

            // Create material instances to avoid modifying shared materials
            runeMaterialInstances = new Material[runeRenderers.Length];
            for (int i = 0; i < runeRenderers.Length; i++)
            {
                if (runeRenderers[i] != null)
                {
                    // Create instance of material
                    if (runeMaterial != null)
                    {
                        runeMaterialInstances[i] = new Material(runeMaterial);
                    }
                    else
                    {
                        // Create default emissive material if none assigned
                        runeMaterialInstances[i] = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        runeMaterialInstances[i].EnableKeyword("_EMISSION");
                        runeMaterialInstances[i].SetColor("_BaseColor", new Color(runeColor.r, runeColor.g, runeColor.b, 1f));
                    }

                    runeRenderers[i].material = runeMaterialInstances[i];
                }
            }

            Debug.Log("[PortalRuneController] Rune controller initialized");
        }

        void Update()
        {
            UpdateRuneGlow();
        }

        private void UpdateRuneGlow()
        {
            // Pulse glow effect using sine wave
            glowTime += Time.deltaTime * glowSpeed;
            float glow = Mathf.Lerp(minGlow, maxGlow, (Mathf.Sin(glowTime) + 1f) / 2f);

            // Update emission for all rune materials
            for (int i = 0; i < runeMaterialInstances.Length; i++)
            {
                if (runeMaterialInstances[i] != null)
                {
                    Color emissionColor = runeColor * glow;
                    runeMaterialInstances[i].SetColor("_EmissionColor", emissionColor);
                }
            }
        }

        void OnDestroy()
        {
            // Clean up material instances
            if (runeMaterialInstances != null)
            {
                foreach (Material mat in runeMaterialInstances)
                {
                    if (mat != null)
                        Destroy(mat);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            // Visualize rune positions
            if (runeRenderers != null)
            {
                Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
                foreach (Renderer rend in runeRenderers)
                {
                    if (rend != null)
                    {
                        Gizmos.DrawWireCube(rend.transform.position, rend.bounds.size);
                    }
                }
            }
        }
    }
}
