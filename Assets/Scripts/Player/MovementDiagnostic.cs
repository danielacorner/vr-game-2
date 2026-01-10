using UnityEngine;
using UnityEngine.XR;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Diagnostic tool to check why dual joystick movement might not be working
    /// Add to XR Origin and check Console for detailed information
    /// </summary>
    public class MovementDiagnostic : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("=== MOVEMENT DIAGNOSTIC START ===");

            // Check CharacterController
            var cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                Debug.Log("✓ CharacterController found");
            }
            else
            {
                Debug.LogError("✗ CharacterController MISSING - dual joystick won't work!");
            }

            // Check PlayerMovementController
            var pmc = GetComponent<PlayerMovementController>();
            if (pmc != null)
            {
                Debug.Log("✓ PlayerMovementController found");
                Debug.Log($"  - Dual Joystick Enabled: {pmc.enableDualJoystickMovement}");
                Debug.Log($"  - Move Speed Multiplier: {pmc.moveSpeedMultiplier}");
            }
            else
            {
                Debug.LogError("✗ PlayerMovementController MISSING!");
            }

            // Check for ActionBasedContinuousMoveProvider
            var moveProvider = GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedContinuousMoveProvider>();
            if (moveProvider != null)
            {
                Debug.Log("✓ ActionBasedContinuousMoveProvider found");
                Debug.Log($"  - Enabled: {moveProvider.enabled}");
            }
            else
            {
                Debug.LogWarning("⚠ ActionBasedContinuousMoveProvider not found");
            }

            Debug.Log("=== MOVEMENT DIAGNOSTIC END ===");
        }

        void Update()
        {
            // Check joystick input every 60 frames
            if (Time.frameCount % 60 == 0)
            {
                Vector2 leftStick = GetJoystickInput(XRNode.LeftHand);
                Vector2 rightStick = GetJoystickInput(XRNode.RightHand);

                if (leftStick.magnitude > 0.1f || rightStick.magnitude > 0.1f)
                {
                    Debug.Log($"[Diagnostic] Joysticks - Left: ({leftStick.x:F2}, {leftStick.y:F2}) | Right: ({rightStick.x:F2}, {rightStick.y:F2})");
                }
            }
        }

        Vector2 GetJoystickInput(XRNode node)
        {
            UnityEngine.XR.InputDevice device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(node);
            if (device.isValid)
            {
                if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out Vector2 value))
                {
                    return value;
                }
            }
            return Vector2.zero;
        }
    }
}
