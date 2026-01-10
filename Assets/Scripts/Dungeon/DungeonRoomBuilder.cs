using UnityEngine;
using System.Collections.Generic;

namespace VRDungeonCrawler.Dungeon
{
    /// <summary>
    /// Procedurally builds dungeon rooms with Polytopia-style low-poly polygons
    /// Ancient Dungeon VR aesthetic with Zelda-style classic dungeon feel
    /// Uses faceted geometry for clean polygon look
    /// </summary>
    public static class DungeonRoomBuilder
    {
        // Color palette - Polytopia-inspired muted stone colors
        public static readonly Color STONE_WALL = new Color(0.45f, 0.45f, 0.5f); // Blue-gray stone
        public static readonly Color STONE_FLOOR = new Color(0.35f, 0.35f, 0.38f); // Darker floor
        public static readonly Color STONE_PILLAR = new Color(0.5f, 0.48f, 0.52f); // Lighter pillars
        public static readonly Color STONE_ACCENT = new Color(0.6f, 0.55f, 0.5f); // Warm accent
        public static readonly Color TORCH_FIRE = new Color(1f, 0.6f, 0.2f); // Orange fire

        // Grid unit size (Ancient Dungeon VR uses 2m grid)
        public const float GRID_SIZE = 2f;
        public const float WALL_HEIGHT = 4f;
        public const float WALL_THICKNESS = 0.4f;

        /// <summary>
        /// Creates a rectangular room with low-poly stone walls
        /// </summary>
        public static GameObject CreateRoom(string name, int widthInGrids, int lengthInGrids, Transform parent = null)
        {
            GameObject room = new GameObject(name);
            if (parent != null)
                room.transform.SetParent(parent);

            room.transform.position = Vector3.zero;

            // Create floor
            GameObject floor = CreateFloor(widthInGrids, lengthInGrids);
            floor.transform.SetParent(room.transform);
            floor.name = "Floor";

            // Create walls
            GameObject walls = new GameObject("Walls");
            walls.transform.SetParent(room.transform);

            // North wall
            GameObject northWall = CreateWall(widthInGrids * GRID_SIZE, WALL_HEIGHT, WALL_THICKNESS);
            northWall.transform.SetParent(walls.transform);
            northWall.transform.localPosition = new Vector3(0f, WALL_HEIGHT / 2f, lengthInGrids * GRID_SIZE / 2f);
            northWall.name = "NorthWall";

            // South wall
            GameObject southWall = CreateWall(widthInGrids * GRID_SIZE, WALL_HEIGHT, WALL_THICKNESS);
            southWall.transform.SetParent(walls.transform);
            southWall.transform.localPosition = new Vector3(0f, WALL_HEIGHT / 2f, -lengthInGrids * GRID_SIZE / 2f);
            southWall.name = "SouthWall";

            // East wall
            GameObject eastWall = CreateWall(WALL_THICKNESS, WALL_HEIGHT, lengthInGrids * GRID_SIZE);
            eastWall.transform.SetParent(walls.transform);
            eastWall.transform.localPosition = new Vector3(widthInGrids * GRID_SIZE / 2f, WALL_HEIGHT / 2f, 0f);
            eastWall.name = "EastWall";

            // West wall
            GameObject westWall = CreateWall(WALL_THICKNESS, WALL_HEIGHT, lengthInGrids * GRID_SIZE);
            westWall.transform.SetParent(walls.transform);
            westWall.transform.localPosition = new Vector3(-widthInGrids * GRID_SIZE / 2f, WALL_HEIGHT / 2f, 0f);
            westWall.name = "WestWall";

            return room;
        }

        /// <summary>
        /// Creates a low-poly faceted floor with grid pattern
        /// </summary>
        public static GameObject CreateFloor(int widthInGrids, int lengthInGrids)
        {
            GameObject floorParent = new GameObject("Floor");

            // Create individual floor tiles for Zelda-style grid
            for (int x = 0; x < widthInGrids; x++)
            {
                for (int z = 0; z < lengthInGrids; z++)
                {
                    GameObject tile = CreateFloorTile();
                    tile.transform.SetParent(floorParent.transform);

                    Vector3 position = new Vector3(
                        (x - widthInGrids / 2f) * GRID_SIZE + GRID_SIZE / 2f,
                        0f,
                        (z - lengthInGrids / 2f) * GRID_SIZE + GRID_SIZE / 2f
                    );
                    tile.transform.localPosition = position;
                    tile.name = $"FloorTile_{x}_{z}";

                    // Slight random height variation for interest
                    float heightVariation = Random.Range(-0.02f, 0.02f);
                    tile.transform.localPosition += new Vector3(0f, heightVariation, 0f);
                }
            }

            return floorParent;
        }

        /// <summary>
        /// Creates a single low-poly floor tile
        /// </summary>
        public static GameObject CreateFloorTile()
        {
            GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.transform.localScale = new Vector3(GRID_SIZE * 0.95f, 0.1f, GRID_SIZE * 0.95f);

            // Apply stone floor material
            Material mat = CreatePolytopiaStone(STONE_FLOOR);
            tile.GetComponent<Renderer>().material = mat;

            // Add slight color variation
            Color variation = new Color(
                Random.Range(-0.05f, 0.05f),
                Random.Range(-0.05f, 0.05f),
                Random.Range(-0.05f, 0.05f)
            );
            mat.color += variation;

            return tile;
        }

        /// <summary>
        /// Creates a low-poly wall segment
        /// </summary>
        public static GameObject CreateWall(float width, float height, float thickness)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.localScale = new Vector3(width, height, thickness);

            Material mat = CreatePolytopiaStone(STONE_WALL);
            wall.GetComponent<Renderer>().material = mat;

            return wall;
        }

        /// <summary>
        /// Creates a Zelda-style stone pillar
        /// </summary>
        public static GameObject CreatePillar(float height = WALL_HEIGHT)
        {
            GameObject pillar = new GameObject("Pillar");

            // Base
            GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseObj.name = "Base";
            baseObj.transform.SetParent(pillar.transform);
            baseObj.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            baseObj.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);
            baseObj.GetComponent<Renderer>().material = CreatePolytopiaStone(STONE_PILLAR);

            // Shaft (multiple segments for faceted look)
            int segments = 3;
            float segmentHeight = (height - 0.8f) / segments;

            for (int i = 0; i < segments; i++)
            {
                GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = $"Shaft_{i}";
                segment.transform.SetParent(pillar.transform);
                segment.transform.localPosition = new Vector3(
                    0f,
                    0.4f + i * segmentHeight + segmentHeight / 2f,
                    0f
                );

                // Slight taper
                float scale = 0.5f - (i * 0.02f);
                segment.transform.localScale = new Vector3(scale, segmentHeight, scale);

                // Alternate colors slightly
                Color color = (i % 2 == 0) ? STONE_PILLAR : STONE_ACCENT;
                segment.GetComponent<Renderer>().material = CreatePolytopiaStone(color);
            }

            // Capital (top)
            GameObject capital = GameObject.CreatePrimitive(PrimitiveType.Cube);
            capital.name = "Capital";
            capital.transform.SetParent(pillar.transform);
            capital.transform.localPosition = new Vector3(0f, height - 0.2f, 0f);
            capital.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);
            capital.GetComponent<Renderer>().material = CreatePolytopiaStone(STONE_ACCENT);

            return pillar;
        }

        /// <summary>
        /// Creates a wall-mounted torch with low-poly fire
        /// </summary>
        public static GameObject CreateWallTorch()
        {
            GameObject torch = new GameObject("WallTorch");

            // Bracket
            GameObject bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bracket.name = "Bracket";
            bracket.transform.SetParent(torch.transform);
            bracket.transform.localPosition = new Vector3(0f, 0f, 0.2f);
            bracket.transform.localScale = new Vector3(0.1f, 0.3f, 0.3f);

            Material bracketMat = CreatePolytopiaStone(new Color(0.3f, 0.25f, 0.2f));
            bracketMat.SetFloat("_Metallic", 0.5f);
            bracket.GetComponent<Renderer>().material = bracketMat;

            // Torch body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "TorchBody";
            body.transform.SetParent(torch.transform);
            body.transform.localPosition = new Vector3(0f, 0f, 0.4f);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            body.transform.localScale = new Vector3(0.08f, 0.25f, 0.08f);

            Material bodyMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            bodyMat.color = new Color(0.4f, 0.3f, 0.2f);
            body.GetComponent<Renderer>().material = bodyMat;

            // Fire (low-poly pyramid)
            GameObject fire = CreateLowPolyFire();
            fire.transform.SetParent(torch.transform);
            fire.transform.localPosition = new Vector3(0f, 0.3f, 0.4f);

            // Light
            GameObject lightObj = new GameObject("TorchLight");
            lightObj.transform.SetParent(torch.transform);
            lightObj.transform.localPosition = new Vector3(0f, 0.3f, 0.4f);

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = TORCH_FIRE;
            light.intensity = 2f;
            light.range = 8f;
            light.shadows = LightShadows.Soft;

            return torch;
        }

        /// <summary>
        /// Creates low-poly fire effect
        /// </summary>
        static GameObject CreateLowPolyFire()
        {
            GameObject fire = new GameObject("Fire");

            // Create 3 overlapping pyramids for faceted fire look
            for (int i = 0; i < 3; i++)
            {
                GameObject pyramid = CreatePyramid();
                pyramid.transform.SetParent(fire.transform);
                pyramid.transform.localPosition = Vector3.zero;
                pyramid.transform.localRotation = Quaternion.Euler(0f, i * 120f, 0f);

                float scale = 0.2f + i * 0.05f;
                pyramid.transform.localScale = Vector3.one * scale;

                // Gradient from orange to yellow
                Color fireColor = Color.Lerp(TORCH_FIRE, Color.yellow, i / 3f);
                Material fireMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                fireMat.color = fireColor;
                fireMat.EnableKeyword("_EMISSION");
                fireMat.SetColor("_EmissionColor", fireColor * 2f);

                pyramid.GetComponent<Renderer>().material = fireMat;

                // Remove collider
                Object.Destroy(pyramid.GetComponent<Collider>());
            }

            return fire;
        }

        /// <summary>
        /// Creates a pyramid mesh (for fire)
        /// </summary>
        static GameObject CreatePyramid()
        {
            GameObject pyramid = new GameObject("Pyramid");
            MeshFilter meshFilter = pyramid.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = pyramid.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();

            // Vertices
            Vector3[] vertices = new Vector3[]
            {
                // Base (square)
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0f, 0.5f),
                new Vector3(-0.5f, 0f, 0.5f),
                // Apex
                new Vector3(0f, 1f, 0f)
            };

            // Triangles (faceted - each face separate)
            int[] triangles = new int[]
            {
                // Base
                0, 2, 1,
                0, 3, 2,
                // Sides
                0, 1, 4,
                1, 2, 4,
                2, 3, 4,
                3, 0, 4
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;

            return pyramid;
        }

        /// <summary>
        /// Creates a doorway opening in a wall
        /// </summary>
        public static GameObject CreateDoorway(float width = 2f, float height = 3f)
        {
            GameObject doorway = new GameObject("Doorway");

            // Left pillar
            GameObject leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftPillar.name = "LeftPillar";
            leftPillar.transform.SetParent(doorway.transform);
            leftPillar.transform.localPosition = new Vector3(-width / 2f, height / 2f, 0f);
            leftPillar.transform.localScale = new Vector3(WALL_THICKNESS, height, WALL_THICKNESS);
            leftPillar.GetComponent<Renderer>().material = CreatePolytopiaStone(STONE_ACCENT);

            // Right pillar
            GameObject rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightPillar.name = "RightPillar";
            rightPillar.transform.SetParent(doorway.transform);
            rightPillar.transform.localPosition = new Vector3(width / 2f, height / 2f, 0f);
            rightPillar.transform.localScale = new Vector3(WALL_THICKNESS, height, WALL_THICKNESS);
            rightPillar.GetComponent<Renderer>().material = CreatePolytopiaStone(STONE_ACCENT);

            // Lintel (top piece)
            GameObject lintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lintel.name = "Lintel";
            lintel.transform.SetParent(doorway.transform);
            lintel.transform.localPosition = new Vector3(0f, height, 0f);
            lintel.transform.localScale = new Vector3(width + WALL_THICKNESS * 2f, 0.4f, WALL_THICKNESS);
            lintel.GetComponent<Renderer>().material = CreatePolytopiaStone(STONE_ACCENT);

            return doorway;
        }

        /// <summary>
        /// Creates Polytopia-style stone material (low-poly, faceted)
        /// </summary>
        public static Material CreatePolytopiaStone(Color baseColor)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = baseColor;
            mat.SetFloat("_Smoothness", 0.1f); // Very rough stone
            mat.SetFloat("_Metallic", 0f);

            return mat;
        }

        /// <summary>
        /// Adds a ceiling to a room
        /// </summary>
        public static GameObject CreateCeiling(int widthInGrids, int lengthInGrids, float height = WALL_HEIGHT)
        {
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";

            float width = widthInGrids * GRID_SIZE;
            float length = lengthInGrids * GRID_SIZE;

            ceiling.transform.localPosition = new Vector3(0f, height, 0f);
            ceiling.transform.localScale = new Vector3(width, 0.2f, length);

            Material mat = CreatePolytopiaStone(new Color(0.3f, 0.3f, 0.32f)); // Darker ceiling
            ceiling.GetComponent<Renderer>().material = mat;

            return ceiling;
        }
    }
}
