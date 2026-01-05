using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;
using VRDungeonCrawler.Player;

namespace VRDungeonCrawler.Spells
{
    /// <summary>
    /// Casts spells when trigger is pulled
    /// Uses the currently selected spell from SpellManager
    /// Creates unique projectiles for each spell type
    /// </summary>
    public class SpellCaster : MonoBehaviour
    {
        [Header("Configuration")]
        public bool isLeftHand = true;

        [Header("Casting Settings")]
        [Tooltip("Point where spells spawn")]
        public Transform spawnPoint;

        [Tooltip("Distance from hand if spawnPoint not set")]
        public float spawnDistance = 0.15f;

        [Tooltip("Cooldown between casts")]
        public float castCooldown = 0.5f;

        private InputDevice device;
        private bool deviceFound = false;
        private float lastCastTime = 0f;
        private bool triggerPressed = false;

        private void Start()
        {
            // Auto-set spawn point if not assigned
            if (spawnPoint == null)
                spawnPoint = transform;

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
                Debug.Log($"[SpellCaster] ✓ Found {(isLeftHand ? "LEFT" : "RIGHT")} controller: {device.name}");
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

            // Check trigger input
            bool triggerValue = false;
            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out bool buttonValue))
            {
                triggerValue = buttonValue;
            }

            // Cast spell on trigger press (not hold)
            if (triggerValue && !triggerPressed)
            {
                triggerPressed = true;
                TryCastSpell();
            }
            else if (!triggerValue && triggerPressed)
            {
                triggerPressed = false;
            }
        }

        private void TryCastSpell()
        {
            // Check cooldown
            if (Time.time < lastCastTime + castCooldown)
            {
                Debug.Log($"[SpellCaster] Spell on cooldown ({Time.time - lastCastTime:F2}s)");
                return;
            }

            // Get current spell
            if (SpellManager.Instance == null || SpellManager.Instance.currentSpell == null)
            {
                Debug.LogWarning("[SpellCaster] No spell selected!");
                return;
            }

            SpellData spell = SpellManager.Instance.currentSpell;
            CastSpell(spell);
            lastCastTime = Time.time;
        }

        private void CastSpell(SpellData spell)
        {
            // Calculate spawn position and direction
            Vector3 spawnPos = spawnPoint.position + spawnPoint.forward * spawnDistance;
            Vector3 direction = spawnPoint.forward;

            // Create projectile
            GameObject projectile = CreateProjectile(spell, spawnPos, direction);

            Debug.Log($"[SpellCaster] Cast {spell.spellName}!");
        }

        private GameObject CreateProjectile(SpellData spell, Vector3 position, Vector3 direction)
        {
            string spellName = spell.spellName.ToLower();

            GameObject projectile;

            if (spellName.Contains("fire") || spellName.Contains("flame"))
                projectile = CreateFireball(spell);
            else if (spellName.Contains("ice") || spellName.Contains("frost") || spellName.Contains("shard"))
                projectile = CreateIceShard(spell);
            else if (spellName.Contains("light") || spellName.Contains("thunder") || spellName.Contains("bolt"))
                projectile = CreateLightningBolt(spell);
            else if (spellName.Contains("wind") || spellName.Contains("air") || spellName.Contains("blast"))
                projectile = CreateWindBlast(spell);
            else
                projectile = CreateDefaultProjectile(spell);

            // Set position and rotation
            projectile.transform.position = position;
            projectile.transform.rotation = Quaternion.LookRotation(direction);

            // Add movement component
            SpellProjectile projScript = projectile.AddComponent<SpellProjectile>();
            projScript.speed = spell.projectileSpeed;
            projScript.direction = direction;
            projScript.lifetime = 5f;
            projScript.spellData = spell; // Pass spell data for explosion effects

            return projectile;
        }

        #region Projectile Visuals

        private GameObject CreateFireball(SpellData spell)
        {
            GameObject projectile = new GameObject($"Fireball_{spell.spellName}");

            // Bright glowing core with emission
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "FireCore";
            core.transform.SetParent(projectile.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.4f;

            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Sprites/Default"));
            if (coreMat.shader == null) coreMat.shader = Shader.Find("Unlit/Color");
            coreMat.color = new Color(1f, 0.3f, 0f, 1f) * 3f; // Bright emissive orange
            core.GetComponent<MeshRenderer>().material = coreMat;

            // Outer glow layer
            GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glow.name = "FireGlow";
            glow.transform.SetParent(projectile.transform);
            glow.transform.localPosition = Vector3.zero;
            glow.transform.localScale = Vector3.one * 0.6f;

            Destroy(glow.GetComponent<Collider>());

            Material glowMat = new Material(Shader.Find("Sprites/Default"));
            if (glowMat.shader == null) glowMat.shader = Shader.Find("Unlit/Color");
            glowMat.color = new Color(1f, 0.5f, 0f, 0.4f);
            glow.GetComponent<MeshRenderer>().material = glowMat;

            // Swirling flame wisps (6) - smaller emissive particles
            for (int i = 0; i < 6; i++)
            {
                GameObject wisp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                wisp.name = $"FireWisp{i}";
                wisp.transform.SetParent(projectile.transform);
                wisp.transform.localScale = Vector3.one * 0.15f;

                Destroy(wisp.GetComponent<Collider>());

                Material wispMat = new Material(Shader.Find("Sprites/Default"));
                wispMat.color = new Color(1f, 0.6f, 0f, 1f) * 2f; // Bright yellow-orange
                wisp.GetComponent<MeshRenderer>().material = wispMat;
            }

            // Trail renderer for motion blur
            TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.3f;
            trail.startWidth = 0.5f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.material.color = new Color(1f, 0.3f, 0f, 0.8f) * 2f;
            trail.startColor = new Color(1f, 0.4f, 0f, 0.8f);
            trail.endColor = new Color(1f, 0.2f, 0f, 0f);

            // Add animation
            FireballAnimation anim = projectile.AddComponent<FireballAnimation>();

            return projectile;
        }

        private GameObject CreateIceShard(SpellData spell)
        {
            GameObject projectile = new GameObject($"IceShard_{spell.spellName}");

            // Main crystal shard - glowing ice core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cube);
            core.name = "IceCore";
            core.transform.SetParent(projectile.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = new Vector3(0.2f, 0.2f, 0.7f);

            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Sprites/Default"));
            coreMat.color = new Color(0.6f, 0.9f, 1f, 1f) * 2f; // Bright cyan glow
            core.GetComponent<MeshRenderer>().material = coreMat;

            // Outer crystalline layer
            GameObject outer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            outer.name = "IceOuter";
            outer.transform.SetParent(projectile.transform);
            outer.transform.localPosition = Vector3.zero;
            outer.transform.localScale = new Vector3(0.25f, 0.25f, 0.8f);

            Destroy(outer.GetComponent<Collider>());

            Material outerMat = new Material(Shader.Find("Sprites/Default"));
            if (outerMat.shader == null) outerMat.shader = Shader.Find("Unlit/Color");
            outerMat.color = new Color(0.7f, 0.95f, 1f, 0.6f);
            outer.GetComponent<MeshRenderer>().material = outerMat;

            // Spinning ice crystals (8)
            for (int i = 0; i < 8; i++)
            {
                GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crystal.name = $"IceCrystal{i}";
                crystal.transform.SetParent(projectile.transform);
                crystal.transform.localScale = new Vector3(0.06f, 0.06f, 0.25f);

                Destroy(crystal.GetComponent<Collider>());

                Material crystalMat = new Material(Shader.Find("Sprites/Default"));
                crystalMat.color = new Color(0.8f, 0.95f, 1f, 1f) * 1.5f;
                crystal.GetComponent<MeshRenderer>().material = crystalMat;
            }

            // Frost trail
            TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.4f;
            trail.startWidth = 0.4f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.material.color = new Color(0.7f, 0.9f, 1f, 0.6f) * 1.5f;
            trail.startColor = new Color(0.7f, 0.95f, 1f, 0.8f);
            trail.endColor = new Color(0.6f, 0.9f, 1f, 0f);

            // Add animation
            IceShardAnimation anim = projectile.AddComponent<IceShardAnimation>();

            return projectile;
        }

        private GameObject CreateLightningBolt(SpellData spell)
        {
            GameObject projectile = new GameObject($"LightningBolt_{spell.spellName}");

            // Super bright energy core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "LightningCore";
            core.transform.SetParent(projectile.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.35f;

            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Sprites/Default"));
            coreMat.color = new Color(1f, 1f, 0.95f, 1f) * 4f; // Extremely bright white-blue
            core.GetComponent<MeshRenderer>().material = coreMat;

            // Electric aura glow
            GameObject aura = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            aura.name = "LightningAura";
            aura.transform.SetParent(projectile.transform);
            aura.transform.localPosition = Vector3.zero;
            aura.transform.localScale = Vector3.one * 0.55f;

            Destroy(aura.GetComponent<Collider>());

            Material auraMat = new Material(Shader.Find("Sprites/Default"));
            if (auraMat.shader == null) auraMat.shader = Shader.Find("Unlit/Color");
            auraMat.color = new Color(0.8f, 0.9f, 1f, 0.3f);
            aura.GetComponent<MeshRenderer>().material = auraMat;

            // Chaotic electric arcs (10)
            for (int i = 0; i < 10; i++)
            {
                GameObject arc = GameObject.CreatePrimitive(PrimitiveType.Cube);
                arc.name = $"Arc{i}";
                arc.transform.SetParent(projectile.transform);
                arc.transform.localScale = new Vector3(0.04f, 0.25f, 0.04f);

                Destroy(arc.GetComponent<Collider>());

                Material arcMat = new Material(Shader.Find("Sprites/Default"));
                arcMat.color = new Color(1f, 1f, 0.95f, 1f) * 3f; // Bright white
                arc.GetComponent<MeshRenderer>().material = arcMat;
            }

            // Electric trail
            TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.25f;
            trail.startWidth = 0.45f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.material.color = new Color(0.9f, 0.95f, 1f, 0.9f) * 2.5f;
            trail.startColor = new Color(1f, 1f, 0.95f, 0.9f);
            trail.endColor = new Color(0.8f, 0.9f, 1f, 0f);

            // Add animation
            LightningAnimation anim = projectile.AddComponent<LightningAnimation>();

            return projectile;
        }

        private GameObject CreateWindBlast(SpellData spell)
        {
            GameObject projectile = new GameObject($"WindBlast_{spell.spellName}");

            // Bright swirling energy core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "WindCore";
            core.transform.SetParent(projectile.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.3f;

            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Sprites/Default"));
            coreMat.color = new Color(0.85f, 0.95f, 1f, 1f) * 2.5f; // Bright pale cyan-white
            core.GetComponent<MeshRenderer>().material = coreMat;

            // Outer swirling wind layer
            GameObject swirl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            swirl.name = "WindSwirl";
            swirl.transform.SetParent(projectile.transform);
            swirl.transform.localPosition = Vector3.zero;
            swirl.transform.localScale = Vector3.one * 0.5f;

            Destroy(swirl.GetComponent<Collider>());

            Material swirlMat = new Material(Shader.Find("Sprites/Default"));
            if (swirlMat.shader == null) swirlMat.shader = Shader.Find("Unlit/Color");
            swirlMat.color = new Color(0.9f, 0.97f, 1f, 0.35f);
            swirl.GetComponent<MeshRenderer>().material = swirlMat;

            // Swirling air particles (12) - increased from 8
            for (int i = 0; i < 12; i++)
            {
                GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                particle.name = $"WindParticle{i}";
                particle.transform.SetParent(projectile.transform);
                particle.transform.localScale = Vector3.one * 0.12f;

                Destroy(particle.GetComponent<Collider>());

                Material particleMat = new Material(Shader.Find("Sprites/Default"));
                particleMat.color = new Color(0.9f, 0.95f, 1f, 1f) * 1.8f; // Bright white-cyan
                particle.GetComponent<MeshRenderer>().material = particleMat;
            }

            // Wind motion trail
            TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.35f;
            trail.startWidth = 0.5f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.material.color = new Color(0.85f, 0.93f, 1f, 0.7f) * 1.8f;
            trail.startColor = new Color(0.9f, 0.95f, 1f, 0.8f);
            trail.endColor = new Color(0.8f, 0.9f, 1f, 0f);

            // Add animation
            WindBlastAnimation anim = projectile.AddComponent<WindBlastAnimation>();

            return projectile;
        }

        private GameObject CreateDefaultProjectile(SpellData spell)
        {
            GameObject projectile = new GameObject($"Projectile_{spell.spellName}");

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(projectile.transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = Vector3.one * 0.25f;

            Destroy(sphere.GetComponent<Collider>());

            Material mat = new Material(Shader.Find("Sprites/Default"));
            if (mat.shader == null) mat.shader = Shader.Find("Unlit/Color");
            mat.color = spell.spellColor;
            sphere.GetComponent<MeshRenderer>().material = mat;

            return projectile;
        }

        #endregion
    }

    #region Projectile Movement

    public class SpellProjectile : MonoBehaviour
    {
        public Vector3 direction;
        public float speed = 20f;
        public float lifetime = 5f;
        public SpellData spellData;
        private float spawnTime;
        private bool hasExploded = false;
        private Vector3 hitNormal = Vector3.up; // Store the surface normal for explosion

        void Start()
        {
            spawnTime = Time.time;

            // Add sphere collider for collision detection (trigger)
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 0.3f;
            collider.isTrigger = true;

            // Add rigidbody (kinematic) for trigger detection
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        void Update()
        {
            // Raycast ahead to detect surface normal before collision
            RaycastHit hit;
            float rayDistance = speed * Time.deltaTime * 2f; // Look ahead
            if (Physics.Raycast(transform.position, direction, out hit, rayDistance))
            {
                // Ignore triggers and XR controllers
                if (!hit.collider.isTrigger &&
                    !hit.collider.gameObject.name.Contains("Controller") &&
                    !hit.collider.gameObject.name.Contains("Hand"))
                {
                    hitNormal = hit.normal; // Store the surface normal
                }
            }

            // Move forward
            transform.position += direction * speed * Time.deltaTime;

            // Destroy after lifetime
            if (Time.time > spawnTime + lifetime)
            {
                Destroy(gameObject);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            // Ignore triggers and XR controllers
            if (other.isTrigger) return;
            if (other.gameObject.name.Contains("Controller")) return;
            if (other.gameObject.name.Contains("Hand")) return;

            // Only explode once
            if (hasExploded) return;
            hasExploded = true;

            Debug.Log($"[SpellProjectile] Hit {other.gameObject.name} with normal {hitNormal}, creating explosion!");

            // Create explosion effect at impact point with surface normal
            CreateExplosion(transform.position, hitNormal);

            // Destroy projectile
            Destroy(gameObject);
        }

        private void CreateExplosion(Vector3 position, Vector3 surfaceNormal)
        {
            if (spellData == null) return;

            string spellName = spellData.spellName.ToLower();

            if (spellName.Contains("fire") || spellName.Contains("flame"))
                CreateFireExplosion(position, surfaceNormal);
            else if (spellName.Contains("ice") || spellName.Contains("frost") || spellName.Contains("shard"))
                CreateIceExplosion(position, surfaceNormal);
            else if (spellName.Contains("light") || spellName.Contains("thunder") || spellName.Contains("bolt"))
                CreateLightningExplosion(position, surfaceNormal);
            else if (spellName.Contains("wind") || spellName.Contains("air") || spellName.Contains("blast"))
                CreateWindExplosion(position, surfaceNormal);
        }

        // Helper method to calculate realistic ricochet direction based on surface normal
        private Vector3 GetRicochetDirection(Vector3 surfaceNormal)
        {
            // Reflect the projectile direction off the surface
            Vector3 reflected = Vector3.Reflect(direction, surfaceNormal);

            // Add random spread to maintain magical burst look (30-60 degree cone)
            float spreadAngle = Random.Range(30f, 60f);
            Vector3 randomOffset = Random.insideUnitSphere * Mathf.Tan(spreadAngle * Mathf.Deg2Rad);

            // Blend reflected direction with random spread (70% reflection, 30% random)
            Vector3 ricochetDir = (reflected.normalized + randomOffset).normalized;

            // Ensure particles mostly go away from surface (dot product with normal should be positive)
            if (Vector3.Dot(ricochetDir, surfaceNormal) < 0.1f)
            {
                // If pointing too much into surface, blend more with surface normal
                ricochetDir = (ricochetDir + surfaceNormal * 0.5f).normalized;
            }

            return ricochetDir;
        }

        private void CreateFireExplosion(Vector3 position, Vector3 surfaceNormal)
        {
            GameObject explosion = new GameObject("FireExplosion");
            explosion.transform.position = position;

            // Expanding fire burst - particles ricochet off surface
            for (int i = 0; i < 12; i++)
            {
                GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flame.name = $"Flame{i}";
                flame.transform.SetParent(explosion.transform);
                flame.transform.localPosition = Vector3.zero;
                flame.transform.localScale = Vector3.one * 0.3f;

                Destroy(flame.GetComponent<Collider>());

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(1f, Random.Range(0.3f, 0.6f), 0f, 1f) * 2f;
                flame.GetComponent<MeshRenderer>().material = mat;

                FireExplosionParticle particle = flame.AddComponent<FireExplosionParticle>();
                particle.direction = GetRicochetDirection(surfaceNormal); // Realistic ricochet
                particle.speed = 2f;
            }

            Destroy(explosion, 1f);
        }

        private void CreateIceExplosion(Vector3 position, Vector3 surfaceNormal)
        {
            GameObject explosion = new GameObject("IceExplosion");
            explosion.transform.position = position;

            // Ice shards ricocheting off surface
            for (int i = 0; i < 10; i++)
            {
                GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = $"IceShard{i}";
                shard.transform.SetParent(explosion.transform);
                shard.transform.localPosition = Vector3.zero;
                shard.transform.localScale = new Vector3(0.1f, 0.4f, 0.1f);
                shard.transform.localRotation = Random.rotation;

                Destroy(shard.GetComponent<Collider>());

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(0.6f, 0.9f, 1f, 1f) * 1.8f;
                shard.GetComponent<MeshRenderer>().material = mat;

                IceExplosionParticle particle = shard.AddComponent<IceExplosionParticle>();
                particle.direction = GetRicochetDirection(surfaceNormal); // Realistic ricochet
                particle.speed = 2.5f;
            }

            Destroy(explosion, 1.2f);
        }

        private void CreateLightningExplosion(Vector3 position, Vector3 surfaceNormal)
        {
            GameObject explosion = new GameObject("LightningExplosion");
            explosion.transform.position = position;

            // Electric blast ricocheting off surface
            for (int i = 0; i < 15; i++)
            {
                GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spark.name = $"Spark{i}";
                spark.transform.SetParent(explosion.transform);
                spark.transform.localPosition = Vector3.zero;
                spark.transform.localScale = Vector3.one * 0.25f;

                Destroy(spark.GetComponent<Collider>());

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(1f, 1f, 0.95f, 1f) * 3f;
                spark.GetComponent<MeshRenderer>().material = mat;

                LightningExplosionParticle particle = spark.AddComponent<LightningExplosionParticle>();
                particle.direction = GetRicochetDirection(surfaceNormal); // Realistic ricochet
                particle.speed = 3.5f;
            }

            Destroy(explosion, 0.8f);
        }

        private void CreateWindExplosion(Vector3 position, Vector3 surfaceNormal)
        {
            GameObject explosion = new GameObject("WindExplosion");
            explosion.transform.position = position;

            // Outward wind burst ricocheting off surface
            for (int i = 0; i < 16; i++)
            {
                GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                particle.name = $"WindParticle{i}";
                particle.transform.SetParent(explosion.transform);
                particle.transform.localPosition = Vector3.zero;
                particle.transform.localScale = Vector3.one * 0.2f;

                Destroy(particle.GetComponent<Collider>());

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(0.85f, 0.95f, 1f, 1f) * 2f;
                particle.GetComponent<MeshRenderer>().material = mat;

                WindExplosionParticle windParticle = particle.AddComponent<WindExplosionParticle>();
                windParticle.direction = GetRicochetDirection(surfaceNormal); // Realistic ricochet
                windParticle.speed = 2.8f;
            }

            Destroy(explosion, 1f);
        }
    }

    #endregion

    #region Projectile Animations

    public class FireballAnimation : MonoBehaviour
    {
        private float time;

        void Update()
        {
            time += Time.deltaTime;

            // Pulse core
            Transform core = transform.Find("FireCore");
            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(time * 8f) * 0.2f;
                core.localScale = Vector3.one * 0.3f * pulse;
            }

            // Orbit trailing flames
            for (int i = 0; i < 3; i++)
            {
                Transform trail = transform.Find($"FireTrail{i}");
                if (trail != null)
                {
                    float angle = (time * 5f + i * 120f) * Mathf.Deg2Rad;
                    float radius = 0.15f;
                    trail.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius,
                        -0.2f
                    );
                }
            }

            // Spin
            transform.Rotate(Vector3.forward, 200f * Time.deltaTime);
        }
    }

    public class IceShardAnimation : MonoBehaviour
    {
        private float time;

        void Update()
        {
            time += Time.deltaTime;

            // Spiral crystals behind main shard
            for (int i = 0; i < 4; i++)
            {
                Transform crystal = transform.Find($"IceCrystal{i}");
                if (crystal != null)
                {
                    float angle = (time * 4f + i * 90f) * Mathf.Deg2Rad;
                    float radius = 0.1f;
                    float z = -0.3f - (i * 0.1f);
                    crystal.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius,
                        z
                    );
                    crystal.localRotation = Quaternion.Euler(0f, time * 200f, 0f);
                }
            }

            // Main shard rotation
            Transform main = transform.Find("IceMain");
            if (main != null)
            {
                main.localRotation = Quaternion.Euler(0f, 0f, time * 300f);
            }
        }
    }

    public class LightningAnimation : MonoBehaviour
    {
        private float time;

        void Update()
        {
            time += Time.deltaTime;

            // Flickering core
            Transform core = transform.Find("LightningCore");
            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(time * 15f) * 0.3f;
                core.localScale = Vector3.one * 0.25f * pulse;
            }

            // Chaotic electric arcs
            for (int i = 0; i < 6; i++)
            {
                Transform arc = transform.Find($"Arc{i}");
                if (arc != null)
                {
                    float angle = (time * 8f + i * 60f) * Mathf.Deg2Rad;
                    float radius = 0.15f + Mathf.PerlinNoise(time * 10f, i) * 0.1f;
                    arc.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius,
                        0f
                    );

                    // Random rotation
                    arc.localRotation = Quaternion.Euler(
                        Mathf.PerlinNoise(time * 5f, i) * 360f,
                        Mathf.PerlinNoise(time * 5f, i + 10f) * 360f,
                        0f
                    );

                    // Flicker alpha
                    float alpha = Mathf.PerlinNoise(time * 20f, i) > 0.4f ? 0.9f : 0.5f;
                    arc.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 0.9f, alpha);
                }
            }
        }
    }

    public class WindBlastAnimation : MonoBehaviour
    {
        private float time;

        void Update()
        {
            time += Time.deltaTime;

            // Pulse core
            Transform core = transform.Find("WindCore");
            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(time * 10f) * 0.15f;
                core.localScale = Vector3.one * 0.3f * pulse;
            }

            // Pulsing swirl
            Transform swirl = transform.Find("WindSwirl");
            if (swirl != null)
            {
                float pulse = 1f + Mathf.Sin(time * 12f) * 0.2f;
                swirl.localScale = Vector3.one * 0.5f * pulse;
            }

            // Swirling particles (12 spiraling streams)
            int particleCount = 12;
            for (int i = 0; i < particleCount; i++)
            {
                Transform particle = transform.Find($"WindParticle{i}");
                if (particle != null)
                {
                    // Three interweaving spiral streams
                    float streamOffset = (i % 3) * 120f; // 3 streams at 120° apart
                    float t = (time * 3f + i / (float)particleCount) % 1f;
                    float angle = (t * 720f + streamOffset) * Mathf.Deg2Rad; // Two full rotations
                    float radius = 0.25f * (1f - t * 0.7f); // Tighter spiral
                    float z = t * 0.6f - 0.3f; // Spread along forward axis

                    particle.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius,
                        z
                    );

                    // Size variation - particles grow then shrink
                    float sizePulse = Mathf.Sin(t * Mathf.PI) * 0.5f + 0.7f;
                    particle.localScale = Vector3.one * 0.12f * sizePulse;
                }
            }

            // Spin entire projectile for additional motion
            transform.Rotate(Vector3.forward, 180f * Time.deltaTime);
        }
    }

    #endregion

    #region Explosion Particle Animations

    public class FireExplosionParticle : MonoBehaviour
    {
        public Vector3 direction;
        public float speed = 2f;
        private float elapsed = 0f;
        private float lifetime = 1f;

        void Update()
        {
            elapsed += Time.deltaTime;

            // Move in the ricochet direction
            transform.position += direction * speed * Time.deltaTime;

            // Fade out
            float alpha = 1f - (elapsed / lifetime);
            if (alpha < 0) alpha = 0;

            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null && renderer.material != null)
            {
                Color col = renderer.material.color;
                col.a = alpha;
                renderer.material.color = col;
            }

            if (elapsed >= lifetime)
                Destroy(gameObject);
        }
    }

    public class IceExplosionParticle : MonoBehaviour
    {
        public Vector3 direction;
        public float speed = 2.5f;
        private float elapsed = 0f;
        private float lifetime = 1.2f;

        void Update()
        {
            elapsed += Time.deltaTime;

            // Move outward
            transform.position += direction * speed * Time.deltaTime;

            // Spin
            transform.Rotate(Vector3.up, 800f * Time.deltaTime);

            // Fade out
            float alpha = 1f - (elapsed / lifetime);
            if (alpha < 0) alpha = 0;

            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null && renderer.material != null)
            {
                Color col = renderer.material.color;
                col.a = alpha;
                renderer.material.color = col;
            }

            if (elapsed >= lifetime)
                Destroy(gameObject);
        }
    }

    public class LightningExplosionParticle : MonoBehaviour
    {
        public Vector3 direction;
        public float speed = 3.5f;
        private float elapsed = 0f;
        private float lifetime = 0.8f;

        void Update()
        {
            elapsed += Time.deltaTime;

            // Fast chaotic movement
            Vector3 randomOffset = new Vector3(
                Mathf.PerlinNoise(Time.time * 10f, elapsed) - 0.5f,
                Mathf.PerlinNoise(Time.time * 10f + 10f, elapsed) - 0.5f,
                Mathf.PerlinNoise(Time.time * 10f + 20f, elapsed) - 0.5f
            );
            transform.position += (direction + randomOffset * 0.5f) * speed * Time.deltaTime;

            // Flicker
            float flicker = Mathf.PerlinNoise(Time.time * 30f, elapsed) > 0.4f ? 1f : 0.6f;
            float alpha = (1f - (elapsed / lifetime)) * flicker;
            if (alpha < 0) alpha = 0;

            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null && renderer.material != null)
            {
                Color col = renderer.material.color;
                col.a = alpha;
                renderer.material.color = col;
            }

            if (elapsed >= lifetime)
                Destroy(gameObject);
        }
    }

    public class WindExplosionParticle : MonoBehaviour
    {
        public Vector3 direction;
        public float speed = 2.8f;
        private float elapsed = 0f;
        private float lifetime = 1f;

        void Update()
        {
            elapsed += Time.deltaTime;

            // Spiral outward
            float spiralAngle = elapsed * 360f * 3f;
            Vector3 spiralOffset = Quaternion.Euler(0f, spiralAngle, 0f) * direction;
            transform.position += spiralOffset * speed * Time.deltaTime;

            // Fade out
            float alpha = 1f - (elapsed / lifetime);
            if (alpha < 0) alpha = 0;

            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null && renderer.material != null)
            {
                Color col = renderer.material.color;
                col.a = alpha;
                renderer.material.color = col;
            }

            if (elapsed >= lifetime)
                Destroy(gameObject);
        }
    }

    #endregion
}
