using UnityEngine;

public class CheckerboardGround : MonoBehaviour
{
    public int width = 5;
    public int depth = 100;
    public float tileSize = 1f;

    void Start()
    {
        CreateGround();
    }

    void CreateGround()
    {
        GameObject ground = new GameObject("CheckerboardGround");
        MeshFilter meshFilter = ground.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = ground.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();

        // Generate vertices and UVs
        Vector3[] vertices = new Vector3[(width + 1) * (depth + 1)];
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                int index = z * (width + 1) + x;
                vertices[index] = new Vector3(x * tileSize, 0, z * tileSize);
                uvs[index] = new Vector2((float)x / width, (float)z / depth);
            }
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

        // Assign mesh data
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        // Create and assign the checkerboard material
        Material checkerMaterial = new Material(Shader.Find("Standard"));
        checkerMaterial.mainTexture = GenerateCheckerTexture();
        meshRenderer.material = checkerMaterial;
    }

    Texture2D GenerateCheckerTexture()
    {
        int textureSize = 64;
        Texture2D texture = new Texture2D(textureSize, textureSize);

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                bool isWhite = (x / (textureSize / width) + y / (textureSize / width)) % 2 == 0;
                texture.SetPixel(x, y, isWhite ? Color.white : Color.black);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.Apply();

        return texture;
    }
}
