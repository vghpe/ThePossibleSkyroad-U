using UnityEngine;

[ExecuteAlways]
public class SimpleThirdPersonCamera : MonoBehaviour
{
    public Transform player;             // The player the camera will follow
    [Range(-90f, 90f)]
    public float pitchAngle = 30f;       // The angle (in degrees) the camera looks down at the player
    public float distance = 10f;         // Distance from the player
    public float rotationOffsetX = 0f;   // Additional rotation adjustment on the X-axis
    public bool previewInEditor = true; // Toggle for previewing in editor

    public float boundsHeight = 6f;      // Fixed height of the vertical bounds
    public float verticalFollowSpeed = 5f; // Speed for vertical movement adjustment

    public float boundsCenterY;         // Center of the current bounding box
    private float currentYPosition;      // Current vertical position of the camera

    private void Start()
    {
        // Initialize the bounds center and camera position to match the player's initial position
        if (player != null)
        {
            boundsCenterY = player.position.y;
            currentYPosition = boundsCenterY;
        }
    }

    private void Update()
    {
        if (player != null)
        {
            UpdateCameraPosition();
        }
    }

    private void OnValidate()
    {
        if (previewInEditor && player != null)
        {
            UpdateCameraPosition();
        }
    }

    private void UpdateCameraPosition()
    {
        // Calculate the upper and lower bounds based on the boundsCenterY
        float lowerBound = boundsCenterY - boundsHeight / 2f;
        float upperBound = boundsCenterY + boundsHeight / 2f;

        // Check if the player is outside the bounds
        if (player.position.y > upperBound)
        {
            // Move the bounding box up
            boundsCenterY = player.position.y - boundsHeight / 2f;
        }
        else if (player.position.y < lowerBound)
        {
            // Move the bounding box down
            boundsCenterY = player.position.y + boundsHeight / 2f;
        }

        // Smoothly adjust the camera's Y position toward the center of the bounds
        float targetY = boundsCenterY;
        currentYPosition = Mathf.Lerp(currentYPosition, targetY, Time.deltaTime * verticalFollowSpeed);

        // Calculate the camera's new position
        float radians = Mathf.Deg2Rad * pitchAngle;
        Vector3 offset = new Vector3(0, Mathf.Sin(radians) * distance, -Mathf.Cos(radians) * distance);
        transform.position = new Vector3(player.position.x, currentYPosition, player.position.z) + offset;

        // Apply rotation with pitch angle and additional offset
        transform.rotation = Quaternion.Euler(pitchAngle + rotationOffsetX, 0f, 0f);
    }

    private void OnDrawGizmos()
    {
        if (player == null)
            return;

        // Calculate the current bounds
        float lowerBound = boundsCenterY - boundsHeight / 2f;
        float upperBound = boundsCenterY + boundsHeight / 2f;

        // Define custom colors
        Color floorColor = new Color(0.2f, 0.6f, 0.2f); // Dark green
        Color ceilingColor = new Color(0.6f, 0.2f, 0.6f); // Purple

        // Draw the floor (lower bound) as a horizontal line
        Gizmos.color = floorColor;
        Gizmos.DrawLine(
            new Vector3(player.position.x - 5, lowerBound, player.position.z),
            new Vector3(player.position.x + 5, lowerBound, player.position.z)
        );

        // Draw the ceiling (upper bound) as a horizontal line
        Gizmos.color = ceilingColor;
        Gizmos.DrawLine(
            new Vector3(player.position.x - 5, upperBound, player.position.z),
            new Vector3(player.position.x + 5, upperBound, player.position.z)
        );
    }
}
