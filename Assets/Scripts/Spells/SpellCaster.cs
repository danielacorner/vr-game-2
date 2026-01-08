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
        public float spawnDistance = 0.3f; // Increased from 0.15f to avoid hitting player

        [Tooltip("Cooldown between casts")]
        public float castCooldown = 0.5f;

        [Tooltip("Time to charge spell before ready to fire")]
        public float chargeUpTime = 0.5f; // 500ms charge time

        private InputDevice device;
        private bool deviceFound = false;
        private float lastCastTime = 0f;
        private bool triggerPressed = false;

        // Charge-up state tracking
        private bool isCharging = false;
        private bool isFullyCharged = false;
        private float chargeStartTime = 0f;
        private GameObject chargeEffect = null;
        private GameObject chargeBubble = null; // Visual bubble that grows with charge
        private float currentChargeProgress = 0f; // 0-1, persists during fade-out
        private bool isFadingOut = false;

        // Hand velocity tracking for throwing
        private Vector3 currentHandVelocity = Vector3.zero;
        private Vector3 releaseVelocity = Vector3.zero;

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

            // Track hand velocity continuously for throwing physics
            if (device.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 velocity))
            {
                currentHandVelocity = velocity;
            }

            // Check trigger input
            bool triggerValue = false;
            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out bool buttonValue))
            {
                triggerValue = buttonValue;
            }

            // Handle charge fade-out when not charging
            if (isFadingOut && !triggerValue)
            {
                UpdateFadeOut();
            }

            // Handle charge-up mechanic
            if (triggerValue && !triggerPressed)
            {
                // Trigger just pressed - start charging (or resume if fading)
                triggerPressed = true;
                StartCharging();
            }
            else if (triggerValue && triggerPressed)
            {
                // Trigger held - update charge state
                UpdateCharging();
            }
            else if (!triggerValue && triggerPressed)
            {
                // Trigger released - capture velocity and fire if fully charged
                triggerPressed = false;
                releaseVelocity = currentHandVelocity; // Capture velocity at moment of release
                if (isFullyCharged)
                {
                    TryCastSpell();
                }
                StopCharging();
            }
        }

        private void StartCharging()
        {
            // Get current spell
            if (SpellManager.Instance == null || SpellManager.Instance.currentSpell == null)
            {
                Debug.LogWarning("[SpellCaster] No spell selected!");
                return;
            }

            // If we're resuming from fade-out, adjust start time to account for existing progress
            if (isFadingOut && currentChargeProgress > 0f)
            {
                // Resume from current progress
                chargeStartTime = Time.time - (currentChargeProgress * chargeUpTime);
                isFadingOut = false;
                Debug.Log($"[SpellCaster] Resuming charge from {currentChargeProgress * 100f:F0}%");
            }
            else
            {
                // Start fresh
                chargeStartTime = Time.time;
                currentChargeProgress = 0f;

                // Create charge-up visual effect in hand
                CreateChargeEffect(SpellManager.Instance.currentSpell);

                // Create charging bubble
                CreateChargingBubble(SpellManager.Instance.currentSpell);

                Debug.Log($"[SpellCaster] Started charging {SpellManager.Instance.currentSpell.spellName}");
            }

            isCharging = true;
            isFullyCharged = false;
            isFadingOut = false;
        }

        private void UpdateCharging()
        {
            if (!isCharging) return;

            currentChargeProgress = (Time.time - chargeStartTime) / chargeUpTime;
            float chargeProgress = currentChargeProgress;

            // Send continuous low rumble while charging (but stop once fully charged)
            if (deviceFound && !isFullyCharged)
            {
                // Low intensity rumble (0.15 amplitude) with short pulses
                device.SendHapticImpulse(0, 0.15f, 0.05f);
            }

            if (chargeProgress >= 1f && !isFullyCharged)
            {
                // Fully charged!
                isFullyCharged = true;
                Debug.Log("[SpellCaster] Spell fully charged! Release trigger to fire.");

                // Strong haptic pulse to indicate fully charged
                if (deviceFound)
                {
                    device.SendHapticImpulse(0, 0.8f, 0.15f);
                }

                // Destroy charging bubble and create "pop" animation
                if (chargeBubble != null)
                {
                    Destroy(chargeBubble);
                    chargeBubble = null;
                }
                CreateChargeCompletePopEffect();

                // Enhance charge effect to show it's ready to fire
                if (chargeEffect != null)
                {
                    // Increase intensity of charge effect
                    ParticleSystem[] particles = chargeEffect.GetComponentsInChildren<ParticleSystem>();
                    foreach (ParticleSystem ps in particles)
                    {
                        var emission = ps.emission;
                        emission.rateOverTime = emission.rateOverTime.constant * 1.5f;
                    }
                }
            }

            // Update charge effect scale based on progress
            if (chargeEffect != null)
            {
                float scale = Mathf.Lerp(0.15f, 0.4f, Mathf.Min(chargeProgress, 1f));
                chargeEffect.transform.localScale = Vector3.one * scale;
            }

            // Update charging bubble scale to grow with progress
            if (chargeBubble != null)
            {
                // Bubble grows from 0.1 to 0.6 as charge progresses
                float bubbleScale = Mathf.Lerp(0.1f, 0.6f, Mathf.Min(chargeProgress, 1f));
                chargeBubble.transform.localScale = Vector3.one * bubbleScale;
            }
        }

        private void StopCharging()
        {
            isCharging = false;
            isFullyCharged = false;

            // Start fade-out instead of destroying immediately
            if (chargeEffect != null && currentChargeProgress > 0f)
            {
                isFadingOut = true;
                Debug.Log("[SpellCaster] Starting charge fade-out");
            }
            else if (chargeEffect != null)
            {
                // No progress, destroy immediately
                Destroy(chargeEffect);
                chargeEffect = null;
                currentChargeProgress = 0f;
            }
        }

        private void UpdateFadeOut()
        {
            if (!isFadingOut || chargeEffect == null) return;

            // Fade out at same rate as charging (0.5s to fully fade)
            currentChargeProgress -= Time.deltaTime / chargeUpTime;

            if (currentChargeProgress <= 0f)
            {
                // Fully faded - destroy effect
                currentChargeProgress = 0f;
                isFadingOut = false;
                if (chargeEffect != null)
                {
                    Destroy(chargeEffect);
                    chargeEffect = null;
                }
                Debug.Log("[SpellCaster] Charge fully faded out");
            }
            else
            {
                // Update effect scale based on remaining progress
                if (chargeEffect != null)
                {
                    float scale = Mathf.Lerp(0.15f, 0.4f, currentChargeProgress);
                    chargeEffect.transform.localScale = Vector3.one * scale;

                    // Also fade out particle emission
                    ParticleSystem[] particles = chargeEffect.GetComponentsInChildren<ParticleSystem>();
                    foreach (ParticleSystem ps in particles)
                    {
                        var main = ps.main;
                        Color color = main.startColor.color;
                        color.a = currentChargeProgress;
                        main.startColor = color;
                    }
                }

                // Shrink charging bubble as charge fades
                if (chargeBubble != null)
                {
                    float bubbleScale = Mathf.Lerp(0.1f, 0.6f, currentChargeProgress);
                    chargeBubble.transform.localScale = Vector3.one * bubbleScale;

                    // Also fade bubble alpha
                    MeshRenderer renderer = chargeBubble.GetComponent<MeshRenderer>();
                    if (renderer != null && renderer.material != null)
                    {
                        Color baseColor = renderer.material.GetColor("_BaseColor");
                        baseColor.a = 0.3f * currentChargeProgress;
                        renderer.material.SetColor("_BaseColor", baseColor);
                    }
                }
            }
        }

        private void CreateChargeCompletePopEffect()
        {
            if (chargeEffect == null) return;

            // Get current spell for color
            SpellData spell = SpellManager.Instance.currentSpell;
            if (spell == null) return;

            // Create expanding sphere at charge effect position
            GameObject popSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            popSphere.name = "ChargeCompletePop";
            popSphere.transform.position = chargeEffect.transform.position;
            popSphere.transform.localScale = Vector3.one * 0.1f;
            Destroy(popSphere.GetComponent<Collider>());

            // Set up material based on spell type
            Material popMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            popMat.EnableKeyword("_EMISSION");

            string spellName = spell.spellName.ToLower();
            Color emissionColor;
            if (spellName.Contains("fire") || spellName.Contains("flame"))
                emissionColor = new Color(2f, 1.2f, 0.4f);
            else if (spellName.Contains("ice") || spellName.Contains("frost") || spellName.Contains("shard"))
                emissionColor = new Color(0.6f, 0.85f, 1f);
            else if (spellName.Contains("light") || spellName.Contains("thunder") || spellName.Contains("bolt"))
                emissionColor = new Color(0.9f, 0.95f, 1f);
            else if (spellName.Contains("wind") || spellName.Contains("air") || spellName.Contains("blast"))
                emissionColor = new Color(0.85f, 0.93f, 1f);
            else
                emissionColor = spell.spellColor;

            popMat.SetColor("_BaseColor", new Color(emissionColor.r, emissionColor.g, emissionColor.b, 0.8f));
            popMat.SetColor("_EmissionColor", emissionColor * 8f);
            popMat.SetFloat("_Surface", 1); // Transparent
            popMat.SetFloat("_Blend", 0); // Alpha blend
            popMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            popMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            popMat.SetFloat("_ZWrite", 0);
            popMat.SetInt("_AlphaClip", 0);
            popMat.renderQueue = 3000;
            popSphere.GetComponent<MeshRenderer>().material = popMat;

            // Add animation component
            ChargePopAnimation popAnim = popSphere.AddComponent<ChargePopAnimation>();
            popAnim.isQuickPop = true; // Make it a quick pop
        }

        private void CreateChargingBubble(SpellData spell)
        {
            if (chargeEffect == null) return;

            // Create sphere at charge effect position
            chargeBubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            chargeBubble.name = "ChargingBubble";
            chargeBubble.transform.SetParent(chargeEffect.transform);
            chargeBubble.transform.localPosition = Vector3.zero;
            chargeBubble.transform.localScale = Vector3.one * 0.1f; // Start small
            Destroy(chargeBubble.GetComponent<Collider>());

            // Set up material based on spell type
            Material bubbleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            bubbleMat.EnableKeyword("_EMISSION");

            string spellName = spell.spellName.ToLower();
            Color emissionColor;
            if (spellName.Contains("fire") || spellName.Contains("flame"))
                emissionColor = new Color(2f, 1.2f, 0.4f);
            else if (spellName.Contains("ice") || spellName.Contains("frost") || spellName.Contains("shard"))
                emissionColor = new Color(0.6f, 0.85f, 1f);
            else if (spellName.Contains("light") || spellName.Contains("thunder") || spellName.Contains("bolt"))
                emissionColor = new Color(0.9f, 0.95f, 1f);
            else if (spellName.Contains("wind") || spellName.Contains("air") || spellName.Contains("blast"))
                emissionColor = new Color(0.85f, 0.93f, 1f);
            else
                emissionColor = spell.spellColor;

            bubbleMat.SetColor("_BaseColor", new Color(emissionColor.r, emissionColor.g, emissionColor.b, 0.3f));
            bubbleMat.SetColor("_EmissionColor", emissionColor * 4f);
            bubbleMat.SetFloat("_Surface", 1); // Transparent
            bubbleMat.SetFloat("_Blend", 0); // Alpha blend
            bubbleMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            bubbleMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            bubbleMat.SetFloat("_ZWrite", 0);
            bubbleMat.SetInt("_AlphaClip", 0);
            bubbleMat.renderQueue = 3000;
            chargeBubble.GetComponent<MeshRenderer>().material = bubbleMat;
        }

        private void TryCastSpell()
        {
            // Get current spell
            if (SpellManager.Instance == null || SpellManager.Instance.currentSpell == null)
            {
                Debug.LogWarning("[SpellCaster] No spell selected!");
                return;
            }

            // Strong haptic pulse when firing
            if (deviceFound)
            {
                device.SendHapticImpulse(0, 1.0f, 0.12f);
            }

            SpellData spell = SpellManager.Instance.currentSpell;
            CastSpell(spell);
            lastCastTime = Time.time;

            // Reset charge progress after firing
            currentChargeProgress = 0f;
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

            // Check if tier 2 - use physics-based projectiles
            if (spell.tier == 2)
            {
                if (spellName.Contains("fire") || spellName.Contains("flame") || spellName.Contains("meteor"))
                    projectile = CreateMeteor(spell);
                else if (spellName.Contains("ice") || spellName.Contains("frost") || spellName.Contains("boulder"))
                    projectile = CreateFrostBoulder(spell);
                else if (spellName.Contains("light") || spellName.Contains("thunder") || spellName.Contains("orb"))
                    projectile = CreateThunderOrb(spell);
                else if (spellName.Contains("wind") || spellName.Contains("air") || spellName.Contains("cyclone"))
                    projectile = CreateCyclone(spell);
                else
                    projectile = CreateDefaultTier2Projectile(spell);

                projectile.transform.position = position;
                projectile.transform.rotation = Quaternion.LookRotation(direction);

                PhysicsSpellProjectile physicsProj = projectile.GetComponent<PhysicsSpellProjectile>();
                if (physicsProj != null)
                {
                    // Use hand velocity with ball-launcher boost for satisfying long throws
                    physicsProj.ThrowWithVelocity(releaseVelocity, velocityBoost: 4.5f);
                }
            }
            else
            {
                // Tier 1 - use original linear movement
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

                // Add collider and rigidbody for collision detection
                SphereCollider collider = projectile.AddComponent<SphereCollider>();
                collider.radius = 0.3f;
                collider.isTrigger = true;

                Rigidbody rb = projectile.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;

                // Add movement component for tier 1
                LinearSpellMovement movement = projectile.AddComponent<LinearSpellMovement>();
                movement.direction = direction;
                movement.speed = spell.projectileSpeed;
                movement.lifetime = 5f;
                movement.spellData = spell;
            }

            return projectile;
        }

        #region Projectile Visuals

        private void CreateChargeEffect(SpellData spell)
        {
            string spellName = spell.spellName.ToLower();

            // Create charge effect at hand position
            chargeEffect = new GameObject($"ChargeEffect_{spell.spellName}");
            chargeEffect.transform.SetParent(spawnPoint);
            chargeEffect.transform.localPosition = Vector3.zero;
            chargeEffect.transform.localRotation = Quaternion.identity;
            chargeEffect.transform.localScale = Vector3.one * 0.15f; // Start very small

            // Create spell-specific charge effect
            if (spellName.Contains("fire") || spellName.Contains("flame"))
                CreateFireChargeEffect(chargeEffect);
            else if (spellName.Contains("ice") || spellName.Contains("frost") || spellName.Contains("shard"))
                CreateIceChargeEffect(chargeEffect);
            else if (spellName.Contains("light") || spellName.Contains("thunder") || spellName.Contains("bolt"))
                CreateLightningChargeEffect(chargeEffect);
            else if (spellName.Contains("wind") || spellName.Contains("air") || spellName.Contains("blast"))
                CreateWindChargeEffect(chargeEffect);
            else
                CreateDefaultChargeEffect(chargeEffect, spell);
        }

        private void CreateFireChargeEffect(GameObject parent)
        {
            // Create fire particles in hand
            GameObject fireObj = new GameObject("FireCharge");
            fireObj.transform.SetParent(parent.transform);
            fireObj.transform.localPosition = Vector3.zero;

            ParticleSystem ps = fireObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.25f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0f, 0.03f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.002f, 0.006f);
            main.maxParticles = 60;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = ps.emission;
            emission.rateOverTime = 50f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.012f;

            // Fire colors
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(2f, 2f, 1.5f), 0f),  // HDR white
                    new GradientColorKey(new Color(2f, 1.2f, 0.4f), 0.5f),  // HDR orange
                    new GradientColorKey(new Color(1.5f, 0.5f, 0.2f), 1f)   // Red
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.5f, 0f),
                    new GradientAlphaKey(0.3f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            // Renderer setup
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.mainTexture = GetFireParticleTexture();
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 1); // Additive
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            mat.SetFloat("_ZWrite", 0);
            renderer.material = mat;

            // Add glowing sphere core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "FireCore";
            core.transform.SetParent(parent.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.08f;
            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetColor("_BaseColor", new Color(1f, 0.6f, 0.3f));
            coreMat.SetColor("_EmissionColor", new Color(2f, 1.2f, 0.4f) * 8f);
            core.GetComponent<MeshRenderer>().material = coreMat;
        }

        private void CreateIceChargeEffect(GameObject parent)
        {
            // Ice particles
            GameObject iceObj = new GameObject("IceCharge");
            iceObj.transform.SetParent(parent.transform);
            iceObj.transform.localPosition = Vector3.zero;

            ParticleSystem ps = iceObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.3f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0f, 0.025f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.002f, 0.005f);
            main.maxParticles = 50;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = ps.emission;
            emission.rateOverTime = 40f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.012f;

            // Ice colors
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.8f, 0.95f, 1f), 0f),
                    new GradientColorKey(new Color(0.5f, 0.85f, 1f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.6f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.mainTexture = GetSoftParticleTexture();
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 1);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            mat.SetFloat("_ZWrite", 0);
            renderer.material = mat;

            // Glowing ice core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "IceCore";
            core.transform.SetParent(parent.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.08f;
            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetColor("_BaseColor", new Color(0.5f, 0.85f, 1f));
            coreMat.SetColor("_EmissionColor", new Color(0.6f, 0.85f, 1f) * 6f);
            core.GetComponent<MeshRenderer>().material = coreMat;
        }

        private void CreateLightningChargeEffect(GameObject parent)
        {
            // Electric particles
            GameObject lightningObj = new GameObject("LightningCharge");
            lightningObj.transform.SetParent(parent.transform);
            lightningObj.transform.localPosition = Vector3.zero;

            ParticleSystem ps = lightningObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.2f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.01f, 0.04f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.002f, 0.006f);
            main.maxParticles = 70;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = ps.emission;
            emission.rateOverTime = 60f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.015f;

            // Lightning colors
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 1f, 0.95f), 0f),
                    new GradientColorKey(new Color(0.85f, 0.92f, 1f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.7f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.mainTexture = GetSoftParticleTexture();
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 1);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            mat.SetFloat("_ZWrite", 0);
            renderer.material = mat;

            // Bright electric core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "LightningCore";
            core.transform.SetParent(parent.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.08f;
            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetColor("_BaseColor", new Color(0.95f, 0.98f, 1f));
            coreMat.SetColor("_EmissionColor", new Color(0.9f, 0.95f, 1f) * 10f);
            core.GetComponent<MeshRenderer>().material = coreMat;
        }

        private void CreateWindChargeEffect(GameObject parent)
        {
            // Wind particles
            GameObject windObj = new GameObject("WindCharge");
            windObj.transform.SetParent(parent.transform);
            windObj.transform.localPosition = Vector3.zero;

            ParticleSystem ps = windObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.3f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.003f, 0.008f);
            main.maxParticles = 55;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = ps.emission;
            emission.rateOverTime = 45f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.015f;

            // Wind colors
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.9f, 0.95f, 1f), 0f),
                    new GradientColorKey(new Color(0.8f, 0.9f, 1f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.5f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            // Add rotation for swirling effect
            var rotationOverLifetime = ps.rotationOverLifetime;
            rotationOverLifetime.enabled = true;
            rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-180f, 180f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.mainTexture = GetSoftParticleTexture();
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 1);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            mat.SetFloat("_ZWrite", 0);
            renderer.material = mat;

            // Glowing wind core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "WindCore";
            core.transform.SetParent(parent.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.08f;
            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetColor("_BaseColor", new Color(0.85f, 0.95f, 1f));
            coreMat.SetColor("_EmissionColor", new Color(0.85f, 0.93f, 1f) * 5f);
            core.GetComponent<MeshRenderer>().material = coreMat;
        }

        private void CreateDefaultChargeEffect(GameObject parent, SpellData spell)
        {
            // Generic charge effect
            GameObject defaultObj = new GameObject("DefaultCharge");
            defaultObj.transform.SetParent(parent.transform);
            defaultObj.transform.localPosition = Vector3.zero;

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(defaultObj.transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = Vector3.one * 0.08f;
            Destroy(sphere.GetComponent<Collider>());

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_BaseColor", spell.spellColor);
            mat.SetColor("_EmissionColor", spell.spellColor * 5f);
            sphere.GetComponent<MeshRenderer>().material = mat;
        }

        // Create organic fire texture with noise (shared across all fireballs)
        private static Texture2D fireParticleTexture;

        private Texture2D GetFireParticleTexture()
        {
            if (fireParticleTexture != null) return fireParticleTexture;

            // Create ULTRA-soft cloud-like texture with multiple noise octaves
            int size = 256;
            fireParticleTexture = new Texture2D(size, size, TextureFormat.RGBA32, true);
            fireParticleTexture.wrapMode = TextureWrapMode.Clamp;
            fireParticleTexture.filterMode = FilterMode.Trilinear;
            fireParticleTexture.anisoLevel = 16; // Maximum anisotropic filtering

            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxDist = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center) / maxDist;

                    // Multi-octave noise for cloud-like appearance
                    float noise1 = Mathf.PerlinNoise(x * 0.03f, y * 0.03f) * 0.4f;        // Large features
                    float noise2 = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.2f;        // Medium detail
                    float noise3 = Mathf.PerlinNoise(x * 0.15f + 100f, y * 0.15f) * 0.1f; // Fine detail
                    float combinedNoise = noise1 + noise2 + noise3;

                    // EXTREME soft falloff - almost gaussian
                    float alpha = Mathf.Clamp01(1f - distance + combinedNoise);
                    alpha = Mathf.SmoothStep(0f, 1f, alpha);
                    alpha = Mathf.Pow(alpha, 4f); // Quartic falloff = ultra-soft edges

                    // Additional edge erosion
                    if (distance > 0.7f)
                    {
                        alpha *= Mathf.Pow(1f - ((distance - 0.7f) / 0.3f), 3f);
                    }

                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            fireParticleTexture.SetPixels(pixels);
            fireParticleTexture.Apply();

            return fireParticleTexture;
        }

        // Create soft gradient for non-fire effects
        private static Texture2D softParticleTexture;

        private Texture2D GetSoftParticleTexture()
        {
            if (softParticleTexture != null) return softParticleTexture;

            int size = 256;
            softParticleTexture = new Texture2D(size, size, TextureFormat.RGBA32, true);
            softParticleTexture.wrapMode = TextureWrapMode.Clamp;
            softParticleTexture.filterMode = FilterMode.Trilinear;
            softParticleTexture.anisoLevel = 16; // Maximum filtering

            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxDist = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center) / maxDist;

                    // Multi-octave wispy smoke noise
                    float noise1 = Mathf.PerlinNoise(x * 0.04f, y * 0.04f) * 0.5f;
                    float noise2 = Mathf.PerlinNoise(x * 0.12f + 50f, y * 0.12f) * 0.25f;
                    float combinedNoise = noise1 + noise2;

                    // Extreme soft falloff for smoke
                    float alpha = Mathf.Clamp01(1f - distance + combinedNoise);
                    alpha = Mathf.Pow(alpha, 4f); // Quartic = ultra soft

                    // Extra edge fade
                    if (distance > 0.65f)
                    {
                        alpha *= Mathf.Pow(1f - ((distance - 0.65f) / 0.35f), 2f);
                    }

                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            softParticleTexture.SetPixels(pixels);
            softParticleTexture.Apply();

            return softParticleTexture;
        }

        private GameObject CreateFireball(SpellData spell)
        {
            GameObject projectile = new GameObject($"Fireball_{spell.spellName}");

            Texture2D fireTex = GetFireParticleTexture();
            Texture2D softTex = GetSoftParticleTexture();

            // === 1. DENSE FIRE CORE PARTICLES (volumetric fire look) ===
            GameObject fireCoreObj = new GameObject("FireCore");
            fireCoreObj.transform.SetParent(projectile.transform);

            ParticleSystem fireCore = fireCoreObj.AddComponent<ParticleSystem>();
            var coreMain = fireCore.main;
            coreMain.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.25f);
            coreMain.startSpeed = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f); // 50% speed range
            coreMain.startSize = new ParticleSystem.MinMaxCurve(0.0075f, 0.02f); // 50% smaller particles
            coreMain.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            coreMain.maxParticles = 2500;
            coreMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var coreEmission = fireCore.emission;
            coreEmission.rateOverTime = 3000f;

            var coreShape = fireCore.shape;
            coreShape.shapeType = ParticleSystemShapeType.Sphere;
            coreShape.radius = 0.075f; // 50% smaller radius (0.15 → 0.075)

            // Add organic turbulence to break up uniformity
            var noise = fireCore.noise;
            noise.enabled = true;
            noise.strength = 0.3f;
            noise.frequency = 1.5f;
            noise.scrollSpeed = 0.5f;
            noise.damping = true;
            noise.octaveCount = 2;
            noise.quality = ParticleSystemNoiseQuality.High;

            // Enable rotation to constantly change particle orientation (breaks up square look!)
            var coreRotation = fireCore.rotationOverLifetime;
            coreRotation.enabled = true;
            coreRotation.z = new ParticleSystem.MinMaxCurve(-180f, 180f); // Random rotation speeds
            coreRotation.separateAxes = false;

            var coreColor = fireCore.colorOverLifetime;
            coreColor.enabled = true;
            Gradient coreGrad = new Gradient();
            coreGrad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(3f, 3f, 2.5f), 0f),      // HDR White (3x bloom)
                    new GradientColorKey(new Color(2.5f, 2.2f, 1.2f), 0.15f), // HDR Yellow
                    new GradientColorKey(new Color(2f, 1.2f, 0.4f), 0.5f),  // HDR Orange
                    new GradientColorKey(new Color(1.5f, 0.5f, 0.2f), 0.85f), // HDR Red-orange
                    new GradientColorKey(new Color(0.8f, 0.2f, 0f), 1f)    // Dim red
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.02f, 0f),  // Very low opacity for 3000 tiny particles
                    new GradientAlphaKey(0.015f, 0.5f), // Nearly invisible individually
                    new GradientAlphaKey(0f, 1f)      // Fully fade out
                }
            );
            coreColor.color = coreGrad;

            var coreRenderer = fireCore.GetComponent<ParticleSystemRenderer>();
            coreRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            coreRenderer.alignment = ParticleSystemRenderSpace.View; // Always face camera in VR

            // Try Unity's BUILT-IN Default-Particle texture (professionally designed)
            Texture2D builtinTex = Resources.GetBuiltinResource<Texture2D>("Default-Particle.psd");
            if (builtinTex == null)
            {
                // Fallback to our custom texture
                builtinTex = fireTex;
                Debug.LogWarning("[SpellCaster] Using fallback texture - built-in not found");
            }

            // Use simple Unlit shader that works on mobile
            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            coreMat.mainTexture = builtinTex;
            coreMat.SetColor("_BaseColor", Color.white);
            coreMat.SetFloat("_Surface", 1); // Transparent
            coreMat.SetFloat("_Blend", 1); // Additive
            coreMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
            coreMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            coreMat.SetFloat("_ZWrite", 0);
            coreMat.renderQueue = 3000;

            coreRenderer.material = coreMat;

            // === 2. HEAT DISTORTION PARTICLES (simulates heat haze) ===
            GameObject heatDistObj = new GameObject("HeatDistortion");
            heatDistObj.transform.SetParent(projectile.transform);

            ParticleSystem heatDist = heatDistObj.AddComponent<ParticleSystem>();
            var distMain = heatDist.main;
            distMain.startLifetime = 0.35f;
            distMain.startSpeed = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f); // 50% speed
            distMain.startSize = new ParticleSystem.MinMaxCurve(0.125f, 0.2f); // 50% smaller
            distMain.maxParticles = 120;
            distMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var distEmission = heatDist.emission;
            distEmission.rateOverTime = 400f;

            var distShape = heatDist.shape;
            distShape.shapeType = ParticleSystemShapeType.Sphere;
            distShape.radius = 0.1f; // 50% smaller radius (0.2 → 0.1)

            // Disable rotation to avoid square artifacts
            var distRotation = heatDist.rotationOverLifetime;
            distRotation.enabled = false;

            var distColor = heatDist.colorOverLifetime;
            distColor.enabled = true;
            Gradient distGrad = new Gradient();
            distGrad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.8f, 0.6f), 0f),
                    new GradientColorKey(new Color(1f, 0.6f, 0.3f), 0.5f),
                    new GradientColorKey(new Color(1f, 0.4f, 0.2f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.04f, 0f), // Ultra transparent
                    new GradientAlphaKey(0.02f, 0.5f), // Almost invisible
                    new GradientAlphaKey(0f, 1f)
                }
            );
            distColor.color = distGrad;

            var distRenderer = heatDist.GetComponent<ParticleSystemRenderer>();
            distRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            distRenderer.alignment = ParticleSystemRenderSpace.Facing;
            distRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            distRenderer.material.mainTexture = fireTex;
            distRenderer.material.SetColor("_BaseColor", Color.white);
            distRenderer.material.SetFloat("_Surface", 1);
            distRenderer.material.SetFloat("_Blend", 1);
            distRenderer.material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
            distRenderer.material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            distRenderer.material.SetFloat("_ZWrite", 0);
            distRenderer.material.renderQueue = 3000;

            // === 3. TRAILING FIRE EMBERS (comet tail) ===
            GameObject emberObj = new GameObject("Embers");
            emberObj.transform.SetParent(projectile.transform);

            ParticleSystem embers = emberObj.AddComponent<ParticleSystem>();
            var emberMain = embers.main;
            emberMain.startLifetime = 0.45f;
            emberMain.startSpeed = new ParticleSystem.MinMaxCurve(-1f, -0.5f); // 50% speed
            emberMain.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.025f); // 50% smaller embers
            emberMain.maxParticles = 100;

            var emberEmission = embers.emission;
            emberEmission.rateOverTime = 80f;

            var emberShape = embers.shape;
            emberShape.shapeType = ParticleSystemShapeType.Cone;
            emberShape.angle = 10f;
            emberShape.radius = 0.04f; // 50% smaller cone radius

            // Disable rotation to avoid square artifacts
            var emberRotation = embers.rotationOverLifetime;
            emberRotation.enabled = false;

            var emberColor = embers.colorOverLifetime;
            emberColor.enabled = true;
            Gradient emberGrad = new Gradient();
            emberGrad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(2.5f, 2.2f, 1.5f), 0f),  // HDR hot white-yellow
                    new GradientColorKey(new Color(2f, 1.2f, 0.5f), 0.3f),  // HDR bright orange
                    new GradientColorKey(new Color(1.5f, 0.6f, 0.2f), 0.7f), // HDR orange-red
                    new GradientColorKey(new Color(0.8f, 0.3f, 0f), 1f)      // Dim red ember
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.4f, 0f),   // More transparent
                    new GradientAlphaKey(0.25f, 0.5f), // Very transparent
                    new GradientAlphaKey(0f, 1f)
                }
            );
            emberColor.color = emberGrad;

            var emberRenderer = embers.GetComponent<ParticleSystemRenderer>();
            emberRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            emberRenderer.alignment = ParticleSystemRenderSpace.Facing;
            emberRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            emberRenderer.material.mainTexture = fireTex;
            emberRenderer.material.SetColor("_BaseColor", Color.white);
            emberRenderer.material.SetFloat("_Surface", 1);
            emberRenderer.material.SetFloat("_Blend", 1);
            emberRenderer.material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
            emberRenderer.material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            emberRenderer.material.SetFloat("_ZWrite", 0);
            emberRenderer.material.renderQueue = 3000;

            // === 4. DARK SMOKE WISPS (adds depth) ===
            GameObject smokeObj = new GameObject("Smoke");
            smokeObj.transform.SetParent(projectile.transform);

            ParticleSystem smoke = smokeObj.AddComponent<ParticleSystem>();
            var smokeMain = smoke.main;
            smokeMain.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.5f);
            smokeMain.startSpeed = new ParticleSystem.MinMaxCurve(-0.5f, -0.15f); // 50% speed
            smokeMain.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.06f); // 50% smaller smoke
            smokeMain.maxParticles = 300;
            smokeMain.simulationSpace = ParticleSystemSimulationSpace.World;
            smokeMain.gravityModifier = -0.1f;

            var smokeEmission = smoke.emission;
            smokeEmission.rateOverTime = 120f;

            var smokeShape = smoke.shape;
            smokeShape.shapeType = ParticleSystemShapeType.Cone;
            smokeShape.angle = 18f;
            smokeShape.radius = 0.06f; // 50% smaller cone radius

            // Disable rotation to avoid square artifacts
            var smokeRotation = smoke.rotationOverLifetime;
            smokeRotation.enabled = false;

            var smokeColor = smoke.colorOverLifetime;
            smokeColor.enabled = true;
            Gradient smokeGrad = new Gradient();
            smokeGrad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.15f, 0.08f, 0.03f), 0f),
                    new GradientColorKey(new Color(0.12f, 0.06f, 0.02f), 0.5f),
                    new GradientColorKey(new Color(0.08f, 0.04f, 0.01f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.3f, 0f),  // Much more transparent smoke
                    new GradientAlphaKey(0.15f, 0.7f), // Very transparent
                    new GradientAlphaKey(0f, 1f)
                }
            );
            smokeColor.color = smokeGrad;

            var smokeSize = smoke.sizeOverLifetime;
            smokeSize.enabled = true;
            // Smoke expands significantly over lifetime like real smoke
            AnimationCurve smokeSizeCurve = new AnimationCurve();
            smokeSizeCurve.AddKey(0f, 0.5f);
            smokeSizeCurve.AddKey(0.3f, 1.2f);
            smokeSizeCurve.AddKey(1f, 2.5f); // Expands to 2.5x size
            smokeSize.size = new ParticleSystem.MinMaxCurve(1f, smokeSizeCurve);

            // Add velocity over lifetime to slow down and drift
            var smokeVelocity = smoke.velocityOverLifetime;
            smokeVelocity.enabled = true;
            smokeVelocity.space = ParticleSystemSimulationSpace.World;
            // Slow down over time (drag effect)
            smokeVelocity.speedModifier = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));
            // Add slight random drift
            smokeVelocity.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
            smokeVelocity.z = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);

            var smokeRenderer = smoke.GetComponent<ParticleSystemRenderer>();
            smokeRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            smokeRenderer.alignment = ParticleSystemRenderSpace.Facing;
            smokeRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            smokeRenderer.material.mainTexture = softTex;
            smokeRenderer.material.SetColor("_BaseColor", new Color(0.15f, 0.08f, 0.03f));
            smokeRenderer.material.SetFloat("_Surface", 1);
            smokeRenderer.material.SetFloat("_Blend", 1); // ADDITIVE blend
            smokeRenderer.material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
            smokeRenderer.material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            smokeRenderer.material.SetFloat("_ZWrite", 0);
            smokeRenderer.material.renderQueue = 3000;

            // === 5. BRIGHT GLOWING TRAIL ===
            TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.5f;
            trail.startWidth = 0.3f; // 50% smaller width (0.6 → 0.3)
            trail.endWidth = 0.025f; // 50% smaller end (0.05 → 0.025)
            trail.numCornerVertices = 5;
            trail.numCapVertices = 5;

            Material trailMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            trailMat.EnableKeyword("_EMISSION");
            trailMat.SetColor("_BaseColor", new Color(1f, 0.7f, 0.3f));
            trailMat.SetColor("_EmissionColor", new Color(1f, 0.6f, 0.2f) * 12f); // Bright HDR
            trailMat.SetFloat("_Surface", 1);
            trailMat.SetFloat("_Blend", 0);
            trailMat.renderQueue = 3000;
            trail.material = trailMat;

            trail.startColor = new Color(1f, 0.8f, 0.4f, 1f);
            trail.endColor = new Color(1f, 0.3f, 0f, 0f);

            // Add animation
            FireballAnimation anim = projectile.AddComponent<FireballAnimation>();

            return projectile;
        }

        private GameObject CreateIceShard(SpellData spell)
        {
            GameObject projectile = new GameObject($"IceShard_{spell.spellName}");

            // === LAYER 1: Brilliant white-blue core (ultra-bright crystalline center) ===
            GameObject brilliantCore = GameObject.CreatePrimitive(PrimitiveType.Cube);
            brilliantCore.name = "BrilliantCore";
            brilliantCore.transform.SetParent(projectile.transform);
            brilliantCore.transform.localPosition = Vector3.zero;
            brilliantCore.transform.localScale = new Vector3(0.15f, 0.15f, 0.5f);
            Destroy(brilliantCore.GetComponent<Collider>());

            Material brilliantMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            brilliantMat.EnableKeyword("_EMISSION");
            brilliantMat.SetColor("_BaseColor", new Color(0.95f, 0.98f, 1f));
            brilliantMat.SetColor("_EmissionColor", new Color(0.8f, 0.95f, 1f) * 10f); // Intense icy white-blue HDR
            brilliantMat.SetFloat("_Smoothness", 1f);
            brilliantMat.SetFloat("_Metallic", 0.9f); // Highly reflective like ice
            brilliantCore.GetComponent<MeshRenderer>().material = brilliantMat;

            // === LAYER 2: Cyan crystalline core ===
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cube);
            core.name = "IceCore";
            core.transform.SetParent(projectile.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = new Vector3(0.25f, 0.25f, 0.7f);
            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetColor("_BaseColor", new Color(0.5f, 0.85f, 1f));
            coreMat.SetColor("_EmissionColor", new Color(0.4f, 0.8f, 1f) * 6f); // Bright cyan HDR
            coreMat.SetFloat("_Smoothness", 0.95f);
            coreMat.SetFloat("_Metallic", 0.8f);
            core.GetComponent<MeshRenderer>().material = coreMat;

            // === LAYER 3: Outer crystalline frost layer (transparent) ===
            GameObject outer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            outer.name = "FrostLayer";
            outer.transform.SetParent(projectile.transform);
            outer.transform.localPosition = Vector3.zero;
            outer.transform.localScale = new Vector3(0.35f, 0.35f, 0.85f);
            Destroy(outer.GetComponent<Collider>());

            Material outerMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            outerMat.EnableKeyword("_EMISSION");
            outerMat.SetColor("_BaseColor", new Color(0.7f, 0.9f, 1f, 0.4f));
            outerMat.SetColor("_EmissionColor", new Color(0.6f, 0.85f, 1f) * 3f);
            outerMat.SetFloat("_Surface", 1); // Transparent
            outerMat.SetFloat("_Blend", 0); // Alpha blend
            outerMat.SetFloat("_Smoothness", 0.9f);
            outerMat.renderQueue = 3000;
            outer.GetComponent<MeshRenderer>().material = outerMat;

            // === LAYER 4: Orbiting ice crystals (16 shards) ===
            for (int i = 0; i < 16; i++)
            {
                GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crystal.name = $"IceCrystal{i}";
                crystal.transform.SetParent(projectile.transform);
                crystal.transform.localScale = new Vector3(0.05f, 0.05f, Random.Range(0.2f, 0.35f));
                Destroy(crystal.GetComponent<Collider>());

                Material crystalMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                crystalMat.EnableKeyword("_EMISSION");
                // Vary between pure white and cyan crystals
                Color crystalColor = (i % 3 == 0) ?
                    new Color(0.9f, 0.95f, 1f) :
                    new Color(0.6f, 0.9f, 1f);
                crystalMat.SetColor("_BaseColor", crystalColor);
                crystalMat.SetColor("_EmissionColor", crystalColor * 5f); // Bright HDR
                crystalMat.SetFloat("_Smoothness", 1f);
                crystalMat.SetFloat("_Metallic", 0.7f);
                crystal.GetComponent<MeshRenderer>().material = crystalMat;
            }

            // === LAYER 5: Frost vapor particles (10 mist particles) ===
            for (int i = 0; i < 10; i++)
            {
                GameObject vapor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                vapor.name = $"FrostVapor{i}";
                vapor.transform.SetParent(projectile.transform);
                vapor.transform.localScale = Vector3.one * Random.Range(0.12f, 0.22f);
                Destroy(vapor.GetComponent<Collider>());

                Material vaporMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                vaporMat.EnableKeyword("_EMISSION");
                vaporMat.SetColor("_BaseColor", new Color(0.85f, 0.95f, 1f, 0.25f));
                vaporMat.SetColor("_EmissionColor", new Color(0.8f, 0.9f, 1f) * 2f);
                vaporMat.SetFloat("_Surface", 1); // Transparent
                vaporMat.SetFloat("_Blend", 0);
                vaporMat.renderQueue = 3000;
                vapor.GetComponent<MeshRenderer>().material = vaporMat;
            }

            // === LAYER 6: Icicle spikes (6 pointed shards) ===
            for (int i = 0; i < 6; i++)
            {
                GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spike.name = $"IcicleSpike{i}";
                spike.transform.SetParent(projectile.transform);
                spike.transform.localScale = new Vector3(0.08f, 0.08f, 0.4f);
                Destroy(spike.GetComponent<Collider>());

                Material spikeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                spikeMat.EnableKeyword("_EMISSION");
                spikeMat.SetColor("_BaseColor", new Color(0.75f, 0.92f, 1f, 0.7f));
                spikeMat.SetColor("_EmissionColor", new Color(0.7f, 0.9f, 1f) * 4f);
                spikeMat.SetFloat("_Surface", 1);
                spikeMat.SetFloat("_Smoothness", 1f);
                spikeMat.SetFloat("_Metallic", 0.85f);
                spikeMat.renderQueue = 3000;
                spike.GetComponent<MeshRenderer>().material = spikeMat;
            }

            // === TRAIL: Frozen frost trail ===
            TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.5f;
            trail.startWidth = 0.55f;
            trail.endWidth = 0f;
            Material trailMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            trailMat.EnableKeyword("_EMISSION");
            trailMat.SetColor("_EmissionColor", new Color(0.6f, 0.85f, 1f) * 4f);
            trail.material = trailMat;
            trail.startColor = new Color(0.7f, 0.9f, 1f, 1f);
            trail.endColor = new Color(0.5f, 0.8f, 1f, 0f);

            // Add enhanced animation
            IceShardAnimation anim = projectile.AddComponent<IceShardAnimation>();

            return projectile;
        }

        private GameObject CreateLightningBolt(SpellData spell)
        {
            GameObject projectile = new GameObject($"LightningBolt_{spell.spellName}");

            // === LAYER 1: Pure white plasma core (EXTREMELY bright) ===
            GameObject plasmaCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            plasmaCore.name = "PlasmaCore";
            plasmaCore.transform.SetParent(projectile.transform);
            plasmaCore.transform.localPosition = Vector3.zero;
            plasmaCore.transform.localScale = Vector3.one * 0.2f;
            Destroy(plasmaCore.GetComponent<Collider>());

            Material plasmaMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            plasmaMat.EnableKeyword("_EMISSION");
            plasmaMat.SetColor("_BaseColor", Color.white);
            plasmaMat.SetColor("_EmissionColor", new Color(1f, 1f, 0.95f) * 15f); // INTENSE white HDR
            plasmaMat.SetFloat("_Smoothness", 1f);
            plasmaCore.GetComponent<MeshRenderer>().material = plasmaMat;

            // === LAYER 2: Electric white-blue energy core ===
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "LightningCore";
            core.transform.SetParent(projectile.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.35f;
            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetColor("_BaseColor", new Color(0.95f, 0.98f, 1f));
            coreMat.SetColor("_EmissionColor", new Color(0.9f, 0.95f, 1f) * 10f); // Bright white-blue HDR
            coreMat.SetFloat("_Smoothness", 0.9f);
            core.GetComponent<MeshRenderer>().material = coreMat;

            // === LAYER 3: Electric aura glow (transparent) ===
            GameObject aura = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            aura.name = "ElectricAura";
            aura.transform.SetParent(projectile.transform);
            aura.transform.localPosition = Vector3.zero;
            aura.transform.localScale = Vector3.one * 0.55f;
            Destroy(aura.GetComponent<Collider>());

            Material auraMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            auraMat.EnableKeyword("_EMISSION");
            auraMat.SetColor("_BaseColor", new Color(0.8f, 0.9f, 1f, 0.35f));
            auraMat.SetColor("_EmissionColor", new Color(0.75f, 0.85f, 1f) * 6f);
            auraMat.SetFloat("_Surface", 1); // Transparent
            auraMat.SetFloat("_Blend", 0);
            auraMat.renderQueue = 3000;
            aura.GetComponent<MeshRenderer>().material = auraMat;

            // === LAYER 4: Chaotic electric arcs (18 lightning tendrils) ===
            for (int i = 0; i < 18; i++)
            {
                GameObject arc = GameObject.CreatePrimitive(PrimitiveType.Cube);
                arc.name = $"Arc{i}";
                arc.transform.SetParent(projectile.transform);
                arc.transform.localScale = new Vector3(0.04f, Random.Range(0.2f, 0.35f), 0.04f);
                Destroy(arc.GetComponent<Collider>());

                Material arcMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                arcMat.EnableKeyword("_EMISSION");
                // Vary between pure white and electric blue arcs
                Color arcColor = (i % 4 == 0) ?
                    new Color(1f, 1f, 1f) :
                    new Color(0.85f, 0.92f, 1f);
                arcMat.SetColor("_BaseColor", arcColor);
                arcMat.SetColor("_EmissionColor", arcColor * 8f); // Very bright HDR
                arcMat.SetFloat("_Smoothness", 0.8f);
                arc.GetComponent<MeshRenderer>().material = arcMat;
            }

            // === LAYER 5: Electric sparks (12 small energy particles) ===
            for (int i = 0; i < 12; i++)
            {
                GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spark.name = $"Spark{i}";
                spark.transform.SetParent(projectile.transform);
                spark.transform.localScale = Vector3.one * Random.Range(0.08f, 0.15f);
                Destroy(spark.GetComponent<Collider>());

                Material sparkMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                sparkMat.EnableKeyword("_EMISSION");
                Color sparkColor = new Color(1f, 1f, 0.9f);
                sparkMat.SetColor("_BaseColor", sparkColor);
                sparkMat.SetColor("_EmissionColor", sparkColor * 12f); // Extremely bright sparks
                spark.GetComponent<MeshRenderer>().material = sparkMat;
            }

            // === LAYER 6: Outer plasma discharge (8 energy wisps) ===
            for (int i = 0; i < 8; i++)
            {
                GameObject discharge = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                discharge.name = $"PlasmaDischarge{i}";
                discharge.transform.SetParent(projectile.transform);
                discharge.transform.localScale = Vector3.one * Random.Range(0.12f, 0.2f);
                Destroy(discharge.GetComponent<Collider>());

                Material dischargeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                dischargeMat.EnableKeyword("_EMISSION");
                dischargeMat.SetColor("_BaseColor", new Color(0.9f, 0.95f, 1f, 0.5f));
                dischargeMat.SetColor("_EmissionColor", new Color(0.85f, 0.9f, 1f) * 5f);
                dischargeMat.SetFloat("_Surface", 1);
                dischargeMat.renderQueue = 3000;
                discharge.GetComponent<MeshRenderer>().material = dischargeMat;
            }

            // === TRAIL: Electric ionization trail ===
            TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.3f;
            trail.startWidth = 0.5f;
            trail.endWidth = 0f;
            Material trailMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            trailMat.EnableKeyword("_EMISSION");
            trailMat.SetColor("_EmissionColor", new Color(0.9f, 0.95f, 1f) * 7f);
            trail.material = trailMat;
            trail.startColor = new Color(1f, 1f, 0.95f, 1f);
            trail.endColor = new Color(0.8f, 0.9f, 1f, 0f);

            // Add enhanced animation
            LightningAnimation anim = projectile.AddComponent<LightningAnimation>();

            return projectile;
        }

        private GameObject CreateWindBlast(SpellData spell)
        {
            GameObject projectile = new GameObject($"WindBlast_{spell.spellName}");

            // === LAYER 1: Bright white energy vortex core ===
            GameObject vortexCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            vortexCore.name = "VortexCore";
            vortexCore.transform.SetParent(projectile.transform);
            vortexCore.transform.localPosition = Vector3.zero;
            vortexCore.transform.localScale = Vector3.one * 0.18f;
            Destroy(vortexCore.GetComponent<Collider>());

            Material vortexMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            vortexMat.EnableKeyword("_EMISSION");
            vortexMat.SetColor("_BaseColor", new Color(0.95f, 0.98f, 1f));
            vortexMat.SetColor("_EmissionColor", new Color(0.9f, 0.95f, 1f) * 8f); // Bright white-cyan HDR
            vortexMat.SetFloat("_Smoothness", 0.9f);
            vortexCore.GetComponent<MeshRenderer>().material = vortexMat;

            // === LAYER 2: Swirling wind energy core ===
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "WindCore";
            core.transform.SetParent(projectile.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.3f;
            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetColor("_BaseColor", new Color(0.85f, 0.95f, 1f));
            coreMat.SetColor("_EmissionColor", new Color(0.8f, 0.92f, 1f) * 5f); // Bright pale cyan HDR
            coreMat.SetFloat("_Smoothness", 0.85f);
            core.GetComponent<MeshRenderer>().material = coreMat;

            // === LAYER 3: Outer swirling wind layer (transparent) ===
            GameObject swirl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            swirl.name = "WindSwirl";
            swirl.transform.SetParent(projectile.transform);
            swirl.transform.localPosition = Vector3.zero;
            swirl.transform.localScale = Vector3.one * 0.5f;
            Destroy(swirl.GetComponent<Collider>());

            Material swirlMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            swirlMat.EnableKeyword("_EMISSION");
            swirlMat.SetColor("_BaseColor", new Color(0.9f, 0.97f, 1f, 0.35f));
            swirlMat.SetColor("_EmissionColor", new Color(0.85f, 0.93f, 1f) * 3f);
            swirlMat.SetFloat("_Surface", 1); // Transparent
            swirlMat.SetFloat("_Blend", 0);
            swirlMat.renderQueue = 3000;
            swirl.GetComponent<MeshRenderer>().material = swirlMat;

            // === LAYER 4: Spiraling air current particles (20 vortex streams) ===
            for (int i = 0; i < 20; i++)
            {
                GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                particle.name = $"WindParticle{i}";
                particle.transform.SetParent(projectile.transform);
                particle.transform.localScale = Vector3.one * Random.Range(0.1f, 0.15f);
                Destroy(particle.GetComponent<Collider>());

                Material particleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                particleMat.EnableKeyword("_EMISSION");
                // Vary between white and pale cyan
                Color particleColor = (i % 3 == 0) ?
                    new Color(1f, 1f, 1f) :
                    new Color(0.85f, 0.95f, 1f);
                particleMat.SetColor("_BaseColor", particleColor);
                particleMat.SetColor("_EmissionColor", particleColor * 6f); // Bright HDR
                particleMat.SetFloat("_Smoothness", 0.8f);
                particle.GetComponent<MeshRenderer>().material = particleMat;
            }

            // === LAYER 5: Turbulent air wisps (12 flowing streams) ===
            for (int i = 0; i < 12; i++)
            {
                GameObject wisp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wisp.name = $"AirWisp{i}";
                wisp.transform.SetParent(projectile.transform);
                wisp.transform.localScale = new Vector3(0.06f, 0.06f, Random.Range(0.25f, 0.4f));
                Destroy(wisp.GetComponent<Collider>());

                Material wispMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                wispMat.EnableKeyword("_EMISSION");
                wispMat.SetColor("_BaseColor", new Color(0.9f, 0.95f, 1f, 0.6f));
                wispMat.SetColor("_EmissionColor", new Color(0.88f, 0.93f, 1f) * 4f);
                wispMat.SetFloat("_Surface", 1);
                wispMat.SetFloat("_Smoothness", 0.75f);
                wispMat.renderQueue = 3000;
                wisp.GetComponent<MeshRenderer>().material = wispMat;
            }

            // === LAYER 6: Outer atmospheric distortion (8 pressure waves) ===
            for (int i = 0; i < 8; i++)
            {
                GameObject pressure = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pressure.name = $"PressureWave{i}";
                pressure.transform.SetParent(projectile.transform);
                pressure.transform.localScale = Vector3.one * Random.Range(0.15f, 0.25f);
                Destroy(pressure.GetComponent<Collider>());

                Material pressureMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                pressureMat.EnableKeyword("_EMISSION");
                pressureMat.SetColor("_BaseColor", new Color(0.92f, 0.96f, 1f, 0.25f));
                pressureMat.SetColor("_EmissionColor", new Color(0.9f, 0.94f, 1f) * 2.5f);
                pressureMat.SetFloat("_Surface", 1);
                pressureMat.SetFloat("_Blend", 0);
                pressureMat.renderQueue = 3000;
                pressure.GetComponent<MeshRenderer>().material = pressureMat;
            }

            // === TRAIL: Wind vortex trail ===
            TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.4f;
            trail.startWidth = 0.55f;
            trail.endWidth = 0f;
            Material trailMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            trailMat.EnableKeyword("_EMISSION");
            trailMat.SetColor("_EmissionColor", new Color(0.85f, 0.93f, 1f) * 4f);
            trail.material = trailMat;
            trail.startColor = new Color(0.9f, 0.95f, 1f, 0.9f);
            trail.endColor = new Color(0.8f, 0.9f, 1f, 0f);

            // Add enhanced animation
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

        #region Tier 2 Physics-Based Projectiles

        private GameObject CreateMeteor(SpellData spell)
        {
            GameObject projectile = new GameObject($"Meteor_{spell.spellName}");

            // === LAYER 1: Ultra-bright core (white-hot center) ===
            GameObject whiteCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            whiteCore.name = "WhiteHotCore";
            whiteCore.transform.SetParent(projectile.transform);
            whiteCore.transform.localPosition = Vector3.zero;
            whiteCore.transform.localScale = Vector3.one * 0.25f;
            Destroy(whiteCore.GetComponent<Collider>());

            Material whiteMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            whiteMat.EnableKeyword("_EMISSION");
            whiteMat.SetColor("_BaseColor", Color.white);
            whiteMat.SetColor("_EmissionColor", Color.white * 15f); // Intense HDR white
            whiteMat.SetFloat("_Smoothness", 1f);
            whiteCore.GetComponent<MeshRenderer>().material = whiteMat;

            // === LAYER 2: Orange-yellow fire core ===
            GameObject fireCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fireCore.name = "FireCore";
            fireCore.transform.SetParent(projectile.transform);
            fireCore.transform.localPosition = Vector3.zero;
            fireCore.transform.localScale = Vector3.one * 0.45f;
            Destroy(fireCore.GetComponent<Collider>());

            Material fireMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            fireMat.EnableKeyword("_EMISSION");
            fireMat.SetColor("_BaseColor", new Color(1f, 0.6f, 0.1f, 0.9f));
            fireMat.SetColor("_EmissionColor", new Color(8f, 4f, 0.5f)); // Bright orange HDR
            fireMat.SetFloat("_Surface", 1); // Transparent
            fireMat.SetFloat("_Blend", 0);
            fireMat.renderQueue = 3000;
            fireCore.GetComponent<MeshRenderer>().material = fireMat;

            // === LAYER 3: Dark red outer shell (lava crust) ===
            GameObject lavaShell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lavaShell.name = "LavaShell";
            lavaShell.transform.SetParent(projectile.transform);
            lavaShell.transform.localPosition = Vector3.zero;
            lavaShell.transform.localScale = Vector3.one * 0.55f;
            Destroy(lavaShell.GetComponent<Collider>());

            Material lavaMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            lavaMat.EnableKeyword("_EMISSION");
            lavaMat.SetColor("_BaseColor", new Color(0.3f, 0.1f, 0.05f, 0.7f));
            lavaMat.SetColor("_EmissionColor", new Color(2f, 0.5f, 0.1f)); // Glowing cracks
            lavaMat.SetFloat("_Surface", 1);
            lavaMat.SetFloat("_Blend", 0);
            lavaMat.SetFloat("_Smoothness", 0.3f);
            lavaMat.renderQueue = 3000;
            lavaShell.GetComponent<MeshRenderer>().material = lavaMat;

            // === LAYER 4: Orbiting lava chunks (10 irregular pieces) ===
            for (int i = 0; i < 10; i++)
            {
                // Mix cubes and spheres for variety
                GameObject chunk = GameObject.CreatePrimitive(i % 3 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere);
                chunk.name = $"LavaChunk{i}";
                chunk.transform.SetParent(projectile.transform);

                // Irregular sizes
                float size = Random.Range(0.08f, 0.15f);
                chunk.transform.localScale = Vector3.one * size;

                // Random positions around the meteor
                float angle = i * 36f * Mathf.Deg2Rad; // 360/10
                float radius = 0.4f + Random.Range(-0.1f, 0.15f);
                float height = Random.Range(-0.15f, 0.15f);
                chunk.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * radius,
                    height,
                    Mathf.Sin(angle) * radius
                );

                // Random rotation
                chunk.transform.localRotation = Random.rotation;
                Destroy(chunk.GetComponent<Collider>());

                // Glowing lava material
                Material chunkMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                chunkMat.EnableKeyword("_EMISSION");
                // Vary between bright orange and dark red
                Color chunkColor = (i % 2 == 0) ?
                    new Color(1f, 0.4f, 0.1f) :
                    new Color(0.5f, 0.15f, 0.05f);
                chunkMat.SetColor("_BaseColor", chunkColor);
                chunkMat.SetColor("_EmissionColor", chunkColor * Random.Range(4f, 8f)); // HDR glow
                chunkMat.SetFloat("_Smoothness", 0.2f);
                chunk.GetComponent<MeshRenderer>().material = chunkMat;
            }

            // === LAYER 5: Smoke wisps (8 dark particles) ===
            for (int i = 0; i < 8; i++)
            {
                GameObject smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                smoke.name = $"SmokeWisp{i}";
                smoke.transform.SetParent(projectile.transform);
                smoke.transform.localScale = Vector3.one * Random.Range(0.06f, 0.12f);

                // Position in cloud around meteor
                float angle = i * 45f * Mathf.Deg2Rad; // 360/8
                float radius = 0.5f + Random.Range(-0.05f, 0.1f);
                smoke.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * radius,
                    Random.Range(-0.2f, 0.2f),
                    Mathf.Sin(angle) * radius
                );

                Destroy(smoke.GetComponent<Collider>());

                // Dark smoke material
                Material smokeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                smokeMat.SetColor("_BaseColor", new Color(0.15f, 0.1f, 0.08f, 0.3f));
                smokeMat.SetFloat("_Surface", 1); // Transparent
                smokeMat.SetFloat("_Blend", 0);
                smokeMat.renderQueue = 3000;
                smoke.GetComponent<MeshRenderer>().material = smokeMat;
            }

            // === PARTICLE SYSTEM 1: Intense fire burst ===
            GameObject fireBurst = new GameObject("FireBurst");
            fireBurst.transform.SetParent(projectile.transform);
            ParticleSystem ps1 = fireBurst.AddComponent<ParticleSystem>();
            var main1 = ps1.main;
            main1.startLifetime = 0.6f;
            main1.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
            main1.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f); // Very very small particles
            main1.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
            main1.maxParticles = 300;
            main1.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission1 = ps1.emission;
            emission1.rateOverTime = 250f;

            var shape1 = ps1.shape;
            shape1.shapeType = ParticleSystemShapeType.Sphere;
            shape1.radius = 0.3f;

            var colorOverLifetime1 = ps1.colorOverLifetime;
            colorOverLifetime1.enabled = true;
            Gradient grad1 = new Gradient();
            grad1.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 1f, 0.9f), 0f),   // White-hot
                    new GradientColorKey(new Color(1f, 0.7f, 0.2f), 0.3f), // Yellow
                    new GradientColorKey(new Color(1f, 0.3f, 0.1f), 0.7f), // Orange
                    new GradientColorKey(new Color(0.3f, 0.05f, 0f), 1f)   // Dark red
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.9f, 0f),
                    new GradientAlphaKey(0.6f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime1.color = grad1;

            // === PARTICLE SYSTEM 2: Smoke trail ===
            GameObject smokeTrail = new GameObject("SmokeTrail");
            smokeTrail.transform.SetParent(projectile.transform);
            ParticleSystem ps2 = smokeTrail.AddComponent<ParticleSystem>();
            var main2 = ps2.main;
            main2.startLifetime = 2f;
            main2.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 1f);
            main2.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.1f); // Much smaller smoke particles
            main2.startColor = new Color(0.1f, 0.05f, 0.05f, 0.3f); // More transparent
            main2.maxParticles = 100;
            main2.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission2 = ps2.emission;
            emission2.rateOverTime = 50f;

            // === TRAIL RENDERER: Fiery streak ===
            TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.8f;
            trail.startWidth = 0.6f;
            trail.endWidth = 0f;
            trail.numCornerVertices = 5;
            trail.numCapVertices = 5;

            Material trailMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            trailMat.SetColor("_BaseColor", new Color(1f, 0.5f, 0.1f, 0.8f));
            trailMat.SetFloat("_Surface", 1);
            trailMat.SetFloat("_Blend", 1); // Additive
            trail.material = trailMat;

            Gradient trailGrad = new Gradient();
            trailGrad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.9f, 0.5f), 0f),
                    new GradientColorKey(new Color(1f, 0.3f, 0.1f), 0.5f),
                    new GradientColorKey(new Color(0.3f, 0.1f, 0f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.5f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            trail.colorGradient = trailGrad;

            // === PHYSICS ===
            SphereCollider collider = projectile.AddComponent<SphereCollider>();
            collider.radius = 0.3f;

            Rigidbody rb = projectile.AddComponent<Rigidbody>();
            rb.mass = 0.1f; // Very light for satisfying long-distance throwing
            rb.useGravity = false;

            PhysicsSpellProjectile physicsProj = projectile.AddComponent<PhysicsSpellProjectile>();
            physicsProj.throwForce = 18f;
            physicsProj.useGravity = true;
            physicsProj.gravityScale = 0.4f; // Low gravity for ball-launcher feel
            physicsProj.bounciness = 0.1f;
            physicsProj.maxBounces = 0;
            physicsProj.explodeOnImpact = true;
            physicsProj.explosionRadius = 4f;
            physicsProj.explosionForce = 800f;
            physicsProj.lifetime = 8f;
            physicsProj.spellData = spell;

            // === ANIMATION: Tumbling meteor with orbiting chunks ===
            MeteorAnimation meteorAnim = projectile.AddComponent<MeteorAnimation>();

            return projectile;
        }

        private GameObject CreateFrostBoulder(SpellData spell)
        {
            GameObject projectile = new GameObject($"FrostBoulder_{spell.spellName}");

            // === LAYER 1: Bright cyan core (frozen heart) ===
            GameObject cyanCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cyanCore.name = "CyanCore";
            cyanCore.transform.SetParent(projectile.transform);
            cyanCore.transform.localPosition = Vector3.zero;
            cyanCore.transform.localScale = Vector3.one * 0.2f;
            Destroy(cyanCore.GetComponent<Collider>());

            Material cyanMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            cyanMat.EnableKeyword("_EMISSION");
            cyanMat.SetColor("_BaseColor", new Color(0.5f, 0.9f, 1f));
            cyanMat.SetColor("_EmissionColor", new Color(0.5f, 0.9f, 1f) * 12f); // Bright cyan HDR
            cyanMat.SetFloat("_Smoothness", 1f);
            cyanCore.GetComponent<MeshRenderer>().material = cyanMat;

            // === LAYER 2: Icy blue middle layer ===
            GameObject iceLayer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            iceLayer.name = "IceLayer";
            iceLayer.transform.SetParent(projectile.transform);
            iceLayer.transform.localPosition = Vector3.zero;
            iceLayer.transform.localScale = Vector3.one * 0.4f;
            Destroy(iceLayer.GetComponent<Collider>());

            Material iceMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            iceMat.EnableKeyword("_EMISSION");
            iceMat.SetColor("_BaseColor", new Color(0.6f, 0.85f, 1f, 0.7f));
            iceMat.SetColor("_EmissionColor", new Color(0.5f, 0.8f, 1f) * 4f);
            iceMat.SetFloat("_Surface", 1);
            iceMat.SetFloat("_Blend", 0);
            iceMat.SetFloat("_Smoothness", 0.9f);
            iceMat.renderQueue = 3000;
            iceLayer.GetComponent<MeshRenderer>().material = iceMat;

            // === LAYER 3: Frosty outer shell ===
            GameObject frostShell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            frostShell.name = "FrostShell";
            frostShell.transform.SetParent(projectile.transform);
            frostShell.transform.localPosition = Vector3.zero;
            frostShell.transform.localScale = Vector3.one * 0.5f;
            Destroy(frostShell.GetComponent<Collider>());

            Material frostMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            frostMat.EnableKeyword("_EMISSION");
            frostMat.SetColor("_BaseColor", new Color(0.7f, 0.85f, 0.95f, 0.5f));
            frostMat.SetColor("_EmissionColor", new Color(0.6f, 0.8f, 1f) * 1.5f);
            frostMat.SetFloat("_Surface", 1);
            frostMat.SetFloat("_Blend", 0);
            frostMat.SetFloat("_Smoothness", 0.7f);
            frostMat.renderQueue = 3000;
            frostShell.GetComponent<MeshRenderer>().material = frostMat;

            // === LAYER 4: Ice spikes (6 major crystal protrusions in hexagonal pattern) ===
            for (int i = 0; i < 6; i++)
            {
                GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spike.name = $"IceSpike{i}";
                spike.transform.SetParent(projectile.transform);

                // Scale to make spike-shaped (tall and thin)
                spike.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);

                // Position in hexagonal pattern around boulder
                float angle = i * 60f * Mathf.Deg2Rad;
                spike.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * 0.3f,
                    0f,
                    Mathf.Sin(angle) * 0.3f
                );

                // Point spike outward from center
                spike.transform.localRotation = Quaternion.Euler(
                    0f,
                    i * 60f,
                    90f // Lay horizontally pointing out
                );

                Destroy(spike.GetComponent<Collider>());

                // Crystalline ice material
                Material spikeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                spikeMat.EnableKeyword("_EMISSION");
                spikeMat.SetColor("_BaseColor", new Color(0.7f, 0.9f, 1f, 0.85f));
                spikeMat.SetColor("_EmissionColor", new Color(0.6f, 0.85f, 1f) * 6f); // Bright HDR
                spikeMat.SetFloat("_Smoothness", 0.95f); // Very smooth like crystal
                spikeMat.SetFloat("_Surface", 1); // Transparent
                spikeMat.SetFloat("_Blend", 0);
                spikeMat.renderQueue = 3000;
                spike.GetComponent<MeshRenderer>().material = spikeMat;
            }

            // === LAYER 5: Orbiting ice fragments (12 small crystalline shards) ===
            for (int i = 0; i < 12; i++)
            {
                GameObject fragment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fragment.name = $"IceFragment{i}";
                fragment.transform.SetParent(projectile.transform);

                // Small shard-like pieces
                fragment.transform.localScale = new Vector3(0.04f, 0.08f, 0.04f);

                // Random initial positions in two rings
                float angle = i * 30f * Mathf.Deg2Rad;
                float radius = (i < 6) ? 0.45f : 0.5f;
                float height = (i < 6) ? 0.1f : -0.1f;
                fragment.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * radius,
                    height,
                    Mathf.Sin(angle) * radius
                );

                fragment.transform.localRotation = Random.rotation;
                Destroy(fragment.GetComponent<Collider>());

                // Glowing ice shard material
                Material fragmentMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                fragmentMat.EnableKeyword("_EMISSION");
                Color shardColor = new Color(0.6f, 0.85f, 1f);
                fragmentMat.SetColor("_BaseColor", shardColor);
                fragmentMat.SetColor("_EmissionColor", shardColor * Random.Range(3f, 5f)); // Varied glow
                fragmentMat.SetFloat("_Smoothness", 0.9f);
                fragmentMat.SetFloat("_Surface", 1);
                fragmentMat.SetFloat("_Blend", 0);
                fragmentMat.renderQueue = 3000;
                fragment.GetComponent<MeshRenderer>().material = fragmentMat;
            }

            // === PARTICLE SYSTEM 1: Ice crystal burst ===
            GameObject iceCrystals = new GameObject("IceCrystals");
            iceCrystals.transform.SetParent(projectile.transform);
            ParticleSystem ps1 = iceCrystals.AddComponent<ParticleSystem>();
            var main1 = ps1.main;
            main1.startLifetime = 0.8f;
            main1.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1.5f);
            main1.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
            main1.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
            main1.maxParticles = 200;
            main1.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission1 = ps1.emission;
            emission1.rateOverTime = 150f;

            var colorOverLifetime1 = ps1.colorOverLifetime;
            colorOverLifetime1.enabled = true;
            Gradient gradient1 = new Gradient();
            gradient1.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.8f, 0.95f, 1f), 0f),
                    new GradientColorKey(new Color(0.5f, 0.7f, 1f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.9f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime1.color = gradient1;

            // === PARTICLE SYSTEM 2: Frost mist trail ===
            GameObject frostMist = new GameObject("FrostMist");
            frostMist.transform.SetParent(projectile.transform);
            ParticleSystem ps2 = frostMist.AddComponent<ParticleSystem>();
            var main2 = ps2.main;
            main2.startLifetime = 1.5f;
            main2.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
            main2.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.15f);
            main2.startColor = new Color(0.7f, 0.85f, 1f, 0.4f);
            main2.maxParticles = 80;
            main2.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission2 = ps2.emission;
            emission2.rateOverTime = 60f;

            // === TRAIL RENDERER: Icy streak ===
            TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.6f;
            trail.startWidth = 0.5f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            trail.material.EnableKeyword("_EMISSION");
            trail.material.SetColor("_BaseColor", new Color(0.6f, 0.85f, 1f, 0.8f));
            trail.material.SetColor("_EmissionColor", new Color(0.5f, 0.8f, 1f) * 3f);
            Gradient trailGradient = new Gradient();
            trailGradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.6f, 0.85f, 1f), 0f),
                    new GradientColorKey(new Color(0.4f, 0.6f, 0.8f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.8f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            trail.colorGradient = trailGradient;

            // === PHYSICS ===
            SphereCollider collider = projectile.AddComponent<SphereCollider>();
            collider.radius = 0.25f;

            Rigidbody rb = projectile.AddComponent<Rigidbody>();
            rb.mass = 0.1f; // Very light for satisfying long-distance throwing
            rb.useGravity = false;

            PhysicsSpellProjectile physicsProj = projectile.AddComponent<PhysicsSpellProjectile>();
            physicsProj.throwForce = 18f;
            physicsProj.useGravity = true;
            physicsProj.gravityScale = 0.4f; // Low gravity for ball-launcher feel
            physicsProj.bounciness = 0.4f;
            physicsProj.maxBounces = 2; // Bounces twice before exploding
            physicsProj.explodeOnImpact = true;
            physicsProj.explosionRadius = 3.5f;
            physicsProj.explosionForce = 600f;
            physicsProj.lifetime = 8f;
            physicsProj.spellData = spell;

            // === ANIMATION: Rotating ice structure with orbiting fragments ===
            FrostBoulderAnimation frostAnim = projectile.AddComponent<FrostBoulderAnimation>();

            return projectile;
        }

        private GameObject CreateThunderOrb(SpellData spell)
        {
            GameObject projectile = new GameObject($"ThunderOrb_{spell.spellName}");

            // === LAYER 1: Pure white lightning core ===
            GameObject whiteCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            whiteCore.name = "LightningCore";
            whiteCore.transform.SetParent(projectile.transform);
            whiteCore.transform.localPosition = Vector3.zero;
            whiteCore.transform.localScale = Vector3.one * 0.18f;
            Destroy(whiteCore.GetComponent<Collider>());

            Material whiteMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            whiteMat.EnableKeyword("_EMISSION");
            whiteMat.SetColor("_BaseColor", Color.white);
            whiteMat.SetColor("_EmissionColor", Color.white * 18f); // Intense white HDR
            whiteMat.SetFloat("_Smoothness", 1f);
            whiteCore.GetComponent<MeshRenderer>().material = whiteMat;

            // === LAYER 2: Electric blue energy layer ===
            GameObject blueLayer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            blueLayer.name = "ElectricBlueLayer";
            blueLayer.transform.SetParent(projectile.transform);
            blueLayer.transform.localPosition = Vector3.zero;
            blueLayer.transform.localScale = Vector3.one * 0.35f;
            Destroy(blueLayer.GetComponent<Collider>());

            Material blueMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            blueMat.EnableKeyword("_EMISSION");
            blueMat.SetColor("_BaseColor", new Color(0.7f, 0.8f, 1f, 0.8f));
            blueMat.SetColor("_EmissionColor", new Color(0.7f, 0.85f, 1f) * 8f);
            blueMat.SetFloat("_Surface", 1);
            blueMat.SetFloat("_Blend", 0);
            blueMat.SetFloat("_Smoothness", 0.9f);
            blueMat.renderQueue = 3000;
            blueLayer.GetComponent<MeshRenderer>().material = blueMat;

            // === LAYER 3: Crackling energy shell ===
            GameObject energyShell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            energyShell.name = "EnergyShell";
            energyShell.transform.SetParent(projectile.transform);
            energyShell.transform.localPosition = Vector3.zero;
            energyShell.transform.localScale = Vector3.one * 0.45f;
            Destroy(energyShell.GetComponent<Collider>());

            Material energyMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            energyMat.EnableKeyword("_EMISSION");
            energyMat.SetColor("_BaseColor", new Color(0.85f, 0.9f, 1f, 0.6f));
            energyMat.SetColor("_EmissionColor", new Color(0.8f, 0.9f, 1f) * 3f);
            energyMat.SetFloat("_Surface", 1);
            energyMat.SetFloat("_Blend", 0);
            energyMat.SetFloat("_Smoothness", 0.8f);
            energyMat.renderQueue = 3000;
            energyShell.GetComponent<MeshRenderer>().material = energyMat;

            // === LAYER 4: Electric arc rings (3 rings, 8 segments each = 24 total) ===
            for (int ring = 0; ring < 3; ring++)
            {
                int segmentsPerRing = 8;
                for (int i = 0; i < segmentsPerRing; i++)
                {
                    GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    segment.name = $"ArcRing{ring}_Seg{i}";
                    segment.transform.SetParent(projectile.transform);

                    // Small rectangular segments that form arcs
                    segment.transform.localScale = new Vector3(0.04f, 0.12f, 0.04f);

                    // Initial position in ring pattern
                    float angle = i * 45f * Mathf.Deg2Rad; // 360/8 = 45 degrees per segment
                    float radius = 0.35f;

                    // Position based on ring orientation
                    Vector3 position = Vector3.zero;
                    if (ring == 0)
                    {
                        // Horizontal ring (XZ plane)
                        position = new Vector3(
                            Mathf.Cos(angle) * radius,
                            0f,
                            Mathf.Sin(angle) * radius
                        );
                    }
                    else if (ring == 1)
                    {
                        // Vertical ring (XY plane)
                        position = new Vector3(
                            Mathf.Cos(angle) * radius,
                            Mathf.Sin(angle) * radius,
                            0f
                        );
                    }
                    else
                    {
                        // Diagonal ring (YZ plane)
                        position = new Vector3(
                            0f,
                            Mathf.Cos(angle) * radius,
                            Mathf.Sin(angle) * radius
                        );
                    }

                    segment.transform.localPosition = position;

                    // Point segment tangent to ring
                    segment.transform.LookAt(projectile.transform.position);
                    Destroy(segment.GetComponent<Collider>());

                    // Bright electric arc material
                    Material arcMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    arcMat.EnableKeyword("_EMISSION");
                    Color arcColor = new Color(0.7f, 0.85f, 1f);
                    arcMat.SetColor("_BaseColor", arcColor);
                    arcMat.SetColor("_EmissionColor", arcColor * Random.Range(8f, 12f)); // Very bright HDR
                    arcMat.SetFloat("_Smoothness", 1f);
                    segment.GetComponent<MeshRenderer>().material = arcMat;
                }
            }

            // === LAYER 5: Crackling energy sparks (10 random jittery particles) ===
            for (int i = 0; i < 10; i++)
            {
                GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spark.name = $"EnergySpark{i}";
                spark.transform.SetParent(projectile.transform);
                spark.transform.localScale = Vector3.one * 0.025f;

                // Random position around orb
                spark.transform.localPosition = Random.insideUnitSphere * 0.3f;
                Destroy(spark.GetComponent<Collider>());

                // Super bright white-blue material
                Material sparkMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                sparkMat.EnableKeyword("_EMISSION");
                Color sparkColor = new Color(1f, 1f, 0.95f);
                sparkMat.SetColor("_BaseColor", sparkColor);
                sparkMat.SetColor("_EmissionColor", sparkColor * Random.Range(10f, 15f)); // Intense HDR
                sparkMat.SetFloat("_Smoothness", 1f);
                spark.GetComponent<MeshRenderer>().material = sparkMat;
            }

            // === PARTICLE SYSTEM 1: Electric sparks ===
            GameObject electricSparks = new GameObject("ElectricSparks");
            electricSparks.transform.SetParent(projectile.transform);
            ParticleSystem ps1 = electricSparks.AddComponent<ParticleSystem>();
            var main1 = ps1.main;
            main1.startLifetime = 0.3f;
            main1.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            main1.startSize = new ParticleSystem.MinMaxCurve(0.01f, 0.03f);
            main1.maxParticles = 250;
            main1.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission1 = ps1.emission;
            emission1.rateOverTime = 300f;

            var shape1 = ps1.shape;
            shape1.shapeType = ParticleSystemShapeType.Sphere;
            shape1.radius = 0.25f;

            var colorOverLifetime1 = ps1.colorOverLifetime;
            colorOverLifetime1.enabled = true;
            Gradient gradient1 = new Gradient();
            gradient1.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 1f, 1f), 0f),
                    new GradientColorKey(new Color(0.6f, 0.75f, 1f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime1.color = gradient1;

            // === PARTICLE SYSTEM 2: Energy crackle ===
            GameObject energyCrackle = new GameObject("EnergyCrackle");
            energyCrackle.transform.SetParent(projectile.transform);
            ParticleSystem ps2 = energyCrackle.AddComponent<ParticleSystem>();
            var main2 = ps2.main;
            main2.startLifetime = 0.5f;
            main2.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main2.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.06f);
            main2.startColor = new Color(0.8f, 0.9f, 1f, 0.7f);
            main2.maxParticles = 120;
            main2.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission2 = ps2.emission;
            emission2.rateOverTime = 150f;

            // === TRAIL RENDERER: Lightning streak ===
            TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.4f;
            trail.startWidth = 0.45f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            trail.material.EnableKeyword("_EMISSION");
            trail.material.SetColor("_BaseColor", new Color(0.85f, 0.9f, 1f, 0.9f));
            trail.material.SetColor("_EmissionColor", new Color(0.8f, 0.9f, 1f) * 5f);
            Gradient trailGradient = new Gradient();
            trailGradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 1f, 1f), 0f),
                    new GradientColorKey(new Color(0.6f, 0.75f, 1f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.9f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            trail.colorGradient = trailGradient;

            // === PHYSICS ===
            SphereCollider collider = projectile.AddComponent<SphereCollider>();
            collider.radius = 0.23f;

            Rigidbody rb = projectile.AddComponent<Rigidbody>();
            rb.mass = 0.1f; // Very light for satisfying long-distance throwing
            rb.useGravity = false;

            PhysicsSpellProjectile physicsProj = projectile.AddComponent<PhysicsSpellProjectile>();
            physicsProj.throwForce = 18f;
            physicsProj.useGravity = true;
            physicsProj.gravityScale = 0.4f; // Low gravity for ball-launcher feel
            physicsProj.bounciness = 0.7f; // Very bouncy
            physicsProj.maxBounces = 3; // Bounces 3 times
            physicsProj.explodeOnImpact = true;
            physicsProj.explosionRadius = 3f;
            physicsProj.explosionForce = 700f;
            physicsProj.lifetime = 10f;
            physicsProj.spellData = spell;

            // === ANIMATION: Rotating arc rings with flickering energy ===
            ThunderOrbAnimation thunderAnim = projectile.AddComponent<ThunderOrbAnimation>();

            return projectile;
        }

        private GameObject CreateCyclone(SpellData spell)
        {
            GameObject projectile = new GameObject($"Cyclone_{spell.spellName}");

            // === LAYER 1: Bright white vortex core ===
            GameObject whiteCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            whiteCore.name = "VortexCore";
            whiteCore.transform.SetParent(projectile.transform);
            whiteCore.transform.localPosition = Vector3.zero;
            whiteCore.transform.localScale = Vector3.one * 0.15f;
            Destroy(whiteCore.GetComponent<Collider>());

            Material whiteMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            whiteMat.EnableKeyword("_EMISSION");
            whiteMat.SetColor("_BaseColor", new Color(0.95f, 0.98f, 1f));
            whiteMat.SetColor("_EmissionColor", new Color(0.95f, 0.98f, 1f) * 10f);
            whiteMat.SetFloat("_Smoothness", 1f);
            whiteCore.GetComponent<MeshRenderer>().material = whiteMat;

            // === LAYER 2: Swirling air layer (spinning) ===
            GameObject airLayer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            airLayer.name = "AirLayer";
            airLayer.transform.SetParent(projectile.transform);
            airLayer.transform.localPosition = Vector3.zero;
            airLayer.transform.localScale = Vector3.one * 0.32f;
            Destroy(airLayer.GetComponent<Collider>());

            Material airMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            airMat.EnableKeyword("_EMISSION");
            airMat.SetColor("_BaseColor", new Color(0.85f, 0.92f, 1f, 0.6f));
            airMat.SetColor("_EmissionColor", new Color(0.85f, 0.92f, 1f) * 3f);
            airMat.SetFloat("_Surface", 1);
            airMat.SetFloat("_Blend", 0);
            airMat.SetFloat("_Smoothness", 0.8f);
            airMat.renderQueue = 3000;
            airLayer.GetComponent<MeshRenderer>().material = airMat;

            // Add spinning animation
            CycloneSpinner spinner = airLayer.AddComponent<CycloneSpinner>();
            spinner.spinSpeed = 450f;

            // === LAYER 3: Outer wind vortex ===
            GameObject windShell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            windShell.name = "WindShell";
            windShell.transform.SetParent(projectile.transform);
            windShell.transform.localPosition = Vector3.zero;
            windShell.transform.localScale = Vector3.one * 0.42f;
            Destroy(windShell.GetComponent<Collider>());

            Material windMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            windMat.EnableKeyword("_EMISSION");
            windMat.SetColor("_BaseColor", new Color(0.88f, 0.94f, 1f, 0.4f));
            windMat.SetColor("_EmissionColor", new Color(0.88f, 0.94f, 1f) * 1.5f);
            windMat.SetFloat("_Surface", 1);
            windMat.SetFloat("_Blend", 0);
            windMat.SetFloat("_Smoothness", 0.7f);
            windMat.renderQueue = 3000;
            windShell.GetComponent<MeshRenderer>().material = windMat;

            // === LAYER 4: Tornado spiral structure (3 layers, 10 particles each = 30 total) ===
            for (int layer = 0; layer < 3; layer++)
            {
                int particlesPerLayer = 10;
                float layerRadius = 0.2f + layer * 0.1f; // Layers get progressively larger

                for (int i = 0; i < particlesPerLayer; i++)
                {
                    GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    particle.name = $"TornadoLayer{layer}_P{i}";
                    particle.transform.SetParent(projectile.transform);

                    // Size varies by layer (outer layers have larger particles)
                    float size = 0.05f + layer * 0.01f;
                    particle.transform.localScale = Vector3.one * size;

                    // Initial spiral position
                    float t = i / (float)particlesPerLayer;
                    float angle = (t * 720f) * Mathf.Deg2Rad; // Two full rotations per layer
                    float radius = layerRadius * (1f - t * 0.3f); // Tightening spiral
                    float height = t * 0.8f - 0.4f; // Vertical spread

                    particle.transform.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        height,
                        Mathf.Sin(angle) * radius
                    );

                    Destroy(particle.GetComponent<Collider>());

                    // Wind particle material (semi-transparent, glowing)
                    Material particleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    particleMat.EnableKeyword("_EMISSION");
                    Color windColor = new Color(0.88f, 0.94f, 1f);
                    float alpha = 0.5f - layer * 0.1f; // Outer layers more transparent
                    windColor.a = alpha;
                    particleMat.SetColor("_BaseColor", windColor);
                    particleMat.SetColor("_EmissionColor", windColor * (3f - layer * 0.5f)); // HDR
                    particleMat.SetFloat("_Surface", 1); // Transparent
                    particleMat.SetFloat("_Blend", 0);
                    particleMat.SetFloat("_Smoothness", 0.8f);
                    particleMat.renderQueue = 3000;
                    particle.GetComponent<MeshRenderer>().material = particleMat;
                }
            }

            // === LAYER 5: Flying debris (8 chaotic tumbling objects) ===
            for (int i = 0; i < 8; i++)
            {
                // Mix cubes and spheres for variety
                GameObject debris = GameObject.CreatePrimitive(i % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere);
                debris.name = $"Debris{i}";
                debris.transform.SetParent(projectile.transform);

                // Small debris pieces
                debris.transform.localScale = Vector3.one * Random.Range(0.04f, 0.08f);

                // Initial random positions around cyclone
                float angle = i * 45f * Mathf.Deg2Rad;
                float radius = 0.35f;
                debris.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * radius,
                    Random.Range(-0.2f, 0.2f),
                    Mathf.Sin(angle) * radius
                );

                debris.transform.localRotation = Random.rotation;
                Destroy(debris.GetComponent<Collider>());

                // Darker debris material (looks like rocks/dust caught in wind)
                Material debrisMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                debrisMat.SetColor("_BaseColor", new Color(0.4f, 0.45f, 0.5f, 0.8f)); // Gray-ish
                debrisMat.SetFloat("_Smoothness", 0.3f); // Rough
                debris.GetComponent<MeshRenderer>().material = debrisMat;
            }

            // === PARTICLE SYSTEM 1: Wind streaks ===
            GameObject windStreaks = new GameObject("WindStreaks");
            windStreaks.transform.SetParent(projectile.transform);
            ParticleSystem ps1 = windStreaks.AddComponent<ParticleSystem>();
            var main1 = ps1.main;
            main1.startLifetime = 1.2f;
            main1.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
            main1.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.04f);
            main1.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
            main1.maxParticles = 180;
            main1.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission1 = ps1.emission;
            emission1.rateOverTime = 150f;

            var shape1 = ps1.shape;
            shape1.shapeType = ParticleSystemShapeType.Cone;
            shape1.angle = 30f;
            shape1.radius = 0.15f;

            var colorOverLifetime1 = ps1.colorOverLifetime;
            colorOverLifetime1.enabled = true;
            Gradient gradient1 = new Gradient();
            gradient1.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.9f, 0.95f, 1f), 0f),
                    new GradientColorKey(new Color(0.7f, 0.85f, 1f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.7f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime1.color = gradient1;

            // === PARTICLE SYSTEM 2: Air burst ===
            GameObject airBurst = new GameObject("AirBurst");
            airBurst.transform.SetParent(projectile.transform);
            ParticleSystem ps2 = airBurst.AddComponent<ParticleSystem>();
            var main2 = ps2.main;
            main2.startLifetime = 0.8f;
            main2.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            main2.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            main2.startColor = new Color(0.88f, 0.94f, 1f, 0.5f);
            main2.maxParticles = 100;
            main2.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission2 = ps2.emission;
            emission2.rateOverTime = 80f;

            var shape2 = ps2.shape;
            shape2.shapeType = ParticleSystemShapeType.Sphere;
            shape2.radius = 0.25f;

            // === TRAIL RENDERER: Wind streak ===
            TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.7f;
            trail.startWidth = 0.5f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            trail.material.EnableKeyword("_EMISSION");
            trail.material.SetColor("_BaseColor", new Color(0.88f, 0.94f, 1f, 0.6f));
            trail.material.SetColor("_EmissionColor", new Color(0.88f, 0.94f, 1f) * 2.5f);
            Gradient trailGradient = new Gradient();
            trailGradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.9f, 0.95f, 1f), 0f),
                    new GradientColorKey(new Color(0.7f, 0.85f, 1f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.6f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            trail.colorGradient = trailGradient;

            // === PHYSICS ===
            SphereCollider collider = projectile.AddComponent<SphereCollider>();
            collider.radius = 0.22f;

            Rigidbody rb = projectile.AddComponent<Rigidbody>();
            rb.mass = 0.1f; // Very light for satisfying long-distance throwing
            rb.useGravity = false;
            rb.linearDamping = 0.5f; // More air resistance

            PhysicsSpellProjectile physicsProj = projectile.AddComponent<PhysicsSpellProjectile>();
            physicsProj.throwForce = 18f;
            physicsProj.useGravity = true;
            physicsProj.gravityScale = 0.3f; // Very floaty for wind
            physicsProj.bounciness = 0.5f;
            physicsProj.maxBounces = 2;
            physicsProj.explodeOnImpact = true;
            physicsProj.explosionRadius = 4f; // Large wind blast
            physicsProj.explosionForce = 1000f; // Strong pushback
            physicsProj.lifetime = 10f;
            physicsProj.spellData = spell;

            // === ANIMATION: Tornado spiral with flying debris ===
            CycloneAnimation cycloneAnim = projectile.AddComponent<CycloneAnimation>();

            return projectile;
        }

        private GameObject CreateDefaultTier2Projectile(SpellData spell)
        {
            // Fallback tier 2 projectile
            return CreateMeteor(spell);
        }

        /// <summary>
        /// Public static helper to create spell explosions with ricochet particles
        /// Can be called from LinearSpellMovement or other projectile classes
        /// </summary>
        public static void CreateSpellExplosion(Vector3 position, Vector3 surfaceNormal, Vector3 direction, float speed, SpellData spellData)
        {
            if (spellData == null) return;

            string spellName = spellData.spellName.ToLower();

            if (spellName.Contains("fire") || spellName.Contains("flame"))
                CreateFireExplosionStatic(position, surfaceNormal, direction, speed);
            else if (spellName.Contains("ice") || spellName.Contains("frost") || spellName.Contains("shard"))
                CreateIceExplosionStatic(position, surfaceNormal, direction, speed);
            else if (spellName.Contains("light") || spellName.Contains("thunder") || spellName.Contains("bolt"))
                CreateLightningExplosionStatic(position, surfaceNormal, direction, speed);
            else if (spellName.Contains("wind") || spellName.Contains("air") || spellName.Contains("blast"))
                CreateWindExplosionStatic(position, surfaceNormal, direction, speed);
        }

        private static Vector3 GetRicochetDirectionStatic(Vector3 direction, Vector3 surfaceNormal)
        {
            Vector3 reflected = Vector3.Reflect(direction, surfaceNormal);
            float spreadAngle = Random.Range(30f, 60f);
            Vector3 randomOffset = Random.insideUnitSphere * Mathf.Tan(spreadAngle * Mathf.Deg2Rad);
            Vector3 ricochetDir = (reflected.normalized + randomOffset).normalized;

            if (Vector3.Dot(ricochetDir, surfaceNormal) < 0.1f)
            {
                ricochetDir = (ricochetDir + surfaceNormal * 0.5f).normalized;
            }

            return ricochetDir;
        }

        private static void CreateFireExplosionStatic(Vector3 position, Vector3 surfaceNormal, Vector3 direction, float speed)
        {
            GameObject explosion = new GameObject("FireExplosion");
            explosion.transform.position = position;

            for (int i = 0; i < 12; i++)
            {
                GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flame.name = $"Flame{i}";
                flame.transform.SetParent(explosion.transform);
                flame.transform.localPosition = Vector3.zero;
                flame.transform.localScale = Vector3.one * 0.15f;

                SphereCollider collider = flame.GetComponent<SphereCollider>();
                if (collider != null) collider.radius = 0.075f;

                Rigidbody rb = flame.AddComponent<Rigidbody>();
                rb.mass = 0.1f;
                rb.linearDamping = 0.5f;
                rb.useGravity = false;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                PhysicsMaterial bounceMat = new PhysicsMaterial("FireParticleBounce");
                bounceMat.bounciness = 0.6f;
                bounceMat.dynamicFriction = 0.3f;
                bounceMat.staticFriction = 0.3f;
                bounceMat.frictionCombine = PhysicsMaterialCombine.Average;
                bounceMat.bounceCombine = PhysicsMaterialCombine.Average;
                collider.material = bounceMat;

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(1f, Random.Range(0.3f, 0.6f), 0f, 1f) * 2f;
                flame.GetComponent<MeshRenderer>().material = mat;

                FireExplosionParticle particle = flame.AddComponent<FireExplosionParticle>();
                particle.direction = GetRicochetDirectionStatic(direction, surfaceNormal);
                particle.speed = speed * 0.4f;
                particle.rb = rb;
            }

            Object.Destroy(explosion, 1f);
        }

        private static void CreateIceExplosionStatic(Vector3 position, Vector3 surfaceNormal, Vector3 direction, float speed)
        {
            GameObject explosion = new GameObject("IceExplosion");
            explosion.transform.position = position;

            for (int i = 0; i < 10; i++)
            {
                GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = $"IceShard{i}";
                shard.transform.SetParent(explosion.transform);
                shard.transform.localPosition = Vector3.zero;
                shard.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
                shard.transform.localRotation = Random.rotation;

                BoxCollider collider = shard.GetComponent<BoxCollider>();
                if (collider != null) collider.size = new Vector3(0.8f, 0.8f, 0.8f);

                Rigidbody rb = shard.AddComponent<Rigidbody>();
                rb.mass = 0.15f;
                rb.linearDamping = 0.4f;
                rb.useGravity = false;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                PhysicsMaterial bounceMat = new PhysicsMaterial("IceParticleBounce");
                bounceMat.bounciness = 0.7f;
                bounceMat.dynamicFriction = 0.1f;
                bounceMat.staticFriction = 0.1f;
                bounceMat.frictionCombine = PhysicsMaterialCombine.Minimum;
                bounceMat.bounceCombine = PhysicsMaterialCombine.Average;
                collider.material = bounceMat;

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(0.6f, 0.9f, 1f, 1f) * 1.8f;
                shard.GetComponent<MeshRenderer>().material = mat;

                IceExplosionParticle particle = shard.AddComponent<IceExplosionParticle>();
                particle.direction = GetRicochetDirectionStatic(direction, surfaceNormal);
                particle.speed = speed * 0.45f;
                particle.rb = rb;
            }

            Object.Destroy(explosion, 1.2f);
        }

        private static void CreateLightningExplosionStatic(Vector3 position, Vector3 surfaceNormal, Vector3 direction, float speed)
        {
            GameObject explosion = new GameObject("LightningExplosion");
            explosion.transform.position = position;

            for (int i = 0; i < 15; i++)
            {
                GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spark.name = $"Spark{i}";
                spark.transform.SetParent(explosion.transform);
                spark.transform.localPosition = Vector3.zero;
                spark.transform.localScale = Vector3.one * 0.125f;

                SphereCollider collider = spark.GetComponent<SphereCollider>();
                if (collider != null) collider.radius = 0.0625f;

                Rigidbody rb = spark.AddComponent<Rigidbody>();
                rb.mass = 0.05f;
                rb.linearDamping = 0.3f;
                rb.useGravity = false;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                PhysicsMaterial bounceMat = new PhysicsMaterial("LightningParticleBounce");
                bounceMat.bounciness = 0.8f;
                bounceMat.dynamicFriction = 0.05f;
                bounceMat.staticFriction = 0.05f;
                bounceMat.frictionCombine = PhysicsMaterialCombine.Minimum;
                bounceMat.bounceCombine = PhysicsMaterialCombine.Maximum;
                collider.material = bounceMat;

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(1f, 1f, 0.95f, 1f) * 3f;
                spark.GetComponent<MeshRenderer>().material = mat;

                LightningExplosionParticle particle = spark.AddComponent<LightningExplosionParticle>();
                particle.direction = GetRicochetDirectionStatic(direction, surfaceNormal);
                particle.speed = speed * 0.55f;
                particle.rb = rb;
            }

            Object.Destroy(explosion, 0.8f);
        }

        private static void CreateWindExplosionStatic(Vector3 position, Vector3 surfaceNormal, Vector3 direction, float speed)
        {
            GameObject explosion = new GameObject("WindExplosion");
            explosion.transform.position = position;

            for (int i = 0; i < 12; i++)
            {
                GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                particle.name = $"WindParticle{i}";
                particle.transform.SetParent(explosion.transform);
                particle.transform.localPosition = Vector3.zero;
                particle.transform.localScale = Vector3.one * 0.12f;

                SphereCollider collider = particle.GetComponent<SphereCollider>();
                if (collider != null) collider.radius = 0.06f;

                Rigidbody rb = particle.AddComponent<Rigidbody>();
                rb.mass = 0.08f;
                rb.linearDamping = 0.6f;
                rb.useGravity = false;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                PhysicsMaterial bounceMat = new PhysicsMaterial("WindParticleBounce");
                bounceMat.bounciness = 0.65f;
                bounceMat.dynamicFriction = 0.2f;
                bounceMat.staticFriction = 0.2f;
                bounceMat.frictionCombine = PhysicsMaterialCombine.Average;
                bounceMat.bounceCombine = PhysicsMaterialCombine.Average;
                collider.material = bounceMat;

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(0.85f, 0.95f, 1f, 1f) * 2f;
                particle.GetComponent<MeshRenderer>().material = mat;

                WindExplosionParticle windParticle = particle.AddComponent<WindExplosionParticle>();
                windParticle.direction = GetRicochetDirectionStatic(direction, surfaceNormal);
                windParticle.speed = speed * 0.42f;
                windParticle.rb = rb;
            }

            Object.Destroy(explosion, 1f);
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
        private const float GRACE_PERIOD = 0.15f; // Don't collide for first 0.15 seconds

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

        // Helper method to check if object is part of the player/VR rig
        private bool IsPlayerObject(GameObject obj)
        {
            string name = obj.name.ToLower();

            // Check for common player/VR object names
            if (name.Contains("controller")) return true;
            if (name.Contains("hand")) return true;
            if (name.Contains("camera")) return true;
            if (name.Contains("main camera")) return true;
            if (name.Contains("xr")) return true;
            if (name.Contains("player")) return true;
            if (name.Contains("origin")) return true;
            if (name.Contains("offset")) return true;

            // Check parent hierarchy
            Transform current = obj.transform;
            while (current.parent != null)
            {
                string parentName = current.parent.name.ToLower();
                if (parentName.Contains("xr") ||
                    parentName.Contains("player") ||
                    parentName.Contains("origin"))
                {
                    return true;
                }
                current = current.parent;
            }

            return false;
        }

        void Update()
        {
            // Raycast ahead to detect surface normal before collision
            RaycastHit hit;
            float rayDistance = speed * Time.deltaTime * 2f; // Look ahead
            if (Physics.Raycast(transform.position, direction, out hit, rayDistance))
            {
                // Ignore triggers and player objects
                if (!hit.collider.isTrigger && !IsPlayerObject(hit.collider.gameObject))
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
            // Grace period - don't collide immediately after spawning
            if (Time.time < spawnTime + GRACE_PERIOD)
            {
                return;
            }

            // Ignore triggers
            if (other.isTrigger) return;

            // Ignore player/VR objects
            if (IsPlayerObject(other.gameObject)) return;

            // Only explode once
            if (hasExploded) return;
            hasExploded = true;

            Debug.Log($"[SpellProjectile] Hit {other.gameObject.name} with normal {hitNormal}, creating explosion!");

            // Create explosion effect at impact point with surface normal
            CreateExplosion(transform.position, hitNormal);

            // Stop emitting particles but let existing particles finish their lifetime
            ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                var emission = ps.emission;
                emission.enabled = false; // Stop emitting new particles

                // For world-space particles, detach from parent so they persist
                if (ps.main.simulationSpace == ParticleSystemSimulationSpace.World)
                {
                    ps.transform.SetParent(null);
                    Destroy(ps.gameObject, ps.main.startLifetime.constantMax + 1f); // Clean up after particles die
                }
            }

            // Destroy core projectile GameObject after a short delay to let particles detach
            Destroy(gameObject, 0.1f);
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
                flame.transform.localScale = Vector3.one * 0.15f; // Reduced by 50%

                // Keep collider for physics-based collision, but make it smaller
                SphereCollider collider = flame.GetComponent<SphereCollider>();
                if (collider != null)
                {
                    collider.radius = 0.075f; // Smaller than visual size (also 50% reduction)
                }

                // Add Rigidbody for physics-based collision
                Rigidbody rb = flame.AddComponent<Rigidbody>();
                rb.mass = 0.1f; // Light particles (projectile is ~1.0 mass implied)
                rb.linearDamping = 0.5f; // Some air resistance
                rb.useGravity = false; // No gravity on ricochet particles
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // Add physics material for bouncing
                PhysicsMaterial bounceMat = new PhysicsMaterial("FireParticleBounce");
                bounceMat.bounciness = 0.6f; // Bouncy but loses some energy
                bounceMat.dynamicFriction = 0.3f;
                bounceMat.staticFriction = 0.3f;
                bounceMat.frictionCombine = PhysicsMaterialCombine.Average;
                bounceMat.bounceCombine = PhysicsMaterialCombine.Average;
                collider.material = bounceMat;

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(1f, Random.Range(0.3f, 0.6f), 0f, 1f) * 2f;
                flame.GetComponent<MeshRenderer>().material = mat;

                FireExplosionParticle particle = flame.AddComponent<FireExplosionParticle>();
                particle.direction = GetRicochetDirection(surfaceNormal); // Realistic ricochet
                // Scale speed based on projectile speed (40% of projectile speed accounting for mass difference)
                float particleSpeed = this.speed * 0.4f;
                particle.speed = particleSpeed;
                particle.rb = rb; // Pass rigidbody reference
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
                shard.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f); // Reduced by 50%
                shard.transform.localRotation = Random.rotation;

                // Keep collider for physics-based collision
                BoxCollider collider = shard.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    collider.size = new Vector3(0.8f, 0.8f, 0.8f); // Slightly smaller than visual
                }

                // Add Rigidbody for physics-based collision
                Rigidbody rb = shard.AddComponent<Rigidbody>();
                rb.mass = 0.15f; // Slightly heavier than fire (denser ice)
                rb.linearDamping = 0.4f; // Less air resistance (more aerodynamic shards)
                rb.useGravity = false; // No gravity on ricochet particles
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // Add physics material for bouncing
                PhysicsMaterial bounceMat = new PhysicsMaterial("IceParticleBounce");
                bounceMat.bounciness = 0.7f; // More bouncy (hard ice)
                bounceMat.dynamicFriction = 0.1f; // Slippery ice
                bounceMat.staticFriction = 0.1f;
                bounceMat.frictionCombine = PhysicsMaterialCombine.Minimum;
                bounceMat.bounceCombine = PhysicsMaterialCombine.Average;
                collider.material = bounceMat;

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(0.6f, 0.9f, 1f, 1f) * 1.8f;
                shard.GetComponent<MeshRenderer>().material = mat;

                IceExplosionParticle particle = shard.AddComponent<IceExplosionParticle>();
                particle.direction = GetRicochetDirection(surfaceNormal); // Realistic ricochet
                // Scale speed based on projectile speed (45% - slightly faster due to less drag)
                float particleSpeed = this.speed * 0.45f;
                particle.speed = particleSpeed;
                particle.rb = rb; // Pass rigidbody reference
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
                spark.transform.localScale = Vector3.one * 0.125f; // Reduced by 50%

                // Keep collider for physics-based collision
                SphereCollider collider = spark.GetComponent<SphereCollider>();
                if (collider != null)
                {
                    collider.radius = 0.0625f; // Smaller than visual size (also 50% reduction)
                }

                // Add Rigidbody for physics-based collision
                Rigidbody rb = spark.AddComponent<Rigidbody>();
                rb.mass = 0.05f; // Very light (pure energy)
                rb.linearDamping = 0.3f; // Low drag (fast moving energy)
                rb.useGravity = false; // Lightning sparks don't fall immediately
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // Add physics material for bouncing
                PhysicsMaterial bounceMat = new PhysicsMaterial("LightningParticleBounce");
                bounceMat.bounciness = 0.8f; // Very bouncy (energetic)
                bounceMat.dynamicFriction = 0.05f; // Almost no friction (energy discharge)
                bounceMat.staticFriction = 0.05f;
                bounceMat.frictionCombine = PhysicsMaterialCombine.Minimum;
                bounceMat.bounceCombine = PhysicsMaterialCombine.Maximum;
                collider.material = bounceMat;

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(1f, 1f, 0.95f, 1f) * 3f;
                spark.GetComponent<MeshRenderer>().material = mat;

                LightningExplosionParticle particle = spark.AddComponent<LightningExplosionParticle>();
                particle.direction = GetRicochetDirection(surfaceNormal); // Realistic ricochet
                // Scale speed based on projectile speed (55% - fastest ricochet due to energy)
                float particleSpeed = this.speed * 0.55f;
                particle.speed = particleSpeed;
                particle.rb = rb; // Pass rigidbody reference
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
                particle.transform.localScale = Vector3.one * 0.1f; // Reduced by 50%

                // Keep collider for physics-based collision
                SphereCollider collider = particle.GetComponent<SphereCollider>();
                if (collider != null)
                {
                    collider.radius = 0.05f; // Smaller than visual size (also 50% reduction)
                }

                // Add Rigidbody for physics-based collision
                Rigidbody rb = particle.AddComponent<Rigidbody>();
                rb.mass = 0.03f; // Very light (air particles)
                rb.linearDamping = 0.6f; // Higher drag (dispersing air)
                rb.useGravity = false; // Wind particles float initially
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // Add physics material for bouncing
                PhysicsMaterial bounceMat = new PhysicsMaterial("WindParticleBounce");
                bounceMat.bounciness = 0.5f; // Moderate bounce (soft air)
                bounceMat.dynamicFriction = 0.4f;
                bounceMat.staticFriction = 0.4f;
                bounceMat.frictionCombine = PhysicsMaterialCombine.Average;
                bounceMat.bounceCombine = PhysicsMaterialCombine.Average;
                collider.material = bounceMat;

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(0.85f, 0.95f, 1f, 1f) * 2f;
                particle.GetComponent<MeshRenderer>().material = mat;

                WindExplosionParticle windParticle = particle.AddComponent<WindExplosionParticle>();
                windParticle.direction = GetRicochetDirection(surfaceNormal); // Realistic ricochet
                // Scale speed based on projectile speed (42% - moderate speed for air)
                float particleSpeed = this.speed * 0.42f;
                windParticle.speed = particleSpeed;
                windParticle.rb = rb; // Pass rigidbody reference
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

    /// <summary>
    /// Animates tier-2 Meteor projectile with tumbling motion, orbiting lava chunks, and drifting smoke
    /// </summary>
    public class MeteorAnimation : MonoBehaviour
    {
        private float time;

        void Update()
        {
            time += Time.deltaTime;

            // Tumble entire meteor chaotically
            transform.Rotate(new Vector3(0.7f, 1f, 0.5f), 120f * Time.deltaTime);

            // Pulse lava shell (cracking effect)
            Transform lavaShell = transform.Find("LavaShell");
            if (lavaShell != null)
            {
                float pulse = 1f + Mathf.Sin(time * 6f) * 0.08f;
                lavaShell.localScale = Vector3.one * 0.55f * pulse;
            }

            // Orbit lava chunks chaotically
            for (int i = 0; i < 10; i++)
            {
                Transform chunk = transform.Find($"LavaChunk{i}");
                if (chunk != null)
                {
                    // Each chunk orbits at different speed and direction
                    float speed = 2f + (i % 3) * 1f; // Vary speed 2-5
                    float direction = (i % 2 == 0) ? 1f : -1f; // Alternate directions
                    float angle = (time * speed * direction + i * 36f) * Mathf.Deg2Rad;
                    float radius = 0.45f;
                    float heightOffset = Mathf.Sin(time * speed + i) * 0.1f; // Bobbing motion

                    chunk.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        heightOffset,
                        Mathf.Sin(angle) * radius
                    );

                    // Tumble each chunk
                    chunk.Rotate(new Vector3(1f, 0.5f, 0.3f), 300f * Time.deltaTime);
                }
            }

            // Drift smoke wisps upward and outward
            for (int i = 0; i < 8; i++)
            {
                Transform smoke = transform.Find($"SmokeWisp{i}");
                if (smoke != null)
                {
                    // Slow spiral outward
                    float angle = (time * 0.5f + i * 45f) * Mathf.Deg2Rad;
                    float radius = 0.55f + (time * 0.1f) % 0.2f; // Expand slowly
                    float height = Mathf.Sin(time * 0.3f + i) * 0.15f;

                    smoke.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        height,
                        Mathf.Sin(angle) * radius
                    );

                    // Fade and grow smoke
                    float scale = 0.08f + (time * 0.02f) % 0.04f;
                    smoke.localScale = Vector3.one * scale;
                }
            }
        }
    }

    /// <summary>
    /// Animates tier-2 FrostBoulder projectile with rotating ice spikes and orbiting fragments
    /// </summary>
    public class FrostBoulderAnimation : MonoBehaviour
    {
        private float time;

        void Update()
        {
            time += Time.deltaTime;

            // Pulse cyan core
            Transform core = transform.Find("CyanCore");
            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(time * 12f) * 0.15f;
                core.localScale = Vector3.one * 0.2f * pulse;
            }

            // Rotate entire ice structure slowly
            transform.Rotate(Vector3.up, 60f * Time.deltaTime);

            // Rotate ice spikes as a formation
            for (int i = 0; i < 6; i++)
            {
                Transform spike = transform.Find($"IceSpike{i}");
                if (spike != null)
                {
                    // Bob slightly
                    float bob = Mathf.Sin(time * 4f + i) * 0.03f;
                    float angle = i * 60f * Mathf.Deg2Rad;
                    spike.localPosition = new Vector3(
                        Mathf.Cos(angle) * (0.3f + bob),
                        0f,
                        Mathf.Sin(angle) * (0.3f + bob)
                    );
                }
            }

            // Orbit ice fragments
            for (int i = 0; i < 12; i++)
            {
                Transform fragment = transform.Find($"IceFragment{i}");
                if (fragment != null)
                {
                    // Orbit in two counter-rotating rings
                    float speed = (i < 6) ? 3f : -2.5f;
                    float angle = (time * speed + i * 30f) * Mathf.Deg2Rad;
                    float radius = (i < 6) ? 0.45f : 0.5f;
                    float height = (i < 6) ? 0.1f : -0.1f;

                    fragment.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        height + Mathf.Sin(time * 3f + i) * 0.05f,
                        Mathf.Sin(angle) * radius
                    );

                    // Spin fragments
                    fragment.Rotate(Vector3.up, 400f * Time.deltaTime);
                }
            }
        }
    }

    /// <summary>
    /// Animates tier-2 ThunderOrb projectile with rotating arc rings and flickering energy
    /// </summary>
    public class ThunderOrbAnimation : MonoBehaviour
    {
        private float time;

        void Update()
        {
            time += Time.deltaTime;

            // Intense core pulse
            Transform core = transform.Find("LightningCore");
            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(time * 20f) * 0.3f;
                core.localScale = Vector3.one * 0.18f * pulse;
            }

            // Flicker outer layers
            Transform blueLayer = transform.Find("ElectricBlueLayer");
            if (blueLayer != null)
            {
                float flicker = 1f + Random.Range(-0.1f, 0.1f);
                blueLayer.localScale = Vector3.one * 0.35f * flicker;
            }

            // Rotate each arc ring at different speeds
            for (int ring = 0; ring < 3; ring++)
            {
                float ringSpeed = 100f + ring * 50f; // 100, 150, 200 deg/sec
                if (ring == 1) ringSpeed *= -1; // Middle ring counter-rotates

                for (int i = 0; i < 8; i++)
                {
                    Transform segment = transform.Find($"ArcRing{ring}_Seg{i}");
                    if (segment != null)
                    {
                        // Rotate ring
                        float baseAngle = i * 45f;
                        float rotatedAngle = (baseAngle + time * ringSpeed) * Mathf.Deg2Rad;
                        float radius = 0.35f;

                        // Position segments in 3D arc based on ring index
                        Vector3 position = Vector3.zero;
                        if (ring == 0)
                        {
                            // Horizontal ring (XZ plane)
                            position = new Vector3(
                                Mathf.Cos(rotatedAngle) * radius,
                                0f,
                                Mathf.Sin(rotatedAngle) * radius
                            );
                        }
                        else if (ring == 1)
                        {
                            // Vertical ring (XY plane)
                            position = new Vector3(
                                Mathf.Cos(rotatedAngle) * radius,
                                Mathf.Sin(rotatedAngle) * radius,
                                0f
                            );
                        }
                        else
                        {
                            // Diagonal ring (YZ plane)
                            position = new Vector3(
                                0f,
                                Mathf.Cos(rotatedAngle) * radius,
                                Mathf.Sin(rotatedAngle) * radius
                            );
                        }

                        segment.localPosition = position;

                        // Flicker individual segments
                        float flicker = Random.value < 0.1f ? 0.5f : 1f;
                        float scale = 0.04f * flicker;
                        segment.localScale = new Vector3(0.04f, 0.12f, 0.04f) * flicker;
                    }
                }
            }

            // Crackling energy sparks
            for (int i = 0; i < 10; i++)
            {
                Transform spark = transform.Find($"EnergySpark{i}");
                if (spark != null)
                {
                    // Random jittery movement
                    Vector3 randomOffset = Random.insideUnitSphere * 0.3f;
                    spark.localPosition = randomOffset;

                    // Random flicker
                    float flicker = Random.Range(0.5f, 1.5f);
                    spark.localScale = Vector3.one * 0.025f * flicker;
                }
            }
        }
    }

    /// <summary>
    /// Animates tier-2 Cyclone projectile with multi-layer tornado spiral and flying debris
    /// </summary>
    public class CycloneAnimation : MonoBehaviour
    {
        private float time;

        void Update()
        {
            time += Time.deltaTime;

            // Pulse core
            Transform core = transform.Find("VortexCore");
            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(time * 15f) * 0.2f;
                core.localScale = Vector3.one * 0.15f * pulse;
            }

            // Animate tornado spiral layers
            for (int layer = 0; layer < 3; layer++)
            {
                int particlesPerLayer = 10;
                float layerSpeed = 3f + layer * 1.5f; // Each layer spins faster

                for (int i = 0; i < particlesPerLayer; i++)
                {
                    Transform particle = transform.Find($"TornadoLayer{layer}_P{i}");
                    if (particle != null)
                    {
                        // Spiral pattern
                        float t = (time * layerSpeed + i / (float)particlesPerLayer) % 1f;
                        float angle = (t * 720f) * Mathf.Deg2Rad; // Two full rotations
                        float radius = (0.2f + layer * 0.1f) * (1f - t * 0.3f); // Tightening spiral
                        float height = t * 0.8f - 0.4f; // Vertical spread

                        particle.localPosition = new Vector3(
                            Mathf.Cos(angle) * radius,
                            height,
                            Mathf.Sin(angle) * radius
                        );

                        // Fade particles along spiral
                        float alpha = 1f - t;
                        particle.localScale = Vector3.one * 0.06f * alpha;
                    }
                }
            }

            // Flying debris - violent chaotic movement
            for (int i = 0; i < 8; i++)
            {
                Transform debris = transform.Find($"Debris{i}");
                if (debris != null)
                {
                    // Chaotic orbit
                    float speed = 4f + (i % 3);
                    float angle = (time * speed + i * 45f) * Mathf.Deg2Rad;
                    float radius = 0.35f + Mathf.Sin(time * 2f + i) * 0.1f;
                    float height = Mathf.Cos(time * 3f + i) * 0.25f;

                    debris.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        height,
                        Mathf.Sin(angle) * radius
                    );

                    // Violent tumbling
                    debris.Rotate(new Vector3(1f, 0.7f, 0.5f), 500f * Time.deltaTime);
                }
            }

            // Spin entire vortex
            transform.Rotate(Vector3.up, 200f * Time.deltaTime);
        }
    }

    public class ChargePopAnimation : MonoBehaviour
    {
        public bool isQuickPop = false; // If true, smaller expansion and faster
        private float elapsed = 0f;
        private float lifetime;
        private Vector3 startScale;
        private float targetScale;
        private Color initialBaseColor;
        private Color initialEmissionColor;
        private Material material;

        void Start()
        {
            startScale = transform.localScale;

            // Configure based on pop type
            if (isQuickPop)
            {
                // Quick pop: small expansion, fast (bubble was already at 0.6, so expand slightly)
                lifetime = 0.15f;
                targetScale = 0.65f; // Only 8% larger than bubble
            }
            else
            {
                // Normal pop: not used anymore
                lifetime = 0.3f;
                targetScale = 0.6f;
            }

            // Cache material and initial colors
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                material = renderer.material; // Create instance
                initialBaseColor = material.GetColor("_BaseColor");
                initialEmissionColor = material.GetColor("_EmissionColor");
            }
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / lifetime;

            // Expand quickly
            float scale = Mathf.Lerp(startScale.x, targetScale, progress);
            transform.localScale = Vector3.one * scale;

            // Fade out smoothly
            if (material != null)
            {
                // Fade base color alpha
                float alpha = Mathf.Lerp(0.8f, 0f, progress);
                Color baseColor = initialBaseColor;
                baseColor.a = alpha;
                material.SetColor("_BaseColor", baseColor);

                // Fade emission color intensity
                float emissionIntensity = Mathf.Lerp(1f, 0f, progress);
                Color emissionColor = initialEmissionColor * emissionIntensity;
                material.SetColor("_EmissionColor", emissionColor);
            }

            // Destroy when done
            if (elapsed >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }

    #endregion

    #region Explosion Particle Animations

    public class FireExplosionParticle : MonoBehaviour
    {
        public Vector3 direction;
        public float speed = 2f;
        public Rigidbody rb;
        private float elapsed = 0f;
        private float lifetime = 1f;

        void Start()
        {
            // Initialize velocity using Rigidbody physics
            if (rb != null)
            {
                rb.linearVelocity = direction * speed;
            }
        }

        void Update()
        {
            elapsed += Time.deltaTime;

            // Physics handles movement via Rigidbody.velocity
            // Collisions and bouncing handled automatically by PhysicMaterial

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
        public Rigidbody rb;
        private float elapsed = 0f;
        private float lifetime = 1.2f;

        void Start()
        {
            // Initialize velocity using Rigidbody physics
            if (rb != null)
            {
                rb.linearVelocity = direction * speed;
            }
        }

        void Update()
        {
            elapsed += Time.deltaTime;

            // Physics handles movement via Rigidbody.velocity
            // Collisions and bouncing handled automatically by PhysicMaterial

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
        public Rigidbody rb;
        private float elapsed = 0f;
        private float lifetime = 0.8f;

        void Start()
        {
            // Initialize velocity using Rigidbody physics
            if (rb != null)
            {
                rb.linearVelocity = direction * speed;
            }
        }

        void Update()
        {
            elapsed += Time.deltaTime;

            // Physics handles basic movement via Rigidbody.velocity
            // Add chaotic electric behavior as force adjustments
            if (rb != null)
            {
                Vector3 chaoticForce = new Vector3(
                    Mathf.PerlinNoise(Time.time * 10f, elapsed) - 0.5f,
                    Mathf.PerlinNoise(Time.time * 10f + 10f, elapsed) - 0.5f,
                    Mathf.PerlinNoise(Time.time * 10f + 20f, elapsed) - 0.5f
                );
                rb.AddForce(chaoticForce * speed * 0.5f, ForceMode.Force);
            }

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
        public Rigidbody rb;
        private float elapsed = 0f;
        private float lifetime = 1f;

        void Start()
        {
            // Initialize velocity using Rigidbody physics
            if (rb != null)
            {
                rb.linearVelocity = direction * speed;
            }
        }

        void Update()
        {
            elapsed += Time.deltaTime;

            // Physics handles basic movement via Rigidbody.velocity
            // Apply spiral force to create swirling wind effect
            if (rb != null)
            {
                // Rotate velocity vector to create spiral
                float spiralAngle = elapsed * 360f * 3f;
                Vector3 currentVelocity = rb.linearVelocity;
                Vector3 spiralVelocity = Quaternion.Euler(0f, Time.deltaTime * 360f * 3f, 0f) * currentVelocity;
                rb.linearVelocity = spiralVelocity;
            }

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
