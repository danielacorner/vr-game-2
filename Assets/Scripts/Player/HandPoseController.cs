using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Hand pose states for different interactions
    /// </summary>
    public enum HandPoseState
    {
        Relaxed,        // Natural resting pose with slight curl
        Grabbed,        // Gripping an item firmly
        SummonReady,    // Open hand, ready to summon item
        Summoning,      // Hand gesture during summoning animation
        SpellCasting,   // Hand pose for active spell casting
        Pointing        // Finger-pointing pose for UI interaction
    }

    /// <summary>
    /// Manages hand poses for VR interaction states
    /// Supports multiple poses: grabbed, summon-ready, summoning, relaxed, spell-casting
    /// </summary>
    public class HandPoseController : MonoBehaviour
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

        [Header("Current State")]
        public HandPoseState currentPose = HandPoseState.Relaxed;

        private InputDevice device;
        private bool deviceFound = false;

        // Pose definitions (curl amounts for each finger)
        private struct FingerPose
        {
            public float thumb;
            public float index;
            public float middle;
            public float ring;
            public float pinky;

            public FingerPose(float thumb, float index, float middle, float ring, float pinky)
            {
                this.thumb = thumb;
                this.index = index;
                this.middle = middle;
                this.ring = ring;
                this.pinky = pinky;
            }
        }

        private void Start()
        {
            Debug.Log($"[HandPoseController] Starting for {(isLeftHand ? "LEFT" : "RIGHT")} hand");
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
                Debug.Log($"[HandPoseController] ✓ Found {(isLeftHand ? "LEFT" : "RIGHT")} controller: {device.name}");
            }
            else
            {
                Debug.LogWarning($"[HandPoseController] Could not find {(isLeftHand ? "LEFT" : "RIGHT")} controller device");
            }
        }

        private void Update()
        {
            if (!deviceFound)
            {
                if (Time.frameCount % 60 == 0)
                {
                    FindDevice();
                }
                return;
            }

            // Get controller input
            float trigger = 0f;
            float grip = 0f;
            bool thumbTouching = false;

            if (device.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
                trigger = triggerValue;

            if (device.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
                grip = gripValue;

            // Check thumb touching buttons/joystick
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out bool stickTouch))
                thumbTouching = stickTouch;
            if (!thumbTouching && device.TryGetFeatureValue(CommonUsages.primaryTouch, out bool primaryTouch))
                thumbTouching = primaryTouch;
            if (!thumbTouching && device.TryGetFeatureValue(CommonUsages.secondaryTouch, out bool secondaryTouch))
                thumbTouching = secondaryTouch;

            // Get target pose based on current state
            FingerPose targetPose = GetPoseForState(currentPose, trigger, grip, thumbTouching);

            // Animate fingers to target pose
            AnimateFinger(thumbRoot, targetPose.thumb, 2, true); // Thumb uses different curl axis
            AnimateFinger(indexRoot, targetPose.index, 3, false);
            AnimateFinger(middleRoot, targetPose.middle, 3, false);
            AnimateFinger(ringRoot, targetPose.ring, 3, false);
            AnimateFinger(pinkyRoot, targetPose.pinky, 3, false);
        }

        /// <summary>
        /// Returns finger curl amounts for the given pose state
        /// </summary>
        private FingerPose GetPoseForState(HandPoseState state, float trigger, float grip, bool thumbTouch)
        {
            switch (state)
            {
                case HandPoseState.Relaxed:
                    // Natural resting pose with slight curl
                    return new FingerPose(
                        thumb: thumbTouch ? 0f : 0.25f,    // Extends when touching buttons
                        index: Mathf.Max(trigger, 0.15f),   // Slight idle curl
                        middle: Mathf.Max(grip, 0.15f),
                        ring: Mathf.Max(grip, 0.15f),
                        pinky: Mathf.Max(grip, 0.15f)
                    );

                case HandPoseState.Grabbed:
                    // Firm grip around object
                    return new FingerPose(
                        thumb: 0.85f,      // Thumb wraps around
                        index: 0.9f,       // All fingers curl tightly
                        middle: 0.95f,
                        ring: 0.95f,
                        pinky: 0.9f
                    );

                case HandPoseState.SummonReady:
                    // Open hand, palm up, ready to receive item
                    return new FingerPose(
                        thumb: 0f,         // Fully extended
                        index: 0f,
                        middle: 0f,
                        ring: 0f,
                        pinky: 0f
                    );

                case HandPoseState.Summoning:
                    // Dramatic召唤 gesture - fingers slightly curled, tense
                    return new FingerPose(
                        thumb: 0.3f,
                        index: 0.25f,
                        middle: 0.2f,
                        ring: 0.25f,
                        pinky: 0.3f
                    );

                case HandPoseState.SpellCasting:
                    // Mystical hand pose - index and middle extended, others curled
                    return new FingerPose(
                        thumb: 0.6f,       // Curled in
                        index: 0.1f,       // Extended
                        middle: 0.1f,      // Extended
                        ring: 0.8f,        // Curled
                        pinky: 0.8f        // Curled
                    );

                case HandPoseState.Pointing:
                    // Pointing pose for UI interaction - index extended, others curled
                    return new FingerPose(
                        thumb: 0.7f,       // Curled against palm
                        index: 0f,         // Fully extended (pointing)
                        middle: 0.85f,     // Curled down
                        ring: 0.9f,        // Curled down
                        pinky: 0.85f       // Curled down
                    );

                default:
                    return new FingerPose(0.15f, 0.15f, 0.15f, 0.15f, 0.15f);
            }
        }

        /// <summary>
        /// Animates a finger with anatomically correct curl ratios
        /// </summary>
        private void AnimateFinger(Transform fingerRoot, float curlAmount, int segments, bool isThumb)
        {
            if (fingerRoot == null) return;

            Transform currentSegment = fingerRoot;

            // Anatomical curl ratios
            float[] segmentRatios = { 0.45f, 0.30f, 0.25f };

            for (int i = 0; i < segments; i++)
            {
                if (currentSegment == null) break;

                float ratio = i < segmentRatios.Length ? segmentRatios[i] : 0.25f;
                float progressiveMultiplier = 1f + (i * 0.15f);
                float targetAngle = curlAmount * maxFingerCurl * ratio * progressiveMultiplier;

                Quaternion targetRotation;

                if (isThumb)
                {
                    // Thumb curls across palm (Y-axis rotation for opposable motion)
                    targetRotation = Quaternion.Euler(-targetAngle * 0.5f, -targetAngle, 0);
                }
                else
                {
                    // Regular fingers curl forward (Z-axis rotation)
                    targetRotation = Quaternion.Euler(0, 0, -targetAngle);
                }

                currentSegment.localRotation = Quaternion.Slerp(
                    currentSegment.localRotation,
                    targetRotation,
                    Time.deltaTime * animationSpeed
                );

                currentSegment = currentSegment.childCount > 0 ? currentSegment.GetChild(0) : null;
            }
        }

        /// <summary>
        /// Public method to change hand pose state
        /// </summary>
        public void SetPose(HandPoseState newPose)
        {
            currentPose = newPose;
            Debug.Log($"[HandPoseController] {(isLeftHand ? "LEFT" : "RIGHT")} hand pose → {newPose}");
        }

        /// <summary>
        /// For testing - cycle through poses
        /// </summary>
        public void CyclePose()
        {
            int nextPose = ((int)currentPose + 1) % System.Enum.GetValues(typeof(HandPoseState)).Length;
            SetPose((HandPoseState)nextPose);
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
