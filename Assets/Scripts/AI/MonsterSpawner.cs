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
            spawnOffset.y = 0f; // Will calculate Y later based on ground
            Vector3 spawnPosition = transform.position + spawnOffset;

            // Ensure minimum distance from spawner center to avoid collision
            Vector2 horizontalOffset = new Vector2(spawnOffset.x, spawnOffset.z);
            if (horizontalOffset.magnitude < 2.5f)
            {
                horizontalOffset = horizontalOffset.normalized * 2.5f;
                spawnPosition.x = transform.position.x + horizontalOffset.x;
                spawnPosition.z = transform.position.z + horizontalOffset.y;
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

                // Set temporary position high up to calculate mesh bounds
                monster.transform.position = new Vector3(spawnPosition.x, 100f, spawnPosition.z);
                monster.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                // Calculate mesh bounds for collider sizing
                Bounds meshBounds = CalculateMeshBounds(monster);
                Debug.Log($"[MonsterSpawner] Mesh bounds: center={meshBounds.center}, size={meshBounds.size}");

                // Add MonsterBase component and configure
                MonsterBase monsterBase = monster.AddComponent<MonsterBase>();
                monsterBase.monsterType = type;
                monsterBase.maxHP = hp;
                monsterBase.currentHP = hp;
                monsterBase.SetSpawner(this);

                // Add MonsterAI component
                MonsterAI monsterAI = monster.AddComponent<MonsterAI>();
                monsterAI.aggroRange = 2f; // Detect player within 2m
                monsterAI.showDebug = true; // ENABLE DEBUG

                // Add appropriate animator for each monster type
                if (type == MonsterType.Skeleton)
                {
                    SkeletonAnimator animator = monster.AddComponent<SkeletonAnimator>();
                    animator.walkCycleSpeed = 4f;
                    animator.legSwingAngle = 30f;
                    animator.armSwingAngle = 20f;
                    animator.showDebug = true; // ENABLE DEBUG

                    // Add SkeletonAttack for melee combat
                    SkeletonAttack attack = monster.AddComponent<SkeletonAttack>();
                    attack.attackRange = 1.5f;
                    attack.attackDamage = 1;
                    attack.attackCooldown = 2f;
                    attack.showDebug = true; // ENABLE DEBUG

                    // Add SkeletonEyeEffect for fiery red eyes when aggro
                    SkeletonEyeEffect eyeEffect = monster.AddComponent<SkeletonEyeEffect>();
                    eyeEffect.enableFireParticles = true;
                    eyeEffect.fireParticleCount = 10;
                    eyeEffect.showDebug = true; // ENABLE DEBUG
                }
                else if (type == MonsterType.Goblin)
                {
                    GoblinAnimator animator = monster.AddComponent<GoblinAnimator>();
                    animator.walkCycleSpeed = 6f;
                    animator.idleTwitchSpeed = 2f;
                    animator.legSwingAngle = 30f;
                    animator.armSwingAngle = 20f;
                    animator.headTwitchAmount = 8f;
                    animator.earWiggleAmount = 15f;
                    animator.showDebug = true; // ENABLE DEBUG
                }
                else if (type == MonsterType.Slime)
                {
                    SlimeAnimator animator = monster.AddComponent<SlimeAnimator>();
                    animator.bounceCycleSpeed = 4f;
                    animator.jiggleSpeed = 3f;
                    animator.bounceStretchAmount = 0.3f;
                    animator.bounceSquashAmount = 0.2f;
                    animator.bounceHeight = 0.15f;
                    animator.jiggleAmount = 0.05f;
                    animator.showDebug = true; // ENABLE DEBUG
                }

                // Add Rigidbody if not present
                Rigidbody rb = monster.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = monster.AddComponent<Rigidbody>();
                    rb.mass = 1f;
                    rb.linearDamping = 2f;
                    rb.angularDamping = 1f;
                    rb.useGravity = false; // DISABLED initially - will enable after positioning
                    rb.constraints = RigidbodyConstraints.FreezeRotation;
                    rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Prevent sinking through terrain
                    rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooth movement
                }

                // Reset velocity to prevent initial fast movement
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                Debug.Log($"[MonsterSpawner] Rigidbody added with gravity DISABLED temporarily");

                // Calculate lowest vertex FIRST for accurate collider placement
                float lowestLocalY = float.MaxValue;
                MeshFilter[] meshFiltersForLowest = monster.GetComponentsInChildren<MeshFilter>();

                foreach (MeshFilter mf in meshFiltersForLowest)
                {
                    if (mf.mesh == null) continue;

                    Vector3[] vertices = mf.mesh.vertices;
                    Transform meshTransform = mf.transform;

                    foreach (Vector3 localVert in vertices)
                    {
                        Vector3 monsterLocalVert = monster.transform.InverseTransformPoint(meshTransform.TransformPoint(localVert));
                        if (monsterLocalVert.y < lowestLocalY)
                        {
                            lowestLocalY = monsterLocalVert.y;
                        }
                    }
                }

                Debug.Log($"[MonsterSpawner] Calculated lowestLocalY for colliders: {lowestLocalY:F2}");

                // Add colliders sized to ORIGINAL (unscaled) mesh bounds
                // Trigger collider for spell detection - reduced to 60% to match visible model
                BoxCollider triggerCollider = monster.AddComponent<BoxCollider>();
                triggerCollider.isTrigger = true;
                triggerCollider.size = new Vector3(
                    meshBounds.size.x * 0.6f,  // Narrower width
                    meshBounds.size.y * 0.9f,  // Slightly shorter height
                    meshBounds.size.z * 0.6f   // Narrower depth
                );
                triggerCollider.center = meshBounds.center;

                // Physics collider - capsule for better ground contact and anti-sinking
                // Position capsule so its BOTTOM is at the lowest vertex point
                CapsuleCollider physicsCollider = monster.AddComponent<CapsuleCollider>();
                physicsCollider.isTrigger = false;
                physicsCollider.radius = 0.25f; // Wide enough for stability
                physicsCollider.height = 1.0f; // Tall enough to prevent sinking
                physicsCollider.direction = 1; // Y-axis orientation

                // Capsule bottom is at center.y - height/2
                // We want: center.y - height/2 = lowestLocalY
                // So: center.y = lowestLocalY + height/2
                float capsuleCenterY = lowestLocalY + (physicsCollider.height / 2f);
                physicsCollider.center = new Vector3(0f, capsuleCenterY, 0f);

                Debug.Log($"[MonsterSpawner] Capsule: center.y={capsuleCenterY:F2}, bottom will be at {capsuleCenterY - 0.5f:F2} (should match lowestLocalY={lowestLocalY:F2})");

                // Add ground keeper component to prevent sinking through terrain
                MonsterGroundKeeper groundKeeper = monster.AddComponent<MonsterGroundKeeper>();
                groundKeeper.checkInterval = 0.2f; // Check 5 times per second
                groundKeeper.raycastDistance = 5f;
                groundKeeper.groundOffset = 0.05f; // Stay 5cm above ground
                groundKeeper.correctionSpeed = 10f; // Fast correction if sinking
                groundKeeper.showDebug = showDebug;

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

                // === FINAL POSITIONING: Find ground and place monster correctly ===
                // (lowestLocalY was already calculated above for collider setup)
                Debug.Log($"[MonsterSpawner] {type} using lowestLocalY={lowestLocalY:F2}m for positioning");

                // Step 2: Raycast down to find ground level at spawn position
                // Disable monster's colliders so raycast doesn't hit itself
                triggerCollider.enabled = false;
                physicsCollider.enabled = false;

                RaycastHit groundHit;
                Vector3 rayStart = new Vector3(spawnPosition.x, 50f, spawnPosition.z);

                Debug.Log($"[MonsterSpawner] Raycasting from {rayStart} down to find ground...");
                bool hitGround = Physics.Raycast(rayStart, Vector3.down, out groundHit, 100f, ~0, QueryTriggerInteraction.Ignore);

                if (hitGround)
                {
                    Debug.Log($"[MonsterSpawner] ✓ HIT: {groundHit.collider.gameObject.name} at Y={groundHit.point.y:F2}, distance={groundHit.distance:F2}m");
                }
                else
                {
                    Debug.LogError($"[MonsterSpawner] ✗ RAYCAST MISS from {rayStart}!");

                    // Try with debug visualization
                    Debug.DrawRay(rayStart, Vector3.down * 100f, Color.red, 5f);
                }

                // Re-enable colliders immediately
                triggerCollider.enabled = true;
                physicsCollider.enabled = true;

                if (hitGround)
                {
                    // Step 3: Position monster so its lowest vertex is exactly at ground level (plus small offset)
                    float groundY = groundHit.point.y;
                    float groundOffset = 0.05f; // 5cm above ground to prevent z-fighting

                    // Monster's pivot needs to be positioned so that (pivot.y + lowestLocalY) = groundY + offset
                    float monsterPivotY = groundY + groundOffset - lowestLocalY;

                    spawnPosition.y = monsterPivotY;
                    monster.transform.position = spawnPosition;

                    Debug.Log($"[MonsterSpawner] ✓✓✓ POSITIONING ✓✓✓");
                    Debug.Log($"[MonsterSpawner]   Ground Y: {groundY:F2}");
                    Debug.Log($"[MonsterSpawner]   Lowest vertex offset: {lowestLocalY:F2}");
                    Debug.Log($"[MonsterSpawner]   Calculated pivot Y: {monsterPivotY:F2}");
                    Debug.Log($"[MonsterSpawner]   Expected lowest point world Y: {monsterPivotY + lowestLocalY:F2}");
                    Debug.Log($"[MonsterSpawner]   Final position: {monster.transform.position}");

                    // Create visual debug marker at ground hit point
                    if (showDebug)
                    {
                        GameObject groundMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        groundMarker.name = "DEBUG_GroundHitPoint";
                        groundMarker.transform.position = groundHit.point;
                        groundMarker.transform.localScale = Vector3.one * 0.2f;

                        MeshRenderer markerRenderer = groundMarker.GetComponent<MeshRenderer>();
                        Material markerMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        markerMat.color = Color.red;
                        markerMat.EnableKeyword("_EMISSION");
                        markerMat.SetColor("_EmissionColor", Color.red * 2f);
                        markerRenderer.material = markerMat;

                        Destroy(groundMarker.GetComponent<Collider>());
                        Destroy(groundMarker, 10f); // Remove after 10 seconds
                    }
                }
                else
                {
                    Debug.LogError($"[MonsterSpawner] ✗✗✗ RAYCAST MISSED! ✗✗✗");
                    Debug.LogError($"[MonsterSpawner] No ground found at X={spawnPosition.x:F2}, Z={spawnPosition.z:F2}");
                    Debug.LogError($"[MonsterSpawner] This means TERRAIN COLLIDER IS MISSING OR DISABLED!");

                    // Fallback: place at spawner's Y level
                    spawnPosition.y = transform.position.y;
                    monster.transform.position = spawnPosition;
                }

                // Re-enable gravity now that monster is positioned correctly
                rb.useGravity = true;
                Debug.Log($"[MonsterSpawner] ✓ Gravity re-enabled, spawn complete!");

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
