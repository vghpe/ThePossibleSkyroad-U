using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float rotationSpeed = 360f; // Degrees per second for the visual effect

    private Rigidbody rb;
    private Transform boxTransform;
    private bool isJumping = false;
    private float rotationProgress = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxTransform = transform.GetChild(0);
    }

    private void Update()
    {
        // Only move or accept input if the game is in Playing state
        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            // Move forward (z direction as example)
            transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

            // Jump on Space (or any key)
            if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
            {
                // Trigger jump physics
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);

                // Start visual rotation effect
                isJumping = true;
                rotationProgress = 0f;
            }

            // Debug test: Press K to simulate death
            if (Input.GetKeyDown(KeyCode.K))
            {
                GameManager.Instance.OnPlayerDeath();
            }
        }

        // Handle rotation effect during jump
        if (isJumping)
        {
            RotateBoxForward();
        }
    }

    private void RotateBoxForward()
    {
        // Calculate rotation increment
        float rotationAmount = rotationSpeed * Time.deltaTime;
        rotationProgress += rotationAmount;

        // Rotate the box around its local X axis
        boxTransform.Rotate(rotationAmount, 0, 0, Space.Self);

        // Stop rotation after 180 degrees
        if (rotationProgress >= 180f)
        {
            isJumping = false;
            rotationProgress = 0f;

            // Snap to exact 180 degrees to avoid precision errors
            boxTransform.localRotation = Quaternion.Euler(180f, 0, 0);
        }
    }

    public void ResetPlayer()
    {
        // Example reset: Place at origin and clear velocity
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reset box rotation
        if (boxTransform != null)
        {
            boxTransform.localRotation = Quaternion.identity;
        }
    }

    // Example collision detection for future
    private void OnCollisionEnter(Collision collision)
    {
        // If we collide with something lethal (spike), call death
        if (collision.collider.tag == "Spike")
        {
            GameManager.Instance.OnPlayerDeath();
        }
    }
}
