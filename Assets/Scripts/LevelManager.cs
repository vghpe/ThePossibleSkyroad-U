using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("PNG (Texture2D) for level data (ensure Read/Write is enabled)")]
    public Texture2D levelMap;

    [Tooltip("Number of pixels to offset on the forward (Z) axis before placing content.")]
    public int xOffset = 10;

    [Tooltip("Size of each cube in world units (assumed uniform in X, Y, Z).")]
    public float cubeSize = 1f;

    [Header("Prefabs")]
    public GameObject platformPrefab;      // Visual prefab for floor/platform (for black pixels)
    public GameObject groundPrefab;        // Unique floor prefab for blue pixels
    public GameObject spikePrefab;         // Visual prefab for spikes
    public GameObject lavaPrefab;          // Visual prefab for lava

    [Header("Color Threshold Settings")]
    [Tooltip("Tolerance when comparing colors.")]
    public float tolerance = 0.1f;

    private Color blackThreshold = new Color(0.2f, 0.2f, 0.2f);
    private Color blueThreshold = new Color(0.2f, 0.2f, 0.7f);
    private Color redThreshold = new Color(0.7f, 0.3f, 0.3f);
    private Color yellowThreshold = new Color(0.7f, 0.7f, 0.3f);

    [ContextMenu("Generate Level")]
    public void GenerateLevel()
    {
        if (levelMap == null)
        {
            Debug.LogError("Level map texture not assigned!");
            return;
        }

        ClearLevel();

        // Create parent objects for better hierarchy organization
        GameObject geometryParent = new GameObject("Geometry");
        geometryParent.transform.parent = transform;

        GameObject collidersParent = new GameObject("Colliders");
        collidersParent.transform.parent = transform;

        int width = levelMap.width;
        int height = levelMap.height;

        for (int y = 0; y < height; y++)
        {
            int x = 0;
            while (x < width)
            {
                Color pixel = levelMap.GetPixel(x, y);

                if (IsBlack(pixel))
                {
                    int segmentStart = x;

                    while (x < width && IsBlack(levelMap.GetPixel(x, y)))
                    {
                        Vector3 pos = new Vector3(0, y * cubeSize + cubeSize * 0.5f, (x - xOffset) * cubeSize);
                        GameObject cube = Instantiate(platformPrefab, pos, Quaternion.identity, geometryParent.transform);

                        Collider col = cube.GetComponent<Collider>();
                        if (col != null)
                        {
#if UNITY_EDITOR
                            DestroyImmediate(col);
#else
                            Destroy(col);
#endif
                        }
                        x++;
                    }

                    int segmentEnd = x - 1;
                    int segmentLength = segmentEnd - segmentStart + 1;

                    GameObject mergedColliderGO = new GameObject($"MergedPlatformCollider_Black_{segmentStart}_{y}");
                    mergedColliderGO.transform.parent = collidersParent.transform;

                    float centerX = 0;
                    float centerY = y * cubeSize + cubeSize * 0.5f;
                    float centerZ = (((segmentStart + segmentEnd) / 2f) - xOffset) * cubeSize;
                    mergedColliderGO.transform.position = new Vector3(centerX, centerY, centerZ);

                    BoxCollider boxCol = mergedColliderGO.AddComponent<BoxCollider>();
                    boxCol.size = new Vector3(cubeSize, cubeSize, segmentLength * cubeSize);

                    mergedColliderGO.layer = LayerMask.NameToLayer("Ground");
                    mergedColliderGO.tag = "Platform";
                }
                else if (IsBlue(pixel))
                {
                    int segmentStart = x;

                    while (x < width && IsBlue(levelMap.GetPixel(x, y)))
                    {
                        Vector3 pos = new Vector3(0, y * cubeSize + cubeSize * 0.5f, (x - xOffset) * cubeSize);
                        GameObject cube = Instantiate(groundPrefab, pos, Quaternion.identity, geometryParent.transform);

                        Collider col = cube.GetComponent<Collider>();
                        if (col != null)
                        {
#if UNITY_EDITOR
                            DestroyImmediate(col);
#else
                            Destroy(col);
#endif
                        }
                        x++;
                    }

                    int segmentEnd = x - 1;
                    int segmentLength = segmentEnd - segmentStart + 1;

                    GameObject mergedColliderGO = new GameObject($"MergedPlatformCollider_Blue_{segmentStart}_{y}");
                    mergedColliderGO.transform.parent = collidersParent.transform;

                    float centerX = 0;
                    float centerY = y * cubeSize + cubeSize * 0.5f;
                    float centerZ = (((segmentStart + segmentEnd) / 2f) - xOffset) * cubeSize;
                    mergedColliderGO.transform.position = new Vector3(centerX, centerY, centerZ);

                    BoxCollider boxCol = mergedColliderGO.AddComponent<BoxCollider>();
                    boxCol.size = new Vector3(cubeSize, cubeSize, segmentLength * cubeSize);

                    mergedColliderGO.layer = LayerMask.NameToLayer("Ground");
                    mergedColliderGO.tag = "Platform";
                }
                else
                {
                    if (IsRed(pixel))
                    {
                        Vector3 pos = new Vector3(0, y * cubeSize + cubeSize * 0.5f, (x - xOffset) * cubeSize);
                        Instantiate(spikePrefab, pos, Quaternion.identity, geometryParent.transform);
                    }
                    else if (IsYellow(pixel))
                    {
                        Vector3 pos = new Vector3(0, y * cubeSize + cubeSize * 0.5f, (x - xOffset) * cubeSize);
                        Instantiate(lavaPrefab, pos, Quaternion.identity, geometryParent.transform);
                    }
                    x++;
                }
            }
        }
    }

    [ContextMenu("Clear Level")]
    public void ClearLevel()
    {
#if UNITY_EDITOR
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
#else
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
#endif
    }

    bool IsBlack(Color color) => color.r < (blackThreshold.r + tolerance) &&
                                 color.g < (blackThreshold.g + tolerance) &&
                                 color.b < (blackThreshold.b + tolerance);

    bool IsBlue(Color color) => color.b > (blueThreshold.b - tolerance) &&
                                color.r < (blueThreshold.r + tolerance) &&
                                color.g < (blueThreshold.g + tolerance);

    bool IsRed(Color color) => color.r > (redThreshold.r - tolerance) &&
                               color.g < (redThreshold.g + tolerance) &&
                               color.b < (redThreshold.b + tolerance);

    bool IsYellow(Color color) => color.r > (yellowThreshold.r - tolerance) &&
                                  color.g > (yellowThreshold.g - tolerance) &&
                                  color.b < (yellowThreshold.b + tolerance);
}
