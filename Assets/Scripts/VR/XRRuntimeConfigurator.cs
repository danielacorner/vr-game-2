using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.InputSystem;

namespace VRDungeonCrawler.VR
{
    /// <summary>
    /// Standalone GameObject that configures XR Origin at runtime
    /// NOT attached to XR Origin - separate GameObject to avoid execution issues
    /// </summary>
    public class XRRuntimeConfigurator : MonoBehaviour
    {
        [Header("Settings")]
        public float moveSpeed = 20f;
        public InputActionAsset inputActionAsset;

        private bool configured = false;
        private int frameCount = 0;

        private void Awake()
        {
            Debug.Log("==========================================");
            Debug.Log("==========================================");
            Debug.Log("[XRRuntimeConfigurator] AWAKE CALLED!");
            Debug.Log("==========================================");
            Debug.Log("==========================================");

            StartCoroutine(ConfigureXR());
        }

        private void Update()
        {
            frameCount++;

            // ALWAYS log at frame 10
            if (frameCount == 10)
            {
                Debug.LogError("========================================");
                Debug.LogError("========================================");
                Debug.LogError($"[XRRuntimeConfigurator] FRAME 10! configured={configured}");
                Debug.LogError("========================================");
                Debug.LogError("========================================");
            }

            // Force configure at frame 10 to see the logs
            if (frameCount == 10 || frameCount == 100)
            {
                Debug.LogError($"[XRRuntimeConfigurator] *** FORCING CONFIGURATION AT FRAME {frameCount} ***");
                ConfigureXRDirect();
            }

            // Log every 5 seconds and show current moveSpeed
            if (frameCount % 300 == 0)
            {
                Debug.LogError($"[XRRuntimeConfigurator] UPDATE RUNNING! Frame:{frameCount} Configured: {configured}");

                // Check actual moveSpeed in the scene
                XROrigin xrOrigin = FindObjectOfType<XROrigin>();
                if (xrOrigin != null)
                {
                    var moveProvider = xrOrigin.GetComponent<ContinuousMoveProvider>();
                    if (moveProvider != null)
                    {
                        Debug.LogError($"[XRRuntimeConfigurator] CURRENT SCENE moveSpeed: {moveProvider.moveSpeed}");
                    }
                }
            }
        }

        private void ConfigureXRDirect()
        {
            Debug.LogError("========================================");
            Debug.LogError("[XRRuntimeConfigurator] STARTING XR CONFIGURATION!");
            Debug.LogError("========================================");

            // Find XR Origin
            XROrigin xrOrigin = FindObjectOfType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("[XRRuntimeConfigurator] XROrigin not found!");
                return;
            }

            Debug.LogError($"[XRRuntimeConfigurator] Found XROrigin: {xrOrigin.gameObject.name}");

            // Find and configure movement provider
            var moveProvider = xrOrigin.GetComponent<ContinuousMoveProvider>();
            if (moveProvider != null)
            {
                float beforeSpeed = moveProvider.moveSpeed;
                Debug.LogError($"[XRRuntimeConfigurator] BEFORE: moveSpeed = {beforeSpeed}");

                moveProvider.moveSpeed = moveSpeed;

                float afterSpeed = moveProvider.moveSpeed;
                Debug.LogError($"[XRRuntimeConfigurator] AFTER: moveSpeed = {afterSpeed}");
                Debug.LogError($"[XRRuntimeConfigurator] Target was: {moveSpeed}");

                // CRITICAL: Configure input actions for movement
                if (inputActionAsset != null)
                {
                    Debug.LogError("[XRRuntimeConfigurator] Configuring input actions...");

                    // Configure RIGHT hand movement
                    var rightLocomotionMap = inputActionAsset.FindActionMap("XRI RightHand Locomotion");
                    if (rightLocomotionMap != null)
                    {
                        var rightMoveAction = rightLocomotionMap.FindAction("Move");
                        if (rightMoveAction != null)
                        {
                            Debug.LogError($"[XRRuntimeConfigurator] Found RIGHT Move action, binding it...");

                            // CRITICAL: Enable the action FIRST
                            if (!rightMoveAction.enabled)
                            {
                                rightMoveAction.Enable();
                                Debug.LogError($"[XRRuntimeConfigurator] RIGHT Move action ENABLED!");
                            }

                            // Set the input action in the provider
                            var rightHandInput = moveProvider.rightHandMoveInput;
                            rightHandInput.inputAction = rightMoveAction;
                            moveProvider.rightHandMoveInput = rightHandInput;

                            Debug.LogError($"[XRRuntimeConfigurator] RIGHT Move action bound! Action ID: {rightMoveAction.id}");
                        }
                        else
                        {
                            Debug.LogError("[XRRuntimeConfigurator] RIGHT Move action not found!");
                        }
                    }
                    else
                    {
                        Debug.LogError("[XRRuntimeConfigurator] RIGHT Locomotion action map not found!");
                    }

                    // Configure LEFT hand movement
                    var leftLocomotionMap = inputActionAsset.FindActionMap("XRI LeftHand Locomotion");
                    if (leftLocomotionMap != null)
                    {
                        var leftMoveAction = leftLocomotionMap.FindAction("Move");
                        if (leftMoveAction != null)
                        {
                            Debug.LogError($"[XRRuntimeConfigurator] Found LEFT Move action, binding it...");

                            // Enable the action
                            if (!leftMoveAction.enabled)
                            {
                                leftMoveAction.Enable();
                                Debug.LogError($"[XRRuntimeConfigurator] LEFT Move action ENABLED!");
                            }

                            var leftHandInput = moveProvider.leftHandMoveInput;
                            leftHandInput.inputAction = leftMoveAction;
                            moveProvider.leftHandMoveInput = leftHandInput;

                            Debug.LogError($"[XRRuntimeConfigurator] LEFT Move action bound! Action ID: {leftMoveAction.id}");
                        }
                        else
                        {
                            Debug.LogError("[XRRuntimeConfigurator] LEFT Move action not found!");
                        }
                    }
                    else
                    {
                        Debug.LogError("[XRRuntimeConfigurator] LEFT Locomotion action map not found!");
                    }
                }
                else
                {
                    Debug.LogError("[XRRuntimeConfigurator] inputActionAsset is NULL!");
                }

                configured = true;
            }
            else
            {
                Debug.LogError("[XRRuntimeConfigurator] ContinuousMoveProvider not found!");
            }

            // CRITICAL: Configure TrackedPoseDriver for head tracking
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                var trackedPoseDriver = mainCamera.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                if (trackedPoseDriver != null)
                {
                    Debug.LogError("[XRRuntimeConfigurator] Found TrackedPoseDriver, configuring...");

                    if (inputActionAsset != null)
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
                                Debug.LogError("[XRRuntimeConfigurator] Enabling head tracking actions...");
                                positionAction.Enable();
                                rotationAction.Enable();
                                Debug.LogError("[XRRuntimeConfigurator] Head tracking actions ENABLED!");
                            }
                            else
                            {
                                Debug.LogError("[XRRuntimeConfigurator] Position or Rotation action not found!");
                            }
                        }
                        else
                        {
                            Debug.LogError("[XRRuntimeConfigurator] Head/HMD action map not found!");
                        }
                    }
                }
                else
                {
                    Debug.LogError("[XRRuntimeConfigurator] TrackedPoseDriver not found on Main Camera!");
                }
            }
            else
            {
                Debug.LogError("[XRRuntimeConfigurator] Main Camera not found!");
            }

            // CRITICAL: Configure SnapTurnProvider for turning
            var snapTurnProvider = xrOrigin.GetComponent<SnapTurnProvider>();
            if (snapTurnProvider != null)
            {
                Debug.LogError("[XRRuntimeConfigurator] Found SnapTurnProvider, configuring...");

                if (inputActionAsset != null)
                {
                    // Configure RIGHT hand snap turn
                    var rightLocomotionMap = inputActionAsset.FindActionMap("XRI RightHand Locomotion");
                    if (rightLocomotionMap != null)
                    {
                        var rightTurnAction = rightLocomotionMap.FindAction("Turn");
                        if (rightTurnAction == null)
                        {
                            rightTurnAction = rightLocomotionMap.FindAction("Snap Turn");
                        }

                        if (rightTurnAction != null)
                        {
                            Debug.LogError("[XRRuntimeConfigurator] Found RIGHT Turn action, enabling it...");

                            if (!rightTurnAction.enabled)
                            {
                                rightTurnAction.Enable();
                                Debug.LogError("[XRRuntimeConfigurator] RIGHT Turn action ENABLED!");
                            }

                            var rightHandInput = snapTurnProvider.rightHandTurnInput;
                            rightHandInput.inputAction = rightTurnAction;
                            snapTurnProvider.rightHandTurnInput = rightHandInput;

                            Debug.LogError($"[XRRuntimeConfigurator] RIGHT Snap turn action bound! Action ID: {rightTurnAction.id}");
                        }
                        else
                        {
                            Debug.LogError("[XRRuntimeConfigurator] RIGHT Turn action not found!");
                        }
                    }

                    // Configure LEFT hand snap turn
                    var leftLocomotionMap = inputActionAsset.FindActionMap("XRI LeftHand Locomotion");
                    if (leftLocomotionMap != null)
                    {
                        var leftTurnAction = leftLocomotionMap.FindAction("Turn");
                        if (leftTurnAction == null)
                        {
                            leftTurnAction = leftLocomotionMap.FindAction("Snap Turn");
                        }

                        if (leftTurnAction != null)
                        {
                            Debug.LogError("[XRRuntimeConfigurator] Found LEFT Turn action, enabling it...");

                            if (!leftTurnAction.enabled)
                            {
                                leftTurnAction.Enable();
                                Debug.LogError("[XRRuntimeConfigurator] LEFT Turn action ENABLED!");
                            }

                            var leftHandInput = snapTurnProvider.leftHandTurnInput;
                            leftHandInput.inputAction = leftTurnAction;
                            snapTurnProvider.leftHandTurnInput = leftHandInput;

                            Debug.LogError($"[XRRuntimeConfigurator] LEFT Snap turn action bound! Action ID: {leftTurnAction.id}");
                        }
                        else
                        {
                            Debug.LogError("[XRRuntimeConfigurator] LEFT Turn action not found!");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("[XRRuntimeConfigurator] SnapTurnProvider not found!");
            }

            Debug.LogError("========================================");
            Debug.LogError("[XRRuntimeConfigurator] CONFIGURATION COMPLETE!");
            Debug.LogError("========================================");
        }

        private System.Collections.IEnumerator ConfigureXR()
        {
            yield return null; // Wait one frame

            Debug.Log("[XRRuntimeConfigurator] Starting XR configuration...");

            // Find XR Origin
            XROrigin xrOrigin = FindObjectOfType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("[XRRuntimeConfigurator] XROrigin not found!");
                yield break;
            }

            Debug.Log($"[XRRuntimeConfigurator] Found XROrigin: {xrOrigin.gameObject.name}");

            // Find and configure movement provider
            var moveProvider = xrOrigin.GetComponent<ContinuousMoveProvider>();
            if (moveProvider != null)
            {
                Debug.Log($"[XRRuntimeConfigurator] Current moveSpeed: {moveProvider.moveSpeed}");
                moveProvider.moveSpeed = moveSpeed;
                Debug.Log($"[XRRuntimeConfigurator] Set moveSpeed to: {moveSpeed}");
            }
            else
            {
                Debug.LogError("[XRRuntimeConfigurator] ContinuousMoveProvider not found!");
            }

            // Find and configure snap turn
            var snapTurn = xrOrigin.GetComponent<SnapTurnProvider>();
            if (snapTurn != null)
            {
                Debug.Log($"[XRRuntimeConfigurator] Found SnapTurnProvider");
            }

            Debug.Log("[XRRuntimeConfigurator] Configuration complete!");
        }
    }
}
