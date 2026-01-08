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
            int tier = spellData.tier;

            // Create appropriate animation based on spell type and tier
            if (spellName.Contains("fire") || spellName.Contains("flame") || spellName.Contains("meteor"))
            {
                if (tier == 2 || spellName.Contains("meteor"))
                    CreateMeteorAnimation();
                else
                    CreateFireballAnimation();
            }
            else if (spellName.Contains("ice") || spellName.Contains("frost") || spellName.Contains("shard") || spellName.Contains("boulder"))
            {
                if (tier == 2 || spellName.Contains("boulder"))
                    CreateFrostBoulderAnimation();
                else
                    CreateIceShardAnimation();
            }
            else if (spellName.Contains("light") || spellName.Contains("thunder") || spellName.Contains("bolt") || spellName.Contains("orb"))
            {
                if (tier == 2 || spellName.Contains("orb"))
                    CreateThunderOrbAnimation();
                else
                    CreateLightningAnimation();
            }
            else if (spellName.Contains("wind") || spellName.Contains("air") || spellName.Contains("blast") || spellName.Contains("cyclone"))
            {
                if (tier == 2 || spellName.Contains("cyclone"))
                    CreateCycloneAnimation();
                else
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

        /// <summary>
        /// Meteor (Tier 2): Molten lava core with orbiting fiery chunks
        /// </summary>
        private void CreateMeteorAnimation()
        {
            animatedContent = new GameObject("MeteorAnimation");
            animatedContent.transform.SetParent(transform);
            animatedContent.transform.localPosition = Vector3.zero;
            animatedContent.transform.localScale = Vector3.one * 0.75f;

            // Ultra-bright white core (molten center)
            GameObject whiteCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            whiteCore.name = "MoltenCore";
            whiteCore.transform.SetParent(animatedContent.transform);
            whiteCore.transform.localPosition = Vector3.zero;
            whiteCore.transform.localScale = Vector3.one * 0.15f;
            Destroy(whiteCore.GetComponent<Collider>());

            Material whiteMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            whiteMat.EnableKeyword("_EMISSION");
            whiteMat.SetColor("_BaseColor", Color.white);
            whiteMat.SetColor("_EmissionColor", Color.white * 15f);
            whiteMat.SetFloat("_Smoothness", 1f);
            whiteCore.GetComponent<MeshRenderer>().material = whiteMat;

            // Lava layer (orange-red)
            GameObject lavaLayer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lavaLayer.name = "LavaLayer";
            lavaLayer.transform.SetParent(animatedContent.transform);
            lavaLayer.transform.localPosition = Vector3.zero;
            lavaLayer.transform.localScale = Vector3.one * 0.35f;
            Destroy(lavaLayer.GetComponent<Collider>());

            Material lavaMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            lavaMat.EnableKeyword("_EMISSION");
            lavaMat.SetColor("_BaseColor", new Color(1f, 0.3f, 0f, 0.85f));
            lavaMat.SetColor("_EmissionColor", new Color(1f, 0.25f, 0f) * 6f);
            lavaMat.SetFloat("_Surface", 1);
            lavaMat.renderQueue = 3000;
            lavaLayer.GetComponent<MeshRenderer>().material = lavaMat;

            // Orbiting lava chunks (10 irregular pieces)
            for (int i = 0; i < 10; i++)
            {
                GameObject chunk = GameObject.CreatePrimitive(i % 3 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere);
                chunk.name = $"LavaChunk{i}";
                chunk.transform.SetParent(animatedContent.transform);
                chunk.transform.localScale = Vector3.one * (0.12f + (i % 3) * 0.05f);
                Destroy(chunk.GetComponent<Collider>());

                Material chunkMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                chunkMat.EnableKeyword("_EMISSION");
                chunkMat.SetColor("_BaseColor", new Color(0.3f, 0.15f, 0.05f));
                chunkMat.SetColor("_EmissionColor", new Color(2f, 0.8f, 0.1f));
                chunkMat.SetFloat("_Smoothness", 0.3f);
                chunk.GetComponent<MeshRenderer>().material = chunkMat;
            }

            // Smoke wisps (8 dark particles)
            for (int i = 0; i < 8; i++)
            {
                GameObject smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                smoke.name = $"Smoke{i}";
                smoke.transform.SetParent(animatedContent.transform);
                smoke.transform.localScale = Vector3.one * 0.15f;
                Destroy(smoke.GetComponent<Collider>());

                Material smokeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                smokeMat.SetColor("_BaseColor", new Color(0.15f, 0.1f, 0.08f, 0.4f));
                smokeMat.SetFloat("_Surface", 1);
                smokeMat.renderQueue = 3000;
                smoke.GetComponent<MeshRenderer>().material = smokeMat;
            }
        }

        /// <summary>
        /// FrostBoulder (Tier 2): Massive crystalline ice formation
        /// </summary>
        private void CreateFrostBoulderAnimation()
        {
            animatedContent = new GameObject("FrostBoulderAnimation");
            animatedContent.transform.SetParent(transform);
            animatedContent.transform.localPosition = Vector3.zero;
            animatedContent.transform.localScale = Vector3.one * 0.7f;

            // Ultra-bright cyan core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "FrozenCore";
            core.transform.SetParent(animatedContent.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.2f;
            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetColor("_BaseColor", new Color(0.4f, 0.9f, 1f));
            coreMat.SetColor("_EmissionColor", new Color(0.4f, 0.9f, 1f) * 10f);
            coreMat.SetFloat("_Smoothness", 1f);
            core.GetComponent<MeshRenderer>().material = coreMat;

            // Large ice crystals (6 major spikes)
            for (int i = 0; i < 6; i++)
            {
                GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spike.name = $"IceSpike{i}";
                spike.transform.SetParent(animatedContent.transform);
                spike.transform.localScale = new Vector3(0.12f, 0.7f, 0.12f);
                Destroy(spike.GetComponent<Collider>());

                Material spikeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                spikeMat.EnableKeyword("_EMISSION");
                spikeMat.SetColor("_BaseColor", new Color(0.6f, 0.85f, 1f, 0.85f));
                spikeMat.SetColor("_EmissionColor", new Color(0.5f, 0.8f, 1f) * 3f);
                spikeMat.SetFloat("_Smoothness", 0.95f);
                spikeMat.SetFloat("_Surface", 1);
                spikeMat.renderQueue = 3000;
                spike.GetComponent<MeshRenderer>().material = spikeMat;

                // Position in hexagonal pattern
                float angle = i * 60f * Mathf.Deg2Rad;
                spike.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * 0.25f,
                    0f,
                    Mathf.Sin(angle) * 0.25f
                );
                spike.transform.localRotation = Quaternion.Euler(
                    Random.Range(-15f, 15f),
                    angle * Mathf.Rad2Deg,
                    Random.Range(-15f, 15f)
                );
            }

            // Small ice fragments (12 orbiting pieces)
            for (int i = 0; i < 12; i++)
            {
                GameObject fragment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fragment.name = $"IceFragment{i}";
                fragment.transform.SetParent(animatedContent.transform);
                fragment.transform.localScale = new Vector3(0.06f, 0.1f, 0.06f);
                Destroy(fragment.GetComponent<Collider>());

                Material fragMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                fragMat.EnableKeyword("_EMISSION");
                fragMat.SetColor("_BaseColor", new Color(0.7f, 0.9f, 1f, 0.7f));
                fragMat.SetColor("_EmissionColor", new Color(0.6f, 0.85f, 1f) * 2f);
                fragMat.SetFloat("_Smoothness", 0.95f);
                fragMat.SetFloat("_Surface", 1);
                fragMat.renderQueue = 3000;
                fragment.GetComponent<MeshRenderer>().material = fragMat;
            }
        }

        /// <summary>
        /// ThunderOrb (Tier 2): Electric sphere with multiple arc rings
        /// </summary>
        private void CreateThunderOrbAnimation()
        {
            animatedContent = new GameObject("ThunderOrbAnimation");
            animatedContent.transform.SetParent(transform);
            animatedContent.transform.localPosition = Vector3.zero;
            animatedContent.transform.localScale = Vector3.one * 0.75f;

            // Ultra-bright white core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "PlasmaCore";
            core.transform.SetParent(animatedContent.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.2f;
            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetColor("_BaseColor", Color.white);
            coreMat.SetColor("_EmissionColor", Color.white * 18f);
            coreMat.SetFloat("_Smoothness", 1f);
            core.GetComponent<MeshRenderer>().material = coreMat;

            // Electric arc rings (3 rings at different angles)
            for (int ring = 0; ring < 3; ring++)
            {
                int segmentsPerRing = 8;
                for (int i = 0; i < segmentsPerRing; i++)
                {
                    GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    segment.name = $"ArcRing{ring}_Seg{i}";
                    segment.transform.SetParent(animatedContent.transform);
                    segment.transform.localScale = new Vector3(0.04f, 0.15f, 0.04f);
                    Destroy(segment.GetComponent<Collider>());

                    Material segMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    segMat.EnableKeyword("_EMISSION");
                    segMat.SetColor("_BaseColor", new Color(0.8f, 0.85f, 1f));
                    segMat.SetColor("_EmissionColor", new Color(0.7f, 0.8f, 1f) * 5f);
                    segMat.SetFloat("_Smoothness", 0.9f);
                    segment.GetComponent<MeshRenderer>().material = segMat;

                    // Position segments in circle
                    float angle = i * (360f / segmentsPerRing) * Mathf.Deg2Rad;
                    float radius = 0.35f;
                    segment.transform.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius * (ring == 1 ? 0.5f : 0f),
                        Mathf.Sin(angle) * radius * (ring == 2 ? 0.5f : 1f)
                    );
                }
            }

            // Crackling energy spheres (10 random sparks)
            for (int i = 0; i < 10; i++)
            {
                GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spark.name = $"EnergySpark{i}";
                spark.transform.SetParent(animatedContent.transform);
                spark.transform.localScale = Vector3.one * 0.1f;
                Destroy(spark.GetComponent<Collider>());

                Material sparkMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                sparkMat.EnableKeyword("_EMISSION");
                sparkMat.SetColor("_BaseColor", new Color(1f, 1f, 0.9f));
                sparkMat.SetColor("_EmissionColor", new Color(1f, 1f, 0.9f) * 8f);
                sparkMat.SetFloat("_Smoothness", 1f);
                spark.GetComponent<MeshRenderer>().material = sparkMat;
            }
        }

        /// <summary>
        /// Cyclone (Tier 2): Multi-layered vortex with tornado structure
        /// </summary>
        private void CreateCycloneAnimation()
        {
            animatedContent = new GameObject("CycloneAnimation");
            animatedContent.transform.SetParent(transform);
            animatedContent.transform.localPosition = Vector3.zero;
            animatedContent.transform.localScale = Vector3.one * 0.7f;

            // Bright vortex core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "VortexEye";
            core.transform.SetParent(animatedContent.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * 0.15f;
            Destroy(core.GetComponent<Collider>());

            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetColor("_BaseColor", new Color(0.95f, 0.98f, 1f));
            coreMat.SetColor("_EmissionColor", new Color(0.95f, 0.98f, 1f) * 8f);
            coreMat.SetFloat("_Smoothness", 1f);
            core.GetComponent<MeshRenderer>().material = coreMat;

            // Tornado structure - 3 spiral layers
            for (int layer = 0; layer < 3; layer++)
            {
                int particlesPerLayer = 10;
                float layerRadius = 0.2f + layer * 0.1f;

                for (int i = 0; i < particlesPerLayer; i++)
                {
                    GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    particle.name = $"WindLayer{layer}_P{i}";
                    particle.transform.SetParent(animatedContent.transform);
                    particle.transform.localScale = Vector3.one * (0.08f + layer * 0.02f);
                    Destroy(particle.GetComponent<Collider>());

                    Material partMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    partMat.EnableKeyword("_EMISSION");
                    float alpha = 0.5f - layer * 0.1f;
                    partMat.SetColor("_BaseColor", new Color(0.88f, 0.94f, 1f, alpha));
                    partMat.SetColor("_EmissionColor", new Color(0.88f, 0.94f, 1f) * (3f - layer * 0.5f));
                    partMat.SetFloat("_Smoothness", 0.9f);
                    partMat.SetFloat("_Surface", 1);
                    partMat.renderQueue = 3000;
                    particle.GetComponent<MeshRenderer>().material = partMat;
                }
            }

            // Debris particles (8 flying objects)
            for (int i = 0; i < 8; i++)
            {
                GameObject debris = GameObject.CreatePrimitive(i % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere);
                debris.name = $"Debris{i}";
                debris.transform.SetParent(animatedContent.transform);
                debris.transform.localScale = Vector3.one * 0.06f;
                Destroy(debris.GetComponent<Collider>());

                Material debrisMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                debrisMat.SetColor("_BaseColor", new Color(0.7f, 0.75f, 0.8f, 0.6f));
                debrisMat.SetFloat("_Smoothness", 0.5f);
                debrisMat.SetFloat("_Surface", 1);
                debrisMat.renderQueue = 3000;
                debris.GetComponent<MeshRenderer>().material = debrisMat;
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
            int tier = spellData.tier;

            if (spellName.Contains("fire") || spellName.Contains("flame") || spellName.Contains("meteor"))
            {
                if (tier == 2 || spellName.Contains("meteor"))
                    AnimateMeteor();
                else
                    AnimateFireball();
            }
            else if (spellName.Contains("ice") || spellName.Contains("frost") || spellName.Contains("shard") || spellName.Contains("boulder"))
            {
                if (tier == 2 || spellName.Contains("boulder"))
                    AnimateFrostBoulder();
                else
                    AnimateIceShard();
            }
            else if (spellName.Contains("light") || spellName.Contains("thunder") || spellName.Contains("bolt") || spellName.Contains("orb"))
            {
                if (tier == 2 || spellName.Contains("orb"))
                    AnimateThunderOrb();
                else
                    AnimateLightning();
            }
            else if (spellName.Contains("wind") || spellName.Contains("air") || spellName.Contains("blast") || spellName.Contains("cyclone"))
            {
                if (tier == 2 || spellName.Contains("cyclone"))
                    AnimateCyclone();
                else
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

        private void AnimateMeteor()
        {
            // Pulse molten core intensely
            Transform core = animatedContent.transform.Find("MoltenCore");
            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(animationTime * 8f) * 0.4f;
                core.localScale = Vector3.one * 0.15f * pulse;
            }

            // Pulse lava layer
            Transform lava = animatedContent.transform.Find("LavaLayer");
            if (lava != null)
            {
                float pulse = 1f + Mathf.Sin(animationTime * 5f) * 0.25f;
                lava.localScale = Vector3.one * 0.35f * pulse;
            }

            // Orbit lava chunks chaotically
            for (int i = 0; i < 10; i++)
            {
                Transform chunk = animatedContent.transform.Find($"LavaChunk{i}");
                if (chunk != null)
                {
                    float angle = (animationTime * 3f + i * 36f) * Mathf.Deg2Rad;
                    float radius = 0.4f + Mathf.Sin(animationTime * 2f + i) * 0.1f;
                    float height = Mathf.Sin(animationTime * 2.5f + i * 0.5f) * 0.2f;

                    chunk.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        height,
                        Mathf.Sin(angle) * radius
                    );

                    // Tumble chunks
                    chunk.localRotation = Quaternion.Euler(
                        animationTime * 40f + i * 30f,
                        animationTime * 60f + i * 45f,
                        animationTime * 50f + i * 20f
                    );
                }
            }

            // Smoke drifts upward
            for (int i = 0; i < 8; i++)
            {
                Transform smoke = animatedContent.transform.Find($"Smoke{i}");
                if (smoke != null)
                {
                    float t = (animationTime * 0.3f + i / 8f) % 1f;
                    float angle = (t * 180f + i * 45f) * Mathf.Deg2Rad;
                    float radius = 0.35f * (1f - t * 0.5f);
                    float height = (t - 0.5f) * 0.6f + 0.3f;

                    smoke.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        height,
                        Mathf.Sin(angle) * radius
                    );
                }
            }
        }

        private void AnimateFrostBoulder()
        {
            // Pulse frozen core
            Transform core = animatedContent.transform.Find("FrozenCore");
            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(animationTime * 6f) * 0.3f;
                core.localScale = Vector3.one * 0.2f * pulse;
            }

            // Rotate entire formation slowly
            animatedContent.transform.localRotation = Quaternion.Euler(
                Mathf.Sin(animationTime * 0.3f) * 15f,
                animationTime * 40f,
                Mathf.Cos(animationTime * 0.3f) * 15f
            );

            // Individual spike rotation
            for (int i = 0; i < 6; i++)
            {
                Transform spike = animatedContent.transform.Find($"IceSpike{i}");
                if (spike != null)
                {
                    float angle = i * 60f * Mathf.Deg2Rad;
                    spike.transform.localPosition = new Vector3(
                        Mathf.Cos(angle) * 0.25f,
                        Mathf.Sin(animationTime * 2f + i) * 0.05f,
                        Mathf.Sin(angle) * 0.25f
                    );
                }
            }

            // Orbit ice fragments
            for (int i = 0; i < 12; i++)
            {
                Transform fragment = animatedContent.transform.Find($"IceFragment{i}");
                if (fragment != null)
                {
                    float angle = (animationTime * 2.5f + i * 30f) * Mathf.Deg2Rad;
                    float radius = 0.45f + Mathf.Sin(animationTime * 3f + i) * 0.08f;

                    fragment.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(animationTime * 3f + i * 0.5f) * 0.15f,
                        Mathf.Sin(angle) * radius
                    );

                    fragment.localRotation = Quaternion.Euler(
                        animationTime * 50f + i * 20f,
                        animationTime * 40f + i * 30f,
                        0f
                    );
                }
            }
        }

        private void AnimateThunderOrb()
        {
            // Pulse plasma core violently
            Transform core = animatedContent.transform.Find("PlasmaCore");
            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(animationTime * 10f) * 0.5f;
                core.localScale = Vector3.one * 0.2f * pulse;
            }

            // Rotate arc rings at different speeds
            for (int ring = 0; ring < 3; ring++)
            {
                float ringSpeed = 80f + ring * 40f;
                for (int i = 0; i < 8; i++)
                {
                    Transform segment = animatedContent.transform.Find($"ArcRing{ring}_Seg{i}");
                    if (segment != null)
                    {
                        float baseAngle = i * (360f / 8f);
                        float angle = (baseAngle + animationTime * ringSpeed * (ring % 2 == 0 ? 1f : -1f)) * Mathf.Deg2Rad;
                        float radius = 0.35f;

                        Vector3 pos = new Vector3(
                            Mathf.Cos(angle) * radius,
                            Mathf.Sin(angle) * radius * (ring == 1 ? 0.5f : 0f),
                            Mathf.Sin(angle) * radius * (ring == 2 ? 0.5f : 1f)
                        );
                        segment.localPosition = pos;

                        // Flicker effect
                        float flicker = Mathf.PerlinNoise(animationTime * 15f + i, ring) > 0.4f ? 1f : 0.8f;
                        segment.localScale = new Vector3(0.04f * flicker, 0.15f, 0.04f * flicker);
                    }
                }
            }

            // Crackling energy sparks
            for (int i = 0; i < 10; i++)
            {
                Transform spark = animatedContent.transform.Find($"EnergySpark{i}");
                if (spark != null)
                {
                    float angle = (animationTime * 4f + i * 36f) * Mathf.Deg2Rad;
                    float radius = 0.3f + Mathf.Sin(animationTime * 6f + i) * 0.15f;

                    spark.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(animationTime * 5f + i * 2f) * 0.3f,
                        Mathf.Sin(angle) * radius
                    );

                    float pulse = 1f + Mathf.Sin(animationTime * 12f + i) * 0.6f;
                    spark.localScale = Vector3.one * 0.1f * pulse;
                }
            }
        }

        private void AnimateCyclone()
        {
            // Pulse vortex eye
            Transform core = animatedContent.transform.Find("VortexEye");
            if (core != null)
            {
                float pulse = 1f + Mathf.Sin(animationTime * 7f) * 0.3f;
                core.localScale = Vector3.one * 0.15f * pulse;
            }

            // Animate tornado layers
            for (int layer = 0; layer < 3; layer++)
            {
                int particlesPerLayer = 10;
                float layerSpeed = 0.8f - layer * 0.15f;

                for (int i = 0; i < particlesPerLayer; i++)
                {
                    Transform particle = animatedContent.transform.Find($"WindLayer{layer}_P{i}");
                    if (particle != null)
                    {
                        float t = (animationTime * layerSpeed + i / (float)particlesPerLayer) % 1f;
                        float angle = t * 360f * 3f; // Multiple spirals
                        float layerRadius = (0.2f + layer * 0.1f) * (1f - t * 0.3f);
                        float height = (t - 0.5f) * 0.8f;

                        particle.localPosition = new Vector3(
                            Mathf.Cos(angle * Mathf.Deg2Rad) * layerRadius,
                            height,
                            Mathf.Sin(angle * Mathf.Deg2Rad) * layerRadius
                        );

                        // Fade and shrink as they rise
                        float alpha = 0.5f * (1f - t) - layer * 0.1f;
                        Material mat = particle.GetComponent<MeshRenderer>().material;
                        Color col = mat.color;
                        col.a = alpha;
                        mat.color = col;
                    }
                }
            }

            // Debris swirls around violently
            for (int i = 0; i < 8; i++)
            {
                Transform debris = animatedContent.transform.Find($"Debris{i}");
                if (debris != null)
                {
                    float angle = (animationTime * 5f + i * 45f) * Mathf.Deg2Rad;
                    float radius = 0.35f + Mathf.Sin(animationTime * 3f + i) * 0.1f;

                    debris.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(animationTime * 4f + i) * 0.25f,
                        Mathf.Sin(angle) * radius
                    );

                    debris.localRotation = Quaternion.Euler(
                        animationTime * 100f + i * 40f,
                        animationTime * 120f + i * 50f,
                        animationTime * 80f + i * 30f
                    );
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
