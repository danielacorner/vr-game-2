using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Manages hand pose switching when interacting with UI
    /// Switches to pointing pose and enables ray visual when hovering over UI elements
    /// </summary>
    public class UIHandPoseManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The HandPoseController for this hand")]
        public HandPoseController handPoseController;

        [Tooltip("The NearFarInteractor component")]
        private UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor nearFarInteractor;

        [Tooltip("The line visual component (optional - will find automatically)")]
        public UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual lineVisual;

        [Header("Settings")]
        [Tooltip("UI layer to detect (default is 5)")]
        public int uiLayer = 5;

        [Tooltip("Show debug logs")]
        public bool showDebug = false;

        private bool isHoveringUI = false;
        private HandPoseState previousPose = HandPoseState.Relaxed;

        void Start()
        {
            // Get the NearFarInteractor component
            nearFarInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor>();
            if (nearFarInteractor == null)
            {
                Debug.LogError("[UIHandPoseManager] No NearFarInteractor found! This component requires NearFarInteractor.");
                enabled = false;
                return;
            }

            Debug.Log($"[UIHandPoseManager] Found interactor: {nearFarInteractor.GetType().Name}");

            // Find line visual if not assigned
            if (lineVisual == null)
            {
                lineVisual = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual>();
            }

            // Find HandPoseController if not assigned
            if (handPoseController == null)
            {
                handPoseController = GetComponentInChildren<HandPoseController>();
            }

            if (handPoseController == null)
            {
                Debug.LogWarning("[UIHandPoseManager] No HandPoseController found. Hand pose switching will not work.");
            }

            // Configure line visual for UI interaction
            if (lineVisual != null)
            {
                // Ensure line visual is enabled when hovering UI
                lineVisual.enabled = true;

                if (showDebug)
                    Debug.Log("[UIHandPoseManager] Line visual configured for UI interaction");
            }

            if (showDebug)
                Debug.Log($"[UIHandPoseManager] Initialized on {gameObject.name}");
        }

        void Update()
        {
            if (nearFarInteractor == null || handPoseController == null)
                return;

            // Check if we're hovering over UI
            bool nowHoveringUI = IsHoveringUI();

            // Handle state changes
            if (nowHoveringUI && !isHoveringUI)
            {
                // Started hovering UI
                OnStartHoverUI();
            }
            else if (!nowHoveringUI && isHoveringUI)
            {
                // Stopped hovering UI
                OnStopHoverUI();
            }

            isHoveringUI = nowHoveringUI;

            // Control line visual visibility
            if (lineVisual != null)
            {
                // Show line when hovering UI
                lineVisual.enabled = isHoveringUI;
            }
        }

        /// <summary>
        /// Check if the ray interactor is currently hovering over a UI element
        /// </summary>
        bool IsHoveringUI()
        {
            if (nearFarInteractor == null)
                return false;

            // Check if interactor has valid hover targets
            if (nearFarInteractor.hasHover && nearFarInteractor.interactablesHovered.Count > 0)
            {
                // Check if any hovered object is on UI layer
                foreach (var interactable in nearFarInteractor.interactablesHovered)
                {
                    if (interactable != null && interactable.transform.gameObject.layer == uiLayer)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Called when we start hovering over UI
        /// </summary>
        void OnStartHoverUI()
        {
            if (handPoseController != null)
            {
                // Save current pose
                previousPose = handPoseController.currentPose;

                // Switch to pointing pose
                handPoseController.SetPose(HandPoseState.Pointing);

                if (showDebug)
                    Debug.Log($"[UIHandPoseManager] Started hovering UI - switched to Pointing pose (was {previousPose})");
            }
        }

        /// <summary>
        /// Called when we stop hovering over UI
        /// </summary>
        void OnStopHoverUI()
        {
            if (handPoseController != null)
            {
                // Restore previous pose
                handPoseController.SetPose(previousPose);

                if (showDebug)
                    Debug.Log($"[UIHandPoseManager] Stopped hovering UI - restored {previousPose} pose");
            }
        }
    }
}
