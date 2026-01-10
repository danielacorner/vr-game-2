using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Phase 2: Terrain Detail & Vegetation
    /// Adds grass, rocks, leaves, mushrooms, and ferns to HomeArea scene
    /// Run from menu: Tools/VR Dungeon Crawler/Phase 2 - Setup Terrain Detail
    /// </summary>
    public class SetupTerrainDetailPhase2 : UnityEditor.Editor
    {
        private static readonly Color GRASS_COLOR = new Color(0.3f, 0.5f, 0.3f);
        private static readonly Color MUSHROOM_GLOW = new Color(0.3f, 0.7f, 1f);

        [MenuItem("Tools/VR Dungeon Crawler/Phase 2 - Add Grass Detail")]
        public static void AddGrassDetail()
        {
            Debug.Log("========================================");
            Debug.Log("Phase 2: Adding Grass Detail Layer");
            Debug.Log("========================================");

            // Find terrain
            Terrain terrain = Object.FindObjectOfType<Terrain>();
            if (terrain == null)
            {
                Debug.LogError("✗ No Terrain found in scene!");
                return;
            }

            TerrainData terrainData = terrain.terrainData;

            // Create grass detail prototype
            DetailPrototype grassDetail = new DetailPrototype();
            grassDetail.renderMode = DetailRenderMode.Grass;
            grassDetail.healthyColor = new Color(0.4f, 0.6f, 0.3f);
            grassDetail.dryColor = new Color(0.5f, 0.5f, 0.3f);
            grassDetail.minHeight = 0.3f;
            grassDetail.maxHeight = 0.8f;
            grassDetail.minWidth = 0.3f;
            grassDetail.maxWidth = 0.8f;
            grassDetail.noiseSpread = 0.1f;

            // Add grass detail to terrain
            List<DetailPrototype> detailPrototypes = new List<DetailPrototype>(terrainData.detailPrototypes);
            detailPrototypes.Add(grassDetail);
            terrainData.detailPrototypes = detailPrototypes.ToArray();

            // Set grass density (moderate for Quest 3 performance)
            int detailLayer = detailPrototypes.Count - 1;
            int resolution = terrainData.detailResolution;
            int[,] detailMap = new int[resolution, resolution];

            // Populate with grass (30% density)
            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    if (Random.value < 0.3f)
                    {
                        detailMap[x, y] = Random.Range(1, 4); // 1-3 grass blades per patch
                    }
                }
            }

            terrainData.SetDetailLayer(0, 0, detailLayer, detailMap);

            // Enable grass wind
            terrainData.wavingGrassStrength = 0.3f;
            terrainData.wavingGrassSpeed = 0.5f;
            terrainData.wavingGrassAmount = 0.2f;
            terrainData.wavingGrassTint = new Color(0.7f, 0.8f, 0.7f);

            EditorUtility.SetDirty(terrain);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log($"✓ Grass detail added with wind animation");
            Debug.Log("========================================");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Phase 2 - Add Scattered Objects")]
        public static void AddScatteredObjects()
        {
            Debug.Log("========================================");
            Debug.Log("Phase 2: Adding Scattered Detail Objects");
            Debug.Log("========================================");

            // Find or create parent object
            GameObject detailParent = GameObject.Find("DetailObjects");
            if (detailParent == null)
            {
                detailParent = new GameObject("DetailObjects");
                Debug.Log("✓ Created DetailObjects parent");
            }

            // Define scatter area (around player spawn area)
            Vector3 center = new Vector3(0, 0, 0);
            float radius = 40f;

            int rocksCreated = 0;
            int leavesCreated = 0;
            int mushroomsCreated = 0;
            int fernsCreated = 0;

            // Add small rocks (50-100)
            GameObject rocksParent = CreateOrGetChild(detailParent, "Rocks");
            int targetRocks = Random.Range(50, 100);
            for (int i = 0; i < targetRocks; i++)
            {
                Vector3 pos = GetRandomGroundPosition(center, radius);
                if (pos != Vector3.zero)
                {
                    CreateRock(rocksParent.transform, pos, i);
                    rocksCreated++;
                }
            }

            // Add leaf clusters (20-30)
            GameObject leavesParent = CreateOrGetChild(detailParent, "LeafClusters");
            int targetLeaves = Random.Range(20, 30);
            for (int i = 0; i < targetLeaves; i++)
            {
                Vector3 pos = GetRandomGroundPosition(center, radius);
                if (pos != Vector3.zero)
                {
                    CreateLeafCluster(leavesParent.transform, pos, i);
                    leavesCreated++;
                }
            }

            // Add glowing mushrooms (10-15)
            GameObject mushroomsParent = CreateOrGetChild(detailParent, "GlowingMushrooms");
            int targetMushrooms = Random.Range(10, 15);
            for (int i = 0; i < targetMushrooms; i++)
            {
                Vector3 pos = GetRandomGroundPosition(center, radius);
                if (pos != Vector3.zero)
                {
                    CreateGlowingMushroom(mushroomsParent.transform, pos, i);
                    mushroomsCreated++;
                }
            }

            // Add ferns (15-20)
            GameObject fernsParent = CreateOrGetChild(detailParent, "Ferns");
            int targetFerns = Random.Range(15, 20);
            for (int i = 0; i < targetFerns; i++)
            {
                Vector3 pos = GetRandomGroundPosition(center, radius);
                if (pos != Vector3.zero)
                {
                    CreateFern(fernsParent.transform, pos, i);
                    fernsCreated++;
                }
            }

            EditorUtility.SetDirty(detailParent);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log($"✓ Created {rocksCreated} rocks");
            Debug.Log($"✓ Created {leavesCreated} leaf clusters");
            Debug.Log($"✓ Created {mushroomsCreated} glowing mushrooms");
            Debug.Log($"✓ Created {fernsCreated} ferns");
            Debug.Log("========================================");
        }

        private static GameObject CreateOrGetChild(GameObject parent, string name)
        {
            Transform existing = parent.transform.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform);
            return child;
        }

        private static Vector3 GetRandomGroundPosition(Vector3 center, float radius)
        {
            // Random position in circle
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 pos = center + new Vector3(randomCircle.x, 100f, randomCircle.y);

            // Raycast down to find ground
            if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 200f))
            {
                return hit.point;
            }

            // Fallback to terrain height if raycast fails
            Terrain terrain = Object.FindObjectOfType<Terrain>();
            if (terrain != null)
            {
                float height = terrain.SampleHeight(pos);
                return new Vector3(pos.x, height, pos.z);
            }

            return Vector3.zero;
        }

        private static void CreateRock(Transform parent, Vector3 position, int index)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = $"Rock_{index}";
            rock.transform.SetParent(parent);
            rock.transform.position = position;

            // Random size (small rocks)
            float scale = Random.Range(0.15f, 0.4f);
            rock.transform.localScale = new Vector3(scale, scale * Random.Range(0.6f, 1f), scale);

            // Random rotation
            rock.transform.rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

            // Gray material
            Renderer renderer = rock.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.4f, 0.4f, 0.4f);
            renderer.material = mat;

            // Remove collider for performance
            Object.DestroyImmediate(rock.GetComponent<Collider>());
        }

        private static void CreateLeafCluster(Transform parent, Vector3 position, int index)
        {
            GameObject leaves = new GameObject($"LeafCluster_{index}");
            leaves.transform.SetParent(parent);
            leaves.transform.position = position;

            // Create 3-5 small leaf quads
            int leafCount = Random.Range(3, 6);
            for (int i = 0; i < leafCount; i++)
            {
                GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Quad);
                leaf.name = $"Leaf_{i}";
                leaf.transform.SetParent(leaves.transform);
                leaf.transform.localPosition = Random.insideUnitSphere * 0.3f;
                leaf.transform.localScale = Vector3.one * Random.Range(0.15f, 0.25f);
                leaf.transform.rotation = Quaternion.Euler(Random.Range(-45, 45), Random.Range(0, 360), Random.Range(-45, 45));

                // Brown/orange leaf color
                Renderer renderer = leaf.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.6f, 0.4f, 0.2f);
                renderer.material = mat;

                // Remove collider
                Object.DestroyImmediate(leaf.GetComponent<Collider>());
            }
        }

        private static void CreateGlowingMushroom(Transform parent, Vector3 position, int index)
        {
            GameObject mushroom = new GameObject($"Mushroom_{index}");
            mushroom.transform.SetParent(parent);
            mushroom.transform.position = position;

            // Create stem (cylinder)
            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Stem";
            stem.transform.SetParent(mushroom.transform);
            stem.transform.localPosition = Vector3.up * 0.1f;
            stem.transform.localScale = new Vector3(0.05f, 0.1f, 0.05f);

            Renderer stemRenderer = stem.GetComponent<Renderer>();
            Material stemMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            stemMat.color = new Color(0.8f, 0.8f, 0.7f);
            stemRenderer.material = stemMat;
            Object.DestroyImmediate(stem.GetComponent<Collider>());

            // Create cap (sphere)
            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cap.name = "Cap";
            cap.transform.SetParent(mushroom.transform);
            cap.transform.localPosition = Vector3.up * 0.25f;
            cap.transform.localScale = new Vector3(0.2f, 0.1f, 0.2f);

            Renderer capRenderer = cap.GetComponent<Renderer>();
            Material capMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            capMat.color = MUSHROOM_GLOW;
            capMat.EnableKeyword("_EMISSION");
            capMat.SetColor("_EmissionColor", MUSHROOM_GLOW * 0.5f);
            capRenderer.material = capMat;
            Object.DestroyImmediate(cap.GetComponent<Collider>());

            // Add point light for glow
            GameObject lightGO = new GameObject("MushroomLight");
            lightGO.transform.SetParent(mushroom.transform);
            lightGO.transform.localPosition = Vector3.up * 0.25f;

            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = MUSHROOM_GLOW;
            light.intensity = 0.3f;
            light.range = 2f;
            light.shadows = LightShadows.None; // Performance optimization
        }

        private static void CreateFern(Transform parent, Vector3 position, int index)
        {
            GameObject fern = new GameObject($"Fern_{index}");
            fern.transform.SetParent(parent);
            fern.transform.position = position;

            // Create 4-6 fronds
            int frondCount = Random.Range(4, 7);
            for (int i = 0; i < frondCount; i++)
            {
                GameObject frond = GameObject.CreatePrimitive(PrimitiveType.Quad);
                frond.name = $"Frond_{i}";
                frond.transform.SetParent(fern.transform);

                float angle = (360f / frondCount) * i;
                float height = Random.Range(0.3f, 0.6f);

                frond.transform.localPosition = Vector3.up * (height * 0.5f);
                frond.transform.localRotation = Quaternion.Euler(Random.Range(-30, -60), angle, 0);
                frond.transform.localScale = new Vector3(0.2f, height, 0.2f);

                // Green fern color
                Renderer renderer = frond.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.2f, 0.5f, 0.2f);
                renderer.material = mat;

                // Remove collider
                Object.DestroyImmediate(frond.GetComponent<Collider>());
            }
        }

        [MenuItem("Tools/VR Dungeon Crawler/Phase 2 - Complete Setup")]
        public static void CompletePhase2Setup()
        {
            Debug.Log("========================================");
            Debug.Log("RUNNING COMPLETE PHASE 2 SETUP");
            Debug.Log("========================================");

            AddGrassDetail();
            Debug.Log("");
            AddScatteredObjects();

            Debug.Log("");
            Debug.Log("========================================");
            Debug.Log("✓✓✓ PHASE 2 COMPLETE!");
            Debug.Log("Note: Tree material enhancement requires texture assets");
            Debug.Log("========================================");
        }
    }
}
