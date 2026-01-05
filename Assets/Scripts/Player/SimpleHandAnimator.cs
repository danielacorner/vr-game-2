using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// SIMPLE hand animator using direct XR Input
    /// No fancy reflection - just works!
    /// </summary>
    public class SimpleHandAnimator : MonoBehaviour
    {
        [Header("Configuration")]
        public bool isLeftHand = true;
        public Transform controller;

        [Header("Finger Roots")]
        public Transform thumbRoot;
        public Transform indexRoot;
        public Transform middleRoot;
        public Transform ringRoot;
        public Transform pinkyRoot;

        [Header("Animation Settings")]
        [Range(5f, 30f)]
        public float animationSpeed = 15f;
        [Range(0f, 90f)]
        public float maxFingerCurl = 70f;

        private InputDevice device;
        private bool deviceFound = false;

        private void Start()
        {
            Debug.Log($"[SimpleHandAnimator] Starting for {(isLeftHand ? "LEFT" : "RIGHT")} hand");
            FindDevice();
        }

        private void FindDevice()
        {
            var desiredCharacteristics = isLeftHand ?
                InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller :
                InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;

            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, devices);

            if (devices.Count > 0)
            {
                device = devices[0];
                deviceFound = true;
                Debug.Log($"[SimpleHandAnimator] ✓ Found {(isLeftHand ? "LEFT" : "RIGHT")} controller: {device.name}");
            }
            else
            {
                Debug.LogWarning($"[SimpleHandAnimator] Could not find {(isLeftHand ? "LEFT" : "RIGHT")} controller device");
            }
        }

        private void Update()
        {
            if (!deviceFound)
            {
                // Retry finding device
                if (Time.frameCount % 60 == 0) // Every second
                {
                    FindDevice();
                }
                return;
            }

            // Read inputs directly from XR Input
            float trigger = 0f;
            float grip = 0f;
            bool thumbTouching = false;

            if (device.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
            {
                trigger = triggerValue;
            }

            if (device.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
            {
                grip = gripValue;
            }

            // Check if thumb is touching primary button or joystick
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out bool stickTouch))
            {
                thumbTouching = stickTouch;
            }

            if (!thumbTouching && device.TryGetFeatureValue(CommonUsages.primaryTouch, out bool primaryTouch))
            {
                thumbTouching = primaryTouch;
            }

            if (!thumbTouching && device.TryGetFeatureValue(CommonUsages.secondaryTouch, out bool secondaryTouch))
            {
                thumbTouching = secondaryTouch;
            }

            // Animate fingers with natural idle pose (slight curl when relaxed)
            const float idleCurl = 0.15f; // Natural resting curl

            // Index follows trigger, has slight idle curl
            AnimateFinger(indexRoot, Mathf.Max(trigger, idleCurl), 3);

            // Other fingers follow grip, have slight idle curl
            float gripWithIdle = Mathf.Max(grip, idleCurl);
            AnimateFinger(middleRoot, gripWithIdle, 3);
            AnimateFinger(ringRoot, gripWithIdle, 3);
            AnimateFinger(pinkyRoot, gripWithIdle, 3);

            // Thumb: extends when touching, slight curl when not
            AnimateFinger(thumbRoot, thumbTouching ? 0f : 0.25f, 2);
        }

        private void AnimateFinger(Transform fingerRoot, float curlAmount, int segments)
        {
            if (fingerRoot == null) return;

            Transform currentSegment = fingerRoot;

            // Realistic finger curl ratios (based on human anatomy)
            // First joint: 45%, Second joint: 30%, Third joint: 25%
            float[] segmentRatios = { 0.45f, 0.30f, 0.25f };

            for (int i = 0; i < segments; i++)
            {
                if (currentSegment == null) break;

                // Use anatomically correct joint ratios
                float ratio = i < segmentRatios.Length ? segmentRatios[i] : 0.25f;

                // Add slight progressive increase for more natural curl
                float progressiveMultiplier = 1f + (i * 0.15f);
                float targetAngle = curlAmount * maxFingerCurl * ratio * progressiveMultiplier;

                // Curl in Z axis (knuckle curl)
                Quaternion targetRotation = Quaternion.Euler(0, 0, -targetAngle);

                // Smooth, natural interpolation
                currentSegment.localRotation = Quaternion.Slerp(
                    currentSegment.localRotation,
                    targetRotation,
                    Time.deltaTime * animationSpeed
                );

                currentSegment = currentSegment.childCount > 0 ? currentSegment.GetChild(0) : null;
            }
        }

        private void OnDrawGizmos()
        {
            // Visualize finger bones
            if (thumbRoot != null) DrawFingerBones(thumbRoot, Color.red, 2);
            if (indexRoot != null) DrawFingerBones(indexRoot, Color.green, 3);
            if (middleRoot != null) DrawFingerBones(middleRoot, Color.blue, 3);
            if (ringRoot != null) DrawFingerBones(ringRoot, Color.yellow, 3);
            if (pinkyRoot != null) DrawFingerBones(pinkyRoot, Color.magenta, 3);
        }

        private void DrawFingerBones(Transform root, Color color, int segments)
        {
            Gizmos.color = color;
            Transform current = root;

            for (int i = 0; i < segments && current != null; i++)
            {
                if (current.childCount > 0)
                {
                    Transform next = current.GetChild(0);
                    Gizmos.DrawLine(current.position, next.position);
                    Gizmos.DrawWireSphere(current.position, 0.005f);
                }
                current = current.childCount > 0 ? current.GetChild(0) : null;
            }
        }
    }
}
