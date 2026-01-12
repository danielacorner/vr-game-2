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
    /// MANUAL FIX: Run this before building to fix ALL XR issues
    /// </summary>
    public static class FixXROriginNow
    {
        [MenuItem("Tools/VR Dungeon Crawler/✓ FIX XR ORIGIN NOW (RUN BEFORE BUILD)", priority = 0)]
        public static void FixNow()
        {
            Debug.Log("========================================");
            Debug.Log("[FixXROriginNow] FIXING ALL XR ISSUES!");
            Debug.Log("========================================");

            // Find XR Origin
            XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("[FixXROriginNow] XROrigin not found!");
                EditorUtility.DisplayDialog("Error", "XR Origin not found in scene!", "OK");
                return;
            }

            // Load input action asset
            string assetPath = "Assets/Samples/XR Interaction Toolkit/3.3.1/Starter Assets/XRI Default Input Actions.inputactions";
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);
            if (inputActions == null)
            {
                Debug.LogError($"[FixXROriginNow] Could not load input action asset!");
                EditorUtility.DisplayDialog("Error", "Could not load XRI Default Input Actions!", "OK");
                return;
            }

            int fixCount = 0;

            // 1. FIX CAMERA REFERENCE
            Camera mainCamera = Camera.main;
            if (mainCamera != null && xrOrigin.Camera == null)
            {
                Debug.LogError($"[FixXROriginNow] ✓ FIXED: Camera reference");
                xrOrigin.Camera = mainCamera;
                EditorUtility.SetDirty(xrOrigin);
                fixCount++;
            }

            // 2. FIX TRACKING MODE
            if (xrOrigin.RequestedTrackingOriginMode != XROrigin.TrackingOriginMode.Floor)
            {
                Debug.LogError($"[FixXROriginNow] ✓ FIXED: Tracking mode to Floor");
                xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
                EditorUtility.SetDirty(xrOrigin);
                fixCount++;
            }

            // 3. FIX MOVE PROVIDER
            var moveProvider = xrOrigin.GetComponent<ContinuousMoveProvider>();
            if (moveProvider != null)
            {
                if (moveProvider.moveSpeed != 20f)
                {
                    Debug.LogError($"[FixXROriginNow] ✓ FIXED: Move speed to 20 (4x)");
                    moveProvider.moveSpeed = 20f;
                    EditorUtility.SetDirty(moveProvider);
                    fixCount++;
                }

                // Bind movement input actions
                var rightLoco = inputActions.FindActionMap("XRI RightHand Locomotion");
                var leftLoco = inputActions.FindActionMap("XRI LeftHand Locomotion");

                if (rightLoco != null)
                {
                    var rightMove = rightLoco.FindAction("Move");
                    if (rightMove != null)
                    {
                        var input = moveProvider.rightHandMoveInput;
                        input.inputAction = rightMove;
                        moveProvider.rightHandMoveInput = input;
                        fixCount++;
                    }
                }

                if (leftLoco != null)
                {
                    var leftMove = leftLoco.FindAction("Move");
                    if (leftMove != null)
                    {
                        var input = moveProvider.leftHandMoveInput;
                        input.inputAction = leftMove;
                        moveProvider.leftHandMoveInput = input;
                        fixCount++;
                    }
                }

                Debug.LogError("[FixXROriginNow] ✓ FIXED: Movement input actions bound");
                EditorUtility.SetDirty(moveProvider);
            }

            // 4. FIX SNAP TURN
            var snapTurn = xrOrigin.GetComponent<SnapTurnProvider>();
            if (snapTurn != null)
            {
                if (snapTurn.turnAmount != 45f)
                {
                    Debug.LogError($"[FixXROriginNow] ✓ FIXED: Snap turn amount to 45°");
                    snapTurn.turnAmount = 45f;
                    EditorUtility.SetDirty(snapTurn);
                    fixCount++;
                }

                // Bind snap turn input actions
                var rightLoco = inputActions.FindActionMap("XRI RightHand Locomotion");
                var leftLoco = inputActions.FindActionMap("XRI LeftHand Locomotion");

                if (rightLoco != null)
                {
                    var rightTurn = rightLoco.FindAction("Turn");
                    if (rightTurn != null)
                    {
                        var input = snapTurn.rightHandTurnInput;
                        input.inputAction = rightTurn;
                        snapTurn.rightHandTurnInput = input;
                        fixCount++;
                    }
                }

                if (leftLoco != null)
                {
                    var leftTurn = leftLoco.FindAction("Turn");
                    if (leftTurn != null)
                    {
                        var input = snapTurn.leftHandTurnInput;
                        input.inputAction = leftTurn;
                        snapTurn.leftHandTurnInput = input;
                        fixCount++;
                    }
                }

                Debug.LogError("[FixXROriginNow] ✓ FIXED: Snap turn input actions bound");
                EditorUtility.SetDirty(snapTurn);
            }

            // 5. FIX TRACKED POSE DRIVER (HEAD TRACKING)
            if (mainCamera != null)
            {
                var tpd = mainCamera.GetComponent<TrackedPoseDriver>();
                if (tpd != null)
                {
                    var headMap = inputActions.FindActionMap("XRI Head");
                    if (headMap == null) headMap = inputActions.FindActionMap("XRI HMD");

                    if (headMap != null)
                    {
                        var posAction = headMap.FindAction("Position");
                        var rotAction = headMap.FindAction("Rotation");

                        if (posAction != null && rotAction != null)
                        {
                            SerializedObject so = new SerializedObject(tpd);

                            // Set position
                            var posProp = so.FindProperty("m_PositionInput");
                            if (posProp != null)
                            {
                                var actionProp = posProp.FindPropertyRelative("m_Action");
                                actionProp.FindPropertyRelative("m_Name").stringValue = posAction.name;
                                actionProp.FindPropertyRelative("m_Id").stringValue = posAction.id.ToString();
                            }

                            // Set rotation
                            var rotProp = so.FindProperty("m_RotationInput");
                            if (rotProp != null)
                            {
                                var actionProp = rotProp.FindPropertyRelative("m_Action");
                                actionProp.FindPropertyRelative("m_Name").stringValue = rotAction.name;
                                actionProp.FindPropertyRelative("m_Id").stringValue = rotAction.id.ToString();
                            }

                            so.ApplyModifiedProperties();
                            Debug.LogError("[FixXROriginNow] ✓ FIXED: TrackedPoseDriver (head tracking)");
                            fixCount++;
                        }
                    }
                }
            }

            // Mark scene dirty and save
            EditorSceneManager.MarkSceneDirty(xrOrigin.gameObject.scene);
            EditorSceneManager.SaveScene(xrOrigin.gameObject.scene);

            Debug.Log("========================================");
            Debug.LogError($"[FixXROriginNow] ✓✓✓ FIXED {fixCount} ISSUES!");
            Debug.LogError("[FixXROriginNow] Scene saved. Now build and test!");
            Debug.Log("========================================");

            EditorUtility.DisplayDialog(
                "XR Origin Fixed!",
                $"Fixed {fixCount} issues:\n" +
                "✓ Camera reference\n" +
                "✓ Floor tracking mode\n" +
                "✓ Move speed 4x (20 m/s)\n" +
                "✓ Movement input actions\n" +
                "✓ Snap turn input actions\n" +
                "✓ Head tracking\n\n" +
                "Scene saved. Build now!",
                "OK"
            );
        }
    }
}
