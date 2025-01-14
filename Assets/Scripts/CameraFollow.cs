using UnityEngine;

[ExecuteInEditMode]  // Or [ExecuteAlways]
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Position Settings")]
    public Vector3 horizontalOffset = new Vector3(0f, 0f, -5f);
    public float fixedCameraHeight = 2.0f;

    [Header("Look Settings")]
    public float lookHeightOffset = 1.0f;

    [Header("Toggle")]
    public bool ignorePlayerVerticalMotion = true;

    private void Update()
    {
        // We only want to run camera logic in Edit Mode if player reference is assigned
        if (!Application.isPlaying)
        {
            // In Edit Mode, Update() can be very frequent, so we just check if we have a player
            if (player != null)
            {
                PositionCamera();
            }
        }
        else
        {
            // During Play Mode, the usual camera logic
            if (player != null)
            {
                PositionCamera();
            }
        }
    }

    private void PositionCamera()
    {
        // 1. Compute desired position
        Vector3 desiredPosition;

        if (ignorePlayerVerticalMotion)
        {
            desiredPosition = new Vector3(
                player.position.x,
                fixedCameraHeight,
                player.position.z
            ) + horizontalOffset;
        }
        else
        {
            desiredPosition = player.position + horizontalOffset;
        }

        transform.position = desiredPosition;

        // 2. Compute look target
        Vector3 lookTarget;
        if (ignorePlayerVerticalMotion)
        {
            lookTarget = new Vector3(
                player.position.x,
                fixedCameraHeight + lookHeightOffset,
                player.position.z
            );
        }
        else
        {
            lookTarget = player.position + Vector3.up * lookHeightOffset;
        }

        transform.LookAt(lookTarget);
    }
}