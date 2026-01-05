using UnityEngine;
using System.Collections;
using VRDungeonCrawler.Player;

namespace VRDungeonCrawler.Spells
{
    /// <summary>
    /// Creates Bioshock-style spell equip animations on the hand
    /// - Explosive intro animation when spell is first equipped
    /// - Continuous looping animation while spell is equipped
    /// - Different effects for each element type
    /// </summary>
    public class SpellHandVisualEffect : MonoBehaviour
    {
        [Header("Effect Settings")]
        [Tooltip("Distance from hand where effect appears")]
        public float effectDistance = 0.08f;

        [Tooltip("Scale of the effect particles")]
        public float effectScale = 0.15f;

        private SpellData currentSpell;
        private GameObject introEffect;
        private GameObject loopEffect;
        private bool isPlayingIntro = false;

        private void Start()
        {
            // Subscribe to spell selection changes
            if (VRDungeonCrawler.Player.SpellManager.Instance != null)
            {
                VRDungeonCrawler.Player.SpellManager.Instance.OnSpellChanged += OnSpellEquipped;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (VRDungeonCrawler.Player.SpellManager.Instance != null)
            {
                VRDungeonCrawler.Player.SpellManager.Instance.OnSpellChanged -= OnSpellEquipped;
            }

            CleanupEffects();
        }

        private void OnSpellEquipped(SpellData spell)
        {
            if (spell == null)
            {
                CleanupEffects();
                return;
            }

            // If same spell, don't replay intro
            if (currentSpell == spell)
                return;

            currentSpell = spell;
            StartCoroutine(PlayEquipSequence(spell));
        }

        private IEnumerator PlayEquipSequence(SpellData spell)
        {
            // Clean up any existing effects
            CleanupEffects();

            isPlayingIntro = true;

            // Create loop effect FIRST (instant feedback)
            loopEffect = CreateLoopEffect(spell);
            Debug.Log($"[SpellHandVFX] Loop effect active for {spell.spellName}");

            // Create subtle intro effect that plays simultaneously
            introEffect = CreateIntroEffect(spell);
            Debug.Log($"[SpellHandVFX] Playing subtle intro animation");

            // Wait for intro to complete (0.5 seconds - shorter and more subtle)
            yield return new WaitForSeconds(0.5f);

            // Destroy intro effect (loop continues)
            if (introEffect != null)
                Destroy(introEffect);

            isPlayingIntro = false;
            Debug.Log($"[SpellHandVFX] Intro complete, loop continues");
        }

        private GameObject CreateIntroEffect(SpellData spell)
        {
            string spellName = spell.spellName.ToLower();

            if (spellName.Contains("fire") || spellName.Contains("flame"))
                return CreateFireIntro(spell);
            else if (spellName.Contains("ice") || spellName.Contains("frost") || spellName.Contains("shard"))
                return CreateIceIntro(spell);
            else if (spellName.Contains("light") || spellName.Contains("thunder") || spellName.Contains("bolt"))
                return CreateLightningIntro(spell);
            else if (spellName.Contains("wind") || spellName.Contains("air") || spellName.Contains("blast"))
                return CreateWindIntro(spell);
            else
                return CreateDefaultIntro(spell);
        }

        private GameObject CreateLoopEffect(SpellData spell)
        {
            string spellName = spell.spellName.ToLower();

            if (spellName.Contains("fire") || spellName.Contains("flame"))
                return CreateFireLoop(spell);
            else if (spellName.Contains("ice") || spellName.Contains("frost") || spellName.Contains("shard"))
                return CreateIceLoop(spell);
            else if (spellName.Contains("light") || spellName.Contains("thunder") || spellName.Contains("bolt"))
                return CreateLightningLoop(spell);
            else if (spellName.Contains("wind") || spellName.Contains("air") || spellName.Contains("blast"))
                return CreateWindLoop(spell);
            else
                return CreateDefaultLoop(spell);
        }

        #region Fire Effects

        private GameObject CreateFireIntro(SpellData spell)
        {
            GameObject intro = new GameObject("FireIntro");
            intro.transform.SetParent(transform);
            intro.transform.localPosition = Vector3.forward * effectDistance;
            intro.transform.localScale = Vector3.one * effectScale * 1.2f; // Subtle size increase

            // Subtle burst of fire particles (reduced from 20 to 8)
            for (int i = 0; i < 8; i++)
            {
                GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                particle.transform.SetParent(intro.transform);
                particle.transform.localPosition = Vector3.zero;
                particle.transform.localScale = Vector3.one * Random.Range(0.15f, 0.25f); // Smaller

                Destroy(particle.GetComponent<Collider>());

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(1f, Random.Range(0.3f, 0.6f), 0f, 0.7f); // More transparent
                mat.SetFloat("_Metallic", 0f);
                mat.SetFloat("_Smoothness", 0.9f);
                particle.GetComponent<MeshRenderer>().material = mat;

                // Add explosion animation - slower speed
                FireParticleExplosion explosion = particle.AddComponent<FireParticleExplosion>();
                explosion.direction = Random.onUnitSphere;
                explosion.speed = Random.Range(0.5f, 0.8f); // Reduced speed
                explosion.lifetime = 0.5f; // Shorter lifetime
            }

            return intro;
        }

        private GameObject CreateFireLoop(SpellData spell)
        {
            GameObject loop = new GameObject("FireLoop");
            loop.transform.SetParent(transform);
            loop.transform.localPosition = Vector3.forward * effectDistance;
            loop.transform.localScale = Vector3.one * effectScale;

            // Flickering flame core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "FlameCore";
            core.transform.SetParent(loop.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.5f;

            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Sprites/Default"));
            coreMat.color = new Color(1f, 0.4f, 0f, 0.9f);
            coreMat.SetFloat("_Metallic", 0f);
            coreMat.SetFloat("_Smoothness", 0.9f);
            core.GetComponent<MeshRenderer>().material = coreMat;

            // Add pulsing animation
            FirePulseAnimation pulse = loop.AddComponent<FirePulseAnimation>();
            pulse.core = core.transform;

            // Orbiting embers (3)
            for (int i = 0; i < 3; i++)
            {
                GameObject ember = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ember.name = $"Ember{i}";
                ember.transform.SetParent(loop.transform);
                ember.transform.localScale = Vector3.one * 0.2f;

                Destroy(ember.GetComponent<Collider>());

                Material emberMat = new Material(Shader.Find("Sprites/Default"));
                emberMat.color = new Color(1f, 0.6f, 0f, 0.8f);
                emberMat.SetFloat("_Metallic", 0f);
                emberMat.SetFloat("_Smoothness", 0.8f);
                ember.GetComponent<MeshRenderer>().material = emberMat;

                pulse.embers[i] = ember.transform;
            }

            return loop;
        }

        #endregion

        #region Ice Effects

        private GameObject CreateIceIntro(SpellData spell)
        {
            GameObject intro = new GameObject("IceIntro");
            intro.transform.SetParent(transform);
            intro.transform.localPosition = Vector3.forward * effectDistance;
            intro.transform.localScale = Vector3.one * effectScale * 1.2f; // Subtle size increase

            // Subtle crystallization burst (reduced from 12 to 6)
            for (int i = 0; i < 6; i++)
            {
                GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crystal.transform.SetParent(intro.transform);
                crystal.transform.localPosition = Vector3.zero;
                crystal.transform.localScale = new Vector3(0.06f, Random.Range(0.2f, 0.35f), 0.06f); // Smaller
                crystal.transform.localRotation = Random.rotation;

                Destroy(crystal.GetComponent<Collider>());

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(0.6f, 0.9f, 1f, 0.7f); // More transparent
                mat.SetFloat("_Metallic", 0.3f);
                mat.SetFloat("_Smoothness", 0.95f);
                crystal.GetComponent<MeshRenderer>().material = mat;

                // Add explosion animation - slower speed
                IceParticleExplosion explosion = crystal.AddComponent<IceParticleExplosion>();
                explosion.direction = Random.onUnitSphere;
                explosion.speed = Random.Range(0.4f, 0.7f); // Reduced speed
                explosion.lifetime = 0.5f; // Shorter lifetime
            }

            return intro;
        }

        private GameObject CreateIceLoop(SpellData spell)
        {
            GameObject loop = new GameObject("IceLoop");
            loop.transform.SetParent(transform);
            loop.transform.localPosition = Vector3.forward * effectDistance;
            loop.transform.localScale = Vector3.one * effectScale;

            // Rotating ice shards (4)
            for (int i = 0; i < 4; i++)
            {
                GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = $"IceShard{i}";
                shard.transform.SetParent(loop.transform);
                shard.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);

                Destroy(shard.GetComponent<Collider>());

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(0.6f, 0.9f, 1f, 0.8f);
                mat.SetFloat("_Metallic", 0.3f);
                mat.SetFloat("_Smoothness", 0.95f);
                shard.GetComponent<MeshRenderer>().material = mat;
            }

            // Add rotation animation
            IceRotationAnimation rotation = loop.AddComponent<IceRotationAnimation>();

            return loop;
        }

        #endregion

        #region Lightning Effects

        private GameObject CreateLightningIntro(SpellData spell)
        {
            GameObject intro = new GameObject("LightningIntro");
            intro.transform.SetParent(transform);
            intro.transform.localPosition = Vector3.forward * effectDistance;
            intro.transform.localScale = Vector3.one * effectScale * 1.2f; // Subtle size increase

            // Subtle electric burst (reduced from 15 to 6)
            for (int i = 0; i < 6; i++)
            {
                GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spark.transform.SetParent(intro.transform);
                spark.transform.localPosition = Vector3.zero;
                spark.transform.localScale = Vector3.one * Random.Range(0.1f, 0.18f); // Smaller

                Destroy(spark.GetComponent<Collider>());

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(0.8f, 0.9f, 1f, 0.7f); // More transparent
                mat.SetFloat("_Metallic", 0.2f);
                mat.SetFloat("_Smoothness", 1f);
                spark.GetComponent<MeshRenderer>().material = mat;

                // Add explosion animation - slower speed
                LightningParticleExplosion explosion = spark.AddComponent<LightningParticleExplosion>();
                explosion.direction = Random.onUnitSphere;
                explosion.speed = Random.Range(0.8f, 1.2f); // Reduced speed
                explosion.lifetime = 0.5f; // Shorter lifetime
            }

            return intro;
        }

        private GameObject CreateLightningLoop(SpellData spell)
        {
            GameObject loop = new GameObject("LightningLoop");
            loop.transform.SetParent(transform);
            loop.transform.localPosition = Vector3.forward * effectDistance;
            loop.transform.localScale = Vector3.one * effectScale;

            // Central energy core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "LightningCore";
            core.transform.SetParent(loop.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.4f;

            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Sprites/Default"));
            coreMat.color = new Color(0.9f, 0.95f, 1f, 0.95f);
            coreMat.SetFloat("_Metallic", 0.2f);
            coreMat.SetFloat("_Smoothness", 1f);
            core.GetComponent<MeshRenderer>().material = coreMat;

            // Orbiting sparks (6)
            for (int i = 0; i < 6; i++)
            {
                GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spark.name = $"Spark{i}";
                spark.transform.SetParent(loop.transform);
                spark.transform.localScale = Vector3.one * 0.15f;

                Destroy(spark.GetComponent<Collider>());

                Material sparkMat = new Material(Shader.Find("Sprites/Default"));
                sparkMat.color = new Color(1f, 1f, 0.9f, 0.9f);
                sparkMat.SetFloat("_Metallic", 0f);
                sparkMat.SetFloat("_Smoothness", 1f);
                spark.GetComponent<MeshRenderer>().material = sparkMat;
            }

            // Add animation
            LightningOrbitAnimation orbit = loop.AddComponent<LightningOrbitAnimation>();
            orbit.core = core.transform;

            return loop;
        }

        #endregion

        #region Wind Effects

        private GameObject CreateWindIntro(SpellData spell)
        {
            GameObject intro = new GameObject("WindIntro");
            intro.transform.SetParent(transform);
            intro.transform.localPosition = Vector3.forward * effectDistance;
            intro.transform.localScale = Vector3.one * effectScale * 1.2f; // Subtle size increase

            // Subtle swirling wind burst (reduced from 16 to 6)
            for (int i = 0; i < 6; i++)
            {
                GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                particle.transform.SetParent(intro.transform);
                particle.transform.localPosition = Vector3.zero;
                particle.transform.localScale = Vector3.one * Random.Range(0.1f, 0.18f); // Smaller

                Destroy(particle.GetComponent<Collider>());

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(0.85f, 0.95f, 0.95f, 0.5f); // More transparent
                mat.SetFloat("_Metallic", 0f);
                mat.SetFloat("_Smoothness", 0.8f);
                particle.GetComponent<MeshRenderer>().material = mat;

                // Add spiral explosion animation - slower speed
                WindParticleExplosion explosion = particle.AddComponent<WindParticleExplosion>();
                explosion.direction = Random.onUnitSphere;
                explosion.speed = Random.Range(0.5f, 0.9f); // Reduced speed
                explosion.lifetime = 0.5f; // Shorter lifetime
            }

            return intro;
        }

        private GameObject CreateWindLoop(SpellData spell)
        {
            GameObject loop = new GameObject("WindLoop");
            loop.transform.SetParent(transform);
            loop.transform.localPosition = Vector3.forward * effectDistance;
            loop.transform.localScale = Vector3.one * effectScale;

            // Spiraling wind particles (8)
            for (int i = 0; i < 8; i++)
            {
                GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                particle.name = $"WindParticle{i}";
                particle.transform.SetParent(loop.transform);
                particle.transform.localScale = Vector3.one * 0.15f;

                Destroy(particle.GetComponent<Collider>());

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(0.85f, 0.95f, 0.95f, 0.6f);
                mat.SetFloat("_Metallic", 0f);
                mat.SetFloat("_Smoothness", 0.8f);
                particle.GetComponent<MeshRenderer>().material = mat;
            }

            // Add spiral animation
            WindSpiralAnimation spiral = loop.AddComponent<WindSpiralAnimation>();

            return loop;
        }

        #endregion

        #region Default Effects

        private GameObject CreateDefaultIntro(SpellData spell)
        {
            GameObject intro = new GameObject("DefaultIntro");
            intro.transform.SetParent(transform);
            intro.transform.localPosition = Vector3.forward * effectDistance;
            intro.transform.localScale = Vector3.one * effectScale * 2f;

            // Simple burst
            for (int i = 0; i < 10; i++)
            {
                GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                particle.transform.SetParent(intro.transform);
                particle.transform.localPosition = Vector3.zero;
                particle.transform.localScale = Vector3.one * 0.3f;

                Destroy(particle.GetComponent<Collider>());

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = spell.spellColor;
                particle.GetComponent<MeshRenderer>().material = mat;

                // Add explosion animation
                FireParticleExplosion explosion = particle.AddComponent<FireParticleExplosion>();
                explosion.direction = Random.onUnitSphere;
                explosion.speed = Random.Range(1f, 2f);
            }

            return intro;
        }

        private GameObject CreateDefaultLoop(SpellData spell)
        {
            GameObject loop = new GameObject("DefaultLoop");
            loop.transform.SetParent(transform);
            loop.transform.localPosition = Vector3.forward * effectDistance;
            loop.transform.localScale = Vector3.one * effectScale;

            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.transform.SetParent(loop.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.5f;

            Destroy(core.GetComponent<Collider>());

            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = spell.spellColor;
            core.GetComponent<MeshRenderer>().material = mat;

            FirePulseAnimation pulse = loop.AddComponent<FirePulseAnimation>();
            pulse.core = core.transform;

            return loop;
        }

        #endregion

        private void CleanupEffects()
        {
            if (introEffect != null)
            {
                Destroy(introEffect);
                introEffect = null;
            }

            if (loopEffect != null)
            {
                Destroy(loopEffect);
                loopEffect = null;
            }
        }
    }

    #region Animation Helper Components

    // Fire
    public class FireParticleExplosion : MonoBehaviour
    {
        public Vector3 direction;
        public float speed = 1f;
        public float lifetime = 1f;
        private float elapsed = 0f;

        void Update()
        {
            elapsed += Time.deltaTime;
            transform.localPosition += direction * speed * Time.deltaTime;

            float alpha = 1f - (elapsed / lifetime);
            GetComponent<MeshRenderer>().material.color = new Color(1f, 0.4f, 0f, alpha * 0.9f);

            if (elapsed >= lifetime)
                Destroy(gameObject);
        }
    }

    public class FirePulseAnimation : MonoBehaviour
    {
        public Transform core;
        public Transform[] embers = new Transform[3];
        private float time;

        void Update()
        {
            time += Time.deltaTime;

            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(time * 5f) * 0.3f;
                core.localScale = Vector3.one * 0.5f * pulse;
            }

            for (int i = 0; i < embers.Length; i++)
            {
                if (embers[i] != null)
                {
                    float angle = (time * 2f + i * 120f) * Mathf.Deg2Rad;
                    float radius = 0.6f + Mathf.Sin(time * 3f + i) * 0.1f;
                    embers[i].localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(time * 2f + i) * 0.3f,
                        Mathf.Sin(angle) * radius
                    );
                }
            }
        }
    }

    // Ice
    public class IceParticleExplosion : MonoBehaviour
    {
        public Vector3 direction;
        public float speed = 1f;
        public float lifetime = 1f;
        private float elapsed = 0f;

        void Update()
        {
            elapsed += Time.deltaTime;
            transform.localPosition += direction * speed * Time.deltaTime;
            transform.Rotate(Vector3.up, 500f * Time.deltaTime);

            float alpha = 1f - (elapsed / lifetime);
            GetComponent<MeshRenderer>().material.color = new Color(0.6f, 0.9f, 1f, alpha * 0.9f);

            if (elapsed >= lifetime)
                Destroy(gameObject);
        }
    }

    public class IceRotationAnimation : MonoBehaviour
    {
        private float time;

        void Update()
        {
            time += Time.deltaTime;

            transform.Rotate(Vector3.up, 60f * Time.deltaTime);

            for (int i = 0; i < 4; i++)
            {
                Transform shard = transform.Find($"IceShard{i}");
                if (shard != null)
                {
                    float angle = (i * 90f) * Mathf.Deg2Rad;
                    float radius = 0.5f;
                    shard.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(time * 2f + i) * 0.2f,
                        Mathf.Sin(angle) * radius
                    );
                }
            }
        }
    }

    // Lightning
    public class LightningParticleExplosion : MonoBehaviour
    {
        public Vector3 direction;
        public float speed = 2f;
        public float lifetime = 1f;
        private float elapsed = 0f;

        void Update()
        {
            elapsed += Time.deltaTime;
            transform.localPosition += direction * speed * Time.deltaTime;

            float alpha = 1f - (elapsed / lifetime);
            float flicker = Mathf.PerlinNoise(Time.time * 20f, 0f) > 0.5f ? 1f : 0.7f;
            GetComponent<MeshRenderer>().material.color = new Color(0.8f, 0.9f, 1f, alpha * 0.95f * flicker);

            if (elapsed >= lifetime)
                Destroy(gameObject);
        }
    }

    public class LightningOrbitAnimation : MonoBehaviour
    {
        public Transform core;
        private float time;

        void Update()
        {
            time += Time.deltaTime;

            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(time * 10f) * 0.2f;
                core.localScale = Vector3.one * 0.4f * pulse;
            }

            for (int i = 0; i < 6; i++)
            {
                Transform spark = transform.Find($"Spark{i}");
                if (spark != null)
                {
                    float angle = (time * 3f + i * 60f) * Mathf.Deg2Rad;
                    float radius = 0.6f;
                    spark.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius,
                        0f
                    );

                    // Flicker
                    float alpha = Mathf.PerlinNoise(Time.time * 15f + i, 0f) > 0.4f ? 0.9f : 0.6f;
                    spark.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 0.9f, alpha);
                }
            }
        }
    }

    // Wind
    public class WindParticleExplosion : MonoBehaviour
    {
        public Vector3 direction;
        public float speed = 1.5f;
        public float lifetime = 1f;
        private float elapsed = 0f;

        void Update()
        {
            elapsed += Time.deltaTime;

            // Spiral outward
            float spiralAngle = elapsed * 360f * 2f;
            Vector3 spiralOffset = Quaternion.Euler(0f, spiralAngle, 0f) * direction;
            transform.localPosition += spiralOffset * speed * Time.deltaTime;

            float alpha = 1f - (elapsed / lifetime);
            GetComponent<MeshRenderer>().material.color = new Color(0.85f, 0.95f, 0.95f, alpha * 0.7f);

            if (elapsed >= lifetime)
                Destroy(gameObject);
        }
    }

    public class WindSpiralAnimation : MonoBehaviour
    {
        private float time;

        void Update()
        {
            time += Time.deltaTime;

            for (int i = 0; i < 8; i++)
            {
                Transform particle = transform.Find($"WindParticle{i}");
                if (particle != null)
                {
                    float t = (time * 0.5f + i / 8f) % 1f;
                    float angle = t * 360f * 3f; // Three full spirals
                    float height = (t - 0.5f) * 1f;
                    float radius = 0.4f * (1f - t * 0.5f); // Spiral inward

                    particle.localPosition = new Vector3(
                        Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                        height,
                        Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                    );

                    // Fade based on position in spiral
                    float alpha = 0.6f * (1f - t);
                    Material mat = particle.GetComponent<MeshRenderer>().material;
                    Color col = mat.color;
                    col.a = alpha;
                    mat.color = col;
                }
            }
        }
    }

    #endregion
}
