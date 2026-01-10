using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Phase 7: Decorative Environmental Objects & Lighting Fixtures
    /// Adds variety and points of interest to the scene
    /// Run from menu: Tools/VR Dungeon Crawler/Phase 7 - Setup Decorative Objects
    /// </summary>
    public class SetupDecorativeObjectsPhase7 : UnityEditor.Editor
    {
        private static readonly Color CRYSTAL_BLUE = new Color(0.3f, 0.7f, 1f, 0.8f);
        private static readonly Color CRYSTAL_PURPLE = new Color(0.7f, 0.3f, 1f, 0.8f);
        private static readonly Color CRYSTAL_GREEN = new Color(0.3f, 1f, 0.7f, 0.8f);
        private static readonly Color TORCH_FLAME = new Color(1f, 0.6f, 0.2f);
        private static readonly Color LANTERN_GLOW = new Color(0.8f, 1f, 0.9f, 0.6f);

        [MenuItem("Tools/VR Dungeon Crawler/Phase 7 - Setup Decorative Objects")]
        public static void SetupDecorativeObjects()
        {
            Debug.Log("========================================");
            Debug.Log("Phase 7: Setting Up Decorative Objects");
            Debug.Log("========================================");

            // Create parent object
            GameObject decorativeParent = GameObject.Find("DecorativeObjects");
            if (decorativeParent == null)
            {
                decorativeParent = new GameObject("DecorativeObjects");
                decorativeParent.transform.position = Vector3.zero;
                Debug.Log("✓ Created DecorativeObjects parent");
            }

            // Add broken carts/wagons near ruins
            CreateBrokenCarts(decorativeParent.transform);
            Debug.Log("✓ Created broken carts");

            // Add weapon props
            CreateWeaponProps(decorativeParent.transform);
            Debug.Log("✓ Created weapon props");

            // Add scattered bones
            CreateScatteredBones(decorativeParent.transform);
            Debug.Log("✓ Created scattered bones");

            // Add glowing crystal clusters
            CreateCrystalClusters(decorativeParent.transform);
            Debug.Log("✓ Created crystal clusters");

            EditorUtility.SetDirty(decorativeParent);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log("✓✓✓ Decorative objects setup complete!");
            Debug.Log("========================================");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Phase 7 - Setup Lighting Fixtures")]
        public static void SetupLightingFixtures()
        {
            Debug.Log("========================================");
            Debug.Log("Phase 7: Setting Up Lighting Fixtures");
            Debug.Log("========================================");

            // Create parent object
            GameObject lightingParent = GameObject.Find("LightingFixtures");
            if (lightingParent == null)
            {
                lightingParent = new GameObject("LightingFixtures");
                lightingParent.transform.position = Vector3.zero;
                Debug.Log("✓ Created LightingFixtures parent");
            }

            // Add torch holders near ruins
            CreateTorchHolders(lightingParent.transform);
            Debug.Log("✓ Created torch holders");

            // Add hanging lanterns
            CreateHangingLanterns(lightingParent.transform);
            Debug.Log("✓ Created hanging lanterns");

            // Add light pillars
            CreateLightPillars(lightingParent.transform);
            Debug.Log("✓ Created magical light pillars");

            EditorUtility.SetDirty(lightingParent);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log("✓✓✓ Lighting fixtures setup complete!");
            Debug.Log("========================================");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Phase 7 - Setup Natural Features")]
        public static void SetupNaturalFeatures()
        {
            Debug.Log("========================================");
            Debug.Log("Phase 7: Setting Up Natural Features");
            Debug.Log("========================================");

            // Create parent object
            GameObject naturalParent = GameObject.Find("NaturalFeatures");
            if (naturalParent == null)
            {
                naturalParent = new GameObject("NaturalFeatures");
                naturalParent.transform.position = Vector3.zero;
                Debug.Log("✓ Created NaturalFeatures parent");
            }

            // Add rock formations
            CreateRockFormations(naturalParent.transform);
            Debug.Log("✓ Created rock formations");

            // Add cairns
            CreateCairns(naturalParent.transform);
            Debug.Log("✓ Created cairns");

            // Add hollow log
            CreateHollowLog(naturalParent.transform);
            Debug.Log("✓ Created hollow log");

            EditorUtility.SetDirty(naturalParent);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log("✓✓✓ Natural features setup complete!");
            Debug.Log("========================================");
        }

        // ============== DECORATIVE OBJECTS ==============

        private static void CreateBrokenCarts(Transform parent)
        {
            Vector3[] cartPositions = {
                new Vector3(15f, 0f, 15f),
                new Vector3(-18f, 0f, 12f),
                new Vector3(8f, 0f, -20f)
            };

            for (int i = 0; i < cartPositions.Length; i++)
            {
                GameObject cart = new GameObject($"BrokenCart_{i + 1}");
                cart.transform.SetParent(parent);

                Vector3 groundPos = GetGroundPosition(cartPositions[i]);
                cart.transform.position = groundPos;
                cart.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), Random.Range(-15f, 15f));

                // Cart body (broken box)
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "CartBody";
                body.transform.SetParent(cart.transform);
                body.transform.localPosition = Vector3.zero;
                body.transform.localScale = new Vector3(1.2f, 0.4f, 1.8f);
                Object.DestroyImmediate(body.GetComponent<Collider>()); // Remove collider for performance

                // Broken wheel 1
                GameObject wheel1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel1.name = "Wheel1";
                wheel1.transform.SetParent(cart.transform);
                wheel1.transform.localPosition = new Vector3(0.7f, -0.3f, 0.6f);
                wheel1.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                wheel1.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
                Object.DestroyImmediate(wheel1.GetComponent<Collider>());

                // Broken wheel 2 (tilted)
                GameObject wheel2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel2.name = "Wheel2";
                wheel2.transform.SetParent(cart.transform);
                wheel2.transform.localPosition = new Vector3(-0.7f, -0.5f, -0.6f);
                wheel2.transform.localRotation = Quaternion.Euler(90f, 0f, 45f);
                wheel2.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
                Object.DestroyImmediate(wheel2.GetComponent<Collider>());

                // Apply weathered wood material
                Material woodMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                woodMat.color = new Color(0.3f, 0.2f, 0.15f);
                woodMat.SetFloat("_Smoothness", 0.1f);

                body.GetComponent<Renderer>().material = woodMat;
                wheel1.GetComponent<Renderer>().material = woodMat;
                wheel2.GetComponent<Renderer>().material = woodMat;
            }
        }

        private static void CreateWeaponProps(Transform parent)
        {
            Vector3[] weaponPositions = {
                new Vector3(10f, 0f, 8f),
                new Vector3(-12f, 0f, -8f),
                new Vector3(5f, 0f, -15f),
                new Vector3(-8f, 0f, 18f)
            };

            for (int i = 0; i < weaponPositions.Length; i++)
            {
                bool isSword = i % 2 == 0;
                GameObject weapon = isSword ? CreateSword(i) : CreateSpear(i);
                weapon.transform.SetParent(parent);

                Vector3 groundPos = GetGroundPosition(weaponPositions[i]);
                weapon.transform.position = groundPos;
                weapon.transform.rotation = Quaternion.Euler(Random.Range(-30f, -60f), Random.Range(0f, 360f), 0f);
            }
        }

        private static GameObject CreateSword(int index)
        {
            GameObject sword = new GameObject($"Sword_{index + 1}");

            // Blade
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Blade";
            blade.transform.SetParent(sword.transform);
            blade.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            blade.transform.localScale = new Vector3(0.08f, 1.2f, 0.02f);
            Object.DestroyImmediate(blade.GetComponent<Collider>());

            // Crossguard
            GameObject guard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            guard.name = "Guard";
            guard.transform.SetParent(sword.transform);
            guard.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            guard.transform.localScale = new Vector3(0.4f, 0.05f, 0.08f);
            Object.DestroyImmediate(guard.GetComponent<Collider>());

            // Handle
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Handle";
            handle.transform.SetParent(sword.transform);
            handle.transform.localPosition = new Vector3(0f, -0.15f, 0f);
            handle.transform.localScale = new Vector3(0.06f, 0.25f, 0.06f);
            Object.DestroyImmediate(handle.GetComponent<Collider>());

            // Materials
            Material metalMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            metalMat.color = new Color(0.6f, 0.6f, 0.65f);
            metalMat.SetFloat("_Metallic", 0.8f);
            metalMat.SetFloat("_Smoothness", 0.6f);

            blade.GetComponent<Renderer>().material = metalMat;
            guard.GetComponent<Renderer>().material = metalMat;

            Material leatherMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            leatherMat.color = new Color(0.3f, 0.2f, 0.15f);
            handle.GetComponent<Renderer>().material = leatherMat;

            return sword;
        }

        private static GameObject CreateSpear(int index)
        {
            GameObject spear = new GameObject($"Spear_{index + 1}");

            // Shaft
            GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaft.name = "Shaft";
            shaft.transform.SetParent(spear.transform);
            shaft.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            shaft.transform.localScale = new Vector3(0.04f, 1.6f, 0.04f);
            Object.DestroyImmediate(shaft.GetComponent<Collider>());

            // Spearhead
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Spearhead";
            head.transform.SetParent(spear.transform);
            head.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            head.transform.localScale = new Vector3(0.1f, 0.4f, 0.03f);
            Object.DestroyImmediate(head.GetComponent<Collider>());

            // Materials
            Material woodMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            woodMat.color = new Color(0.4f, 0.3f, 0.2f);
            shaft.GetComponent<Renderer>().material = woodMat;

            Material metalMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            metalMat.color = new Color(0.6f, 0.6f, 0.65f);
            metalMat.SetFloat("_Metallic", 0.8f);
            head.GetComponent<Renderer>().material = metalMat;

            return spear;
        }

        private static void CreateScatteredBones(Transform parent)
        {
            // Scatter 10-15 bone piles
            int boneCount = Random.Range(10, 16);

            for (int i = 0; i < boneCount; i++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(-20f, 20f),
                    0f,
                    Random.Range(-20f, 20f)
                );

                Vector3 groundPos = GetGroundPosition(randomPos);

                GameObject bone = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                bone.name = $"Bone_{i + 1}";
                bone.transform.SetParent(parent);
                bone.transform.position = groundPos + Vector3.up * 0.05f;
                bone.transform.rotation = Quaternion.Euler(Random.Range(0f, 90f), Random.Range(0f, 360f), Random.Range(0f, 90f));
                bone.transform.localScale = new Vector3(0.04f, 0.15f, 0.04f);

                Object.DestroyImmediate(bone.GetComponent<Collider>());

                Material boneMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                boneMat.color = new Color(0.9f, 0.85f, 0.75f);
                boneMat.SetFloat("_Smoothness", 0.3f);
                bone.GetComponent<Renderer>().material = boneMat;
            }
        }

        private static void CreateCrystalClusters(Transform parent)
        {
            Vector3[] clusterPositions = {
                new Vector3(12f, 0f, -10f),
                new Vector3(-15f, 0f, 8f),
                new Vector3(8f, 0f, 18f),
                new Vector3(-8f, 0f, -18f),
                new Vector3(20f, 0f, 5f)
            };

            Color[] crystalColors = { CRYSTAL_BLUE, CRYSTAL_PURPLE, CRYSTAL_GREEN };

            for (int i = 0; i < clusterPositions.Length; i++)
            {
                GameObject cluster = new GameObject($"CrystalCluster_{i + 1}");
                cluster.transform.SetParent(parent);

                Vector3 groundPos = GetGroundPosition(clusterPositions[i]);
                cluster.transform.position = groundPos;

                // Create 3-5 crystals per cluster
                int crystalCount = Random.Range(3, 6);
                Color clusterColor = crystalColors[i % crystalColors.Length];

                for (int j = 0; j < crystalCount; j++)
                {
                    GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    crystal.name = $"Crystal_{j + 1}";
                    crystal.transform.SetParent(cluster.transform);

                    Vector3 offset = Random.insideUnitCircle * 0.3f;
                    crystal.transform.localPosition = new Vector3(offset.x, Random.Range(0.2f, 0.5f), offset.y);
                    crystal.transform.localRotation = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0f, 360f), Random.Range(-15f, 15f));
                    crystal.transform.localScale = new Vector3(0.15f, Random.Range(0.4f, 0.8f), 0.15f);

                    Object.DestroyImmediate(crystal.GetComponent<Collider>());

                    // Glowing crystal material
                    Material crystalMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    crystalMat.color = clusterColor;
                    crystalMat.SetFloat("_Smoothness", 0.9f);
                    crystalMat.EnableKeyword("_EMISSION");
                    crystalMat.SetColor("_EmissionColor", clusterColor * 1.5f);
                    crystal.GetComponent<Renderer>().material = crystalMat;
                }

                // Add point light
                GameObject lightGO = new GameObject("CrystalLight");
                lightGO.transform.SetParent(cluster.transform);
                lightGO.transform.localPosition = Vector3.up * 0.3f;
                Light light = lightGO.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = clusterColor;
                light.intensity = 1.5f;
                light.range = 4f;
                light.shadows = LightShadows.None;
            }
        }

        // ============== LIGHTING FIXTURES ==============

        private static void CreateTorchHolders(Transform parent)
        {
            Vector3[] torchPositions = {
                new Vector3(10f, 0f, 10f),
                new Vector3(-10f, 0f, 10f),
                new Vector3(10f, 0f, -10f),
                new Vector3(-10f, 0f, -10f),
                new Vector3(15f, 0f, 0f),
                new Vector3(-15f, 0f, 0f),
                new Vector3(0f, 0f, 15f)
            };

            for (int i = 0; i < torchPositions.Length; i++)
            {
                GameObject torch = new GameObject($"Torch_{i + 1}");
                torch.transform.SetParent(parent);

                Vector3 groundPos = GetGroundPosition(torchPositions[i]);
                torch.transform.position = groundPos;

                // Pole
                GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.name = "Pole";
                pole.transform.SetParent(torch.transform);
                pole.transform.localPosition = new Vector3(0f, 1.5f, 0f);
                pole.transform.localScale = new Vector3(0.08f, 1.5f, 0.08f);
                Object.DestroyImmediate(pole.GetComponent<Collider>());

                Material poleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                poleMat.color = new Color(0.2f, 0.2f, 0.2f);
                poleMat.SetFloat("_Metallic", 0.6f);
                pole.GetComponent<Renderer>().material = poleMat;

                // Flame (glowing sphere)
                GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flame.name = "Flame";
                flame.transform.SetParent(torch.transform);
                flame.transform.localPosition = new Vector3(0f, 3.2f, 0f);
                flame.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);
                Object.DestroyImmediate(flame.GetComponent<Collider>());

                Material flameMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                flameMat.color = TORCH_FLAME;
                flameMat.EnableKeyword("_EMISSION");
                flameMat.SetColor("_EmissionColor", TORCH_FLAME * 2f);
                flame.GetComponent<Renderer>().material = flameMat;

                // Light
                GameObject lightGO = new GameObject("TorchLight");
                lightGO.transform.SetParent(torch.transform);
                lightGO.transform.localPosition = new Vector3(0f, 3.2f, 0f);
                Light light = lightGO.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = TORCH_FLAME;
                light.intensity = 2f;
                light.range = 8f;
                light.shadows = LightShadows.None;
            }
        }

        private static void CreateHangingLanterns(Transform parent)
        {
            // Find trees to hang lanterns from
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int lanternsCreated = 0;

            foreach (GameObject obj in allObjects)
            {
                if (lanternsCreated >= 5) break;

                if (obj.name.ToLower().Contains("tree") && Random.value > 0.7f)
                {
                    GameObject lantern = new GameObject($"HangingLantern_{lanternsCreated + 1}");
                    lantern.transform.SetParent(parent);
                    lantern.transform.position = obj.transform.position + new Vector3(Random.Range(-2f, 2f), 4f, Random.Range(-2f, 2f));

                    // Lantern body (small cube)
                    GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    body.name = "LanternBody";
                    body.transform.SetParent(lantern.transform);
                    body.transform.localPosition = Vector3.zero;
                    body.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
                    Object.DestroyImmediate(body.GetComponent<Collider>());

                    Material lanternMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    lanternMat.color = LANTERN_GLOW;
                    lanternMat.EnableKeyword("_EMISSION");
                    lanternMat.SetColor("_EmissionColor", LANTERN_GLOW * 1.5f);
                    body.GetComponent<Renderer>().material = lanternMat;

                    // Light
                    Light light = lantern.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = LANTERN_GLOW;
                    light.intensity = 1f;
                    light.range = 6f;
                    light.shadows = LightShadows.None;

                    lanternsCreated++;
                }
            }

            Debug.Log($"Created {lanternsCreated} hanging lanterns");
        }

        private static void CreateLightPillars(Transform parent)
        {
            Vector3[] pillarPositions = {
                new Vector3(18f, 0f, 18f),
                new Vector3(-18f, 0f, -18f),
                new Vector3(18f, 0f, -18f)
            };

            for (int i = 0; i < pillarPositions.Length; i++)
            {
                GameObject pillar = new GameObject($"LightPillar_{i + 1}");
                pillar.transform.SetParent(parent);

                Vector3 groundPos = GetGroundPosition(pillarPositions[i]);
                pillar.transform.position = groundPos;

                // Base stone
                GameObject baseStone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                baseStone.name = "Base";
                baseStone.transform.SetParent(pillar.transform);
                baseStone.transform.localPosition = new Vector3(0f, 0.3f, 0f);
                baseStone.transform.localScale = new Vector3(0.8f, 0.3f, 0.8f);
                Object.DestroyImmediate(baseStone.GetComponent<Collider>());

                Material stoneMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                stoneMat.color = new Color(0.4f, 0.4f, 0.45f);
                baseStone.GetComponent<Renderer>().material = stoneMat;

                // Light beam (tall glowing cylinder)
                GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                beam.name = "LightBeam";
                beam.transform.SetParent(pillar.transform);
                beam.transform.localPosition = new Vector3(0f, 10f, 0f);
                beam.transform.localScale = new Vector3(0.3f, 10f, 0.3f);
                Object.DestroyImmediate(beam.GetComponent<Collider>());

                Color beamColor = i == 0 ? CRYSTAL_BLUE : (i == 1 ? CRYSTAL_PURPLE : CRYSTAL_GREEN);
                Material beamMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                beamMat.color = new Color(beamColor.r, beamColor.g, beamColor.b, 0.3f);
                beamMat.EnableKeyword("_EMISSION");
                beamMat.SetColor("_EmissionColor", beamColor * 3f);

                // Make transparent
                beamMat.SetFloat("_Surface", 1); // Transparent
                beamMat.SetFloat("_Blend", 0); // Alpha
                beam.GetComponent<Renderer>().material = beamMat;

                // Spotlight
                GameObject lightGO = new GameObject("PillarLight");
                lightGO.transform.SetParent(pillar.transform);
                lightGO.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                lightGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Light light = lightGO.AddComponent<Light>();
                light.type = LightType.Spot;
                light.color = beamColor;
                light.intensity = 3f;
                light.range = 25f;
                light.spotAngle = 15f;
                light.shadows = LightShadows.None;
            }
        }

        // ============== NATURAL FEATURES ==============

        private static void CreateRockFormations(Transform parent)
        {
            Vector3[] formationPositions = {
                new Vector3(22f, 0f, 10f),
                new Vector3(-22f, 0f, -8f),
                new Vector3(8f, 0f, 22f),
                new Vector3(-10f, 0f, -22f)
            };

            for (int i = 0; i < formationPositions.Length; i++)
            {
                GameObject formation = new GameObject($"RockFormation_{i + 1}");
                formation.transform.SetParent(parent);

                Vector3 groundPos = GetGroundPosition(formationPositions[i]);
                formation.transform.position = groundPos;

                // Create 3-5 large boulders
                int boulderCount = Random.Range(3, 6);
                for (int j = 0; j < boulderCount; j++)
                {
                    GameObject boulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    boulder.name = $"Boulder_{j + 1}";
                    boulder.transform.SetParent(formation.transform);

                    Vector3 offset = Random.insideUnitCircle * 2f;
                    boulder.transform.localPosition = new Vector3(offset.x, Random.Range(0.5f, 1f), offset.y);
                    boulder.transform.localScale = Vector3.one * Random.Range(1f, 2.5f);

                    Object.DestroyImmediate(boulder.GetComponent<Collider>());

                    Material rockMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    rockMat.color = new Color(0.35f, 0.35f, 0.4f);
                    rockMat.SetFloat("_Smoothness", 0.2f);
                    boulder.GetComponent<Renderer>().material = rockMat;
                }
            }
        }

        private static void CreateCairns(Transform parent)
        {
            Vector3[] cairnPositions = {
                new Vector3(12f, 0f, 5f),
                new Vector3(-8f, 0f, -12f),
                new Vector3(5f, 0f, -8f)
            };

            for (int i = 0; i < cairnPositions.Length; i++)
            {
                GameObject cairn = new GameObject($"Cairn_{i + 1}");
                cairn.transform.SetParent(parent);

                Vector3 groundPos = GetGroundPosition(cairnPositions[i]);
                cairn.transform.position = groundPos;

                // Stack 4-6 stones
                int stoneCount = Random.Range(4, 7);
                float currentHeight = 0f;

                for (int j = 0; j < stoneCount; j++)
                {
                    GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    stone.name = $"Stone_{j + 1}";
                    stone.transform.SetParent(cairn.transform);

                    float stoneSize = Mathf.Lerp(0.4f, 0.2f, (float)j / stoneCount); // Smaller at top
                    stone.transform.localPosition = new Vector3(
                        Random.Range(-0.05f, 0.05f),
                        currentHeight + stoneSize * 0.5f,
                        Random.Range(-0.05f, 0.05f)
                    );
                    stone.transform.localScale = Vector3.one * stoneSize;
                    currentHeight += stoneSize * 0.9f; // Slight overlap

                    Object.DestroyImmediate(stone.GetComponent<Collider>());

                    Material rockMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    rockMat.color = new Color(0.4f, 0.4f, 0.45f);
                    rockMat.SetFloat("_Smoothness", 0.2f);
                    stone.GetComponent<Renderer>().material = rockMat;
                }
            }
        }

        private static void CreateHollowLog(Transform parent)
        {
            Vector3 logPosition = new Vector3(-5f, 0f, -5f);
            Vector3 groundPos = GetGroundPosition(logPosition);

            GameObject hollowLog = new GameObject("HollowLog");
            hollowLog.transform.SetParent(parent);
            hollowLog.transform.position = groundPos;
            hollowLog.transform.rotation = Quaternion.Euler(0f, 45f, 0f);

            // Main log body
            GameObject logBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            logBody.name = "LogBody";
            logBody.transform.SetParent(hollowLog.transform);
            logBody.transform.localPosition = Vector3.zero;
            logBody.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            logBody.transform.localScale = new Vector3(0.8f, 2f, 0.8f);

            Material barkMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            barkMat.color = new Color(0.3f, 0.2f, 0.15f);
            barkMat.SetFloat("_Smoothness", 0.2f);
            logBody.GetComponent<Renderer>().material = barkMat;

            // Hollow interior (darker cylinder inside)
            GameObject interior = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            interior.name = "Interior";
            interior.transform.SetParent(hollowLog.transform);
            interior.transform.localPosition = Vector3.zero;
            interior.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            interior.transform.localScale = new Vector3(0.6f, 2.1f, 0.6f);
            Object.DestroyImmediate(interior.GetComponent<Collider>());

            Material interiorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            interiorMat.color = new Color(0.1f, 0.08f, 0.06f);
            interior.GetComponent<Renderer>().material = interiorMat;
        }

        // ============== HELPERS ==============

        private static Vector3 GetGroundPosition(Vector3 position)
        {
            RaycastHit hit;
            Vector3 startPos = new Vector3(position.x, 50f, position.z);

            if (Physics.Raycast(startPos, Vector3.down, out hit, 100f))
            {
                return hit.point;
            }

            return new Vector3(position.x, 0f, position.z);
        }

        [MenuItem("Tools/VR Dungeon Crawler/Phase 7 - Complete Setup")]
        public static void CompletePhase7Setup()
        {
            Debug.Log("========================================");
            Debug.Log("RUNNING COMPLETE PHASE 7 SETUP");
            Debug.Log("========================================");

            SetupDecorativeObjects();
            Debug.Log("");
            SetupLightingFixtures();
            Debug.Log("");
            SetupNaturalFeatures();

            Debug.Log("");
            Debug.Log("========================================");
            Debug.Log("✓✓✓ PHASE 7 COMPLETE!");
            Debug.Log("Decorative objects, lighting fixtures, and natural features added!");
            Debug.Log("========================================");
        }
    }
}
