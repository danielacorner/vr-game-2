using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections.Generic;
using System.Linq;

namespace VRDungeonCrawler.Interaction
{
    /// <summary>
    /// Handles summoning items to the player's hand
    /// Attach to controller/hand that should be able to summon
    /// Simple button-based summoning - hold button to summon nearest item
    /// </summary>
    public class ItemSummoner : MonoBehaviour
    {
        [Header("Configuration")]
        public bool isLeftHand = true;

        [Header("Summoning Settings")]
        [Tooltip("Use secondary button (Y on left, B on right) for summoning")]
        public bool useSecondaryButton = true;

        [Tooltip("Maximum distance to search for summonable items")]
        public float maxSearchDistance = 15f;

        [Header("References")]
        [Tooltip("Transform where item will be summoned to (usually attach point on hand)")]
        public Transform summonTarget;

        [Tooltip("Hand pose controller for animation")]
        public Player.HandPoseController handPoseController;

        private InputDevice device;
        private bool deviceFound = false;
        private SummonableItem currentlySummoning;
        private bool summonButtonHeld = false;

        private void Start()
        {
            // Auto-find summontarget if not set
            if (summonTarget == null)
                summonTarget = transform;

            // Auto-find hand pose controller
            if (handPoseController == null)
                handPoseController = GetComponentInChildren<Player.HandPoseController>();

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
                Debug.Log($"[ItemSummoner] ✓ Found {(isLeftHand ? "LEFT" : "RIGHT")} controller: {device.name}");
            }
        }

        private void Update()
        {
            if (!deviceFound)
            {
                if (Time.frameCount % 60 == 0)
                    FindDevice();
                return;
            }

            // Check summon button (Y on left, B on right)
            bool buttonPressed = false;
            if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool buttonValue))
            {
                buttonPressed = buttonValue;
            }

            // Button pressed - start summoning
            if (buttonPressed && !summonButtonHeld)
            {
                summonButtonHeld = true;
                StartSummoning();
            }
            // Button released - cancel summoning
            else if (!buttonPressed && summonButtonHeld)
            {
                summonButtonHeld = false;
                CancelSummoning();
            }
        }

        private void StartSummoning()
        {
            // Find nearest summonable item
            SummonableItem nearestItem = FindNearestSummonableItem();

            if (nearestItem != null)
            {
                currentlySummoning = nearestItem;
                nearestItem.StartSummon(summonTarget);

                // Set hand to summoning pose
                if (handPoseController != null)
                {
                    handPoseController.SetPose(Player.HandPoseState.Summoning);
                }

                Debug.Log($"[ItemSummoner] {(isLeftHand ? "LEFT" : "RIGHT")} hand summoning {nearestItem.itemName}");
            }
            else
            {
                Debug.Log($"[ItemSummoner] No summonable items found within {maxSearchDistance}m");
            }
        }

        private void CancelSummoning()
        {
            if (currentlySummoning != null)
            {
                currentlySummoning.CancelSummon();
                currentlySummoning = null;
            }

            // Return hand to relaxed pose
            if (handPoseController != null)
            {
                handPoseController.SetPose(Player.HandPoseState.Relaxed);
            }
        }

        /// <summary>
        /// Find the nearest summonable item within range
        /// </summary>
        private SummonableItem FindNearestSummonableItem()
        {
            SummonableItem[] allItems = FindObjectsOfType<SummonableItem>();

            SummonableItem nearest = null;
            float nearestDistance = maxSearchDistance;

            foreach (SummonableItem item in allItems)
            {
                // Skip if already being held or summoned
                if (item.IsBeingHeld || item.IsSummoning)
                    continue;

                float distance = Vector3.Distance(transform.position, item.transform.position);

                // Check if within item's max summon distance and closer than current nearest
                if (distance <= item.maxSummonDistance && distance < nearestDistance)
                {
                    nearest = item;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize search radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, maxSearchDistance);
        }
    }
}
