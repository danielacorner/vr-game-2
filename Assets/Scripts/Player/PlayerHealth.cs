using UnityEngine;
using UnityEngine.Events;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Manages player health, damage, and death
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [Tooltip("Maximum health (hearts)")]
        public float maxHealth = 5f;

        [Tooltip("Current health (supports half hearts)")]
        public float currentHealth = 5f;

        [Tooltip("Invulnerability time after taking damage (seconds)")]
        public float invulnerabilityDuration = 1f;

        [Header("Events")]
        public UnityEvent<float, float> onHealthChanged; // current, max
        public UnityEvent onDeath;

        [Header("Debug")]
        public bool showDebug = false;

        private bool isInvulnerable = false;
        private float invulnerabilityEndTime = 0f;

        // Singleton instance
        private static PlayerHealth instance;
        public static PlayerHealth Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<PlayerHealth>();
                }
                return instance;
            }
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            currentHealth = maxHealth;
        }

        void Update()
        {
            // Check invulnerability timer
            if (isInvulnerable && Time.time >= invulnerabilityEndTime)
            {
                isInvulnerable = false;
                if (showDebug)
                    Debug.Log("[PlayerHealth] Invulnerability ended");
            }
        }

        /// <summary>
        /// Apply damage to the player (supports half hearts)
        /// </summary>
        public void TakeDamage(float damage, Vector3 damageSource)
        {
            if (isInvulnerable)
            {
                if (showDebug)
                    Debug.Log("[PlayerHealth] Damage blocked - invulnerable");
                return;
            }

            if (currentHealth <= 0)
                return; // Already dead

            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);

            if (showDebug)
                Debug.Log($"[PlayerHealth] Took {damage} damage. Health: {currentHealth}/{maxHealth}");

            // Trigger invulnerability
            isInvulnerable = true;
            invulnerabilityEndTime = Time.time + invulnerabilityDuration;

            // Notify listeners
            onHealthChanged?.Invoke(currentHealth, maxHealth);

            // Check for death
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Heal the player (supports half hearts)
        /// </summary>
        public void Heal(float amount)
        {
            if (currentHealth >= maxHealth)
                return;

            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            if (showDebug)
                Debug.Log($"[PlayerHealth] Healed {amount}. Health: {currentHealth}/{maxHealth}");

            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Handle player death
        /// </summary>
        void Die()
        {
            if (showDebug)
                Debug.Log("[PlayerHealth] Player died!");

            onDeath?.Invoke();

            // TODO: Implement death behavior (respawn, game over screen, etc.)
        }

        /// <summary>
        /// Reset health to max (for respawn)
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isInvulnerable = false;
            onHealthChanged?.Invoke(currentHealth, maxHealth);

            if (showDebug)
                Debug.Log("[PlayerHealth] Health reset");
        }

        public bool IsInvulnerable()
        {
            return isInvulnerable;
        }

        public float GetHealthPercentage()
        {
            return (float)currentHealth / maxHealth;
        }
    }
}
