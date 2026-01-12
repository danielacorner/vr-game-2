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
    public static class UnpackAndFixXROrigin
    {
        [MenuItem("Tools/VR Dungeon Crawler/UNPACK & FIX XR ORIGIN (Final Fix)", priority = -1)]
        public static void UnpackAndFix()
        {
            XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
            if (xrOrigin == null)
            {
                EditorUtility.DisplayDialog("Error", "XR Origin not found!", "OK");
                return;
            }

            // Check if it's a prefab instance
            bool isPrefab = PrefabUtility.IsPartOfPrefabInstance(xrOrigin.gameObject);
            Debug.Log($"[UnpackAndFix] XR Origin is prefab instance: {isPrefab}");

            if (isPrefab)
            {
                Debug.LogError("[UnpackAndFix] UNPACKING PREFAB to break prefab connection...");
                PrefabUtility.UnpackPrefabInstance(xrOrigin.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                Debug.LogError("[UnpackAndFix] ✓ Prefab unpacked!");
            }

            // Now apply all fixes
            FixAllXRSettings(xrOrigin);

            // Save scene
            EditorSceneManager.MarkSceneDirty(xrOrigin.gameObject.scene);
            EditorSceneManager.SaveScene(xrOrigin.gameObject.scene);

            EditorUtility.DisplayDialog(
                "XR Origin Fixed!",
                "XR Origin unpacked from prefab and all settings fixed!\n\n" +
                "✓ Camera reference set\n" +
                "✓ Floor tracking mode\n" +
                "✓ Move speed 20 (4x)\n" +
                "✓ Snap turn 45°\n" +
                "✓ Input actions configured\n\n" +
                "BUILD NOW and test!",
                "OK"
            );
        }

        private static void FixAllXRSettings(XROrigin xrOrigin)
        {
            string assetPath = "Assets/Samples/XR Interaction Toolkit/3.3.1/Starter Assets/XRI Default Input Actions.inputactions";
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);
            if (inputActions == null)
            {
                Debug.LogError("[UnpackAndFix] Could not load input actions!");
                return;
            }

            // Fix camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                xrOrigin.Camera = mainCamera;
                Debug.LogError("[UnpackAndFix] ✓ Camera set");
            }

            // Fix tracking mode
            xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            Debug.LogError("[UnpackAndFix] ✓ Tracking mode set to Floor");

            EditorUtility.SetDirty(xrOrigin);

            // Fix movement provider
            var moveProvider = xrOrigin.GetComponent<ContinuousMoveProvider>();
            if (moveProvider != null)
            {
                moveProvider.moveSpeed = 20f;

                // Get the action maps and actions
                var rightLocoMap = inputActions.FindActionMap("XRI RightHand Locomotion");
                var leftLocoMap = inputActions.FindActionMap("XRI LeftHand Locomotion");

                if (rightLocoMap != null)
                {
                    var moveAction = rightLocoMap.FindAction("Move");
                    if (moveAction != null)
                    {
                        // Enable the action
                        if (!moveAction.enabled)
                        {
                            moveAction.Enable();
                        }

                        // Set it on the provider
                        var input = moveProvider.rightHandMoveInput;
                        input.inputAction = moveAction;
                        moveProvider.rightHandMoveInput = input;
                    }
                }

                if (leftLocoMap != null)
                {
                    var moveAction = leftLocoMap.FindAction("Move");
                    if (moveAction != null)
                    {
                        if (!moveAction.enabled)
                        {
                            moveAction.Enable();
                        }

                        var input = moveProvider.leftHandMoveInput;
                        input.inputAction = moveAction;
                        moveProvider.leftHandMoveInput = input;
                    }
                }

                Debug.LogError("[UnpackAndFix] ✓ Movement provider fixed");
                EditorUtility.SetDirty(moveProvider);
            }

            // Fix snap turn provider
            var snapTurn = xrOrigin.GetComponent<SnapTurnProvider>();
            if (snapTurn != null)
            {
                snapTurn.turnAmount = 45f;

                var rightLocoMap = inputActions.FindActionMap("XRI RightHand Locomotion");
                var leftLocoMap = inputActions.FindActionMap("XRI LeftHand Locomotion");

                if (rightLocoMap != null)
                {
                    var turnAction = rightLocoMap.FindAction("Turn");
                    if (turnAction != null)
                    {
                        if (!turnAction.enabled)
                        {
                            turnAction.Enable();
                        }

                        var input = snapTurn.rightHandTurnInput;
                        input.inputAction = turnAction;
                        snapTurn.rightHandTurnInput = input;
                    }
                }

                if (leftLocoMap != null)
                {
                    var turnAction = leftLocoMap.FindAction("Turn");
                    if (turnAction != null)
                    {
                        if (!turnAction.enabled)
                        {
                            turnAction.Enable();
                        }

                        var input = snapTurn.leftHandTurnInput;
                        input.inputAction = turnAction;
                        snapTurn.leftHandTurnInput = input;
                    }
                }

                Debug.LogError("[UnpackAndFix] ✓ Snap turn provider fixed");
                EditorUtility.SetDirty(snapTurn);
            }

            // Fix TrackedPoseDriver
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
                            // Enable actions
                            if (!posAction.enabled) posAction.Enable();
                            if (!rotAction.enabled) rotAction.Enable();

                            SerializedObject so = new SerializedObject(tpd);

                            var posProp = so.FindProperty("m_PositionInput");
                            if (posProp != null)
                            {
                                var actionProp = posProp.FindPropertyRelative("m_Action");
                                actionProp.FindPropertyRelative("m_Name").stringValue = posAction.name;
                                actionProp.FindPropertyRelative("m_Id").stringValue = posAction.id.ToString();
                            }

                            var rotProp = so.FindProperty("m_RotationInput");
                            if (rotProp != null)
                            {
                                var actionProp = rotProp.FindPropertyRelative("m_Action");
                                actionProp.FindPropertyRelative("m_Name").stringValue = rotAction.name;
                                actionProp.FindPropertyRelative("m_Id").stringValue = rotAction.id.ToString();
                            }

                            so.ApplyModifiedProperties();
                            Debug.LogError("[UnpackAndFix] ✓ TrackedPoseDriver fixed");
                        }
                    }
                }
            }

            Debug.LogError("[UnpackAndFix] ✓✓✓ ALL FIXES APPLIED!");
        }
    }
}
