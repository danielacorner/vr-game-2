using UnityEngine;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Creates and animates 3D spell representations inside hollow spheres
    /// Each spell type has unique elemental animation
    /// </summary>
    public class SpellIconAnimator : MonoBehaviour
    {
        public SpellData spellData;

        private GameObject animatedContent;
        private float animationTime = 0f;

        private void Start()
        {
            if (spellData != null)
            {
                CreateSpellAnimation();
            }
        }

        private void Update()
        {
            animationTime += Time.deltaTime;
            AnimateSpell();
        }

        public void SetSpell(SpellData spell)
        {
            spellData = spell;

            // Clear existing content
            if (animatedContent != null)
                Destroy(animatedContent);

            CreateSpellAnimation();
        }

        private void CreateSpellAnimation()
        {
            if (spellData == null) return;

            string spellName = spellData.spellName.ToLower();

            // Create appropriate animation based on spell type
            if (spellName.Contains("fire") || spellName.Contains("flame"))
            {
                CreateFireballAnimation();
            }
            else if (spellName.Contains("ice") || spellName.Contains("frost") || spellName.Contains("shard"))
            {
                CreateIceShardAnimation();
            }
            else if (spellName.Contains("light") || spellName.Contains("thunder") || spellName.Contains("bolt"))
            {
                CreateLightningAnimation();
            }
            else if (spellName.Contains("wind") || spellName.Contains("air") || spellName.Contains("blast"))
            {
                CreateWindBlastAnimation();
            }
            else
            {
                // Default: simple pulsing sphere
                CreateDefaultAnimation();
            }
        }

        /// <summary>
        /// Fireball: Swirling flame with pulsing core
        /// </summary>
        private void CreateFireballAnimation()
        {
            animatedContent = new GameObject("FireballAnimation");
            animatedContent.transform.SetParent(transform);
            animatedContent.transform.localPosition = Vector3.zero;
            animatedContent.transform.localScale = Vector3.one * 0.7f;

            // Core flame
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "FlameCore";
            core.transform.SetParent(animatedContent.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.5f;

            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            coreMat.SetFloat("_Metallic", 0f);
            coreMat.SetFloat("_Smoothness", 0.9f);
            coreMat.color = new Color(1f, 0.3f, 0f, 0.9f); // Bright orange
            core.GetComponent<MeshRenderer>().material = coreMat;

            // Outer flames (3 wisps)
            for (int i = 0; i < 3; i++)
            {
                GameObject wisp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                wisp.name = $"Wisp{i}";
                wisp.transform.SetParent(animatedContent.transform);
                wisp.transform.localScale = Vector3.one * 0.3f;

                Destroy(wisp.GetComponent<Collider>());

                Material wispMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                wispMat.SetFloat("_Metallic", 0f);
                wispMat.SetFloat("_Smoothness", 0.7f);
                wispMat.color = new Color(1f, 0.5f, 0f, 0.6f); // Orange-red
                wisp.GetComponent<MeshRenderer>().material = wispMat;
            }
        }

        /// <summary>
        /// Ice Shard: Rotating crystalline shards
        /// </summary>
        private void CreateIceShardAnimation()
        {
            animatedContent = new GameObject("IceShardAnimation");
            animatedContent.transform.SetParent(transform);
            animatedContent.transform.localPosition = Vector3.zero;
            animatedContent.transform.localScale = Vector3.one * 0.6f;

            // Create 4 elongated ice shards
            for (int i = 0; i < 4; i++)
            {
                GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = $"IceShard{i}";
                shard.transform.SetParent(animatedContent.transform);
                shard.transform.localScale = new Vector3(0.1f, 0.6f, 0.1f);

                Destroy(shard.GetComponent<Collider>());

                Material shardMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                shardMat.SetFloat("_Metallic", 0.3f);
                shardMat.SetFloat("_Smoothness", 0.95f);
                shardMat.color = new Color(0.5f, 0.8f, 1f, 0.8f); // Icy blue
                shard.GetComponent<MeshRenderer>().material = shardMat;

                // Position shards in cross pattern
                float angle = i * 90f * Mathf.Deg2Rad;
                shard.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * 0.3f,
                    0f,
                    Mathf.Sin(angle) * 0.3f
                );
            }
        }

        /// <summary>
        /// Lightning: Jagged bolt with electric arcs
        /// </summary>
        private void CreateLightningAnimation()
        {
            animatedContent = new GameObject("LightningAnimation");
            animatedContent.transform.SetParent(transform);
            animatedContent.transform.localPosition = Vector3.zero;
            animatedContent.transform.localScale = Vector3.one * 0.7f;

            // Central bolt (zigzag of cubes)
            int boltSegments = 5;
            for (int i = 0; i < boltSegments; i++)
            {
                GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = $"BoltSegment{i}";
                segment.transform.SetParent(animatedContent.transform);
                segment.transform.localScale = new Vector3(0.08f, 0.2f, 0.08f);

                Destroy(segment.GetComponent<Collider>());

                Material boltMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                boltMat.SetFloat("_Metallic", 0.2f);
                boltMat.SetFloat("_Smoothness", 0.9f);
                boltMat.color = new Color(0.7f, 0.7f, 1f, 0.9f); // Electric blue-white
                segment.GetComponent<MeshRenderer>().material = boltMat;

                // Zigzag pattern
                float yPos = (i / (float)boltSegments - 0.5f) * 0.8f;
                float xOffset = (i % 2 == 0 ? -1f : 1f) * 0.1f;
                segment.transform.localPosition = new Vector3(xOffset, yPos, 0f);
                segment.transform.localRotation = Quaternion.Euler(0f, 0f, (i % 2 == 0 ? -15f : 15f));
            }

            // Energy spheres at joints
            for (int i = 0; i < 3; i++)
            {
                GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spark.name = $"Spark{i}";
                spark.transform.SetParent(animatedContent.transform);
                spark.transform.localScale = Vector3.one * 0.15f;

                Destroy(spark.GetComponent<Collider>());

                Material sparkMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                sparkMat.SetFloat("_Metallic", 0f);
                sparkMat.SetFloat("_Smoothness", 1f);
                sparkMat.color = new Color(1f, 1f, 0.8f, 0.9f); // Bright white-yellow
                spark.GetComponent<MeshRenderer>().material = sparkMat;
            }
        }

        /// <summary>
        /// Wind Blast: Swirling spiral of air particles
        /// </summary>
        private void CreateWindBlastAnimation()
        {
            animatedContent = new GameObject("WindBlastAnimation");
            animatedContent.transform.SetParent(transform);
            animatedContent.transform.localPosition = Vector3.zero;
            animatedContent.transform.localScale = Vector3.one * 0.6f;

            // Create spiral of wind particles
            int particleCount = 8;
            for (int i = 0; i < particleCount; i++)
            {
                GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                particle.name = $"WindParticle{i}";
                particle.transform.SetParent(animatedContent.transform);
                particle.transform.localScale = Vector3.one * 0.15f;

                Destroy(particle.GetComponent<Collider>());

                Material particleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                particleMat.SetFloat("_Metallic", 0f);
                particleMat.SetFloat("_Smoothness", 0.8f);
                particleMat.color = new Color(0.8f, 0.95f, 0.95f, 0.5f); // Light cyan/white
                particle.GetComponent<MeshRenderer>().material = particleMat;
            }
        }

        private void CreateDefaultAnimation()
        {
            animatedContent = new GameObject("DefaultAnimation");
            animatedContent.transform.SetParent(transform);
            animatedContent.transform.localPosition = Vector3.zero;
            animatedContent.transform.localScale = Vector3.one * 0.6f;

            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.transform.SetParent(animatedContent.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.5f;

            Destroy(core.GetComponent<Collider>());

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = spellData.spellColor;
            core.GetComponent<MeshRenderer>().material = mat;
        }

        private void AnimateSpell()
        {
            if (animatedContent == null || spellData == null) return;

            string spellName = spellData.spellName.ToLower();

            if (spellName.Contains("fire") || spellName.Contains("flame"))
            {
                AnimateFireball();
            }
            else if (spellName.Contains("ice") || spellName.Contains("frost") || spellName.Contains("shard"))
            {
                AnimateIceShard();
            }
            else if (spellName.Contains("light") || spellName.Contains("thunder") || spellName.Contains("bolt"))
            {
                AnimateLightning();
            }
            else if (spellName.Contains("wind") || spellName.Contains("air") || spellName.Contains("blast"))
            {
                AnimateWindBlast();
            }
            else
            {
                AnimateDefault();
            }
        }

        private void AnimateFireball()
        {
            // Pulse core
            Transform core = animatedContent.transform.Find("FlameCore");
            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(animationTime * 4f) * 0.2f;
                core.localScale = Vector3.one * 0.5f * pulse;
            }

            // Rotate and orbit wisps
            for (int i = 0; i < 3; i++)
            {
                Transform wisp = animatedContent.transform.Find($"Wisp{i}");
                if (wisp != null)
                {
                    float angle = (animationTime * 2f + i * 120f) * Mathf.Deg2Rad;
                    float radius = 0.4f + Mathf.Sin(animationTime * 3f + i) * 0.1f;
                    wisp.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(animationTime * 2f + i) * 0.2f,
                        Mathf.Sin(angle) * radius
                    );
                }
            }
        }

        private void AnimateIceShard()
        {
            // Rotate all shards around center
            animatedContent.transform.localRotation = Quaternion.Euler(
                Mathf.Sin(animationTime * 0.5f) * 20f,
                animationTime * 60f,
                Mathf.Cos(animationTime * 0.5f) * 20f
            );

            // Individual shard rotation
            for (int i = 0; i < 4; i++)
            {
                Transform shard = animatedContent.transform.Find($"IceShard{i}");
                if (shard != null)
                {
                    shard.localRotation = Quaternion.Euler(
                        0f,
                        animationTime * 30f * (i % 2 == 0 ? 1f : -1f),
                        0f
                    );
                }
            }
        }

        private void AnimateLightning()
        {
            // Flicker bolt segments
            int boltSegments = 5;
            for (int i = 0; i < boltSegments; i++)
            {
                Transform segment = animatedContent.transform.Find($"BoltSegment{i}");
                if (segment != null)
                {
                    // Random flicker effect
                    float flicker = Mathf.PerlinNoise(animationTime * 10f + i, i) > 0.3f ? 1f : 0.7f;
                    segment.localScale = new Vector3(0.08f * flicker, 0.2f, 0.08f * flicker);

                    // Jitter position slightly
                    float jitterX = (Mathf.PerlinNoise(animationTime * 15f, i) - 0.5f) * 0.05f;
                    float baseX = (i % 2 == 0 ? -1f : 1f) * 0.1f;
                    float yPos = (i / (float)boltSegments - 0.5f) * 0.8f;
                    segment.localPosition = new Vector3(baseX + jitterX, yPos, 0f);
                }
            }

            // Animate sparks
            for (int i = 0; i < 3; i++)
            {
                Transform spark = animatedContent.transform.Find($"Spark{i}");
                if (spark != null)
                {
                    float yPos = Mathf.Sin(animationTime * 3f + i * 2f) * 0.3f;
                    spark.localPosition = new Vector3(0f, yPos, 0f);

                    float pulse = 1f + Mathf.Sin(animationTime * 8f + i) * 0.3f;
                    spark.localScale = Vector3.one * 0.15f * pulse;
                }
            }
        }

        private void AnimateWindBlast()
        {
            // Spiral particles upward
            int particleCount = 8;
            for (int i = 0; i < particleCount; i++)
            {
                Transform particle = animatedContent.transform.Find($"WindParticle{i}");
                if (particle != null)
                {
                    float t = (animationTime * 0.5f + i / (float)particleCount) % 1f;
                    float angle = t * 360f * 2f; // Two full spirals
                    float height = (t - 0.5f) * 0.8f;
                    float radius = 0.3f * (1f - t); // Spiral inward

                    particle.localPosition = new Vector3(
                        Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                        height,
                        Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                    );

                    // Fade out as they reach top
                    float alpha = 0.5f * (1f - t);
                    Material mat = particle.GetComponent<MeshRenderer>().material;
                    Color col = mat.color;
                    col.a = alpha;
                    mat.color = col;
                }
            }
        }

        private void AnimateDefault()
        {
            // Simple pulse
            float pulse = 1f + Mathf.Sin(animationTime * 3f) * 0.15f;
            animatedContent.transform.localScale = Vector3.one * 0.6f * pulse;
        }
    }
}
