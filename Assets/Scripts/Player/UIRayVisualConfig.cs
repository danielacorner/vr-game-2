using UnityEngine;


namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Configures the XR ray visual for UI interaction
    /// Sets up line renderer, reticle, and visual parameters
    /// </summary>
    public class UIRayVisualConfig : MonoBehaviour
    {
        [Header("Line Settings")]
        [Tooltip("Width of the ray line")]
        public float lineWidth = 0.005f;

        [Tooltip("Color of the ray when pointing at UI")]
        public Color lineColor = new Color(1f, 0.9f, 0.4f, 0.8f); // Gold color

        [Tooltip("Color when hovering over a clickable button")]
        public Color hoverColor = new Color(0.3f, 1f, 0.3f, 0.9f); // Green

        [Header("Reticle Settings")]
        [Tooltip("Show a reticle at the end of the ray")]
        public bool showReticle = true;

        [Tooltip("Size of the reticle")]
        public float reticleSize = 0.025f;

        [Tooltip("Offset reticle toward camera to avoid z-fighting")]
        public float reticleOffset = 0.005f;

        [Tooltip("Color of the reticle cursor")]
        public Color reticleColor = Color.white;

        private UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual lineVisual;
        private LineRenderer lineRenderer;
        private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;

        void Start()
        {
            lineVisual = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual>();
            rayInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();

            if (lineVisual == null)
            {
                Debug.LogError("[UIRayVisualConfig] No XRInteractorLineVisual found!");
                enabled = false;
                return;
            }

            ConfigureLineVisual();
        }

        void ConfigureLineVisual()
        {
            // Configure the line visual parameters
            lineVisual.enabled = true;

            // Get or create line renderer
            lineRenderer = lineVisual.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                // Line visual should create it automatically, but check
                Debug.LogWarning("[UIRayVisualConfig] No LineRenderer found on XRInteractorLineVisual");
            }
            else
            {
                // Configure line renderer appearance
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = lineWidth;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = lineColor;
                lineRenderer.endColor = lineColor;

                // Use world space
                lineRenderer.useWorldSpace = true;

                Debug.Log("[UIRayVisualConfig] Line visual configured");
            }

            // Configure reticle if enabled
            if (showReticle)
            {
                SetupReticle();
            }
        }

        void SetupReticle()
        {
            // Check if reticle already exists
            Transform reticleTransform = lineVisual.transform.Find("Reticle");
            GameObject reticle;

            if (reticleTransform != null)
            {
                reticle = reticleTransform.gameObject;
            }
            else
            {
                // Create reticle GameObject
                reticle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                reticle.name = "Reticle";
                reticle.transform.SetParent(lineVisual.transform);
                reticle.transform.localScale = Vector3.one * reticleSize;

                // Remove collider
                Destroy(reticle.GetComponent<Collider>());

                // Configure material with unlit shader
                Renderer renderer = reticle.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Use Unlit/Color shader for consistent appearance
                    Material reticleMat = new Material(Shader.Find("Unlit/Color"));
                    reticleMat.color = reticleColor;
                    renderer.material = reticleMat;

                    // Disable shadows
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }

                Debug.Log("[UIRayVisualConfig] Created reticle");
            }

            reticle.SetActive(true);
        }

        void Update()
        {
            if (lineRenderer == null || rayInteractor == null)
                return;

            // Update line color based on hover state
            bool isHoveringInteractable = rayInteractor.hasHover && rayInteractor.interactablesHovered.Count > 0;

            Color targetColor = isHoveringInteractable ? hoverColor : lineColor;

            lineRenderer.startColor = Color.Lerp(lineRenderer.startColor, targetColor, Time.deltaTime * 10f);
            lineRenderer.endColor = Color.Lerp(lineRenderer.endColor, targetColor, Time.deltaTime * 10f);

            // Position reticle at ray hit point - ONLY show on UI elements
            Transform reticleTransform = lineVisual.transform.Find("Reticle");
            if (reticleTransform != null)
            {
                bool showReticle = false;
                Vector3 hitPoint = Vector3.zero;
                Vector3 hitNormal = Vector3.forward;

                // ONLY check for UI hits - don't show reticle on 3D objects
                if (rayInteractor.TryGetCurrentUIRaycastResult(out UnityEngine.EventSystems.RaycastResult uiResult))
                {
                    if (uiResult.gameObject != null && uiResult.gameObject.layer == 5) // UI layer
                    {
                        hitPoint = uiResult.worldPosition;
                        hitNormal = uiResult.worldNormal;
                        showReticle = true;
                    }
                }

                if (showReticle)
                {
                    // Offset reticle slightly toward camera to avoid z-fighting
                    Vector3 offsetPosition = hitPoint + hitNormal * reticleOffset;
                    reticleTransform.position = offsetPosition;

                    // Make reticle always face the camera (billboard effect)
                    if (Camera.main != null)
                    {
                        Vector3 dirToCamera = Camera.main.transform.position - reticleTransform.position;
                        if (dirToCamera.sqrMagnitude > 0.001f)
                        {
                            reticleTransform.rotation = Quaternion.LookRotation(-dirToCamera);
                        }
                    }

                    reticleTransform.gameObject.SetActive(true);
                }
                else
                {
                    reticleTransform.gameObject.SetActive(false);
                }
            }
        }
    }
}
