using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Debug script to monitor snap turn input and diagnose issues
    /// Attach to XR Origin to see what's happening with snap turn
    /// </summary>
    public class SnapTurnDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [Tooltip("Show detailed input logs")]
        public bool showInputLogs = true;

        [Tooltip("Show logs every frame")]
        public bool verboseLogging = false;

        private ActionBasedSnapTurnProvider snapTurnProvider;
        private ActionBasedContinuousMoveProvider moveProvider;
        private InputAction snapTurnAction;
        private float lastLogTime;
        private float logInterval = 1f;

        void Start()
        {
            snapTurnProvider = GetComponent<ActionBasedSnapTurnProvider>();
            moveProvider = GetComponent<ActionBasedContinuousMoveProvider>();

            if (snapTurnProvider == null)
            {
                Debug.LogError("[SnapTurnDebugger] ❌ ActionBasedSnapTurnProvider not found on XR Origin!");
                return;
            }

            if (moveProvider == null)
            {
                Debug.LogWarning("[SnapTurnDebugger] ⚠ ActionBasedContinuousMoveProvider not found");
            }

            // Get the snap turn action
            if (snapTurnProvider.rightHandSnapTurnAction != null)
            {
                snapTurnAction = snapTurnProvider.rightHandSnapTurnAction.action;

                if (snapTurnAction != null)
                {
                    Debug.Log($"[SnapTurnDebugger] ✓ Snap turn action found: {snapTurnAction.name}");
                    Debug.Log($"[SnapTurnDebugger] Action enabled: {snapTurnAction.enabled}");
                    Debug.Log($"[SnapTurnDebugger] Action bindings: {snapTurnAction.bindings.Count}");

                    // Enable the action if not already enabled
                    if (!snapTurnAction.enabled)
                    {
                        Debug.Log("[SnapTurnDebugger] Enabling snap turn action...");
                        snapTurnAction.Enable();
                    }
                }
                else
                {
                    Debug.LogError("[SnapTurnDebugger] ❌ Snap turn action is null!");
                }
            }
            else
            {
                Debug.LogError("[SnapTurnDebugger] ❌ rightHandSnapTurnAction property is null!");
            }

            // Log provider settings
            Debug.Log("[SnapTurnDebugger] === Snap Turn Provider Settings ===");
            Debug.Log($"  Turn amount: {snapTurnProvider.turnAmount}°");
            Debug.Log($"  Debounce time: {snapTurnProvider.debounceTime}s");
            Debug.Log($"  Enable turn left/right: {snapTurnProvider.enableTurnLeftRight}");
            Debug.Log($"  Enable turn around: {snapTurnProvider.enableTurnAround}");
            Debug.Log($"  Provider enabled: {snapTurnProvider.enabled}");

            if (moveProvider != null)
            {
                Debug.Log("[SnapTurnDebugger] === Move Provider Settings ===");
                Debug.Log($"  Move speed: {moveProvider.moveSpeed}m/s");
                Debug.Log($"  Enable strafe: {moveProvider.enableStrafe}");
                Debug.Log($"  Provider enabled: {moveProvider.enabled}");
            }
        }

        void Update()
        {
            if (!showInputLogs || snapTurnAction == null)
                return;

            // Read snap turn input value
            Vector2 turnInput = snapTurnAction.ReadValue<Vector2>();

            // Log periodically or when there's input
            bool shouldLog = verboseLogging ||
                           (Time.time - lastLogTime > logInterval) ||
                           (turnInput.magnitude > 0.1f);

            if (shouldLog && (verboseLogging || turnInput.magnitude > 0.1f))
            {
                Debug.Log($"[SnapTurnDebugger] Snap turn input: {turnInput} (magnitude: {turnInput.magnitude:F2})");

                if (turnInput.x < -0.5f)
                    Debug.Log("[SnapTurnDebugger] >>> LEFT TURN DETECTED <<<");
                else if (turnInput.x > 0.5f)
                    Debug.Log("[SnapTurnDebugger] >>> RIGHT TURN DETECTED <<<");

                lastLogTime = Time.time;
            }

            // Check if action is still enabled
            if (Time.time - lastLogTime > 5f && !verboseLogging)
            {
                Debug.Log($"[SnapTurnDebugger] Status check - Action enabled: {snapTurnAction.enabled}, Provider enabled: {snapTurnProvider.enabled}");
                lastLogTime = Time.time;
            }
        }

        void OnEnable()
        {
            if (snapTurnAction != null && !snapTurnAction.enabled)
            {
                snapTurnAction.Enable();
                Debug.Log("[SnapTurnDebugger] Re-enabled snap turn action on component enable");
            }
        }
    }
}
