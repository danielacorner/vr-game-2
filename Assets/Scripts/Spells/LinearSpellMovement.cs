using UnityEngine;

namespace VRDungeonCrawler.Spells
{
    /// <summary>
    /// Simple linear movement for tier-1 spell projectiles
    /// Moves in a straight line at constant speed
    /// </summary>
    public class LinearSpellMovement : MonoBehaviour
    {
        public Vector3 direction = Vector3.forward;
        public float speed = 20f;
        public float lifetime = 5f;

        private float spawnTime;

        void Start()
        {
            spawnTime = Time.time;
        }

        void Update()
        {
            // Move forward
            transform.position += direction * speed * Time.deltaTime;

            // Check lifetime
            if (Time.time - spawnTime > lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
