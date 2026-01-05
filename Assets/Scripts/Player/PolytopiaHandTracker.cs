using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Animates Polytopia-style fingers based on CONTROLLER SENSORS (like Half-Life Alyx)
    /// Uses Quest controller inputs: trigger, grip, and button touches
    /// </summary>
    public class PolytopiaHandTracker : MonoBehaviour
    {
        [Header("Hand Configuration")]
        public bool isLeftHand = true;

        [Header("Finger Bone References")]
        public Transform thumbRoot;
        public Transform indexRoot;
        public Transform middleRoot;
        public Transform ringRoot;
        public Transform pinkyRoot;

        [Header("Animation Settings")]
        [Range(1f, 30f)]
        public float animationSpeed = 15f; // Speed of finger curl animation
        [Range(0f, 90f)]
        public float maxFingerCurl = 75f; // Max curl angle per segment

        [Header("Debug")]
        public bool showDebugLogs = true;

        // Input values
        private float triggerValue = 0f;
        private float gripValue = 0f;
        private bool thumbTouching = false;

        // Auto-detected actions
        private InputAction triggerAction;
        private InputAction gripAction;
        private InputAction primaryButtonAction;
        private InputAction secondaryButtonAction;
        private InputAction thumbstickTouchAction;

        private bool initialized = false;
        private float initAttemptTimer = 0f;

        private void Start()
        {
            if (showDebugLogs)
                Debug.Log($"[PolytopiaHandTracker] Starting finger tracking for {(isLeftHand ? "LEFT" : "RIGHT")} hand");

            TryInitializeInputActions();
        }

        private void TryInitializeInputActions()
        {
            // Try to find XRI input actions asset
            var inputActionsAsset = Resources.Load<InputActionAsset>("XRI Default Input Actions");

            if (inputActionsAsset == null)
            {
                // Try alternative path
                inputActionsAsset = UnityEngine.Object.FindObjectOfType<UnityEngine.InputSystem.PlayerInput>()?.actions;
            }

            if (inputActionsAsset != null)
            {
                string handPrefix = isLeftHand ? "XRI LeftHand" : "XRI RightHand";

                // Find actions
                triggerAction = inputActionsAsset.FindAction($"{handPrefix}/Activate");
                gripAction = inputActionsAsset.FindAction($"{handPrefix}/Select");

                if (triggerAction != null)
                {
                    triggerAction.Enable();
                    if (showDebugLogs)
                        Debug.Log($"[PolytopiaHandTracker] ✓ Found trigger action for {(isLeftHand ? "LEFT" : "RIGHT")}");
                }

                if (gripAction != null)
                {
                    gripAction.Enable();
                    if (showDebugLogs)
                        Debug.Log($"[PolytopiaHandTracker] ✓ Found grip action for {(isLeftHand ? "LEFT" : "RIGHT")}");
                }

                initialized = (triggerAction != null || gripAction != null);
            }
            else
            {
                if (showDebugLogs)
                    Debug.LogWarning($"[PolytopiaHandTracker] Could not find XRI input actions asset. Will retry...");
            }

            // Fallback: Try to get from parent NearFarInteractor
            if (!initialized)
            {
                TryAutoDetectFromInteractor();
            }

            if (showDebugLogs)
            {
                Debug.Log($"[PolytopiaHandTracker] Initialization: Trigger={triggerAction != null}, Grip={gripAction != null}");
            }
        }

        private void TryAutoDetectFromInteractor()
        {
            var nearFarInteractor = GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor>();

            if (nearFarInteractor != null)
            {
                // Try to access activateInput (trigger) via reflection
                var activateInputProp = nearFarInteractor.GetType().GetProperty("activateInput",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (activateInputProp != null)
                {
                    var inputReader = activateInputProp.GetValue(nearFarInteractor);
                    if (inputReader != null)
                    {
                        var actionProp = inputReader.GetType().GetProperty("inputActionValue",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                        if (actionProp != null)
                        {
                            triggerAction = actionProp.GetValue(inputReader) as InputAction;
                            if (triggerAction != null)
                            {
                                triggerAction.Enable();
                                initialized = true;
                                if (showDebugLogs)
                                    Debug.Log($"[PolytopiaHandTracker] ✓ Auto-detected trigger from NearFarInteractor!");
                            }
                        }
                    }
                }

                // Try to access selectInput (grip) via reflection
                var selectInputProp = nearFarInteractor.GetType().GetProperty("selectInput",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (selectInputProp != null)
                {
                    var inputReader = selectInputProp.GetValue(nearFarInteractor);
                    if (inputReader != null)
                    {
                        var actionProp = inputReader.GetType().GetProperty("inputActionValue",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                        if (actionProp != null)
                        {
                            gripAction = actionProp.GetValue(inputReader) as InputAction;
                            if (gripAction != null)
                            {
                                gripAction.Enable();
                                initialized = true;
                                if (showDebugLogs)
                                    Debug.Log($"[PolytopiaHandTracker] ✓ Auto-detected grip from NearFarInteractor!");
                            }
                        }
                    }
                }
            }
        }

        private void Update()
        {
            // Retry initialization if needed
            if (!initialized)
            {
                initAttemptTimer += Time.deltaTime;
                if (initAttemptTimer > 2f) // Retry every 2 seconds
                {
                    initAttemptTimer = 0f;
                    TryInitializeInputActions();
                }
                return;
            }

            // Read controller inputs
            ReadControllerInputs();

            // Animate fingers based on controller sensors
            AnimateIndexFinger(triggerValue);      // Index follows trigger
            AnimateGripFingers(gripValue);          // Middle/Ring/Pinky follow grip
            AnimateThumb(thumbTouching);            // Thumb follows button touches
        }

        private void ReadControllerInputs()
        {
            // Read trigger value
            if (triggerAction != null && triggerAction.enabled)
            {
                try
                {
                    triggerValue = triggerAction.ReadValue<float>();
                }
                catch
                {
                    // Action might not be ready yet
                }
            }

            // Read grip value
            if (gripAction != null && gripAction.enabled)
            {
                try
                {
                    gripValue = gripAction.ReadValue<float>();
                }
                catch
                {
                    // Action might not be ready yet
                }
            }

            // For now, thumb stays slightly curled (no button touch detection yet)
            thumbTouching = false; // Default: thumb relaxed
        }

        private void AnimateIndexFinger(float curlAmount)
        {
            AnimateFinger(indexRoot, curlAmount, 3);
        }

        private void AnimateGripFingers(float curlAmount)
        {
            // Middle, ring, and pinky all curl together with grip
            AnimateFinger(middleRoot, curlAmount, 3);
            AnimateFinger(ringRoot, curlAmount, 3);
            AnimateFinger(pinkyRoot, curlAmount, 3);
        }

        private void AnimateThumb(bool touching)
        {
            // Thumb extends when touching buttons, curls when not
            float curlAmount = touching ? 0f : 0.3f; // Slight curl when not touching
            AnimateFinger(thumbRoot, curlAmount, 2);
        }

        private void AnimateFinger(Transform fingerRoot, float curlAmount, int segments)
        {
            if (fingerRoot == null) return;

            Transform currentSegment = fingerRoot;

            for (int i = 0; i < segments; i++)
            {
                if (currentSegment == null) break;

                // Calculate target curl angle for this segment
                // Each segment curls progressively more (more natural look)
                float segmentCurlMultiplier = 1f + (i * 0.3f); // Later segments curl more
                float targetAngle = curlAmount * maxFingerCurl * segmentCurlMultiplier;

                // Apply curl rotation around Z axis (for fingers pointing forward)
                Quaternion targetRotation = Quaternion.Euler(0, 0, -targetAngle);

                // Smooth interpolation for natural movement
                currentSegment.localRotation = Quaternion.Lerp(
                    currentSegment.localRotation,
                    targetRotation,
                    Time.deltaTime * animationSpeed
                );

                // Move to next segment
                currentSegment = currentSegment.childCount > 0 ? currentSegment.GetChild(0) : null;
            }
        }

        private void OnDrawGizmos()
        {
            // Visualize finger bones in editor
            if (thumbRoot != null) DrawFingerBones(thumbRoot, Color.red, 2);
            if (indexRoot != null) DrawFingerBones(indexRoot, Color.green, 3);
            if (middleRoot != null) DrawFingerBones(middleRoot, Color.blue, 3);
            if (ringRoot != null) DrawFingerBones(ringRoot, Color.yellow, 3);
            if (pinkyRoot != null) DrawFingerBones(pinkyRoot, Color.magenta, 3);
        }

        private void DrawFingerBones(Transform root, Color color, int segments)
        {
            Gizmos.color = color;
            Transform current = root;

            for (int i = 0; i < segments && current != null; i++)
            {
                if (current.childCount > 0)
                {
                    Transform next = current.GetChild(0);
                    Gizmos.DrawLine(current.position, next.position);
                    Gizmos.DrawWireSphere(current.position, 0.005f);
                }
                current = current.childCount > 0 ? current.GetChild(0) : null;
            }
        }

        private void OnDestroy()
        {
            // Clean up actions
            if (triggerAction != null)
                triggerAction.Disable();
            if (gripAction != null)
                gripAction.Disable();
        }
    }
}
