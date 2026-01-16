using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Mystical evil spawner device that creates dungeon monsters
    /// Spawns up to 3 monsters (one of each type) at 5-second intervals
    /// Detects when monsters die and respawns them after 5 seconds
    /// Visual appearance: Angular polygonal evil device with dark crystals and glow
    /// </summary>
    public class MonsterSpawner : MonoBehaviour
    {
        [Header("Spawning")]
        [Tooltip("Time between spawn attempts")]
        public float spawnInterval = 1f;

        [Tooltip("Spawn radius around spawner")]
        public float spawnRadius = 8f;

        [Tooltip("Maximum monsters of each type")]
        public int maxMonstersPerType = 5;

        [Tooltip("Starting HP for Goblin")]
        public int goblinHP = 6;

        [Tooltip("Starting HP for Skeleton")]
        public int skeletonHP = 10;

        [Tooltip("Starting HP for Slime")]
        public int slimeHP = 8;

        [Header("Visual Effects")]
        [Tooltip("Glow color for spawner crystals")]
        public Color evilGlowColor = new Color(0.5f, 0f, 0.8f); // Purple

        [Tooltip("Particle system for spawn effect")]
        public ParticleSystem spawnParticles;

        [Header("Debug")]
        [Tooltip("Show debug logs")]
        public bool showDebug = true;

        // Internal state
        private Dictionary<MonsterType, List<GameObject>> activeMonsters = new Dictionary<MonsterType, List<GameObject>>();
        private float nextSpawnTime;
        private List<MonsterType> spawnQueue = new List<MonsterType>();

        void Start()
        {
            if (showDebug)
                Debug.Log($"[MonsterSpawner] Starting spawner at position {transform.position}");

            // Build spawner visual appearance
            BuildSpawnerModel();

            // Initialize active monsters dictionary
            activeMonsters[MonsterType.Goblin] = new List<GameObject>();
            activeMonsters[MonsterType.Skeleton] = new List<GameObject>();
            activeMonsters[MonsterType.Slime] = new List<GameObject>();

            // Initialize spawn queue with all monster types (5 of each)
            for (int i = 0; i < maxMonstersPerType; i++)
            {
                spawnQueue.Add(MonsterType.Goblin);
                spawnQueue.Add(MonsterType.Skeleton);
                spawnQueue.Add(MonsterType.Slime);
            }

            // Shuffle queue for random order
            ShuffleSpawnQueue();

            // Schedule first spawn immediately
            nextSpawnTime = Time.time + 0.1f;

            if (showDebug)
                Debug.Log($"[MonsterSpawner] Spawner initialized with {spawnQueue.Count} monsters queued");
        }

        void Update()
        {
            // Check if time to spawn
            if (Time.time >= nextSpawnTime)
            {
                TrySpawnMonster();
                nextSpawnTime = Time.time + spawnInterval;
            }
        }

        void TrySpawnMonster()
        {
            // Check if we have any monsters to spawn
            if (spawnQueue.Count == 0)
            {
                if (showDebug)
                    Debug.Log("[MonsterSpawner] All monsters active, no spawn needed");
                return;
            }

            // Get next monster type from queue
            MonsterType typeToSpawn = spawnQueue[0];
            spawnQueue.RemoveAt(0);

            // Spawn the monster
            SpawnMonster(typeToSpawn);

            if (showDebug)
                Debug.Log($"[MonsterSpawner] Spawned {typeToSpawn}, {spawnQueue.Count} remaining in queue");
        }

        void SpawnMonster(MonsterType type)
        {
            // Calculate spawn position (random around spawner, away from structure)
            Vector3 spawnOffset = Random.insideUnitSphere * spawnRadius;
            spawnOffset.y = 0f; // Keep on ground level
            Vector3 spawnPosition = transform.position + spawnOffset;

            // Ensure minimum distance from spawner center to avoid collision
            Vector2 horizontalOffset = new Vector2(spawnOffset.x, spawnOffset.z);
            if (horizontalOffset.magnitude < 2.5f)
            {
                horizontalOffset = horizontalOffset.normalized * 2.5f;
                spawnPosition.x = transform.position.x + horizontalOffset.x;
                spawnPosition.z = transform.position.z + horizontalOffset.y;
            }

            // Raycast down to find the actual ground height
            RaycastHit hit;
            Vector3 rayStart = new Vector3(spawnPosition.x, 50f, spawnPosition.z);
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 100f))
            {
                // Spawn 1.05m above ground (collider bottom is at -1.0, so this puts it at +0.05 above ground)
                spawnPosition.y = hit.point.y + 1.05f;

                if (showDebug)
                    Debug.Log($"[MonsterSpawner] Ground found at Y={hit.point.y}, spawning at Y={spawnPosition.y}");
            }
            else
            {
                // Fallback if raycast fails
                spawnPosition.y = 1.05f;

                if (showDebug)
                    Debug.LogWarning($"[MonsterSpawner] No ground found, using fallback Y=1.05");
            }

            // Build monster based on type
            GameObject monster = null;
            int hp = 0;

            switch (type)
            {
                case MonsterType.Goblin:
                    monster = MonsterBuilder.CreateGoblin();
                    hp = goblinHP;
                    break;
                case MonsterType.Skeleton:
                    monster = MonsterBuilder.CreateSkeleton();
                    hp = skeletonHP;
                    break;
                case MonsterType.Slime:
                    monster = MonsterBuilder.CreateSlime();
                    hp = slimeHP;
                    break;
            }

            if (monster != null)
            {
                Debug.Log($"[MonsterSpawner] ========== SPAWNING {type} ==========");

                // Calculate mesh bounds BEFORE scaling (for accurate collider sizing)
                Bounds meshBounds = CalculateMeshBounds(monster);

                Debug.Log($"[MonsterSpawner] Original mesh bounds: center={meshBounds.center}, size={meshBounds.size}");

                // DON'T scale visual meshes - keep monsters at original size for now to debug
                // ScaleVisualMeshes(monster, 1.5f);

                // Simple approach: spawn at ground level + half monster height
                // This ensures the monster's bottom is at ground level
                float monsterHeight = meshBounds.size.y;
                float monsterBottom = meshBounds.center.y - (monsterHeight / 2f);
                spawnPosition.y = 0f - monsterBottom; // Ground is at Y=0

                Debug.Log($"[MonsterSpawner] Monster height={monsterHeight:F2}, bottom offset={monsterBottom:F2}, spawn Y={spawnPosition.y:F2}");

                // Set initial position and rotation
                monster.transform.position = spawnPosition;
                monster.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                Debug.Log($"[MonsterSpawner] Monster positioned at {monster.transform.position}");

                // Add MonsterBase component and configure
                MonsterBase monsterBase = monster.AddComponent<MonsterBase>();
                monsterBase.monsterType = type;
                monsterBase.maxHP = hp;
                monsterBase.currentHP = hp;
                monsterBase.SetSpawner(this);

                // Add MonsterAI component
                MonsterAI monsterAI = monster.AddComponent<MonsterAI>();

                // Add Rigidbody if not present
                Rigidbody rb = monster.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = monster.AddComponent<Rigidbody>();
                    rb.mass = 1f;
                    rb.linearDamping = 2f;
                    rb.angularDamping = 1f;
                    rb.useGravity = true;
                    rb.constraints = RigidbodyConstraints.FreezeRotation;
                }

                // Reset velocity to prevent initial fast movement
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // Add colliders sized to ORIGINAL (unscaled) mesh bounds
                // This keeps hitboxes reasonable while visual is larger
                // Trigger collider for spell detection - matches original mesh size
                BoxCollider triggerCollider = monster.AddComponent<BoxCollider>();
                triggerCollider.isTrigger = true;
                triggerCollider.size = meshBounds.size;
                triggerCollider.center = meshBounds.center;

                // Physics collider - slightly smaller to prevent getting stuck
                BoxCollider physicsCollider = monster.AddComponent<BoxCollider>();
                physicsCollider.isTrigger = false;
                physicsCollider.size = new Vector3(meshBounds.size.x * 0.9f, meshBounds.size.y, meshBounds.size.z * 0.9f);
                physicsCollider.center = meshBounds.center;

                // DISABLED: Add ground alignment component for robust floor positioning
                // Using simple Y position calculation instead for now
                // MonsterGroundAlignment groundAlignment = monster.AddComponent<MonsterGroundAlignment>();
                // groundAlignment.showDebug = showDebug;

                // Add visual debug sphere to see collider bounds
                if (showDebug)
                {
                    GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    debugSphere.name = "DebugColliderVisual";
                    debugSphere.transform.SetParent(monster.transform);
                    debugSphere.transform.localPosition = Vector3.up * 0.2f;
                    debugSphere.transform.localScale = new Vector3(1.2f, 2.4f, 1.2f);

                    // Make it transparent green
                    MeshRenderer debugRenderer = debugSphere.GetComponent<MeshRenderer>();
                    Material debugMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    debugMat.color = new Color(0f, 1f, 0f, 0.3f);
                    debugMat.SetFloat("_Surface", 1); // Transparent
                    debugMat.SetFloat("_Blend", 0);
                    debugMat.renderQueue = 3000;
                    debugRenderer.material = debugMat;

                    // Remove the sphere's collider so it doesn't interfere
                    Destroy(debugSphere.GetComponent<Collider>());
                }

                // Final ground position adjustment using raycast
                // Find the actual lowest point of the monster's visual mesh in world space
                float lowestY = float.MaxValue;
                MeshRenderer[] renderers = monster.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer renderer in renderers)
                {
                    // Get world space bounds of this renderer
                    Bounds worldBounds = renderer.bounds;
                    float rendererBottom = worldBounds.min.y;
                    if (rendererBottom < lowestY)
                    {
                        lowestY = rendererBottom;
                    }
                }

                Debug.Log($"[MonsterSpawner] Actual visual mesh lowest point in world space: Y={lowestY:F2}");

                // Raycast down from monster to find exact ground level
                RaycastHit groundHit;
                Vector3 groundRayStart = monster.transform.position + Vector3.up * 1f;
                if (Physics.Raycast(groundRayStart, Vector3.down, out groundHit, 10f, LayerMask.GetMask("Default")))
                {
                    // Calculate how much to adjust: difference between ground and current lowest point
                    float yAdjustment = groundHit.point.y - lowestY;
                    Vector3 finalPos = monster.transform.position;
                    finalPos.y += yAdjustment;
                    monster.transform.position = finalPos;

                    Debug.Log($"[MonsterSpawner] Ground at Y={groundHit.point.y:F2}, lowest mesh at Y={lowestY:F2}, adjustment={yAdjustment:F2}, final Y={finalPos.y:F2}");
                }
                else
                {
                    Debug.LogWarning($"[MonsterSpawner] Ground raycast missed! Monster may float.");
                }

                // Track active monster
                activeMonsters[type].Add(monster);

                // Play spawn particles if available
                if (spawnParticles != null)
                {
                    spawnParticles.transform.position = spawnPosition;
                    spawnParticles.Play();
                }

                if (showDebug)
                    Debug.Log($"[MonsterSpawner] {type} spawned at {spawnPosition} with {hp} HP");
            }
        }

        /// <summary>
        /// Called by MonsterBase when a monster dies
        /// </summary>
        public void OnMonsterDied(MonsterType type, GameObject monster)
        {
            // Remove from active monsters list
            if (activeMonsters.ContainsKey(type))
            {
                activeMonsters[type].Remove(monster);
            }

            // Add back to spawn queue (will respawn after next interval)
            spawnQueue.Add(type);

            if (showDebug)
                Debug.Log($"[MonsterSpawner] {type} died, added to respawn queue");
        }

        void ShuffleSpawnQueue()
        {
            // Fisher-Yates shuffle
            for (int i = spawnQueue.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                MonsterType temp = spawnQueue[i];
                spawnQueue[i] = spawnQueue[j];
                spawnQueue[j] = temp;
            }
        }

        /// <summary>
        /// Scale up visual meshes without affecting colliders
        /// Only scales the mesh renderer transforms, not the root GameObject
        /// </summary>
        void ScaleVisualMeshes(GameObject obj, float scale)
        {
            MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer renderer in renderers)
            {
                // Scale the renderer's transform (not the root object)
                renderer.transform.localScale *= scale;

                if (showDebug)
                    Debug.Log($"[MonsterSpawner] Scaled {renderer.gameObject.name} by {scale}x");
            }
        }

        /// <summary>
        /// Calculate the bounds of all mesh renderers in a GameObject
        /// Returns the combined bounds in local space
        /// </summary>
        Bounds CalculateMeshBounds(GameObject obj)
        {
            MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();

            if (renderers.Length == 0)
            {
                // No meshes found, return default bounds
                return new Bounds(Vector3.zero, Vector3.one);
            }

            // Start with first renderer's bounds
            Bounds bounds = new Bounds(renderers[0].transform.localPosition, Vector3.zero);

            // Expand to include all renderers
            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer.GetComponent<MeshFilter>() != null)
                {
                    Mesh mesh = renderer.GetComponent<MeshFilter>().mesh;
                    if (mesh != null)
                    {
                        // Get mesh bounds in local space
                        Bounds meshBounds = mesh.bounds;

                        // Transform corners to GameObject's local space
                        Vector3[] corners = new Vector3[8];
                        corners[0] = renderer.transform.localPosition + renderer.transform.localRotation * (meshBounds.center + new Vector3(meshBounds.extents.x, meshBounds.extents.y, meshBounds.extents.z));
                        corners[1] = renderer.transform.localPosition + renderer.transform.localRotation * (meshBounds.center + new Vector3(-meshBounds.extents.x, meshBounds.extents.y, meshBounds.extents.z));
                        corners[2] = renderer.transform.localPosition + renderer.transform.localRotation * (meshBounds.center + new Vector3(meshBounds.extents.x, -meshBounds.extents.y, meshBounds.extents.z));
                        corners[3] = renderer.transform.localPosition + renderer.transform.localRotation * (meshBounds.center + new Vector3(-meshBounds.extents.x, -meshBounds.extents.y, meshBounds.extents.z));
                        corners[4] = renderer.transform.localPosition + renderer.transform.localRotation * (meshBounds.center + new Vector3(meshBounds.extents.x, meshBounds.extents.y, -meshBounds.extents.z));
                        corners[5] = renderer.transform.localPosition + renderer.transform.localRotation * (meshBounds.center + new Vector3(-meshBounds.extents.x, meshBounds.extents.y, -meshBounds.extents.z));
                        corners[6] = renderer.transform.localPosition + renderer.transform.localRotation * (meshBounds.center + new Vector3(meshBounds.extents.x, -meshBounds.extents.y, -meshBounds.extents.z));
                        corners[7] = renderer.transform.localPosition + renderer.transform.localRotation * (meshBounds.center + new Vector3(-meshBounds.extents.x, -meshBounds.extents.y, -meshBounds.extents.z));

                        // Encapsulate all corners
                        foreach (Vector3 corner in corners)
                        {
                            bounds.Encapsulate(corner);
                        }
                    }
                }
            }

            return bounds;
        }

        /// <summary>
        /// Build the visual model of the spawner using angular shapes
        /// Creates a mystical evil device appearance with dark crystals and glow
        /// </summary>
        void BuildSpawnerModel()
        {
            // Base platform: Dark hexagonal-ish angular base
            GameObject basePlatform = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            basePlatform.name = "BasePlatform";
            basePlatform.transform.SetParent(transform);
            basePlatform.transform.localPosition = Vector3.zero;
            basePlatform.transform.localScale = new Vector3(3f, 0.3f, 3f);
            ApplyMaterial(basePlatform, new Color(0.1f, 0.1f, 0.15f)); // Very dark blue-gray

            // Center pedestal: Tapered column
            GameObject pedestal = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateTrapezoid(0.6f));
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(transform);
            pedestal.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            pedestal.transform.localScale = new Vector3(1f, 1.2f, 1f);
            ApplyMaterial(pedestal, new Color(0.15f, 0.1f, 0.2f)); // Dark purple-gray

            // Central crystal: Evil glowing purple crystal (multiple angular shards)
            for (int i = 0; i < 5; i++)
            {
                GameObject crystal = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateWedge());
                crystal.name = $"Crystal_{i}";
                crystal.transform.SetParent(transform);

                // Arrange crystals in circle, pointing outward and up
                float angle = (i * 72f) * Mathf.Deg2Rad; // 360/5 = 72 degrees
                float radius = 0.8f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, 1.8f, Mathf.Sin(angle) * radius);
                crystal.transform.localPosition = position;

                // Rotate to point outward and up
                crystal.transform.localRotation = Quaternion.Euler(45f, i * 72f, 0f);
                crystal.transform.localScale = new Vector3(0.3f, 0.8f, 0.3f);

                // Evil purple glow
                ApplyMaterial(crystal, evilGlowColor, emissive: true);
            }

            // Top floating crystal: Main evil eye
            GameObject topCrystal = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateWedge());
            topCrystal.name = "TopCrystal";
            topCrystal.transform.SetParent(transform);
            topCrystal.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            topCrystal.transform.localRotation = Quaternion.Euler(0f, 0f, 180f); // Point down
            topCrystal.transform.localScale = new Vector3(0.4f, 1f, 0.4f);
            ApplyMaterial(topCrystal, evilGlowColor, emissive: true);

            // Add slow rotation to top crystal
            StartCoroutine(RotateTopCrystal(topCrystal.transform));

            // Corner pillars: 4 dark angular pillars at corners
            for (int i = 0; i < 4; i++)
            {
                GameObject pillar = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
                pillar.name = $"Pillar_{i}";
                pillar.transform.SetParent(transform);

                // Place at corners
                float cornerAngle = (i * 90f + 45f) * Mathf.Deg2Rad;
                float cornerRadius = 1.8f;
                Vector3 pillarPos = new Vector3(Mathf.Cos(cornerAngle) * cornerRadius, 0.8f, Mathf.Sin(cornerAngle) * cornerRadius);
                pillar.transform.localPosition = pillarPos;
                pillar.transform.localScale = new Vector3(0.25f, 1.6f, 0.25f);
                ApplyMaterial(pillar, new Color(0.12f, 0.08f, 0.15f));

                // Small glowing top on each pillar
                GameObject pillarTop = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
                pillarTop.name = $"PillarTop_{i}";
                pillarTop.transform.SetParent(pillar.transform);
                pillarTop.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                pillarTop.transform.localScale = new Vector3(1.2f, 0.2f, 1.2f);
                ApplyMaterial(pillarTop, evilGlowColor * 0.5f, emissive: true);
            }

            // Runes on base: Small glowing angular symbols
            for (int i = 0; i < 6; i++)
            {
                GameObject rune = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
                rune.name = $"Rune_{i}";
                rune.transform.SetParent(basePlatform.transform);

                float runeAngle = (i * 60f) * Mathf.Deg2Rad;
                float runeRadius = 0.4f;
                Vector3 runePos = new Vector3(Mathf.Cos(runeAngle) * runeRadius, 0.55f, Mathf.Sin(runeAngle) * runeRadius);
                rune.transform.localPosition = runePos;
                rune.transform.localScale = new Vector3(0.06f, 0.02f, 0.08f);
                ApplyMaterial(rune, evilGlowColor * 0.7f, emissive: true);

                // Add pulsing glow
                StartCoroutine(PulseGlow(rune, i * 0.2f));
            }

            if (showDebug)
                Debug.Log("[MonsterSpawner] Spawner visual model built");
        }

        IEnumerator RotateTopCrystal(Transform crystal)
        {
            while (true)
            {
                crystal.Rotate(Vector3.up, 30f * Time.deltaTime);
                yield return null;
            }
        }

        IEnumerator PulseGlow(GameObject obj, float offset)
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            if (renderer == null) yield break;

            Material mat = renderer.material;
            Color baseColor = evilGlowColor;

            while (true)
            {
                float pulse = Mathf.Sin((Time.time + offset) * 2f) * 0.5f + 0.5f;
                Color newColor = baseColor * (0.5f + pulse * 0.5f);
                mat.SetColor("_EmissionColor", newColor * 2f);
                yield return null;
            }
        }

        GameObject CreateWithMesh(Mesh mesh)
        {
            GameObject obj = new GameObject();
            MeshFilter filter = obj.AddComponent<MeshFilter>();
            filter.mesh = mesh;
            obj.AddComponent<MeshRenderer>();
            return obj;
        }

        void ApplyMaterial(GameObject obj, Color color, bool emissive = false)
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();

            if (emissive)
            {
                // Use Unlit shader for glowing parts (better for mobile VR)
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                mat.color = color;
                renderer.material = mat;

                if (showDebug)
                    Debug.Log($"[MonsterSpawner] Applied emissive material (Unlit) to {obj.name}");
            }
            else
            {
                // Use Lit shader for non-glowing parts
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = color;
                mat.SetFloat("_Smoothness", 0.2f);
                mat.SetFloat("_Metallic", 0.1f);
                renderer.material = mat;

                if (showDebug)
                    Debug.Log($"[MonsterSpawner] Applied standard material (Lit) to {obj.name}");
            }
        }
    }
}
