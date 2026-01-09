using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using UnityEngine.InputSystem;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Controls player movement including base speed adjustment and dash mechanic
    /// Increases default movement speed by 1.5x
    /// Provides quick-dash on right controller B button with cooldown
    /// Both left and right joysticks control movement (inputs are combined)
    /// </summary>
    public class PlayerMovementController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Base movement speed multiplier (1.5 = 1.5x faster)")]
        public float moveSpeedMultiplier = 2f;

        [Tooltip("Enable dual joystick movement (both left and right sticks move player)")]
        public bool enableDualJoystickMovement = true;

        [Tooltip("Reference to ContinuousMoveProvider (auto-found if null)")]
        public ActionBasedContinuousMoveProvider continuousMoveProvider;

        [Header("Dash Settings")]
        [Tooltip("Dash force multiplier")]
        public float dashForce = 3f;

        [Tooltip("Dash duration in seconds")]
        public float dashDuration = 0.3f;

        [Tooltip("Dash cooldown in seconds")]
        public float dashCooldown = 1.5f;

        [Tooltip("Right hand controller reference (auto-found if null)")]
        public ActionBasedController rightController;

        [Header("Debug")]
        [Tooltip("Show debug information")]
        public bool showDebug = false;

        private float originalMoveSpeed;
        private bool isDashing = false;
        private float lastDashTime = -999f;
        private CharacterController characterController;
        private Vector3 dashDirection;

        void Start()
        {
            // Auto-find ContinuousMoveProvider if not assigned
            if (continuousMoveProvider == null)
            {
                continuousMoveProvider = FindObjectOfType<ActionBasedContinuousMoveProvider>();
                if (continuousMoveProvider == null)
                {
                    Debug.LogWarning("[PlayerMovementController] No ActionBasedContinuousMoveProvider found! Movement speed won't be adjusted.");
                }
            }

            // Get CharacterController for dash movement
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                Debug.LogWarning("[PlayerMovementController] No CharacterController found! Dash won't work.");
            }

            // Apply speed multiplier and configure dual joystick
            if (continuousMoveProvider != null)
            {
                originalMoveSpeed = continuousMoveProvider.moveSpeed;
                continuousMoveProvider.moveSpeed = originalMoveSpeed * moveSpeedMultiplier;

                if (showDebug)
                    Debug.Log($"[PlayerMovementController] Movement speed increased from {originalMoveSpeed} to {continuousMoveProvider.moveSpeed}");

                // Disable ContinuousMoveProvider if using dual joystick (we handle movement manually)
                if (enableDualJoystickMovement)
                {
                    continuousMoveProvider.enabled = false;
                    if (showDebug)
                        Debug.Log("[PlayerMovementController] ContinuousMoveProvider disabled - using dual joystick movement");
                }
            }

            // Auto-find right controller if not assigned
            if (rightController == null)
            {
                ActionBasedController[] controllers = FindObjectsOfType<ActionBasedController>();
                foreach (var controller in controllers)
                {
                    if (controller.name.ToLower().Contains("right"))
                    {
                        rightController = controller;
                        break;
                    }
                }

                if (rightController == null)
                {
                    Debug.LogWarning("[PlayerMovementController] Right controller not found! Dash won't work.");
                }
            }

            // Setup dash input
            SetupDashInput();
        }

        void SetupDashInput()
        {
            // Listen for B button input using XR Input
            // B button is OVRInput.Button.Two on right controller
            if (showDebug)
                Debug.Log("[PlayerMovementController] Dash input setup complete. Press right controller B button to dash.");
        }

        void Update()
        {
            // Check for B button press (using XR input)
            CheckDashInput();

            // Apply dual joystick movement
            if (enableDualJoystickMovement && !isDashing)
            {
                ApplyDualJoystickMovement();
            }

            // Apply dash movement
            if (isDashing)
            {
                ApplyDashMovement();
            }
        }

        void ApplyDualJoystickMovement()
        {
            if (characterController == null || continuousMoveProvider == null) return;

            // Get joystick input from both controllers
            Vector2 leftStickInput = GetJoystickInput(XRNode.LeftHand);
            Vector2 rightStickInput = GetJoystickInput(XRNode.RightHand);

            // Combine inputs (take the larger magnitude input, or add them)
            Vector2 combinedInput = leftStickInput + rightStickInput;

            // Clamp to prevent overly fast movement
            combinedInput = Vector2.ClampMagnitude(combinedInput, 1f);

            if (combinedInput.magnitude > 0.1f)
            {
                // Get camera for direction
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    // Calculate movement direction based on camera forward/right
                    Vector3 forward = mainCamera.transform.forward;
                    Vector3 right = mainCamera.transform.right;

                    // Flatten to XZ plane
                    forward.y = 0;
                    right.y = 0;
                    forward.Normalize();
                    right.Normalize();

                    // Calculate movement vector
                    Vector3 moveDirection = (forward * combinedInput.y + right * combinedInput.x);

                    // Apply movement
                    float moveSpeed = continuousMoveProvider.moveSpeed;
                    Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;

                    characterController.Move(movement);

                    if (showDebug && Time.frameCount % 60 == 0)
                    {
                        Debug.Log($"[PlayerMovementController] Dual joystick movement: L({leftStickInput.x:F2},{leftStickInput.y:F2}) R({rightStickInput.x:F2},{rightStickInput.y:F2})");
                    }
                }
            }
        }

        Vector2 GetJoystickInput(XRNode node)
        {
            UnityEngine.XR.InputDevice device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(node);
            if (device.isValid)
            {
                if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out Vector2 joystickValue))
                {
                    return joystickValue;
                }
            }
            return Vector2.zero;
        }

        void CheckDashInput()
        {
            // Check if B button is pressed on right controller
            // For Oculus Quest, B button is the secondary button on right controller
            bool bButtonPressed = false;

            // Try multiple input methods for compatibility
            UnityEngine.XR.InputDevice rightHandDevice = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (rightHandDevice.isValid)
            {
                // Check secondary button (B button on Oculus)
                if (rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool secondaryButton))
                {
                    bButtonPressed = secondaryButton;
                }
            }

            // Trigger dash if button pressed and not on cooldown
            if (bButtonPressed && !isDashing && Time.time >= lastDashTime + dashCooldown)
            {
                StartDash();
            }
        }

        void StartDash()
        {
            if (characterController == null) return;

            isDashing = true;
            lastDashTime = Time.time;

            // Get current movement direction from controller input
            dashDirection = GetDashDirection();

            if (showDebug)
                Debug.Log($"[PlayerMovementController] Dash started! Direction: {dashDirection}");

            // Optional: Add dash visual/audio feedback here
            StartCoroutine(EndDashAfterDuration());
        }

        Vector3 GetDashDirection()
        {
            // Get the direction the player is moving or looking
            Vector3 direction = Vector3.zero;

            // Try to get current movement input from ContinuousMoveProvider
            if (continuousMoveProvider != null)
            {
                // Get the camera forward direction for dash
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    // Dash forward in the direction the player is looking (XZ plane only)
                    direction = mainCamera.transform.forward;
                    direction.y = 0;
                    direction.Normalize();
                }
            }

            // If no direction, dash forward relative to play space
            if (direction.magnitude < 0.1f)
            {
                direction = transform.forward;
                direction.y = 0;
                direction.Normalize();
            }

            return direction;
        }

        void ApplyDashMovement()
        {
            if (characterController == null || dashDirection.magnitude < 0.1f) return;

            // Apply dash force
            float normalSpeed = continuousMoveProvider != null ? continuousMoveProvider.moveSpeed : 2f;
            Vector3 dashVelocity = dashDirection * normalSpeed * dashForce;

            characterController.Move(dashVelocity * Time.deltaTime);
        }

        System.Collections.IEnumerator EndDashAfterDuration()
        {
            yield return new WaitForSeconds(dashDuration);

            isDashing = false;

            if (showDebug)
                Debug.Log($"[PlayerMovementController] Dash ended. Cooldown for {dashCooldown}s");
        }

        void OnGUI()
        {
            if (!showDebug) return;

            // Show dash cooldown on screen
            float cooldownRemaining = Mathf.Max(0, dashCooldown - (Time.time - lastDashTime));
            if (cooldownRemaining > 0)
            {
                GUI.Label(new Rect(10, 10, 300, 30), $"Dash Cooldown: {cooldownRemaining:F1}s");
            }
            else if (!isDashing)
            {
                GUI.Label(new Rect(10, 10, 300, 30), "Dash Ready! (Press Right B)");
            }
            else
            {
                GUI.Label(new Rect(10, 10, 300, 30), "DASHING!");
            }
        }

        // Public method to manually trigger dash (for testing)
        [ContextMenu("Test Dash")]
        public void TestDash()
        {
            if (!isDashing && Time.time >= lastDashTime + dashCooldown)
            {
                StartDash();
            }
        }
    }
}
