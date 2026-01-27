using UnityEngine;
using VRDungeonCrawler.Player;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Handles skeleton melee attacks against the player
    /// </summary>
    public class SkeletonAttack : MonoBehaviour
    {
        [Header("Attack Settings")]
        [Tooltip("Attack range")]
        public float attackRange = 1.5f;

        [Tooltip("Damage dealt per hit")]
        public int attackDamage = 1;

        [Tooltip("Time between attacks")]
        public float attackCooldown = 2f;

        [Header("Debug")]
        public bool showDebug = false;

        [HideInInspector]
        public bool isAttacking = false;

        private Transform playerTarget;
        private float lastAttackTime = 0f;
        private MonsterAI monsterAI;
        private bool isAggro = false;

        void Start()
        {
            monsterAI = GetComponent<MonsterAI>();

            // Find player
            GameObject xrOrigin = GameObject.Find("XR Origin");
            if (xrOrigin != null)
            {
                playerTarget = xrOrigin.transform;
            }
            else
            {
                Debug.LogWarning("[SkeletonAttack] Player (XR Origin) not found!");
            }
        }

        void Update()
        {
            // Check aggro state (using public property)
            if (monsterAI != null)
            {
                isAggro = monsterAI.IsAggro;
            }

            // Only attack when aggro
            if (!isAggro || playerTarget == null)
            {
                isAttacking = false;
                return;
            }

            // Check if player is in range
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

            if (distanceToPlayer <= attackRange)
            {
                // Try to attack
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                }
                else
                {
                    // Still in cooldown
                    isAttacking = true;
                }
            }
            else
            {
                isAttacking = false;
            }
        }

        void Attack()
        {
            if (showDebug)
                Debug.Log($"[SkeletonAttack] {gameObject.name} attacking player!");

            isAttacking = true;
            lastAttackTime = Time.time;

            // Deal damage to player
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.TakeDamage(attackDamage, transform.position);
            }
        }

        void OnDrawGizmosSelected()
        {
            // Visualize attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
