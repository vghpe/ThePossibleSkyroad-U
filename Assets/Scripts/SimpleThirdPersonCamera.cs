using UnityEngine;

[ExecuteAlways]
public class SimpleThirdPersonCamera : MonoBehaviour
{
    [Header("Player & Basic Settings")]
    public Transform player;              
    [Range(-90f, 90f)]
    public float pitchAngle = 30f;        
    public float distance = 10f;          
    public float rotationOffsetX = 0f;    
    public bool previewInEditor = true;   

    [Header("Vertical Bounds Settings")]
    public float boundsHeight = 6f;        
    public float verticalFollowSpeed = 5f; // Speed of bounding box’s “center” movement
    public float boundsCenterY;            
    private float currentYPosition;        

    private float defaultBoundsCenterY;
    private float defaultBoundsHeight;

    [Header("Camera Smoothing")]
    public float cameraSmoothTime = 0.2f;  // Overall smoothing for camera position
    private Vector3 velocitySmoothDamp;    // Internal var for SmoothDamp

    private void Start()
    {
        defaultBoundsCenterY = boundsCenterY;
        defaultBoundsHeight = boundsHeight;

        if (boundsCenterY == 0f && player != null)
        {
            boundsCenterY = player.position.y;
        }
        currentYPosition = boundsCenterY;
    }

    public void ResetVerticalBounds()
    {
        boundsCenterY = defaultBoundsCenterY;
        boundsHeight = defaultBoundsHeight;
        currentYPosition = boundsCenterY;
    }

    public void ResetVerticalBounds(float defaultCenterY, float defaultBoundsH)
    {
        boundsCenterY = defaultCenterY;
        boundsHeight = defaultBoundsH;
        currentYPosition = boundsCenterY;
    }

    private void LateUpdate()
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
        // 1. Vertical bounding logic
        float halfHeight = boundsHeight / 2f;
        float lowerBound = boundsCenterY - halfHeight;
        float upperBound = boundsCenterY + halfHeight;

        float playerY = player.position.y;
        if (playerY > upperBound)
        {
            // Shift the bounding box upward
            boundsCenterY = playerY - halfHeight;
        }
        else if (playerY < lowerBound)
        {
            // Shift the bounding box downward
            boundsCenterY = playerY + halfHeight;
        }

        // Smoothly move our 'currentYPosition' to the new box center
        float targetY = boundsCenterY;
        currentYPosition = Mathf.Lerp(currentYPosition, targetY, Time.deltaTime * verticalFollowSpeed);

        // 2. Compute raw target position (using pitch/distance)
        float radians = Mathf.Deg2Rad * pitchAngle;
        Vector3 offset = new Vector3
        (
            0f,
            Mathf.Sin(radians) * distance,
            -Mathf.Cos(radians) * distance
        );

        // The final desired spot
        Vector3 desiredPos = new Vector3(player.position.x, currentYPosition, player.position.z) + offset;

        // 3. SmoothDamp the camera from current position to 'desiredPos'
        //    This smooths X, Y, and Z in one pass, reducing jitter
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref velocitySmoothDamp,
            cameraSmoothTime
        );

        // 4. Handle rotation
        transform.rotation = Quaternion.Euler(pitchAngle + rotationOffsetX, 0f, 0f);
    }

    private void OnDrawGizmos()
    {
        if (player == null)
            return;

        float halfHeight = boundsHeight / 2f;
        float lowerBound = boundsCenterY - halfHeight;
        float upperBound = boundsCenterY + halfHeight;

        Color floorColor = new Color(0.2f, 0.6f, 0.2f);  
        Color ceilingColor = new Color(0.6f, 0.2f, 0.6f);

        // Draw bounding lines in the Scene view
        Gizmos.color = floorColor;
        Gizmos.DrawLine(
            new Vector3(player.position.x - 5, lowerBound, player.position.z),
            new Vector3(player.position.x + 5, lowerBound, player.position.z)
        );

        Gizmos.color = ceilingColor;
        Gizmos.DrawLine(
            new Vector3(player.position.x - 5, upperBound, player.position.z),
            new Vector3(player.position.x + 5, upperBound, player.position.z)
        );
    }
}
