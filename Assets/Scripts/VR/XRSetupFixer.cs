using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using System.Collections;

namespace VRDungeonCrawler.VR
{
    /// <summary>
    /// All-in-one XR setup fixer - ensures camera, tracking, and input are all configured for XRI 3.0+
    /// </summary>
    [RequireComponent(typeof(XROrigin))]
    [DefaultExecutionOrder(-100)] // Run before everything else
    public class XRSetupFixer : MonoBehaviour
    {
        [Header("Settings")]
        public float moveSpeed = 20f; // 4x default speed
        public InputActionAsset inputActionAsset;

        private void Awake()
        {
            Debug.Log("========================================");
            Debug.Log("[XRSetupFixer] STARTING XR SETUP FIX");
            Debug.Log("========================================");

            StartCoroutine(FixEverything());
        }

        private IEnumerator FixEverything()
        {
            // Wait a frame for Unity to initialize
            yield return null;

            // 1. FIX CAMERA REFERENCE
            XROrigin xrOrigin = GetComponent<XROrigin>();
            Camera mainCamera = Camera.main;

            if (mainCamera != null)
            {
                xrOrigin.Camera = mainCamera;
                Debug.Log($"[XRSetupFixer] ✓ Set camera: {mainCamera.name}");
                Debug.Log($"[XRSetupFixer]   Camera position: {mainCamera.transform.position}");
                Debug.Log($"[XRSetupFixer]   Camera parent: {mainCamera.transform.parent?.name}");
            }
            else
            {
                Debug.LogError("[XRSetupFixer] ❌ Could not find Main Camera!");
            }

            // 2. SET TRACKING MODE TO FLOOR
            xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            Debug.Log($"[XRSetupFixer] ✓ Set tracking mode to Floor");

            // Wait another frame
            yield return null;

            // 3. CONFIGURE MOVEMENT PROVIDER (XRI 3.0+)
            var moveProvider = GetComponent<ContinuousMoveProvider>();
            if (moveProvider != null)
            {
                Debug.Log($"[XRSetupFixer] Found move provider, current speed: {moveProvider.moveSpeed}");

                moveProvider.moveSpeed = moveSpeed;
                moveProvider.enableStrafe = true;
                moveProvider.useGravity = true;

                Debug.Log($"[XRSetupFixer] ✓ Set move speed to {moveSpeed}");

                // Configure input action (XRI 3.0+ uses XRInputValueReader)
                if (inputActionAsset != null)
                {
                    var actionMap = inputActionAsset.FindActionMap("XRI RightHand Locomotion");
                    if (actionMap == null)
                    {
                        actionMap = inputActionAsset.FindActionMap("XRI Right Locomotion");
                    }

                    if (actionMap != null)
                    {
                        var moveAction = actionMap.FindAction("Move");
                        if (moveAction != null)
                        {
                            // XRI 3.0+ way: Set the inputAction in the XRInputValueReader
                            var rightHandInput = moveProvider.rightHandMoveInput;
                            rightHandInput.inputAction = moveAction;
                            moveProvider.rightHandMoveInput = rightHandInput;
                            Debug.Log($"[XRSetupFixer] ✓ Configured move input action (XRI 3.0+)");
                        }
                        else
                        {
                            Debug.LogError("[XRSetupFixer] ❌ Could not find 'Move' action");
                        }
                    }
                    else
                    {
                        Debug.LogError("[XRSetupFixer] ❌ Could not find locomotion action map");
                    }
                }
                else
                {
                    Debug.LogError("[XRSetupFixer] ❌ Input action asset not assigned!");
                }
            }
            else
            {
                Debug.LogError("[XRSetupFixer] ❌ ContinuousMoveProvider not found!");
            }

            // 4. CONFIGURE SNAP TURN PROVIDER (XRI 3.0+)
            var snapTurnProvider = GetComponent<SnapTurnProvider>();
            if (snapTurnProvider != null)
            {
                snapTurnProvider.turnAmount = 45f;
                snapTurnProvider.debounceTime = 0.3f;
                snapTurnProvider.enableTurnLeftRight = true;
                snapTurnProvider.enableTurnAround = false;

                Debug.Log($"[XRSetupFixer] ✓ Configured snap turn");

                // Configure input action (XRI 3.0+)
                if (inputActionAsset != null)
                {
                    var actionMap = inputActionAsset.FindActionMap("XRI RightHand Locomotion");
                    if (actionMap == null)
                    {
                        actionMap = inputActionAsset.FindActionMap("XRI Right Locomotion");
                    }

                    if (actionMap != null)
                    {
                        var snapTurnAction = actionMap.FindAction("Turn");
                        if (snapTurnAction == null)
                        {
                            snapTurnAction = actionMap.FindAction("Snap Turn");
                        }

                        if (snapTurnAction != null)
                        {
                            // XRI 3.0+ way: Set the inputAction in the XRInputValueReader
                            var rightHandInput = snapTurnProvider.rightHandTurnInput;
                            rightHandInput.inputAction = snapTurnAction;
                            snapTurnProvider.rightHandTurnInput = rightHandInput;
                            Debug.Log($"[XRSetupFixer] ✓ Configured snap turn input action (XRI 3.0+)");
                        }
                        else
                        {
                            Debug.LogWarning("[XRSetupFixer] ⚠ Could not find snap turn action");
                        }
                    }
                }
            }

            // 4. CONFIGURE TRACKED POSE DRIVER (for head tracking)
            TrackedPoseDriver trackedPoseDriver = mainCamera?.GetComponent<TrackedPoseDriver>();
            if (trackedPoseDriver != null && inputActionAsset != null)
            {
                var headMap = inputActionAsset.FindActionMap("XRI Head");
                if (headMap == null)
                {
                    headMap = inputActionAsset.FindActionMap("XRI HMD");
                }

                if (headMap != null)
                {
                    var positionAction = headMap.FindAction("Position");
                    var rotationAction = headMap.FindAction("Rotation");

                    if (positionAction != null && rotationAction != null)
                    {
                        // Enable the actions if they're not enabled
                        if (!positionAction.enabled)
                        {
                            positionAction.Enable();
                        }
                        if (!rotationAction.enabled)
                        {
                            rotationAction.Enable();
                        }

                        Debug.Log($"[XRSetupFixer] ✓ TrackedPoseDriver will use Head actions from {headMap.name}");
                    }
                    else
                    {
                        Debug.LogWarning("[XRSetupFixer] ⚠ Could not find Position or Rotation actions in Head map");
                    }
                }
                else
                {
                    Debug.LogWarning("[XRSetupFixer] ⚠ Could not find XRI Head or HMD action map");
                }
            }

            // Wait for XR to initialize
            yield return new WaitForSeconds(2f);

            // 5. FINAL STATUS CHECK
            Debug.Log("========================================");
            Debug.Log("[XRSetupFixer] FINAL STATUS:");
            Debug.Log($"  XROrigin.Camera: {xrOrigin.Camera?.name ?? "NULL"}");
            Debug.Log($"  Tracking Mode: {xrOrigin.RequestedTrackingOriginMode}");
            Debug.Log($"  Move Speed: {moveProvider?.moveSpeed ?? 0}");
            Debug.Log($"  Move Action Name: {moveProvider?.rightHandMoveInput.inputAction.name ?? "NULL"}");
            Debug.Log($"  Snap Turn Action Name: {snapTurnProvider?.rightHandTurnInput.inputAction.name ?? "NULL"}");

            if (mainCamera != null)
            {
                Debug.Log($"  Camera World Pos: {mainCamera.transform.position}");
                Debug.Log($"  Camera Local Pos: {mainCamera.transform.localPosition}");
            }
            Debug.Log("========================================");
        }
    }
}
