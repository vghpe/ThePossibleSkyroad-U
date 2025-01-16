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
    public GameObject groundPrefab;  // Unique floor prefab for blue pixels
    public GameObject spikePrefab;         // Visual prefab for spikes
    public GameObject lavaPrefab;          // Visual prefab for lava

    [Header("Color Threshold Settings")]
    [Tooltip("Tolerance when comparing colors.")]
    public float tolerance = 0.1f;

    // Base threshold colors.
    private Color blackThreshold  = new Color(0.2f, 0.2f, 0.2f);
    private Color blueThreshold   = new Color(0.2f, 0.2f, 0.7f);
    private Color redThreshold    = new Color(0.7f, 0.3f, 0.3f);
    private Color yellowThreshold = new Color(0.7f, 0.7f, 0.3f);

    /// <summary>
    /// Generates the level from the assigned PNG.
    /// </summary>
    [ContextMenu("Generate Level")]
    public void GenerateLevel()
    {
        if (levelMap == null)
        {
            Debug.LogError("Level map texture not assigned!");
            return;
        }

        // Clear previous content (if any)
        ClearLevel();

        int width = levelMap.width;
        int height = levelMap.height;

        // Loop through each row of the PNG.
        // PNG x-coordinate maps to world Z (forward),
        // PNG y-coordinate maps to world Y (vertical),
        // and world X is kept constant (0) for a centered level.
        for (int y = 0; y < height; y++)
        {
            int x = 0;
            while (x < width)
            {
                Color pixel = levelMap.GetPixel(x, y);

                // Handle black (regular platform) pixels.
                if (IsBlack(pixel))
                {
                    // Found a contiguous segment of black pixels.
                    int segmentStart = x;

                    // Instantiate individual visual cubes for all contiguous black pixels.
                    while (x < width && IsBlack(levelMap.GetPixel(x, y)))
                    {
                        Vector3 pos = new Vector3(0, y * cubeSize + cubeSize * 0.5f, (x - xOffset) * cubeSize);
                        GameObject cube = Instantiate(platformPrefab, pos, Quaternion.identity, transform);

                        // Remove the individual collider entirely.
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

                    // Create a merged collider for this contiguous black segment.
                    int segmentEnd = x - 1;
                    int segmentLength = segmentEnd - segmentStart + 1;

                    GameObject mergedColliderGO = new GameObject("MergedPlatformCollider_Black_" + segmentStart + "_" + y);
                    mergedColliderGO.transform.parent = transform;

                    float centerX = 0;
                    float centerY = y * cubeSize + cubeSize * 0.5f;
                    float centerZ = (((segmentStart + segmentEnd) / 2f) - xOffset) * cubeSize;
                    mergedColliderGO.transform.position = new Vector3(centerX, centerY, centerZ);

                    BoxCollider boxCol = mergedColliderGO.AddComponent<BoxCollider>();
                    boxCol.size = new Vector3(cubeSize, cubeSize, segmentLength * cubeSize);
                }
                // Handle blue (unique floor) pixels.
                else if (IsBlue(pixel))
                {
                    int segmentStart = x;

                    // Instantiate individual visual cubes for contiguous blue pixels.
                    while (x < width && IsBlue(levelMap.GetPixel(x, y)))
                    {
                        Vector3 pos = new Vector3(0, y * cubeSize + cubeSize * 0.5f, (x - xOffset) * cubeSize);
                        GameObject cube = Instantiate(groundPrefab, pos, Quaternion.identity, transform);

                        // Remove its collider entirely.
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

                    GameObject mergedColliderGO = new GameObject("MergedPlatformCollider_Blue_" + segmentStart + "_" + y);
                    mergedColliderGO.transform.parent = transform;

                    float centerX = 0;
                    float centerY = y * cubeSize + cubeSize * 0.5f;
                    float centerZ = (((segmentStart + segmentEnd) / 2f) - xOffset) * cubeSize;
                    mergedColliderGO.transform.position = new Vector3(centerX, centerY, centerZ);

                    BoxCollider boxCol = mergedColliderGO.AddComponent<BoxCollider>();
                    boxCol.size = new Vector3(cubeSize, cubeSize, segmentLength * cubeSize);
                }
                else
                {
                    // Check for spikes or lava.
                    if (IsRed(pixel))
                    {
                        Vector3 pos = new Vector3(0, y * cubeSize + cubeSize * 0.5f, (x - xOffset) * cubeSize);
                        Instantiate(spikePrefab, pos, Quaternion.identity, transform);
                    }
                    else if (IsYellow(pixel))
                    {
                        Vector3 pos = new Vector3(0, y * cubeSize + cubeSize * 0.5f, (x - xOffset) * cubeSize);
                        Instantiate(lavaPrefab, pos, Quaternion.identity, transform);
                    }
                    // For any other color, simply move on.
                    x++;
                }
            }
        }
    }

    /// <summary>
    /// Clears all generated child objects (both visuals and merged colliders).
    /// </summary>
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

    /// <summary>
    /// Returns true if the color is considered "black" (platform) within the tolerance.
    /// </summary>
    bool IsBlack(Color color)
    {
        return color.r < (blackThreshold.r + tolerance) &&
               color.g < (blackThreshold.g + tolerance) &&
               color.b < (blackThreshold.b + tolerance);
    }

    /// <summary>
    /// Returns true if the color is considered "blue" (unique floor) within the tolerance.
    /// </summary>
    bool IsBlue(Color color)
    {
        return color.b > (blueThreshold.b - tolerance) &&
               color.r < (blueThreshold.r + tolerance) &&
               color.g < (blueThreshold.g + tolerance);
    }

    /// <summary>
    /// Returns true if the color is considered "red" (spike) within the tolerance.
    /// </summary>
    bool IsRed(Color color)
    {
        return color.r > (redThreshold.r - tolerance) &&
               color.g < (redThreshold.g + tolerance) &&
               color.b < (redThreshold.b + tolerance);
    }

    /// <summary>
    /// Returns true if the color is considered "yellow" (lava) within the tolerance.
    /// </summary>
    bool IsYellow(Color color)
    {
        return color.r > (yellowThreshold.r - tolerance) &&
               color.g > (yellowThreshold.g - tolerance) &&
               color.b < (yellowThreshold.b + tolerance);
    }
}
