using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using System.Collections;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Animates teleportation with a smooth transition instead of instant teleport
    /// Intercepts teleportation requests and animates the camera movement
    /// </summary>
    [RequireComponent(typeof(XROrigin))]
    public class AnimatedTeleportation : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("Duration of teleport animation in seconds")]
        [Range(0.1f, 1f)]
        public float animationDuration = 0.25f;

        [Tooltip("Animation curve for smooth transition")]
        public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private XROrigin xrOrigin;
        private TeleportationProvider teleportProvider;
        private bool isTeleporting = false;
        private Vector3 pendingTeleportPosition;
        private bool hasPendingTeleport = false;

        private void Awake()
        {
            xrOrigin = GetComponent<XROrigin>();
            teleportProvider = GetComponent<TeleportationProvider>();
        }

        private void OnEnable()
        {
            // Find all TeleportationAreas in the scene
            TeleportationArea[] teleportAreas = FindObjectsByType<TeleportationArea>(FindObjectsSortMode.None);

            foreach (var area in teleportAreas)
            {
                area.selectExited.AddListener(OnTeleportRequested);
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from all areas
            TeleportationArea[] teleportAreas = FindObjectsByType<TeleportationArea>(FindObjectsSortMode.None);

            foreach (var area in teleportAreas)
            {
                area.selectExited.RemoveListener(OnTeleportRequested);
            }
        }

        private void OnTeleportRequested(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
        {
            if (isTeleporting)
                return;

            // Get the target position from the interactor's attach transform
            var rayInteractor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor;
            if (rayInteractor != null)
            {
                // Get the hit point from the ray interactor
                if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                {
                    pendingTeleportPosition = hit.point;
                    hasPendingTeleport = true;
                }
            }
        }

        private void Update()
        {
            // Check if we have a pending teleport and start animation
            if (hasPendingTeleport && !isTeleporting)
            {
                hasPendingTeleport = false;
                StartCoroutine(AnimateTeleport(pendingTeleportPosition));
            }
        }

        private IEnumerator AnimateTeleport(Vector3 targetPosition)
        {
            isTeleporting = true;

            Vector3 startPosition = xrOrigin.transform.position;

            // Keep the same Y as current position to avoid height changes
            targetPosition.y = startPosition.y;

            float elapsedTime = 0f;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / animationDuration);
                float curveValue = animationCurve.Evaluate(t);

                // Interpolate position
                xrOrigin.transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);

                yield return null;
            }

            // Ensure final position is exact
            xrOrigin.transform.position = targetPosition;

            isTeleporting = false;
        }
    }
}
