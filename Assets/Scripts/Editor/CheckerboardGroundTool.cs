using UnityEngine;
using UnityEditor;

public class CheckerboardGroundTool : EditorWindow
{
    private int width = 5;
    private int depth = 5;
    private float tileSize = 1f;
    private GameObject currentGround;

    [MenuItem("Tools/Checkerboard Ground Tool")]
    public static void ShowWindow()
    {
        GetWindow<CheckerboardGroundTool>("Checkerboard Ground Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Checkerboard Ground Settings", EditorStyles.boldLabel);

        width = EditorGUILayout.IntField("Width (cells)", width);
        depth = EditorGUILayout.IntField("Depth (cells)", depth);
        tileSize = EditorGUILayout.FloatField("Tile Size", tileSize);

        if (GUILayout.Button("Create Ground"))
        {
            CreateGround();
        }

        if (GUILayout.Button("Clear Ground"))
        {
            ClearGround();
        }
    }

    private void CreateGround()
    {
        ClearGround();

        currentGround = new GameObject("CheckerboardGround");
        MeshFilter meshFilter = currentGround.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = currentGround.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        // (Mesh generation code remains the same)

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        // Add MeshCollider and assign the mesh
        MeshCollider meshCollider = currentGround.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        // Generate vertices and UVs
        Vector3[] vertices = new Vector3[(width + 1) * (depth + 1)];
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                int index = z * (width + 1) + x;

                // Position in world space
                vertices[index] = new Vector3(x * tileSize, 0, z * tileSize);

                // UV = (worldX, worldZ); we rely on material.mainTextureScale to repeat
                uvs[index] = new Vector2(x * tileSize, z * tileSize);
            }
        }

        // Offset the ground so its center is at (0,0) in XZ-plane
        Vector3 offset = new Vector3(-width * tileSize / 2f, 0, -depth * tileSize / 2f);
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] += offset;
        }

        // Generate triangles
        int[] triangles = new int[width * depth * 6];
        int t = 0;
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int topLeft = z * (width + 1) + x;
                int topRight = topLeft + 1;
                int bottomLeft = topLeft + (width + 1);
                int bottomRight = bottomLeft + 1;

                triangles[t++] = topLeft;
                triangles[t++] = bottomLeft;
                triangles[t++] = topRight;

                triangles[t++] = topRight;
                triangles[t++] = bottomLeft;
                triangles[t++] = bottomRight;
            }
        }

        // Assign to mesh
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        // Create and assign the checkerboard material
        Material checkerMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        checkerMaterial.mainTexture = GenerateCheckerTexture();

        // --- KEY PART: Ensure each square is 1 unit in world space. ---
        // Because our UV is in worldSpace units, setting the texture scale to (1/tileSize, 1/tileSize)
        // will make 1 world unit = 1 texture tile repeat. Since the generated checker is a 2x2 pattern
        // in the 0..1 UV space, repeating it across each 1 unit keeps the squares uniform.
        checkerMaterial.mainTextureScale = new Vector2(1f / tileSize, 1f / tileSize);

        meshRenderer.material = checkerMaterial;

        // Optional: reposition so that the "bottom" is at z=0 if you need that.
        // Currently we center the ground at (0,0). If you prefer the plane’s bottom edge at z=0, you can do:
        // currentGround.transform.position = new Vector3(0, 0, depth * tileSize / 2f);
        
        // Assign tag to ground
        currentGround.tag = "Ground";
    }

    private void ClearGround()
    {
        if (currentGround != null)
        {
            DestroyImmediate(currentGround);
        }
    }

    private Texture2D GenerateCheckerTexture()
    {
        // We only need a small texture that has 2x2 squares in UV space
        int textureSize = 64;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);

        // cellSize = half the texture so we get 2 cells across and 2 cells down
        int cellSize = textureSize / 2;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                int cellX = x / cellSize;
                int cellY = y / cellSize;

                bool isWhite = (cellX + cellY) % 2 == 0;
                texture.SetPixel(x, y, isWhite ? Color.white : Color.black);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.Apply();

        return texture;
    }
}
