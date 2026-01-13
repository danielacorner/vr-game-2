using UnityEngine;
using System.Collections.Generic;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// Creates a dense forest perimeter around the HomeArea clearing
    /// Includes ancient ruins, rune tablets, and dense trees to create an enclosed feeling
    /// </summary>
    public class ForestPerimeterBuilder : MonoBehaviour
    {
        [Header("Perimeter Settings")]
        [Tooltip("Radius of the clearing (inner boundary)")]
        public float clearingRadius = 20f;

        [Tooltip("Thickness of the forest perimeter")]
        public float forestThickness = 8f;

        [Tooltip("Number of tree rings in the perimeter")]
        [Range(2, 5)]
        public int treeRingCount = 3;

        [Header("Tree Settings")]
        [Tooltip("Trees per ring")]
        public int treesPerRing = 24;

        [Tooltip("Random position offset for natural look")]
        public float positionRandomness = 1.5f;

        [Tooltip("Tree scale variation")]
        public Vector2 scaleRange = new Vector2(1.2f, 2.0f);

        [Header("Ancient Boundary Markers")]
        [Tooltip("Number of rune tablets around perimeter")]
        public int runeTabletCount = 8;

        [Tooltip("Number of ancient pillars")]
        public int ancientPillarCount = 12;

        [Tooltip("Number of stone arches")]
        public int stoneArchCount = 4;

        [Header("Colors & Materials")]
        [Tooltip("Tree bark color (dark forest)")]
        public Color barkColor = new Color(0.2f, 0.15f, 0.1f);

        [Tooltip("Tree leaves color (dark green)")]
        public Color leavesColor = new Color(0.1f, 0.3f, 0.1f);

        [Tooltip("Rune glow color")]
        public Color runeGlowColor = new Color(0.3f, 0.6f, 1.0f);

        [Tooltip("Stone material color")]
        public Color stoneColor = new Color(0.4f, 0.4f, 0.45f);

        [Header("Organization")]
        [Tooltip("Parent object for all perimeter elements")]
        public Transform perimeterParent;

        private Transform treeContainer;
        private Transform runeContainer;
        private Transform pillarContainer;
        private Transform archContainer;

        [ContextMenu("Build Forest Perimeter")]
        public void BuildPerimeter()
        {
            Debug.Log("[ForestPerimeterBuilder] Building dense forest perimeter...");

            SetupContainers();
            ClearExistingPerimeter();

            BuildTreePerimeter();
            BuildRuneTablets();
            BuildAncientPillars();
            BuildStoneArches();

            Debug.Log("[ForestPerimeterBuilder] ✓ Forest perimeter complete!");
        }

        private void SetupContainers()
        {
            if (perimeterParent == null)
            {
                GameObject parent = GameObject.Find("ForestPerimeter");
                if (parent == null)
                {
                    parent = new GameObject("ForestPerimeter");
                }
                perimeterParent = parent.transform;
            }

            // Create sub-containers
            treeContainer = GetOrCreateChild(perimeterParent, "PerimeterTrees");
            runeContainer = GetOrCreateChild(perimeterParent, "RuneTablets");
            pillarContainer = GetOrCreateChild(perimeterParent, "AncientPillars");
            archContainer = GetOrCreateChild(perimeterParent, "StoneArches");
        }

        private Transform GetOrCreateChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child == null)
            {
                GameObject obj = new GameObject(name);
                obj.transform.SetParent(parent);
                child = obj.transform;
            }
            return child;
        }

        private void ClearExistingPerimeter()
        {
            ClearChildren(treeContainer);
            ClearChildren(runeContainer);
            ClearChildren(pillarContainer);
            ClearChildren(archContainer);
        }

        private void ClearChildren(Transform parent)
        {
            if (parent == null) return;

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        private void BuildTreePerimeter()
        {
            Debug.Log($"[ForestPerimeterBuilder] Creating {treeRingCount} tree rings with {treesPerRing} trees each...");

            for (int ring = 0; ring < treeRingCount; ring++)
            {
                float ringRadius = clearingRadius + (ring * (forestThickness / treeRingCount));
                float angleStep = 360f / treesPerRing;

                for (int i = 0; i < treesPerRing; i++)
                {
                    float angle = i * angleStep + Random.Range(-angleStep * 0.3f, angleStep * 0.3f);
                    float radius = ringRadius + Random.Range(-positionRandomness, positionRandomness);

                    Vector3 position = new Vector3(
                        Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                        0,
                        Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                    );

                    CreateDenseTree(position, ring);
                }
            }
        }

        private void CreateDenseTree(Vector3 position, int ringIndex)
        {
            // Create trunk
            GameObject tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tree.name = $"PerimeterTree_{ringIndex}";
            tree.transform.SetParent(treeContainer);
            tree.transform.position = position;

            float height = Random.Range(8f, 12f);
            float thickness = Random.Range(0.4f, 0.7f);
            tree.transform.localScale = new Vector3(thickness, height / 2f, thickness);

            // Position trunk base at ground
            tree.transform.position = position + Vector3.up * (height / 2f);

            // Random rotation
            tree.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            // Bark material
            Renderer treeRenderer = tree.GetComponent<Renderer>();
            Material barkMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            barkMat.color = barkColor;
            barkMat.SetFloat("_Smoothness", 0.1f);
            treeRenderer.material = barkMat;

            // Create dense foliage (multiple spheres for dense look)
            CreateDenseFoliage(tree.transform, height, thickness);
        }

        private void CreateDenseFoliage(Transform trunk, float treeHeight, float trunkThickness)
        {
            // Create 3-4 layers of foliage for density
            int foliageLayers = Random.Range(3, 5);

            for (int i = 0; i < foliageLayers; i++)
            {
                GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                foliage.name = "Foliage";
                foliage.transform.SetParent(trunk);

                // Stack foliage spheres
                float heightOffset = treeHeight * 0.3f + (i * treeHeight * 0.2f);
                foliage.transform.localPosition = Vector3.up * heightOffset;

                // Large, overlapping foliage for dense forest look
                float foliageSize = Random.Range(4f, 6f) * (1f - i * 0.15f);
                foliage.transform.localScale = Vector3.one * foliageSize;

                // Leaves material
                Renderer foliageRenderer = foliage.GetComponent<Renderer>();
                Material leavesMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                leavesMat.color = leavesColor;
                leavesMat.SetFloat("_Smoothness", 0.3f);
                foliageRenderer.material = leavesMat;

                // Remove collider for performance
                DestroyImmediate(foliage.GetComponent<Collider>());
            }
        }

        private void BuildRuneTablets()
        {
            Debug.Log($"[ForestPerimeterBuilder] Creating {runeTabletCount} rune tablets...");

            float angleStep = 360f / runeTabletCount;

            for (int i = 0; i < runeTabletCount; i++)
            {
                float angle = i * angleStep;
                Vector3 position = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * clearingRadius,
                    0,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * clearingRadius
                );

                CreateRuneTablet(position, angle);
            }
        }

        private void CreateRuneTablet(Vector3 position, float facingAngle)
        {
            // Create tablet (tall, thin cube)
            GameObject tablet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tablet.name = "RuneTablet";
            tablet.transform.SetParent(runeContainer);
            tablet.transform.position = position + Vector3.up * 1.5f; // 3m tall, half at ground
            tablet.transform.localScale = new Vector3(1.5f, 3f, 0.3f);

            // Face inward toward center
            tablet.transform.rotation = Quaternion.Euler(0, facingAngle + 180f, 0);

            // Stone material
            Renderer tabletRenderer = tablet.GetComponent<Renderer>();
            Material stoneMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            stoneMat.color = stoneColor;
            stoneMat.SetFloat("_Smoothness", 0.2f);
            tabletRenderer.material = stoneMat;

            // Add glowing rune symbol (plane in front)
            CreateGlowingRune(tablet.transform);
        }

        private void CreateGlowingRune(Transform tablet)
        {
            GameObject rune = GameObject.CreatePrimitive(PrimitiveType.Quad);
            rune.name = "Rune";
            rune.transform.SetParent(tablet);
            rune.transform.localPosition = new Vector3(0, 0.3f, -0.16f); // Slightly in front
            rune.transform.localRotation = Quaternion.identity;
            rune.transform.localScale = new Vector3(0.8f, 1.2f, 1f);

            // Glowing rune material
            Renderer runeRenderer = rune.GetComponent<Renderer>();
            Material runeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            runeMat.color = runeGlowColor;
            runeMat.SetFloat("_Smoothness", 0.9f);
            runeMat.EnableKeyword("_EMISSION");
            runeMat.SetColor("_EmissionColor", runeGlowColor * 2f);
            runeRenderer.material = runeMat;

            // Remove collider
            DestroyImmediate(rune.GetComponent<Collider>());

            // Add pulsing light
            Light runeLight = rune.AddComponent<Light>();
            runeLight.type = LightType.Point;
            runeLight.color = runeGlowColor;
            runeLight.intensity = 2f;
            runeLight.range = 5f;

            // Add pulsing animation
            RunePulse pulse = rune.AddComponent<RunePulse>();
            pulse.minIntensity = 1.5f;
            pulse.maxIntensity = 3f;
            pulse.pulseSpeed = 0.5f;
        }

        private void BuildAncientPillars()
        {
            Debug.Log($"[ForestPerimeterBuilder] Creating {ancientPillarCount} ancient pillars...");

            float angleStep = 360f / ancientPillarCount;

            for (int i = 0; i < ancientPillarCount; i++)
            {
                float angle = i * angleStep + Random.Range(-15f, 15f);
                float radius = clearingRadius + Random.Range(-1f, 1f);

                Vector3 position = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    0,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                );

                CreateAncientPillar(position);
            }
        }

        private void CreateAncientPillar(Vector3 position)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "AncientPillar";
            pillar.transform.SetParent(pillarContainer);

            float height = Random.Range(4f, 6f);
            pillar.transform.position = position + Vector3.up * (height / 2f);
            pillar.transform.localScale = new Vector3(0.5f, height / 2f, 0.5f);

            // Random lean for ancient look
            pillar.transform.rotation = Quaternion.Euler(Random.Range(-5f, 5f), Random.Range(0f, 360f), Random.Range(-5f, 5f));

            // Weathered stone material
            Renderer pillarRenderer = pillar.GetComponent<Renderer>();
            Material stoneMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            stoneMat.color = stoneColor * Random.Range(0.8f, 1.2f);
            stoneMat.SetFloat("_Smoothness", 0.1f);
            pillarRenderer.material = stoneMat;
        }

        private void BuildStoneArches()
        {
            Debug.Log($"[ForestPerimeterBuilder] Creating {stoneArchCount} stone arches...");

            float angleStep = 360f / stoneArchCount;

            for (int i = 0; i < stoneArchCount; i++)
            {
                float angle = i * angleStep;
                Vector3 position = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * clearingRadius,
                    0,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * clearingRadius
                );

                CreateStoneArch(position, angle);
            }
        }

        private void CreateStoneArch(Vector3 position, float facingAngle)
        {
            GameObject arch = new GameObject("StoneArch");
            arch.transform.SetParent(archContainer);
            arch.transform.position = position;
            arch.transform.rotation = Quaternion.Euler(0, facingAngle + 180f, 0);

            // Create two pillars
            CreateArchPillar(arch.transform, new Vector3(-1.5f, 0, 0));
            CreateArchPillar(arch.transform, new Vector3(1.5f, 0, 0));

            // Create arch top (curved)
            CreateArchTop(arch.transform);
        }

        private void CreateArchPillar(Transform arch, Vector3 localPos)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = "ArchPillar";
            pillar.transform.SetParent(arch);
            pillar.transform.localPosition = localPos + Vector3.up * 2f;
            pillar.transform.localScale = new Vector3(0.6f, 4f, 0.6f);

            // Stone material
            Renderer pillarRenderer = pillar.GetComponent<Renderer>();
            Material stoneMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            stoneMat.color = stoneColor;
            stoneMat.SetFloat("_Smoothness", 0.15f);
            pillarRenderer.material = stoneMat;
        }

        private void CreateArchTop(Transform arch)
        {
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "ArchTop";
            top.transform.SetParent(arch);
            top.transform.localPosition = Vector3.up * 4f;
            top.transform.localScale = new Vector3(3.5f, 0.5f, 0.6f);

            // Stone material
            Renderer topRenderer = top.GetComponent<Renderer>();
            Material stoneMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            stoneMat.color = stoneColor;
            stoneMat.SetFloat("_Smoothness", 0.15f);
            topRenderer.material = stoneMat;
        }

        [ContextMenu("Clear Perimeter")]
        public void ClearPerimeter()
        {
            if (perimeterParent != null)
            {
                DestroyImmediate(perimeterParent.gameObject);
                Debug.Log("[ForestPerimeterBuilder] Perimeter cleared");
            }
        }
    }

    /// <summary>
    /// Simple component to pulse rune glow intensity
    /// </summary>
    public class RunePulse : MonoBehaviour
    {
        public float minIntensity = 1.5f;
        public float maxIntensity = 3f;
        public float pulseSpeed = 0.5f;

        private Light runeLight;
        private float time;

        private void Start()
        {
            runeLight = GetComponent<Light>();
        }

        private void Update()
        {
            if (runeLight == null) return;

            time += Time.deltaTime * pulseSpeed;
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, (Mathf.Sin(time) + 1f) / 2f);
            runeLight.intensity = intensity;
        }
    }
}
