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
            if (spellName.Contains("fire") || spellName.Contains("flame") || spellName.Contains("meteor"))
            {
                CreateFireballAnimation();
            }
            else if (spellName.Contains("ice") || spellName.Contains("frost") || spellName.Contains("shard") || spellName.Contains("boulder"))
            {
                CreateIceShardAnimation();
            }
            else if (spellName.Contains("light") || spellName.Contains("thunder") || spellName.Contains("bolt") || spellName.Contains("orb"))
            {
                CreateLightningAnimation();
            }
            else if (spellName.Contains("wind") || spellName.Contains("air") || spellName.Contains("blast") || spellName.Contains("cyclone"))
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
        /// Fireball: Swirling flame with pulsing core (enhanced with HDR emission)
        /// </summary>
        private void CreateFireballAnimation()
        {
            animatedContent = new GameObject("FireballAnimation");
            animatedContent.transform.SetParent(transform);
            animatedContent.transform.localPosition = Vector3.zero;
            animatedContent.transform.localScale = Vector3.one * 0.7f;

            // Bright white-hot core
            GameObject innerCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            innerCore.name = "InnerCore";
            innerCore.transform.SetParent(animatedContent.transform);
            innerCore.transform.localPosition = Vector3.zero;
            innerCore.transform.localScale = Vector3.one * 0.25f;
            Destroy(innerCore.GetComponent<Collider>());

            Material innerMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            innerMat.EnableKeyword("_EMISSION");
            innerMat.SetFloat("_Metallic", 0f);
            innerMat.SetFloat("_Smoothness", 1f);
            innerMat.SetColor("_BaseColor", Color.white);
            innerMat.SetColor("_EmissionColor", Color.white * 8f); // Bright HDR white
            innerCore.GetComponent<MeshRenderer>().material = innerMat;

            // Orange flame core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "FlameCore";
            core.transform.SetParent(animatedContent.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.5f;
            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetFloat("_Metallic", 0f);
            coreMat.SetFloat("_Smoothness", 0.9f);
            coreMat.SetColor("_BaseColor", new Color(1f, 0.4f, 0f, 0.8f));
            coreMat.SetColor("_EmissionColor", new Color(1f, 0.3f, 0f) * 3f); // HDR orange
            core.GetComponent<MeshRenderer>().material = coreMat;

            // Outer flames (6 wisps for more drama)
            for (int i = 0; i < 6; i++)
            {
                GameObject wisp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                wisp.name = $"Wisp{i}";
                wisp.transform.SetParent(animatedContent.transform);
                wisp.transform.localScale = Vector3.one * 0.25f;
                Destroy(wisp.GetComponent<Collider>());

                Material wispMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                wispMat.EnableKeyword("_EMISSION");
                wispMat.SetFloat("_Metallic", 0f);
                wispMat.SetFloat("_Smoothness", 0.7f);
                wispMat.SetColor("_BaseColor", new Color(1f, 0.5f, 0f, 0.6f));
                wispMat.SetColor("_EmissionColor", new Color(1f, 0.4f, 0f) * 2f); // Glowing orange
                wisp.GetComponent<MeshRenderer>().material = wispMat;
            }
        }

        /// <summary>
        /// Ice Shard: Rotating crystalline shards (enhanced with HDR emission)
        /// </summary>
        private void CreateIceShardAnimation()
        {
            animatedContent = new GameObject("IceShardAnimation");
            animatedContent.transform.SetParent(transform);
            animatedContent.transform.localPosition = Vector3.zero;
            animatedContent.transform.localScale = Vector3.one * 0.6f;

            // Bright cyan core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "IceCore";
            core.transform.SetParent(animatedContent.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.3f;
            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetFloat("_Metallic", 0f);
            coreMat.SetFloat("_Smoothness", 1f);
            coreMat.SetColor("_BaseColor", new Color(0.5f, 0.9f, 1f, 0.9f));
            coreMat.SetColor("_EmissionColor", new Color(0.5f, 0.9f, 1f) * 5f); // Bright cyan glow
            core.GetComponent<MeshRenderer>().material = coreMat;

            // Create 8 elongated ice shards (double for tier-2)
            for (int i = 0; i < 8; i++)
            {
                GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = $"IceShard{i}";
                shard.transform.SetParent(animatedContent.transform);
                shard.transform.localScale = new Vector3(0.08f, 0.5f, 0.08f);
                Destroy(shard.GetComponent<Collider>());

                Material shardMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                shardMat.EnableKeyword("_EMISSION");
                shardMat.SetFloat("_Metallic", 0.2f);
                shardMat.SetFloat("_Smoothness", 0.95f);
                shardMat.SetColor("_BaseColor", new Color(0.6f, 0.85f, 1f, 0.8f));
                shardMat.SetColor("_EmissionColor", new Color(0.5f, 0.8f, 1f) * 2f); // Icy glow
                shard.GetComponent<MeshRenderer>().material = shardMat;

                // Position shards in circular pattern
                float angle = i * 45f * Mathf.Deg2Rad;
                shard.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * 0.35f,
                    0f,
                    Mathf.Sin(angle) * 0.35f
                );
            }
        }

        /// <summary>
        /// Lightning: Jagged bolt with electric arcs (enhanced with HDR emission)
        /// </summary>
        private void CreateLightningAnimation()
        {
            animatedContent = new GameObject("LightningAnimation");
            animatedContent.transform.SetParent(transform);
            animatedContent.transform.localPosition = Vector3.zero;
            animatedContent.transform.localScale = Vector3.one * 0.7f;

            // Bright white energy core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "EnergyCore";
            core.transform.SetParent(animatedContent.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.25f;
            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetFloat("_Metallic", 0f);
            coreMat.SetFloat("_Smoothness", 1f);
            coreMat.SetColor("_BaseColor", Color.white);
            coreMat.SetColor("_EmissionColor", Color.white * 10f); // Intense white glow
            core.GetComponent<MeshRenderer>().material = coreMat;

            // Central bolt (zigzag of cubes) - more segments
            int boltSegments = 7;
            for (int i = 0; i < boltSegments; i++)
            {
                GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = $"BoltSegment{i}";
                segment.transform.SetParent(animatedContent.transform);
                segment.transform.localScale = new Vector3(0.06f, 0.18f, 0.06f);
                Destroy(segment.GetComponent<Collider>());

                Material boltMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                boltMat.EnableKeyword("_EMISSION");
                boltMat.SetFloat("_Metallic", 0.1f);
                boltMat.SetFloat("_Smoothness", 0.9f);
                boltMat.SetColor("_BaseColor", new Color(0.8f, 0.85f, 1f, 0.9f));
                boltMat.SetColor("_EmissionColor", new Color(0.7f, 0.8f, 1f) * 4f); // Electric blue glow
                segment.GetComponent<MeshRenderer>().material = boltMat;

                // Zigzag pattern
                float yPos = (i / (float)boltSegments - 0.5f) * 0.9f;
                float xOffset = (i % 2 == 0 ? -1f : 1f) * 0.12f;
                segment.transform.localPosition = new Vector3(xOffset, yPos, 0f);
                segment.transform.localRotation = Quaternion.Euler(0f, 0f, (i % 2 == 0 ? -18f : 18f));
            }

            // Energy spheres at joints (more for tier-2)
            for (int i = 0; i < 6; i++)
            {
                GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spark.name = $"Spark{i}";
                spark.transform.SetParent(animatedContent.transform);
                spark.transform.localScale = Vector3.one * 0.12f;
                Destroy(spark.GetComponent<Collider>());

                Material sparkMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                sparkMat.EnableKeyword("_EMISSION");
                sparkMat.SetFloat("_Metallic", 0f);
                sparkMat.SetFloat("_Smoothness", 1f);
                sparkMat.SetColor("_BaseColor", new Color(1f, 1f, 0.9f, 0.9f));
                sparkMat.SetColor("_EmissionColor", new Color(1f, 1f, 0.9f) * 6f); // Bright white-yellow glow
                spark.GetComponent<MeshRenderer>().material = sparkMat;
            }
        }

        /// <summary>
        /// Wind Blast: Swirling spiral of air particles (enhanced with HDR emission)
        /// </summary>
        private void CreateWindBlastAnimation()
        {
            animatedContent = new GameObject("WindBlastAnimation");
            animatedContent.transform.SetParent(transform);
            animatedContent.transform.localPosition = Vector3.zero;
            animatedContent.transform.localScale = Vector3.one * 0.6f;

            // Bright white vortex core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "VortexCore";
            core.transform.SetParent(animatedContent.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.2f;
            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetFloat("_Metallic", 0f);
            coreMat.SetFloat("_Smoothness", 1f);
            coreMat.SetColor("_BaseColor", new Color(0.95f, 0.98f, 1f));
            coreMat.SetColor("_EmissionColor", new Color(0.95f, 0.98f, 1f) * 6f); // Bright white-blue glow
            core.GetComponent<MeshRenderer>().material = coreMat;

            // Create spiral of wind particles (more for tier-2)
            int particleCount = 14;
            for (int i = 0; i < particleCount; i++)
            {
                GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                particle.name = $"WindParticle{i}";
                particle.transform.SetParent(animatedContent.transform);
                particle.transform.localScale = Vector3.one * 0.12f;
                Destroy(particle.GetComponent<Collider>());

                Material particleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                particleMat.EnableKeyword("_EMISSION");
                particleMat.SetFloat("_Metallic", 0f);
                particleMat.SetFloat("_Smoothness", 0.9f);
                particleMat.SetColor("_BaseColor", new Color(0.85f, 0.94f, 1f, 0.6f));
                particleMat.SetColor("_EmissionColor", new Color(0.85f, 0.94f, 1f) * 2.5f); // Glowing air
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

            if (spellName.Contains("fire") || spellName.Contains("flame") || spellName.Contains("meteor"))
            {
                AnimateFireball();
            }
            else if (spellName.Contains("ice") || spellName.Contains("frost") || spellName.Contains("shard") || spellName.Contains("boulder"))
            {
                AnimateIceShard();
            }
            else if (spellName.Contains("light") || spellName.Contains("thunder") || spellName.Contains("bolt") || spellName.Contains("orb"))
            {
                AnimateLightning();
            }
            else if (spellName.Contains("wind") || spellName.Contains("air") || spellName.Contains("blast") || spellName.Contains("cyclone"))
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
            // Pulse inner core (white-hot)
            Transform innerCore = animatedContent.transform.Find("InnerCore");
            if (innerCore != null)
            {
                float pulse = 1f + Mathf.Sin(animationTime * 6f) * 0.3f;
                innerCore.localScale = Vector3.one * 0.25f * pulse;
            }

            // Pulse flame core
            Transform core = animatedContent.transform.Find("FlameCore");
            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(animationTime * 4f) * 0.2f;
                core.localScale = Vector3.one * 0.5f * pulse;
            }

            // Rotate and orbit wisps (6 total)
            for (int i = 0; i < 6; i++)
            {
                Transform wisp = animatedContent.transform.Find($"Wisp{i}");
                if (wisp != null)
                {
                    float angle = (animationTime * 2.5f + i * 60f) * Mathf.Deg2Rad;
                    float radius = 0.45f + Mathf.Sin(animationTime * 3f + i) * 0.12f;
                    wisp.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(animationTime * 2.5f + i) * 0.25f,
                        Mathf.Sin(angle) * radius
                    );
                }
            }
        }

        private void AnimateIceShard()
        {
            // Pulse ice core
            Transform core = animatedContent.transform.Find("IceCore");
            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(animationTime * 5f) * 0.2f;
                core.localScale = Vector3.one * 0.3f * pulse;
            }

            // Rotate all shards around center
            animatedContent.transform.localRotation = Quaternion.Euler(
                Mathf.Sin(animationTime * 0.5f) * 20f,
                animationTime * 70f,
                Mathf.Cos(animationTime * 0.5f) * 20f
            );

            // Individual shard rotation (8 shards)
            for (int i = 0; i < 8; i++)
            {
                Transform shard = animatedContent.transform.Find($"IceShard{i}");
                if (shard != null)
                {
                    shard.localRotation = Quaternion.Euler(
                        0f,
                        animationTime * 35f * (i % 2 == 0 ? 1f : -1f),
                        0f
                    );
                }
            }
        }

        private void AnimateLightning()
        {
            // Pulse energy core
            Transform core = animatedContent.transform.Find("EnergyCore");
            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(animationTime * 7f) * 0.35f;
                core.localScale = Vector3.one * 0.25f * pulse;
            }

            // Flicker bolt segments (7 segments)
            int boltSegments = 7;
            for (int i = 0; i < boltSegments; i++)
            {
                Transform segment = animatedContent.transform.Find($"BoltSegment{i}");
                if (segment != null)
                {
                    // Random flicker effect
                    float flicker = Mathf.PerlinNoise(animationTime * 12f + i, i) > 0.3f ? 1f : 0.7f;
                    segment.localScale = new Vector3(0.06f * flicker, 0.18f, 0.06f * flicker);

                    // Jitter position slightly
                    float jitterX = (Mathf.PerlinNoise(animationTime * 18f, i) - 0.5f) * 0.06f;
                    float baseX = (i % 2 == 0 ? -1f : 1f) * 0.12f;
                    float yPos = (i / (float)boltSegments - 0.5f) * 0.9f;
                    segment.localPosition = new Vector3(baseX + jitterX, yPos, 0f);
                }
            }

            // Animate sparks (6 sparks)
            for (int i = 0; i < 6; i++)
            {
                Transform spark = animatedContent.transform.Find($"Spark{i}");
                if (spark != null)
                {
                    float angle = (animationTime * 3f + i * 60f) * Mathf.Deg2Rad;
                    float radius = 0.25f + Mathf.Sin(animationTime * 4f + i) * 0.1f;
                    spark.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(animationTime * 3.5f + i * 1.5f) * 0.35f,
                        Mathf.Sin(angle) * radius
                    );

                    float pulse = 1f + Mathf.Sin(animationTime * 9f + i) * 0.4f;
                    spark.localScale = Vector3.one * 0.12f * pulse;
                }
            }
        }

        private void AnimateWindBlast()
        {
            // Pulse vortex core
            Transform core = animatedContent.transform.Find("VortexCore");
            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(animationTime * 6f) * 0.25f;
                core.localScale = Vector3.one * 0.2f * pulse;
            }

            // Spiral particles upward (14 particles)
            int particleCount = 14;
            for (int i = 0; i < particleCount; i++)
            {
                Transform particle = animatedContent.transform.Find($"WindParticle{i}");
                if (particle != null)
                {
                    float t = (animationTime * 0.6f + i / (float)particleCount) % 1f;
                    float angle = t * 360f * 2.5f; // More spirals
                    float height = (t - 0.5f) * 0.9f;
                    float radius = 0.35f * (1f - t); // Spiral inward

                    particle.localPosition = new Vector3(
                        Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                        height,
                        Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                    );

                    // Fade out as they reach top
                    float alpha = 0.6f * (1f - t);
                    Material mat = particle.GetComponent<MeshRenderer>().material;
                    Color col = mat.color;
                    col.a = alpha;
                    mat.color = col;

                    // Scale down as they rise
                    float scale = 0.12f * (1f - t * 0.5f);
                    particle.localScale = Vector3.one * scale;
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
