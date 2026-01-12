using UnityEngine;
using UnityEditor;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEditor.SceneManagement;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// CRITICAL FIX: Properly bind input action references for XRI 3.0+
    /// This is the ROOT CAUSE of head tracking, movement, and snap turn not working
    /// </summary>
    public static class FixXRInputBindings
    {
        [MenuItem("Tools/VR Dungeon Crawler/Fix XR Input Bindings (CRITICAL)")]
        public static void Fix()
        {
            Debug.Log("========================================");
            Debug.Log("CRITICAL FIX: Binding XR Input Actions");
            Debug.Log("========================================");

            // Find XR Origin
            XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("XROrigin not found!");
                return;
            }

            // Load the input action asset
            string assetPath = "Assets/Samples/XR Interaction Toolkit/3.3.1/Starter Assets/XRI Default Input Actions.inputactions";
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);
            if (inputActions == null)
            {
                Debug.LogError($"Could not load input action asset at {assetPath}");
                return;
            }

            Debug.Log($"✓ Loaded input action asset: {inputActions.name}");

            // 1. FIX CONTINUOUS MOVE PROVIDER
            var moveProvider = xrOrigin.GetComponent<ContinuousMoveProvider>();
            if (moveProvider != null)
            {
                Debug.Log("Fixing ContinuousMoveProvider input bindings...");

                // Set move speed to 20 (4x default)
                moveProvider.moveSpeed = 20f;
                moveProvider.enableStrafe = true;

                // Find the locomotion action map
                var locomotionMap = inputActions.FindActionMap("XRI RightHand Locomotion");
                if (locomotionMap != null)
                {
                    var moveAction = locomotionMap.FindAction("Move");
                    if (moveAction != null)
                    {
                        // Create XRInputValueReader and set the action directly
                        var rightHandInput = moveProvider.rightHandMoveInput;
                        rightHandInput.inputAction = moveAction;
                        moveProvider.rightHandMoveInput = rightHandInput;

                        Debug.Log($"✓ ContinuousMoveProvider RIGHT hand input bound to '{moveAction.name}'");
                    }
                }

                var leftLocomotionMap = inputActions.FindActionMap("XRI LeftHand Locomotion");
                if (leftLocomotionMap != null)
                {
                    var leftMoveAction = leftLocomotionMap.FindAction("Move");
                    if (leftMoveAction != null)
                    {
                        var leftHandInput = moveProvider.leftHandMoveInput;
                        leftHandInput.inputAction = leftMoveAction;
                        moveProvider.leftHandMoveInput = leftHandInput;

                        Debug.Log($"✓ ContinuousMoveProvider LEFT hand input bound to '{leftMoveAction.name}'");
                    }
                }

                EditorUtility.SetDirty(moveProvider);
                Debug.Log($"✓ ContinuousMoveProvider configured (speed={moveProvider.moveSpeed})");
            }
            else
            {
                Debug.LogError("ContinuousMoveProvider not found!");
            }

            // 2. FIX SNAP TURN PROVIDER
            var snapTurn = xrOrigin.GetComponent<SnapTurnProvider>();
            if (snapTurn != null)
            {
                Debug.Log("Fixing SnapTurnProvider input bindings...");

                snapTurn.turnAmount = 45f;
                snapTurn.debounceTime = 0.3f;
                snapTurn.enableTurnLeftRight = true;
                snapTurn.enableTurnAround = false;

                var locomotionMap = inputActions.FindActionMap("XRI RightHand Locomotion");
                if (locomotionMap != null)
                {
                    var turnAction = locomotionMap.FindAction("Turn");
                    if (turnAction != null)
                    {
                        var rightHandInput = snapTurn.rightHandTurnInput;
                        rightHandInput.inputAction = turnAction;
                        snapTurn.rightHandTurnInput = rightHandInput;

                        Debug.Log($"✓ SnapTurnProvider RIGHT hand input bound to '{turnAction.name}'");
                    }
                }

                var leftLocomotionMap = inputActions.FindActionMap("XRI LeftHand Locomotion");
                if (leftLocomotionMap != null)
                {
                    var leftTurnAction = leftLocomotionMap.FindAction("Turn");
                    if (leftTurnAction != null)
                    {
                        var leftHandInput = snapTurn.leftHandTurnInput;
                        leftHandInput.inputAction = leftTurnAction;
                        snapTurn.leftHandTurnInput = leftHandInput;

                        Debug.Log($"✓ SnapTurnProvider LEFT hand input bound to '{leftTurnAction.name}'");
                    }
                }

                EditorUtility.SetDirty(snapTurn);
                Debug.Log("✓ SnapTurnProvider configured");
            }
            else
            {
                Debug.LogError("SnapTurnProvider not found!");
            }

            // 3. FIX TRACKED POSE DRIVER (HEAD TRACKING)
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                var trackedPoseDriver = mainCamera.GetComponent<TrackedPoseDriver>();
                if (trackedPoseDriver != null)
                {
                    Debug.Log("Fixing TrackedPoseDriver input bindings...");

                    // Find the HMD/Head action map
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
                            // Use SerializedObject to modify TrackedPoseDriver
                            // This is necessary because TrackedPoseDriver uses InputActionProperty
                            SerializedObject serializedDriver = new SerializedObject(trackedPoseDriver);

                            // Set position input
                            SerializedProperty positionInputProp = serializedDriver.FindProperty("m_PositionInput");
                            if (positionInputProp != null)
                            {
                                SerializedProperty positionActionProp = positionInputProp.FindPropertyRelative("m_Action");
                                if (positionActionProp != null)
                                {
                                    positionActionProp.FindPropertyRelative("m_Name").stringValue = positionAction.name;
                                    positionActionProp.FindPropertyRelative("m_Id").stringValue = positionAction.id.ToString();
                                }
                            }

                            // Set rotation input
                            SerializedProperty rotationInputProp = serializedDriver.FindProperty("m_RotationInput");
                            if (rotationInputProp != null)
                            {
                                SerializedProperty rotationActionProp = rotationInputProp.FindPropertyRelative("m_Action");
                                if (rotationActionProp != null)
                                {
                                    rotationActionProp.FindPropertyRelative("m_Name").stringValue = rotationAction.name;
                                    rotationActionProp.FindPropertyRelative("m_Id").stringValue = rotationAction.id.ToString();
                                }
                            }

                            serializedDriver.ApplyModifiedProperties();
                            Debug.Log($"✓ TrackedPoseDriver bound to Position and Rotation actions");
                        }
                        else
                        {
                            Debug.LogError("Position or Rotation action not found in Head action map!");
                        }
                    }
                    else
                    {
                        Debug.LogError("Head/HMD action map not found!");
                    }

                    EditorUtility.SetDirty(trackedPoseDriver);
                }
                else
                {
                    Debug.LogError("TrackedPoseDriver not found on Main Camera!");
                }
            }
            else
            {
                Debug.LogError("Main Camera not found!");
            }

            // 4. VERIFY XR ORIGIN CAMERA REFERENCE
            if (xrOrigin.Camera == null)
            {
                xrOrigin.Camera = mainCamera;
                Debug.Log("✓ Set XROrigin.Camera reference");
            }
            else
            {
                Debug.Log($"✓ XROrigin.Camera already set to '{xrOrigin.Camera.name}'");
            }

            // 5. VERIFY TRACKING ORIGIN MODE
            if (xrOrigin.RequestedTrackingOriginMode != XROrigin.TrackingOriginMode.Floor)
            {
                xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
                Debug.Log("✓ Set tracking origin mode to Floor");
            }
            else
            {
                Debug.Log("✓ Tracking origin mode already set to Floor");
            }

            EditorUtility.SetDirty(xrOrigin);

            // Mark scene dirty
            EditorSceneManager.MarkSceneDirty(xrOrigin.gameObject.scene);

            Debug.Log("========================================");
            Debug.Log("✓✓✓ ALL INPUT BINDINGS FIXED!");
            Debug.Log("IMPORTANT: SAVE THE SCENE NOW!");
            Debug.Log("Then rebuild and test on Quest 3");
            Debug.Log("========================================");

            // Save assets
            AssetDatabase.SaveAssets();
        }
    }
}
