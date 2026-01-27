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
        public float moveSpeedMultiplier = 2f; // Reduced by half from 4f

        [Tooltip("Enable dual joystick movement (both left and right sticks move player)")]
        public bool enableDualJoystickMovement = true;

        [Tooltip("Reference to ContinuousMoveProvider (auto-found if null)")]
        public ActionBasedContinuousMoveProvider continuousMoveProvider;

        [Header("Snap Turn Settings")]
        [Tooltip("Angle to turn when snap-turning")]
        public float snapTurnAngle = 45f;

        [Tooltip("Deadzone for snap-turn (must move joystick this far horizontally to trigger)")]
        [Range(0.1f, 0.9f)]
        public float snapTurnDeadzone = 0.75f;

        [Tooltip("Cooldown between snap turns (prevents double snap-turns)")]
        public float snapTurnCooldown = 1.0f;

        [Tooltip("Joystick must return below this threshold before allowing another snap-turn")]
        [Range(0.05f, 0.3f)]
        public float snapTurnResetThreshold = 0.15f;

        [Tooltip("Max vertical movement allowed during snap-turn (prevents snap-turn when moving forward/back)")]
        [Range(0.1f, 0.7f)]
        public float snapTurnMaxVertical = 0.4f;

        [Header("Movement Direction Settings")]
        [Tooltip("Invert backward movement (check this if pulling back flips you around instead of moving backward)")]
        public bool invertBackwardMovement = false;

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
        private Transform cameraTransform; // Cache VR camera transform
        private float lastSnapTurnTime = -999f;
        private bool wasSnapTurnPressed = false;
        private VRDungeonCrawler.Environment.BoundaryController boundaryController;

        void Awake()
        {
            Debug.Log("========================================");
            Debug.Log("========================================");
            Debug.Log("PLAYERMOVEMENT AWAKE CALLED!!!");
            Debug.Log("GameObject: " + gameObject.name);
            Debug.Log("enableDualJoystick: " + enableDualJoystickMovement);
            Debug.Log("========================================");
            Debug.Log("========================================");

            // CRITICAL: Disable teleportation FIRST before anything else runs
            if (enableDualJoystickMovement)
            {
                StartCoroutine(DisableTeleportationSystemsDelayed());
            }
        }

        void Start()
        {
            // Find boundary controller
            boundaryController = FindObjectOfType<VRDungeonCrawler.Environment.BoundaryController>();
            if (boundaryController == null && showDebug)
            {
                Debug.LogWarning("[PlayerMovementController] No BoundaryController found - player may fall off edges!");
            }

            // Find the VR camera (should be a child of this XR Origin)
            Camera mainCamera = GetComponentInChildren<Camera>();
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
                Debug.Log($"[PlayerMovementController] Found VR camera: {mainCamera.name}");
            }
            else
            {
                Debug.LogError("[PlayerMovementController] No camera found in XR Origin children!");
                // Fallback to Camera.main
                if (Camera.main != null)
                {
                    cameraTransform = Camera.main.transform;
                    Debug.LogWarning("[PlayerMovementController] Using Camera.main as fallback");
                }
            }

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

                    // CRITICAL: Disable teleportation - use delayed coroutine for proper timing
                    StartCoroutine(DisableTeleportationSystemsDelayed());
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
                ApplySnapTurn();
            }

            // Apply dash movement
            if (isDashing)
            {
                ApplyDashMovement();
            }

            // Enforce boundary constraints
            if (boundaryController != null)
            {
                boundaryController.EnforceBoundary(transform, characterController);
            }
        }

        void ApplyDualJoystickMovement()
        {
            if (characterController == null) return;

            // Get joystick input from both controllers
            Vector2 leftStickInput = GetJoystickInput(XRNode.LeftHand);
            Vector2 rightStickInput = GetJoystickInput(XRNode.RightHand);

            // Right joystick: only use Y-axis (forward/back) for movement
            // X-axis (left/right) is reserved for snap-turn
            // Store original for snap-turn detection before zeroing
            float rightStickX = rightStickInput.x;
            rightStickInput.x = 0;

            // Apply inversion if needed (some users experience backward flipping without this)
            if (invertBackwardMovement)
            {
                rightStickInput.y = -rightStickInput.y;
                if (showDebug && Time.frameCount % 60 == 0 && Mathf.Abs(rightStickInput.y) > 0.1f)
                {
                    Debug.Log($"[PlayerMovementController] Right stick Y INVERTED: {rightStickInput.y:F2}");
                }
            }

            // Combine inputs (left stick full movement + right stick forward/back only)
            Vector2 combinedInput = leftStickInput + rightStickInput;

            // Clamp to prevent overly fast movement
            combinedInput = Vector2.ClampMagnitude(combinedInput, 1f);

            if (combinedInput.magnitude > 0.1f)
            {
                // Use cached VR camera's forward direction to track both head rotation and snap-turn
                if (cameraTransform != null)
                {
                    // Get the direction where the headset is actually looking
                    Vector3 cameraForward = cameraTransform.forward;
                    Vector3 cameraRight = cameraTransform.right;

                    // Flatten to XZ plane (ignore vertical look angle)
                    cameraForward.y = 0;
                    cameraRight.y = 0;
                    cameraForward.Normalize();
                    cameraRight.Normalize();

                    // Calculate movement vector
                    // Forward = positive Y, Backward = negative Y (both relative to headset facing direction)
                    // combinedInput.y is positive when pushing forward, negative when pulling back
                    Vector3 moveDirection = (cameraForward * combinedInput.y + cameraRight * combinedInput.x);

                    // Apply movement - use moveSpeedMultiplier as base speed if no ContinuousMoveProvider
                    float moveSpeed = continuousMoveProvider != null ? continuousMoveProvider.moveSpeed : (2f * moveSpeedMultiplier);

                    // Reduce backward speed to 60% for better VR comfort
                    if (combinedInput.y < 0)
                    {
                        moveSpeed *= 0.6f;
                    }

                    Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;

                    characterController.Move(movement);

                    // Log movement details every frame when moving backward
                    Vector2 rightStickRaw = GetJoystickInput(XRNode.RightHand);
                    if (rightStickRaw.y < -0.2f && Time.frameCount % 30 == 0)
                    {
                        Debug.Log($"========================================");
                        Debug.Log($"[BACKWARD MOVEMENT]");
                        Debug.Log($"  Right stick Y: {rightStickRaw.y:F3}");
                        Debug.Log($"  Combined Y: {combinedInput.y:F3}");
                        Debug.Log($"  Camera Forward: ({cameraForward.x:F2}, {cameraForward.z:F2})");
                        Debug.Log($"  Move Direction: ({moveDirection.x:F2}, {moveDirection.z:F2})");
                        Debug.Log($"  Player rotation: {transform.eulerAngles.y:F1}°");
                        Debug.Log($"========================================");
                    }
                }
                else
                {
                    Debug.LogWarning("[PlayerMovementController] Camera transform is NULL!");
                }
            }
        }

        void ApplySnapTurn()
        {
            // Get right joystick input for snap-turn
            Vector2 rightStickInput = GetJoystickInput(XRNode.RightHand);

            float absX = Mathf.Abs(rightStickInput.x);
            float absY = Mathf.Abs(rightStickInput.y);

            // CRITICAL: Log ALL joystick input when active to debug the flipping issue
            if ((absX > 0.3f || absY > 0.3f) && Time.frameCount % 30 == 0)
            {
                Debug.Log($"[JOYSTICK] Right stick RAW: X={rightStickInput.x:F3}, Y={rightStickInput.y:F3}");
            }

            // Reset pressed flag ONLY when joystick returns very close to center
            // This prevents double-snaps from quick back-and-forth movements
            if (absX < snapTurnResetThreshold && absY < snapTurnResetThreshold)
            {
                if (wasSnapTurnPressed)
                {
                    wasSnapTurnPressed = false;
                    Debug.Log($"[PlayerMovementController] Snap-turn RESET (joystick at center)");
                }
            }

            // CRITICAL: Completely block snap-turn if user is moving backward
            // This prevents the "backward flipping" issue
            bool isMovingBackward = rightStickInput.y < -0.2f;
            if (isMovingBackward)
            {
                // Don't even check snap-turn conditions when moving backward
                return;
            }

            // Very strict conditions for snap-turn:
            // 1. Horizontal movement is above deadzone threshold (0.75)
            // 2. Vertical movement is LOW (below 0.4) - not trying to move forward/back
            // 3. Horizontal is 3x larger than vertical (very intentional left/right)
            // 4. Not already pressed (prevents holding to spam)
            // 5. Cooldown has elapsed (0.8s minimum between snaps)
            bool horizontalAboveDeadzone = absX > snapTurnDeadzone;
            bool verticalIsLow = absY < snapTurnMaxVertical;
            bool horizontalMuchLargerThanVertical = absX > absY * 3f;

            float timeSinceLastSnap = Time.time - lastSnapTurnTime;
            bool cooldownElapsed = timeSinceLastSnap >= snapTurnCooldown;

            bool isIntentionalHorizontalMovement = horizontalAboveDeadzone &&
                                                   verticalIsLow &&
                                                   horizontalMuchLargerThanVertical;

            if (isIntentionalHorizontalMovement && !wasSnapTurnPressed && cooldownElapsed)
            {
                // Determine turn direction
                float turnDirection = Mathf.Sign(rightStickInput.x);
                float turnAngle = snapTurnAngle * turnDirection;

                // Rotate the XR Origin (this GameObject)
                transform.Rotate(0, turnAngle, 0);

                lastSnapTurnTime = Time.time;
                wasSnapTurnPressed = true;

                Debug.Log($"========================================");
                Debug.Log($"[SNAP-TURN TRIGGERED] Angle: {turnAngle}°");
                Debug.Log($"  Input: X={rightStickInput.x:F3}, Y={rightStickInput.y:F3}");
                Debug.Log($"  Conditions: H_deadzone={horizontalAboveDeadzone}, V_low={verticalIsLow}, H>V*3={horizontalMuchLargerThanVertical}");
                Debug.Log($"========================================");
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

        /// <summary>
        /// Disables Unity XR Toolkit teleportation systems - runs with delay to ensure XR is initialized
        /// Tries multiple times over 2 seconds to catch all components
        /// </summary>
        System.Collections.IEnumerator DisableTeleportationSystemsDelayed()
        {
            Debug.Log("[PlayerMovementController] === STARTING TELEPORTATION DISABLE (DELAYED) ===");

            // Wait for XR to initialize
            yield return new WaitForSeconds(1f);

            // Try multiple times
            for (int attempt = 0; attempt < 3; attempt++)
            {
                Debug.Log($"[PlayerMovementController] Attempt {attempt + 1}/3 to disable teleportation...");

                int disabledCount = 0;

                // Find and disable XRRayInteractor on right controller (this shows the teleport ray)
                var allRayInteractors = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>(true);
                Debug.Log($"[PlayerMovementController] Found {allRayInteractors.Length} total XRRayInteractor components");

                foreach (var rayInteractor in allRayInteractors)
                {
                    string objName = rayInteractor.gameObject.name.ToLower();
                    Debug.Log($"[PlayerMovementController] Checking XRRayInteractor on: {rayInteractor.gameObject.name}");

                    // Disable all teleport interactors (both left and right)
                    if (objName.Contains("teleport"))
                    {
                        // Disable the component
                        rayInteractor.enabled = false;

                        // Also disable the GameObject to be extra sure
                        rayInteractor.gameObject.SetActive(false);

                        disabledCount++;
                        Debug.Log($"[PlayerMovementController] ✓✓✓ DISABLED XRRayInteractor GameObject: {rayInteractor.gameObject.name}");
                    }
                }

                // Disable all TeleportationProvider components
                var teleportProviders = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>(true);
                Debug.Log($"[PlayerMovementController] Found {teleportProviders.Length} TeleportationProvider components");

                foreach (var provider in teleportProviders)
                {
                    if (provider.enabled)
                    {
                        provider.enabled = false;
                        disabledCount++;
                        Debug.Log($"[PlayerMovementController] ✓ Disabled TeleportationProvider on {provider.gameObject.name}");
                    }
                }

                // Disable turn providers - we handle snap-turn manually with proper deadzone
                var snapTurnProviders = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.ActionBasedSnapTurnProvider>(true);
                var continuousTurnProviders = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.ActionBasedContinuousTurnProvider>(true);

                foreach (var provider in snapTurnProviders)
                {
                    provider.enabled = false;
                    disabledCount++;
                    Debug.Log($"[PlayerMovementController] ✓ Disabled snap turn provider (using manual snap-turn with deadzone)");
                }

                foreach (var provider in continuousTurnProviders)
                {
                    provider.enabled = false;
                    disabledCount++;
                    Debug.Log($"[PlayerMovementController] ✓ Disabled continuous turn provider");
                }

                Debug.Log($"[PlayerMovementController] Disabled {snapTurnProviders.Length} snap turn and {continuousTurnProviders.Length} continuous turn providers");

                Debug.Log($"[PlayerMovementController] === DISABLED {disabledCount} COMPONENTS in attempt {attempt + 1} ===");

                if (disabledCount > 0)
                {
                    Debug.Log($"[PlayerMovementController] ✓✓✓ SUCCESS! Right joystick ready for movement!");
                    yield break; // Success, stop trying
                }

                // Wait before next attempt
                yield return new WaitForSeconds(0.5f);
            }

            Debug.LogWarning("[PlayerMovementController] ⚠️ WARNING: Could not find teleportation components after 3 attempts!");
            Debug.LogWarning("[PlayerMovementController] The teleport ray may still appear. Check Console for XRRayInteractor names.");
        }
    }
}
