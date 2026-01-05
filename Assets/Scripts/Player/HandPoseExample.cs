using UnityEngine;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Example showing how to control hand poses from other scripts
    /// Use this as a reference for integrating with your item system
    /// </summary>
    public class HandPoseExample : MonoBehaviour
    {
        [Header("References")]
        public HandPoseController leftHand;
        public HandPoseController rightHand;

        [Header("Testing (Inspector)")]
        [Tooltip("Change this in inspector to test different poses")]
        public HandPoseState testPoseLeft = HandPoseState.Relaxed;
        public HandPoseState testPoseRight = HandPoseState.Relaxed;

        private HandPoseState lastTestPoseLeft;
        private HandPoseState lastTestPoseRight;

        private void Start()
        {
            // Example: Find hand pose controllers automatically
            if (leftHand == null || rightHand == null)
            {
                HandPoseController[] controllers = FindObjectsOfType<HandPoseController>();
                foreach (var controller in controllers)
                {
                    if (controller.isLeftHand)
                        leftHand = controller;
                    else
                        rightHand = controller;
                }
            }
        }

        private void Update()
        {
            // Allow inspector changes to trigger pose updates (for testing)
            if (testPoseLeft != lastTestPoseLeft && leftHand != null)
            {
                leftHand.SetPose(testPoseLeft);
                lastTestPoseLeft = testPoseLeft;
            }

            if (testPoseRight != lastTestPoseRight && rightHand != null)
            {
                rightHand.SetPose(testPoseRight);
                lastTestPoseRight = testPoseRight;
            }
        }

        // ========== EXAMPLE USAGE METHODS ==========

        /// <summary>
        /// Example: When player grabs an item
        /// </summary>
        public void OnItemGrabbed(bool isLeftHand)
        {
            HandPoseController hand = isLeftHand ? leftHand : rightHand;
            if (hand != null)
            {
                hand.SetPose(HandPoseState.Grabbed);
            }
        }

        /// <summary>
        /// Example: When player releases an item
        /// </summary>
        public void OnItemReleased(bool isLeftHand)
        {
            HandPoseController hand = isLeftHand ? leftHand : rightHand;
            if (hand != null)
            {
                hand.SetPose(HandPoseState.Relaxed);
            }
        }

        /// <summary>
        /// Example: When player initiates item summon
        /// </summary>
        public void OnSummonStart(bool isLeftHand)
        {
            HandPoseController hand = isLeftHand ? leftHand : rightHand;
            if (hand != null)
            {
                hand.SetPose(HandPoseState.SummonReady);
            }
        }

        /// <summary>
        /// Example: During summon animation
        /// </summary>
        public void OnSummoning(bool isLeftHand)
        {
            HandPoseController hand = isLeftHand ? leftHand : rightHand;
            if (hand != null)
            {
                hand.SetPose(HandPoseState.Summoning);
            }
        }

        /// <summary>
        /// Example: When casting a spell
        /// </summary>
        public void OnSpellCasting(bool isLeftHand)
        {
            HandPoseController hand = isLeftHand ? leftHand : rightHand;
            if (hand != null)
            {
                hand.SetPose(HandPoseState.SpellCasting);
            }
        }

        /// <summary>
        /// Example: Reset to idle/relaxed
        /// </summary>
        public void ResetToIdle(bool isLeftHand)
        {
            HandPoseController hand = isLeftHand ? leftHand : rightHand;
            if (hand != null)
            {
                hand.SetPose(HandPoseState.Relaxed);
            }
        }
    }
}
