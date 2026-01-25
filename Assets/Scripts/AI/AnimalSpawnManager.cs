using UnityEngine;
using System.Collections.Generic;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Spawns and manages animal population in home area
    /// Creates 5-8 animals of various types (rabbit, squirrel, bird, deer, fox)
    /// Distributes them naturally across the terrain
    /// </summary>
    public class AnimalSpawnManager : MonoBehaviour
    {
        [Header("Animal Prefabs")]
        [Tooltip("Rabbit prefab (small ground animal)")]
        public GameObject rabbitPrefab;

        [Tooltip("Squirrel prefab (small ground animal)")]
        public GameObject squirrelPrefab;

        [Tooltip("Bird prefab (flying animal)")]
        public GameObject birdPrefab;

        [Tooltip("Deer prefab (large ground animal)")]
        public GameObject deerPrefab;

        [Tooltip("Fox prefab (medium ground animal)")]
        public GameObject foxPrefab;

        [Header("Spawn Settings")]
        [Tooltip("Minimum number of animals to spawn")]
        [Range(3, 15)]
        public int minAnimals = 5;

        [Tooltip("Maximum number of animals to spawn")]
        [Range(3, 15)]
        public int maxAnimals = 8;

        [Tooltip("Radius around spawn center to place animals")]
        public float spawnRadius = 20f;

        [Tooltip("Center point for animal spawning")]
        public Vector3 spawnCenter = Vector3.zero;

        [Tooltip("Avoid spawning animals within this radius of center (for campfire)")]
        public float avoidCenterRadius = 8f;

        [Header("Type Distribution")]
        [Tooltip("Weight for spawning rabbits (higher = more likely)")]
        [Range(0f, 1f)]
        public float rabbitWeight = 0.3f;

        [Tooltip("Weight for spawning squirrels")]
        [Range(0f, 1f)]
        public float squirrelWeight = 0.3f;

        [Tooltip("Weight for spawning birds")]
        [Range(0f, 1f)]
        public float birdWeight = 0.15f;

        [Tooltip("Weight for spawning deer (larger animals)")]
        [Range(0f, 1f)]
        public float deerWeight = 0.15f;

        [Tooltip("Weight for spawning foxes")]
        [Range(0f, 1f)]
        public float foxWeight = 0.1f;

        [Header("Debug")]
        [Tooltip("Show spawn information in console")]
        public bool showDebug = true;

        private List<GameObject> spawnedAnimals = new List<GameObject>();

        void Start()
        {
            SpawnAnimals();
        }

        [ContextMenu("Spawn Animals")]
        public void SpawnAnimals()
        {
            // Clear existing animals
            ClearAnimals();

            // Validate prefabs
            if (!ValidatePrefabs())
            {
                Debug.LogError("[AnimalSpawnManager] Missing animal prefabs! Cannot spawn animals.");
                return;
            }

            // Determine how many animals to spawn
            int animalCount = Random.Range(minAnimals, maxAnimals + 1);

            if (showDebug)
                Debug.Log($"[AnimalSpawnManager] Spawning {animalCount} animals...");

            // Spawn animals
            for (int i = 0; i < animalCount; i++)
            {
                SpawnRandomAnimal(i);
            }

            if (showDebug)
                Debug.Log($"[AnimalSpawnManager] Successfully spawned {spawnedAnimals.Count} animals!");
        }

        [ContextMenu("Clear Animals")]
        public void ClearAnimals()
        {
            foreach (GameObject animal in spawnedAnimals)
            {
                if (animal != null)
                {
                    if (Application.isPlaying)
                        Destroy(animal);
                    else
                        DestroyImmediate(animal);
                }
            }
            spawnedAnimals.Clear();

            if (showDebug)
                Debug.Log("[AnimalSpawnManager] Cleared all animals");
        }

        void SpawnRandomAnimal(int index)
        {
            // Select random animal type based on weights
            AnimalType animalType = SelectRandomAnimalType();

            // Find valid spawn position
            Vector3 spawnPosition = GetRandomSpawnPosition();
            if (spawnPosition == Vector3.zero)
            {
                if (showDebug)
                    Debug.LogWarning($"[AnimalSpawnManager] Could not find valid spawn position for animal {index}");
                return;
            }

            // Create the animal using AnimalBuilder (with realistic colors!)
            GameObject animal = CreateAnimalWithType(animalType);
            if (animal == null)
            {
                Debug.LogWarning($"[AnimalSpawnManager] Failed to create {animalType}, skipping spawn");
                return;
            }

            animal.transform.position = spawnPosition;
            animal.transform.rotation = Quaternion.identity;
            animal.transform.SetParent(transform);
            animal.name = $"{animalType}_{index}";

            // Add Rigidbody for physics movement
            Rigidbody rb = animal.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
            rb.linearDamping = 2f;
            rb.angularDamping = 1f;
            // Disable gravity for birds - they control their own Y position to prevent twitching
            rb.useGravity = (animalType != AnimalType.Bird);
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            // Add AI component
            AnimalAI ai = animal.AddComponent<AnimalAI>();
            ai.animalType = animalType;
            ai.showDebug = showDebug;

            // Add trigger collider for spell detection
            SphereCollider triggerCol = animal.AddComponent<SphereCollider>();
            triggerCol.radius = 0.3f;
            triggerCol.isTrigger = true;

            // Add solid collider for physics (prevents falling through floor)
            CapsuleCollider physicsCol = animal.AddComponent<CapsuleCollider>();
            physicsCol.radius = 0.25f;
            physicsCol.height = 0.5f;
            physicsCol.center = Vector3.zero;

            // Add animation controller
            AnimalAnimationController animController = animal.AddComponent<AnimalAnimationController>();

            spawnedAnimals.Add(animal);

            if (showDebug)
                Debug.Log($"[AnimalSpawnManager] Spawned {animalType} {animal.name} at {spawnPosition}");
        }

        GameObject CreateAnimalWithType(AnimalType type)
        {
            switch (type)
            {
                case AnimalType.Rabbit:
                    return AnimalBuilder.CreateRabbit();
                case AnimalType.Squirrel:
                    return AnimalBuilder.CreateSquirrel();
                case AnimalType.Bird:
                    return AnimalBuilder.CreateBird();
                case AnimalType.Deer:
                    return AnimalBuilder.CreateDeer();
                case AnimalType.Fox:
                    return AnimalBuilder.CreateFox();
                default:
                    return AnimalBuilder.CreateRabbit();
            }
        }

        AnimalType SelectRandomAnimalType()
        {
            // Normalize weights
            float totalWeight = rabbitWeight + squirrelWeight + birdWeight + deerWeight + foxWeight;
            if (totalWeight == 0)
            {
                return AnimalType.Rabbit;
            }

            // Random selection based on weights
            float roll = Random.Range(0f, totalWeight);

            if (roll < rabbitWeight)
                return AnimalType.Rabbit;
            else if (roll < rabbitWeight + squirrelWeight)
                return AnimalType.Squirrel;
            else if (roll < rabbitWeight + squirrelWeight + birdWeight)
                return AnimalType.Bird;
            else if (roll < rabbitWeight + squirrelWeight + birdWeight + deerWeight)
                return AnimalType.Deer;
            else
                return AnimalType.Fox;
        }

        GameObject SelectRandomPrefab()
        {
            // Normalize weights
            float totalWeight = rabbitWeight + squirrelWeight + birdWeight;
            if (totalWeight == 0)
            {
                Debug.LogError("[AnimalSpawnManager] All animal weights are 0!");
                return rabbitPrefab ?? squirrelPrefab ?? birdPrefab;
            }

            // Random selection based on weights
            float roll = Random.Range(0f, totalWeight);

            if (roll < rabbitWeight && rabbitPrefab != null)
                return rabbitPrefab;
            else if (roll < rabbitWeight + squirrelWeight && squirrelPrefab != null)
                return squirrelPrefab;
            else if (birdPrefab != null)
                return birdPrefab;

            // Fallback to any available prefab
            return rabbitPrefab ?? squirrelPrefab ?? birdPrefab;
        }

        Vector3 GetRandomSpawnPosition()
        {
            int maxAttempts = 30;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Random point in ring (avoiding center)
                Vector2 randomCircle = Random.insideUnitCircle.normalized *
                                      Random.Range(avoidCenterRadius, spawnRadius);

                Vector3 randomPos = spawnCenter + new Vector3(randomCircle.x, 0f, randomCircle.y);

                // Raycast down to find ground
                RaycastHit hit;
                if (Physics.Raycast(randomPos + Vector3.up * 10f, Vector3.down, out hit, 20f))
                {
                    return hit.point + Vector3.up * 0.1f; // Slightly above ground
                }

                // Fallback: use spawnCenter Y
                randomPos.y = spawnCenter.y;
                return randomPos;
            }

            Debug.LogWarning("[AnimalSpawnManager] Failed to find valid spawn position after max attempts");
            return Vector3.zero;
        }

        bool ValidatePrefabs()
        {
            int prefabCount = 0;
            if (rabbitPrefab != null) prefabCount++;
            if (squirrelPrefab != null) prefabCount++;
            if (birdPrefab != null) prefabCount++;

            return prefabCount > 0;
        }

        void OnDrawGizmosSelected()
        {
            // Draw spawn area
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            DrawGizmoCircle(spawnCenter, spawnRadius, 32);

            // Draw avoid zone
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            DrawGizmoCircle(spawnCenter, avoidCenterRadius, 16);

            // Draw spawned animal positions
            Gizmos.color = Color.yellow;
            foreach (GameObject animal in spawnedAnimals)
            {
                if (animal != null)
                {
                    Gizmos.DrawWireSphere(animal.transform.position, 0.5f);
                }
            }
        }

        void DrawGizmoCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 previousPoint = center + new Vector3(radius, 0f, 0f);

            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector3 point = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                );

                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }
        }
    }
}
