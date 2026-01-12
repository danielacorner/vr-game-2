using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEditor.SceneManagement;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// CRITICAL: This runs BEFORE EVERY BUILD to fix the XR Origin issues
    /// that were broken in the "dungeon1 and interaction kit" commit
    /// </summary>
    public class PreBuildXRFix : IProcessSceneWithReport
    {
        public int callbackOrder => -1000; // Run very early

        public void OnProcessScene(UnityEngine.SceneManagement.Scene scene, BuildReport report)
        {
            if (!scene.isLoaded)
                return;

            Debug.Log("========================================");
            Debug.Log($"[PreBuildXRFix] Processing scene: {scene.name}");
            Debug.Log("========================================");

            // Find XR Origin
            XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.Log("[PreBuildXRFix] No XR Origin found in scene - skipping");
                return;
            }

            bool madeChanges = false;

            // 1. FIX CAMERA REFERENCE
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                if (xrOrigin.Camera == null)
                {
                    Debug.LogWarning($"[PreBuildXRFix] FIXING: XROrigin.Camera was NULL!");
                    xrOrigin.Camera = mainCamera;
                    EditorUtility.SetDirty(xrOrigin);
                    madeChanges = true;
                }
                else
                {
                    Debug.Log($"[PreBuildXRFix] ✓ XROrigin.Camera already set to '{xrOrigin.Camera.name}'");
                }
            }

            // 2. FIX TRACKING ORIGIN MODE
            if (xrOrigin.RequestedTrackingOriginMode != XROrigin.TrackingOriginMode.Floor)
            {
                Debug.LogWarning($"[PreBuildXRFix] FIXING: Tracking mode was {xrOrigin.RequestedTrackingOriginMode}, setting to Floor");
                xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
                EditorUtility.SetDirty(xrOrigin);
                madeChanges = true;
            }
            else
            {
                Debug.Log("[PreBuildXRFix] ✓ Tracking mode is Floor");
            }

            // 3. LOAD INPUT ACTION ASSET
            string assetPath = "Assets/Samples/XR Interaction Toolkit/3.3.1/Starter Assets/XRI Default Input Actions.inputactions";
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);
            if (inputActions == null)
            {
                Debug.LogError($"[PreBuildXRFix] Could not load input action asset at {assetPath}");
                return;
            }

            // 4. FIX CONTINUOUS MOVE PROVIDER
            var moveProvider = xrOrigin.GetComponent<ContinuousMoveProvider>();
            if (moveProvider != null)
            {
                // Set speed to 20 (4x)
                if (moveProvider.moveSpeed != 20f)
                {
                    Debug.LogWarning($"[PreBuildXRFix] FIXING: moveSpeed was {moveProvider.moveSpeed}, setting to 20");
                    moveProvider.moveSpeed = 20f;
                    EditorUtility.SetDirty(moveProvider);
                    madeChanges = true;
                }
                else
                {
                    Debug.Log("[PreBuildXRFix] ✓ moveSpeed is 20");
                }

                // Bind input actions
                var rightLocomotionMap = inputActions.FindActionMap("XRI RightHand Locomotion");
                var leftLocomotionMap = inputActions.FindActionMap("XRI LeftHand Locomotion");

                if (rightLocomotionMap != null)
                {
                    var rightMoveAction = rightLocomotionMap.FindAction("Move");
                    if (rightMoveAction != null)
                    {
                        var rightHandInput = moveProvider.rightHandMoveInput;
                        rightHandInput.inputAction = rightMoveAction;
                        moveProvider.rightHandMoveInput = rightHandInput;
                        Debug.Log("[PreBuildXRFix] ✓ Bound RIGHT hand Move action");
                        EditorUtility.SetDirty(moveProvider);
                        madeChanges = true;
                    }
                }

                if (leftLocomotionMap != null)
                {
                    var leftMoveAction = leftLocomotionMap.FindAction("Move");
                    if (leftMoveAction != null)
                    {
                        var leftHandInput = moveProvider.leftHandMoveInput;
                        leftHandInput.inputAction = leftMoveAction;
                        moveProvider.leftHandMoveInput = leftHandInput;
                        Debug.Log("[PreBuildXRFix] ✓ Bound LEFT hand Move action");
                        EditorUtility.SetDirty(moveProvider);
                        madeChanges = true;
                    }
                }
            }

            // 5. FIX SNAP TURN PROVIDER
            var snapTurn = xrOrigin.GetComponent<SnapTurnProvider>();
            if (snapTurn != null)
            {
                if (snapTurn.turnAmount != 45f)
                {
                    Debug.LogWarning($"[PreBuildXRFix] FIXING: turnAmount was {snapTurn.turnAmount}, setting to 45");
                    snapTurn.turnAmount = 45f;
                    EditorUtility.SetDirty(snapTurn);
                    madeChanges = true;
                }

                // Bind input actions
                var rightLocomotionMap = inputActions.FindActionMap("XRI RightHand Locomotion");
                var leftLocomotionMap = inputActions.FindActionMap("XRI LeftHand Locomotion");

                if (rightLocomotionMap != null)
                {
                    var rightTurnAction = rightLocomotionMap.FindAction("Turn");
                    if (rightTurnAction != null)
                    {
                        var rightHandInput = snapTurn.rightHandTurnInput;
                        rightHandInput.inputAction = rightTurnAction;
                        snapTurn.rightHandTurnInput = rightHandInput;
                        Debug.Log("[PreBuildXRFix] ✓ Bound RIGHT hand Turn action");
                        EditorUtility.SetDirty(snapTurn);
                        madeChanges = true;
                    }
                }

                if (leftLocomotionMap != null)
                {
                    var leftTurnAction = leftLocomotionMap.FindAction("Turn");
                    if (leftTurnAction != null)
                    {
                        var leftHandInput = snapTurn.leftHandTurnInput;
                        leftHandInput.inputAction = leftTurnAction;
                        snapTurn.leftHandTurnInput = leftHandInput;
                        Debug.Log("[PreBuildXRFix] ✓ Bound LEFT hand Turn action");
                        EditorUtility.SetDirty(snapTurn);
                        madeChanges = true;
                    }
                }
            }

            // 6. FIX TRACKED POSE DRIVER
            if (mainCamera != null)
            {
                var trackedPoseDriver = mainCamera.GetComponent<TrackedPoseDriver>();
                if (trackedPoseDriver != null)
                {
                    var headMap = inputActions.FindActionMap("XRI Head");
                    if (headMap == null)
                    {
                        headMap = inputActions.FindActionMap("XRI HMD");
                    }

                    if (headMap != null)
                    {
                        var positionAction = headMap.FindAction("Position");
                        var rotationAction = headMap.FindAction("Rotation");

                        if (positionAction != null && rotationAction != null)
                        {
                            // Use SerializedObject to set the input action references
                            SerializedObject serializedDriver = new SerializedObject(trackedPoseDriver);

                            // Set position input
                            SerializedProperty positionInputProp = serializedDriver.FindProperty("m_PositionInput");
                            if (positionInputProp != null)
                            {
                                SerializedProperty useRefProp = positionInputProp.FindPropertyRelative("m_UseReference");
                                if (useRefProp != null && useRefProp.boolValue == false)
                                {
                                    SerializedProperty actionProp = positionInputProp.FindPropertyRelative("m_Action");
                                    if (actionProp != null)
                                    {
                                        actionProp.FindPropertyRelative("m_Name").stringValue = positionAction.name;
                                        actionProp.FindPropertyRelative("m_Id").stringValue = positionAction.id.ToString();
                                        Debug.Log("[PreBuildXRFix] ✓ Bound Position action to TrackedPoseDriver");
                                        madeChanges = true;
                                    }
                                }
                            }

                            // Set rotation input
                            SerializedProperty rotationInputProp = serializedDriver.FindProperty("m_RotationInput");
                            if (rotationInputProp != null)
                            {
                                SerializedProperty useRefProp = rotationInputProp.FindPropertyRelative("m_UseReference");
                                if (useRefProp != null && useRefProp.boolValue == false)
                                {
                                    SerializedProperty actionProp = rotationInputProp.FindPropertyRelative("m_Action");
                                    if (actionProp != null)
                                    {
                                        actionProp.FindPropertyRelative("m_Name").stringValue = rotationAction.name;
                                        actionProp.FindPropertyRelative("m_Id").stringValue = rotationAction.id.ToString();
                                        Debug.Log("[PreBuildXRFix] ✓ Bound Rotation action to TrackedPoseDriver");
                                        madeChanges = true;
                                    }
                                }
                            }

                            serializedDriver.ApplyModifiedProperties();
                            EditorUtility.SetDirty(trackedPoseDriver);
                        }
                    }
                }
            }

            if (madeChanges)
            {
                Debug.LogWarning("========================================");
                Debug.LogWarning("[PreBuildXRFix] ✓✓✓ FIXED XR CONFIGURATION!");
                Debug.LogWarning("[PreBuildXRFix] Changes will be included in build");
                Debug.LogWarning("========================================");
            }
            else
            {
                Debug.Log("========================================");
                Debug.Log("[PreBuildXRFix] All XR settings already correct");
                Debug.Log("========================================");
            }
        }
    }
}
