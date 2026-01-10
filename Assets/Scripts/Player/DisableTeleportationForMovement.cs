using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Disables right controller teleportation system to free up joystick for movement
    /// Run this before PlayerMovementController to ensure right stick controls movement
    /// </summary>
    [DefaultExecutionOrder(-100)] // Run before other scripts
    public class DisableTeleportationForMovement : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Disable teleportation on right controller")]
        public bool disableRightTeleportation = true;

        [Tooltip("Disable continuous turn provider")]
        public bool disableTurnProvider = false;

        [Header("Debug")]
        public bool showDebug = true;

        void Awake()
        {
            if (showDebug)
                Debug.Log("[DisableTeleportation] Starting locomotion system cleanup...");

            if (disableRightTeleportation)
            {
                DisableTeleportationComponents();
            }

            if (disableTurnProvider)
            {
                DisableTurnComponents();
            }

            if (showDebug)
                Debug.Log("[DisableTeleportation] ✓ Locomotion cleanup complete");
        }

        void DisableTeleportationComponents()
        {
            // Find and disable all TeleportationProvider components
            UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider[] teleportProviders = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>();
            foreach (var provider in teleportProviders)
            {
                provider.enabled = false;
                if (showDebug)
                    Debug.Log($"[DisableTeleportation] ✓ Disabled TeleportationProvider on {provider.gameObject.name}");
            }

            // Find and disable XRRayInteractor on right controller (shows teleport ray)
            var rayInteractors = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
            foreach (var rayInteractor in rayInteractors)
            {
                if (rayInteractor.gameObject.name.ToLower().Contains("right"))
                {
                    rayInteractor.enabled = false;
                    if (showDebug)
                        Debug.Log($"[DisableTeleportation] ✓ Disabled XRRayInteractor on {rayInteractor.gameObject.name}");
                }
            }

            // Find and disable any LocomotionProvider that might be using right stick
            UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider[] locomotionProviders = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider>();
            foreach (var provider in locomotionProviders)
            {
                // Check if it's a snap turn or continuous turn provider
                if (provider is ActionBasedSnapTurnProvider || provider is ActionBasedContinuousTurnProvider)
                {
                    // We'll handle turn providers separately
                    continue;
                }

                // Disable other locomotion providers that might use right stick
                if (provider.GetType().Name.Contains("Teleport"))
                {
                    provider.enabled = false;
                    if (showDebug)
                        Debug.Log($"[DisableTeleportation] ✓ Disabled {provider.GetType().Name} on {provider.gameObject.name}");
                }
            }
        }

        void DisableTurnComponents()
        {
            // Find and disable turn providers if user wants right stick only for movement
            ActionBasedSnapTurnProvider[] snapTurnProviders = FindObjectsOfType<ActionBasedSnapTurnProvider>();
            foreach (var provider in snapTurnProviders)
            {
                provider.enabled = false;
                if (showDebug)
                    Debug.Log($"[DisableTeleportation] ✓ Disabled SnapTurnProvider on {provider.gameObject.name}");
            }

            ActionBasedContinuousTurnProvider[] continuousTurnProviders = FindObjectsOfType<ActionBasedContinuousTurnProvider>();
            foreach (var provider in continuousTurnProviders)
            {
                provider.enabled = false;
                if (showDebug)
                    Debug.Log($"[DisableTeleportation] ✓ Disabled ContinuousTurnProvider on {provider.gameObject.name}");
            }
        }
    }
}
