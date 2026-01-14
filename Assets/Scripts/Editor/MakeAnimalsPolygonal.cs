using UnityEngine;
using UnityEditor;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Converts animal mesh primitives from smooth spheres to polygonal cubes
    /// for a Polytopia-style low-poly aesthetic
    /// </summary>
    public static class MakeAnimalsPolygonal
    {
        [MenuItem("Tools/VR Dungeon Crawler/Make Animals Polygonal")]
        public static void ConvertAnimalsToPolygonal()
        {
            string[] animalPrefabPaths = new string[]
            {
                "Assets/Prefabs/Animals/Rabbit.prefab",
                "Assets/Prefabs/Animals/Squirrel.prefab",
                "Assets/Prefabs/Animals/Bird.prefab"
            };

            int convertedCount = 0;

            foreach (string prefabPath in animalPrefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    Debug.LogWarning($"[Polygonal] Prefab not found: {prefabPath}");
                    continue;
                }

                // Instantiate prefab for editing
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                // Find all MeshFilter components in the animal
                MeshFilter[] meshFilters = instance.GetComponentsInChildren<MeshFilter>();

                foreach (MeshFilter meshFilter in meshFilters)
                {
                    // Check if it's using a smooth primitive mesh (Sphere, Capsule, or high-poly mesh)
                    if (meshFilter.sharedMesh != null &&
                        (meshFilter.sharedMesh.name.Contains("Sphere") ||
                         meshFilter.sharedMesh.name.Contains("Capsule") ||
                         meshFilter.sharedMesh.name.Contains("Cylinder") ||
                         meshFilter.sharedMesh.vertexCount > 24)) // Cube has 24 vertices, anything more is too smooth
                    {
                        // Replace with cube mesh (24 vertices, flat faces)
                        GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Mesh cubeMesh = tempCube.GetComponent<MeshFilter>().sharedMesh;
                        Object.DestroyImmediate(tempCube);

                        meshFilter.sharedMesh = cubeMesh;

                        // Also update colliders to BoxCollider
                        SphereCollider sphereCollider = meshFilter.GetComponent<SphereCollider>();
                        if (sphereCollider != null)
                        {
                            Vector3 center = sphereCollider.center;
                            float radius = sphereCollider.radius;
                            bool isTrigger = sphereCollider.isTrigger;

                            Object.DestroyImmediate(sphereCollider);

                            BoxCollider boxCollider = meshFilter.gameObject.AddComponent<BoxCollider>();
                            boxCollider.center = center;
                            boxCollider.size = Vector3.one * radius * 2f;
                            boxCollider.isTrigger = isTrigger;
                        }

                        CapsuleCollider capsuleCollider = meshFilter.GetComponent<CapsuleCollider>();
                        if (capsuleCollider != null)
                        {
                            Vector3 center = capsuleCollider.center;
                            float radius = capsuleCollider.radius;
                            float height = capsuleCollider.height;
                            bool isTrigger = capsuleCollider.isTrigger;

                            Object.DestroyImmediate(capsuleCollider);

                            BoxCollider boxCollider = meshFilter.gameObject.AddComponent<BoxCollider>();
                            boxCollider.center = center;
                            boxCollider.size = new Vector3(radius * 2f, height, radius * 2f);
                            boxCollider.isTrigger = isTrigger;
                        }

                        convertedCount++;
                    }
                }

                // Apply changes back to prefab
                PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.AutomatedAction);
                Object.DestroyImmediate(instance);

                Debug.Log($"[Polygonal] ✓ Converted {prefab.name} to polygonal style");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Animals Made Polygonal!",
                $"✓ Converted {convertedCount} mesh parts to polygonal cubes\n" +
                $"✓ Animals now have Polytopia-style low-poly look\n" +
                $"✓ Sphere colliders replaced with box colliders",
                "OK"
            );
        }
    }
}
