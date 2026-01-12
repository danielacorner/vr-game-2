using UnityEngine;
using UnityEditor;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace VRDungeonCrawler.Editor
{
    public static class ConfigureXRInputActions
    {
        [MenuItem("Tools/VR Dungeon Crawler/Configure XR Input Actions")]
        public static void Configure()
        {
            Debug.Log("========================================");
            Debug.Log("Configuring XR Input Actions in Editor");
            Debug.Log("========================================");

            // Find XR Origin
            XROrigin xrOrigin = Object.FindObjectOfType<XROrigin>();
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

            Debug.Log($"Loaded input action asset: {inputActions.name}");

            // Configure ContinuousMoveProvider
            var moveProvider = xrOrigin.GetComponent<ContinuousMoveProvider>();
            if (moveProvider != null)
            {
                Debug.Log("Configuring ContinuousMoveProvider...");
                moveProvider.moveSpeed = 20f;
                moveProvider.enableStrafe = true;
                moveProvider.useGravity = true;

                EditorUtility.SetDirty(moveProvider);
                Debug.Log("✓ ContinuousMoveProvider configured (speed=20)");
            }

            // Configure SnapTurnProvider  
            var snapTurn = xrOrigin.GetComponent<SnapTurnProvider>();
            if (snapTurn != null)
            {
                Debug.Log("Configuring SnapTurnProvider...");
                snapTurn.turnAmount = 45f;
                snapTurn.debounceTime = 0.3f;
                snapTurn.enableTurnLeftRight = true;
                snapTurn.enableTurnAround = false;

                EditorUtility.SetDirty(snapTurn);
                Debug.Log("✓ SnapTurnProvider configured");
            }

            // Configure TrackedPoseDriver
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                var trackedPoseDriver = mainCamera.GetComponent<TrackedPoseDriver>();
                if (trackedPoseDriver != null)
                {
                    Debug.Log("Found TrackedPoseDriver");
                    EditorUtility.SetDirty(trackedPoseDriver);
                    Debug.Log("✓ TrackedPoseDriver marked dirty");
                }
            }

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(xrOrigin.gameObject.scene);

            Debug.Log("========================================");
            Debug.Log("Configuration Complete!");
            Debug.Log("IMPORTANT: Save the scene and rebuild!");
            Debug.Log("========================================");

            AssetDatabase.SaveAssets();
        }
    }
}
