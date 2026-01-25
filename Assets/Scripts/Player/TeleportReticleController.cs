using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Controls the teleportation reticle to always show it (even when invalid)
    /// and change its color based on validity
    /// </summary>
    [RequireComponent(typeof(XRRayInteractor))]
    public class TeleportReticleController : MonoBehaviour
    {
        [Header("Reticle Settings")]
        [Tooltip("Valid teleport location color (green)")]
        public Color validColor = new Color(0.3f, 1f, 0.3f, 0.9f);

        [Tooltip("Invalid teleport location color (red)")]
        public Color invalidColor = new Color(1f, 0.3f, 0.3f, 0.9f);

        [Tooltip("Reticle size")]
        public float reticleSize = 0.15f;

        [Tooltip("Show debug logs")]
        public bool showDebug = false;

        private XRRayInteractor rayInteractor;
        private XRInteractorLineVisual lineVisual;
        private GameObject reticle;
        private Renderer reticleRenderer;
        private Material reticleMaterial;

        private void Awake()
        {
            rayInteractor = GetComponent<XRRayInteractor>();
            lineVisual = GetComponent<XRInteractorLineVisual>();
        }

        private void Start()
        {
            CreateReticle();
        }

        private void CreateReticle()
        {
            // Create a custom reticle GameObject
            reticle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            reticle.name = "TeleportReticle";
            reticle.transform.SetParent(transform);

            // Flatten cylinder to make a disc
            reticle.transform.localScale = new Vector3(reticleSize, 0.01f, reticleSize);

            // Remove collider
            Destroy(reticle.GetComponent<Collider>());

            // Setup material
            reticleRenderer = reticle.GetComponent<Renderer>();
            reticleMaterial = new Material(Shader.Find("Unlit/Color"));
            reticleMaterial.color = validColor;
            reticleRenderer.material = reticleMaterial;
            reticleRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            reticleRenderer.receiveShadows = false;

            // Start hidden until we have a raycast hit
            reticle.SetActive(false);

            if (showDebug)
                Debug.Log("[TeleportReticleController] Created teleport reticle");
        }

        private void Update()
        {
            if (reticle == null || rayInteractor == null)
                return;

            // Check if ray interactor has a hit
            bool hasHit = rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);

            if (hasHit)
            {
                // Position reticle at hit point
                reticle.transform.position = hit.point + hit.normal * 0.01f; // Slightly above surface
                reticle.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                // Determine if this is a valid teleport location
                // Check if the hit object has a TeleportationArea or TeleportationAnchor component
                bool isValid = hit.collider.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea>() != null ||
                               hit.collider.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor>() != null;

                // Update color based on validity
                Color targetColor = isValid ? validColor : invalidColor;
                reticleMaterial.color = Color.Lerp(reticleMaterial.color, targetColor, Time.deltaTime * 10f);

                // Always show reticle when we have a hit
                if (!reticle.activeSelf)
                {
                    reticle.SetActive(true);
                    if (showDebug)
                        Debug.Log($"[TeleportReticleController] Showing reticle at {hit.point}, valid: {isValid}");
                }
            }
            else
            {
                // No hit - hide reticle
                if (reticle.activeSelf)
                {
                    reticle.SetActive(false);
                    if (showDebug)
                        Debug.Log("[TeleportReticleController] Hiding reticle - no raycast hit");
                }
            }
        }

        private void OnDestroy()
        {
            if (reticle != null)
            {
                Destroy(reticle);
            }
        }
    }
}
