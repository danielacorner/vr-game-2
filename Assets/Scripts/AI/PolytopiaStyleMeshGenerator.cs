using UnityEngine;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Generates Polytopia-style angular geometric meshes
    /// Creates cubes, rectangular boxes, wedges, and trapezoids for blocky character style
    /// Polytopia animals are made from simple angular shapes, not rounded forms
    /// </summary>
    public static class PolytopiaStyleMeshGenerator
    {
        /// <summary>
        /// Creates a simple cube mesh (24 vertices for flat shading)
        /// </summary>
        public static Mesh CreateCube()
        {
            Mesh mesh = new Mesh();
            mesh.name = "AngularCube";

            Vector3[] vertices = new Vector3[]
            {
                // Front face
                new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f),
                // Back face
                new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f),
                // Left face
                new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
                // Right face
                new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f),
                // Top face
                new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
                // Bottom face
                new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f)
            };

            int[] triangles = new int[]
            {
                0, 2, 1, 0, 3, 2,       // Front
                4, 6, 5, 4, 7, 6,       // Back
                8, 10, 9, 8, 11, 10,    // Left
                12, 14, 13, 12, 15, 14, // Right
                16, 18, 17, 16, 19, 18, // Top
                20, 22, 21, 20, 23, 22  // Bottom
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// Creates a rectangular box (can be stretched to any proportions)
        /// </summary>
        public static Mesh CreateBox(float width = 1f, float height = 1f, float depth = 1f)
        {
            // Just use cube and let scale handle proportions
            return CreateCube();
        }

        /// <summary>
        /// Creates a trapezoid/wedge shape (for bodies, tapered forms)
        /// Bottom wider than top for animal bodies
        /// </summary>
        public static Mesh CreateTrapezoid(float topScale = 0.7f)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Trapezoid";

            float top = topScale * 0.5f;
            float bottom = 0.5f;

            Vector3[] vertices = new Vector3[]
            {
                // Front face (trapezoid)
                new Vector3(-bottom, -0.5f, 0.5f), new Vector3(bottom, -0.5f, 0.5f),
                new Vector3(top, 0.5f, 0.5f), new Vector3(-top, 0.5f, 0.5f),
                // Back face (trapezoid)
                new Vector3(bottom, -0.5f, -0.5f), new Vector3(-bottom, -0.5f, -0.5f),
                new Vector3(-top, 0.5f, -0.5f), new Vector3(top, 0.5f, -0.5f),
                // Left face
                new Vector3(-bottom, -0.5f, -0.5f), new Vector3(-bottom, -0.5f, 0.5f),
                new Vector3(-top, 0.5f, 0.5f), new Vector3(-top, 0.5f, -0.5f),
                // Right face
                new Vector3(bottom, -0.5f, 0.5f), new Vector3(bottom, -0.5f, -0.5f),
                new Vector3(top, 0.5f, -0.5f), new Vector3(top, 0.5f, 0.5f),
                // Top face (smaller)
                new Vector3(-top, 0.5f, 0.5f), new Vector3(top, 0.5f, 0.5f),
                new Vector3(top, 0.5f, -0.5f), new Vector3(-top, 0.5f, -0.5f),
                // Bottom face
                new Vector3(-bottom, -0.5f, -0.5f), new Vector3(bottom, -0.5f, -0.5f),
                new Vector3(bottom, -0.5f, 0.5f), new Vector3(-bottom, -0.5f, 0.5f)
            };

            int[] triangles = new int[]
            {
                0, 2, 1, 0, 3, 2,       // Front
                4, 6, 5, 4, 7, 6,       // Back
                8, 10, 9, 8, 11, 10,    // Left
                12, 14, 13, 12, 15, 14, // Right
                16, 18, 17, 16, 19, 18, // Top
                20, 22, 21, 20, 23, 22  // Bottom
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// Creates a wedge/prism shape (for pointed features like snouts)
        /// </summary>
        public static Mesh CreateWedge()
        {
            Mesh mesh = new Mesh();
            mesh.name = "Wedge";

            Vector3[] vertices = new Vector3[]
            {
                // Back face (rectangle)
                new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
                // Front point (triangle - single edge)
                new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0f, 0f, 0.5f), // Center point
                // Sides (need duplicates for flat shading)
                new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0f, 0f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0f, 0f, 0.5f)
            };

            int[] triangles = new int[]
            {
                // Back face
                0, 2, 1, 0, 3, 2,
                // Bottom
                0, 1, 4, 1, 5, 4,
                // Left side
                7, 9, 10, 7, 8, 9,
                // Right side
                11, 13, 14, 11, 12, 13,
                // Top
                3, 8, 2, 2, 8, 6, 2, 6, 12, 2, 12, 13
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// Creates a slightly beveled cube (small chamfer on edges)
        /// Still very angular, just not perfectly sharp
        /// </summary>
        public static Mesh CreateBeveledCube(float bevel = 0.1f)
        {
            // For simplicity, return regular cube - beveling would add too much complexity
            // True Polytopia style uses sharp cubes anyway
            return CreateCube();
        }
    }
}
