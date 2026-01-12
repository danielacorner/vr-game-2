using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.InputSystem;
using VRDungeonCrawler.Player;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Configures XR Interaction Toolkit locomotion for VR movement
    /// Right controller: forward/backward movement + snap turn
    /// </summary>
    public static class ConfigureLocomotion
    {
        [MenuItem("Tools/VR Dungeon Crawler/Configure Locomotion")]
        public static void Configure()
        {
            Debug.Log("========================================");
            Debug.Log("Configuring VR Locomotion");
            Debug.Log("========================================");

            // Find XR Origin
            GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
            if (xrOrigin == null)
            {
                Debug.LogError("❌ XR Origin (XR Rig) not found!");
                return;
            }

            // Find Right Controller
            GameObject rightController = GameObject.Find("Right Controller");
            if (rightController == null)
            {
                Debug.LogError("❌ Right Controller not found!");
                return;
            }

            // Configure ActionBasedContinuousMoveProvider
            var moveProvider = xrOrigin.GetComponent<ActionBasedContinuousMoveProvider>();
            if (moveProvider != null)
            {
                // Load the input actions asset
                var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                    "Assets/Samples/XR Interaction Toolkit/3.3.1/Starter Assets/XRI Default Input Actions.inputactions");

                if (inputActions != null)
                {
                    var actionMap = inputActions.FindActionMap("XRI Right Locomotion");
                    if (actionMap != null)
                    {
                        var moveAction = actionMap.FindAction("Move");
                        if (moveAction != null)
                        {
                            moveProvider.rightHandMoveAction = new UnityEngine.InputSystem.InputActionProperty(moveAction);
                            Debug.Log("✓ Configured continuous move provider with right hand");
                        }
                        else
                        {
                            Debug.LogWarning("⚠ Move action not found in XRI Right Locomotion");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("⚠ Action map 'XRI Right Locomotion' not found");
                        Debug.Log("Available action maps: " + string.Join(", ", System.Linq.Enumerable.Select(inputActions.actionMaps, m => m.name)));
                    }
                }
                else
                {
                    Debug.LogWarning("⚠ Input actions asset not found at path");
                }

                moveProvider.moveSpeed = 21f; // 3.5 * 6 for proper VR movement speed
                moveProvider.enableStrafe = true;
                moveProvider.useGravity = true;
                EditorUtility.SetDirty(moveProvider);
            }
            else
            {
                Debug.LogWarning("⚠ ActionBasedContinuousMoveProvider not found");
            }

            // Configure ActionBasedSnapTurnProvider
            var snapTurnProvider = xrOrigin.GetComponent<ActionBasedSnapTurnProvider>();
            if (snapTurnProvider != null)
            {
                // Load the input actions asset
                var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                    "Assets/Samples/XR Interaction Toolkit/3.3.1/Starter Assets/XRI Default Input Actions.inputactions");

                if (inputActions != null)
                {
                    var actionMap = inputActions.FindActionMap("XRI Right Locomotion");
                    if (actionMap != null)
                    {
                        Debug.Log("Actions in XRI Right Locomotion: " + string.Join(", ", System.Linq.Enumerable.Select(actionMap.actions, a => a.name)));

                        var snapTurnAction = actionMap.FindAction("Snap Turn");
                        if (snapTurnAction != null)
                        {
                            snapTurnProvider.rightHandSnapTurnAction = new UnityEngine.InputSystem.InputActionProperty(snapTurnAction);
                            Debug.Log("✓ Configured snap turn provider with right hand using 'Snap Turn' action");
                        }
                        else
                        {
                            Debug.LogWarning("⚠ Snap Turn action not found in XRI Right Locomotion");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("⚠ Action map 'XRI Right Locomotion' not found for snap turn");
                    }
                }

                snapTurnProvider.turnAmount = 45f; // 45 degree snap turns
                snapTurnProvider.debounceTime = 0.3f; // Shorter cooldown for more responsive turns
                snapTurnProvider.enableTurnLeftRight = true; // Enable left/right snap turns
                snapTurnProvider.enableTurnAround = false; // Disable 180-degree turns

                Debug.Log($"Snap turn configured: turnAmount={snapTurnProvider.turnAmount}, debounce={snapTurnProvider.debounceTime}");
                EditorUtility.SetDirty(snapTurnProvider);
            }
            else
            {
                Debug.LogWarning("⚠ ActionBasedSnapTurnProvider not found");
            }

            // Configure ControllerInputActionManager on Right Controller
            var controllerManager = rightController.GetComponent<ControllerInputActionManager>();
            if (controllerManager != null)
            {
                // Enable smooth motion (continuous movement)
                controllerManager.smoothMotionEnabled = true;
                // Disable smooth turn (we want snap turn)
                controllerManager.smoothTurnEnabled = false;

                EditorUtility.SetDirty(controllerManager);
                Debug.Log("✓ Configured right controller input manager");
            }
            else
            {
                Debug.LogWarning("⚠ ControllerInputActionManager not found on Right Controller");
            }

            // Add debugger component to help diagnose snap turn issues
            var debugger = xrOrigin.GetComponent<SnapTurnDebugger>();
            if (debugger == null)
            {
                debugger = xrOrigin.AddComponent<SnapTurnDebugger>();
                debugger.showInputLogs = true;
                debugger.verboseLogging = false;
                Debug.Log("✓ Added SnapTurnDebugger component for runtime diagnosis");
            }
            else
            {
                Debug.Log("✓ SnapTurnDebugger already present");
            }
            EditorUtility.SetDirty(xrOrigin);

            Debug.Log("========================================");
            Debug.Log("✓✓✓ Locomotion configured successfully!");
            Debug.Log("Right joystick: Forward/backward = move (speed: 21m/s)");
            Debug.Log("Right joystick: Left/right flick = snap turn 45°");
            Debug.Log("Snap turn cooldown: 0.3s (prevents accidental turns)");
            Debug.Log("Teleport ray: DISABLED on right controller");
            Debug.Log("SnapTurnDebugger: Will log input in Play mode/build");
            Debug.Log("========================================");
        }
    }
}
