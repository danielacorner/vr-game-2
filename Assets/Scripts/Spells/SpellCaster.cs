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
