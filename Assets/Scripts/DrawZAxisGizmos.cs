using UnityEngine;

[ExecuteInEditMode]
public class DrawYAxisGizmos : MonoBehaviour
{
    [SerializeField] private float spacing = 4f;    // Distance between lines along Z-axis
    [SerializeField] private int lineCount = 10;    // Number of lines to draw
    [SerializeField] private float lineHeight = 1f; // Height of each line in the Y-axis
    [SerializeField] private Color lineColor = Color.red;

    private void OnDrawGizmos()
    {
        Gizmos.color = lineColor;

        for (int i = 0; i < lineCount; i++)
        {
            // Calculate the Z position for this line
            float zPos = i * spacing;

            // Line starts at (0, 0, zPos) and ends at (0, lineHeight, zPos)
            Vector3 startPos = new Vector3(0f, 0f, zPos);
            Vector3 endPos = new Vector3(0f, lineHeight, zPos);

            Gizmos.DrawLine(startPos, endPos);
        }
    }
}