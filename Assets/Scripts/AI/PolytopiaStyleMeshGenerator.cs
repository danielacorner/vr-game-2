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
                0, 1, 2, 0, 2, 3,       // Front (clockwise - correct for Unity)
                4, 5, 6, 4, 6, 7,       // Back
                8, 9, 10, 8, 10, 11,    // Left
                12, 13, 14, 12, 14, 15, // Right
                16, 17, 18, 16, 18, 19, // Top
                20, 21, 22, 20, 22, 23  // Bottom
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
                0, 1, 2, 0, 2, 3,       // Front (clockwise - correct for Unity)
                4, 5, 6, 4, 6, 7,       // Back
                8, 9, 10, 8, 10, 11,    // Left
                12, 13, 14, 12, 14, 15, // Right
                16, 17, 18, 16, 18, 19, // Top
                20, 21, 22, 20, 22, 23  // Bottom
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
                // Back face (reversed winding)
                0, 1, 2, 0, 2, 3,
                // Bottom (reversed winding)
                0, 4, 1, 1, 4, 5,
                // Left side (reversed winding)
                7, 10, 9, 7, 9, 8,
                // Right side (reversed winding)
                11, 14, 13, 11, 13, 12,
                // Top (reversed winding)
                3, 2, 8, 2, 6, 8, 2, 12, 6, 2, 13, 12
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
