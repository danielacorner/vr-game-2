using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRDungeonCrawler.Interaction
{
    /// <summary>
    /// Marks an item as summonable - can be called to the player's hand
    /// Works with standard XRI XRGrabInteractable component
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    [RequireComponent(typeof(Rigidbody))]
    public class SummonableItem : MonoBehaviour
    {
        [Header("Item Info")]
        public string itemName = "Item";
        public ItemType itemType = ItemType.Tool;

        [Header("Summoning Settings")]
        [Tooltip("Maximum distance from which this item can be summoned")]
        public float maxSummonDistance = 10f;

        [Tooltip("Speed at which item flies to hand when summoned")]
        public float summonSpeed = 15f;

        [Tooltip("How close to target before snapping to hand")]
        public float snapDistance = 0.1f;

        private XRGrabInteractable grabInteractable;
        private Rigidbody rb;
        private bool isSummoning = false;
        private Transform summonTarget;

        public bool IsSummoning => isSummoning;
        public bool IsBeingHeld => grabInteractable != null && grabInteractable.isSelected;

        private void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
            rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Start summoning this item to the target transform (usually a hand)
        /// </summary>
        public void StartSummon(Transform target)
        {
            if (IsBeingHeld)
            {
                Debug.Log($"[SummonableItem] {itemName} is already being held, cannot summon");
                return;
            }

            summonTarget = target;
            isSummoning = true;

            // Disable gravity during summon
            if (rb != null)
            {
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log($"[SummonableItem] Summoning {itemName} to hand");
        }

        private void FixedUpdate()
        {
            if (isSummoning && summonTarget != null)
            {
                Vector3 targetPos = summonTarget.position;
                Vector3 direction = (targetPos - transform.position);
                float distance = direction.magnitude;

                if (distance < snapDistance)
                {
                    // Close enough - snap to hand and complete summon
                    transform.position = targetPos;
                    transform.rotation = summonTarget.rotation;
                    CompleteSummon();
                }
                else
                {
                    // Move towards target
                    Vector3 velocity = direction.normalized * summonSpeed;
                    rb.linearVelocity = velocity;

                    // Rotate to face movement direction
                    if (velocity.magnitude > 0.1f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(velocity);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
                    }
                }
            }
        }

        private void CompleteSummon()
        {
            isSummoning = false;

            if (rb != null)
            {
                rb.useGravity = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log($"[SummonableItem] {itemName} summoning complete");
        }

        /// <summary>
        /// Cancel summoning (e.g., if player releases button)
        /// </summary>
        public void CancelSummon()
        {
            if (isSummoning)
            {
                isSummoning = false;
                if (rb != null)
                {
                    rb.useGravity = true;
                }
                Debug.Log($"[SummonableItem] {itemName} summon cancelled");
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Show summon range
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, maxSummonDistance);
        }
    }

    public enum ItemType
    {
        Weapon,
        Tool,
        Potion,
        Misc
    }
}
