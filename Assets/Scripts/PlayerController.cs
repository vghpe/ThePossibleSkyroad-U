using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("BPM-Based Movement")]
    [Range(30f, 300f)]
    public float bpm = 120f;
    [Range(0.1f, 10f)]
    public float unitsPerTick = 4f;

    [Header("Symmetrical Jump Settings")]
    [Range(0.5f, 5f)]
    public float jumpHeight = 2f;
    [Range(0.2f, 3f)]
    public float totalJumpTime = 1f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 100f; // Rotation speed in degrees per second

    [Header("Ground Check Settings")]
    [SerializeField] private LayerMask groundLayer;
    // For a pivot-at-bottom approach, a small distance is enough.
    [SerializeField] private float groundCheckDistance = 0.1f;
    // Offset along the player's forward (z) axis for the dual raycasts.
    // Adjust this based on your cube's depth (front-back) size.
    [SerializeField] private float groundCheckDepthOffset = 0.45f;

    [Header("Readonly Runtime Debug")]
    [SerializeField] private float customGravity;
    [SerializeField] private float initialJumpVelocity;
    [SerializeField] private float jumpDistance; // The horizontal distance traveled during a jump

    private Rigidbody rb;
    private Transform firstChild;
    private bool isJumping = false;
    private float distanceSinceLastTick = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (transform.childCount > 0)
        {
            firstChild = transform.GetChild(0);
        }
    }

    private void Start()
    {
        UpdateJumpParameters();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateJumpParameters();
        }
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing)
            return;

        // BPM-based forward movement
        float forwardSpeed = (bpm / 60f) * unitsPerTick;
        float distThisFrame = forwardSpeed * Time.deltaTime;
        transform.Translate(Vector3.forward * distThisFrame, Space.World);
        distanceSinceLastTick += distThisFrame;
        if (distanceSinceLastTick >= unitsPerTick)
        {
            distanceSinceLastTick -= unitsPerTick;
            Debug.Log("Tick!");
        }

        // Jump input only when grounded
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            Jump();
        }

        // Debug: Force death
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.Instance.OnPlayerDeath();
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            // Apply custom gravity
            Vector3 velocity = rb.linearVelocity;
            velocity.y -= customGravity * Time.fixedDeltaTime;
            rb.linearVelocity = velocity;

            // Check if actually grounded
            bool grounded = IsGrounded();

            if (grounded)
            {
                isJumping = false;
                // Optionally allow auto-jump if space is held
                if (Input.GetKey(KeyCode.Space))
                {
                    Jump();
                }
            }

            // Rotate child (visual) only when in air
            if (firstChild != null)
            {
                if (!grounded)
                {
                    firstChild.Rotate(Vector3.right * rotationSpeed * Time.fixedDeltaTime);
                }
                else
                {
                    firstChild.localRotation = Quaternion.identity;
                }
            }
        }
    }

    /// <summary>
    /// Uses two raycasts to determine if the player is on the ground.
    /// Rays are cast from the two edges along the player's forward (z) axis with a small upward offset.
    /// </summary>
    private bool IsGrounded()
    {
        // A small upward offset ensures we don't start the ray exactly at the collider's edge.
        Vector3 originCenter = transform.position + Vector3.up * 0.05f;
        
        // Calculate the two raycast origins using the depth offset along the player's local z axis.
        Vector3 originFront = originCenter + transform.forward * groundCheckDepthOffset;
        Vector3 originBack = originCenter - transform.forward * groundCheckDepthOffset;

        // Cast two rays downward from the front and back origins.
        bool hitFront = Physics.Raycast(originFront, Vector3.down, groundCheckDistance, groundLayer);
        bool hitBack  = Physics.Raycast(originBack, Vector3.down, groundCheckDistance, groundLayer);

        return hitFront || hitBack;
    }

    /// <summary>
    /// Executes a jump by setting the upward velocity.
    /// </summary>
    private void Jump()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = initialJumpVelocity;
        rb.linearVelocity = velocity;
        isJumping = true;
    }

    public void ResetPlayer()
    {
        transform.position = new Vector3(0, 1, 0);
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        distanceSinceLastTick = 0f;
        isJumping = false;
        UpdateJumpParameters();
    }

    /// <summary>
    /// Updates the jump parameters and recalculates the read-only fields.
    /// </summary>
    private void UpdateJumpParameters()
    {
        // Using a symmetrical jump formula:
        // customGravity = 8 * jumpHeight / (totalJumpTime^2)
        // initialJumpVelocity = 4 * jumpHeight / totalJumpTime
        customGravity = 8f * jumpHeight / (totalJumpTime * totalJumpTime);
        initialJumpVelocity = 4f * jumpHeight / totalJumpTime;

        // Calculate forward speed (units per second) based on BPM-based movement.
        float forwardSpeed = (bpm / 60f) * unitsPerTick;
        // Calculate how far the player will travel in the air during a jump.
        jumpDistance = forwardSpeed * totalJumpTime;
    }

    /// <summary>
    /// Detects collisions and triggers death on side collisions or if hitting a spike.
    /// Uses an angle check for platform collisions.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Spike"))
        {
            GameManager.Instance.OnPlayerDeath();
            return;
        }

        if (collision.collider.CompareTag("Platform"))
        {
            bool sideHit = false;
            const float sideAngleThreshold = 60f;
            foreach (ContactPoint contact in collision.contacts)
            {
                float angle = Vector3.Angle(contact.normal, Vector3.up);
                if (angle > sideAngleThreshold)
                {
                    sideHit = true;
                    break;
                }
            }
            if (sideHit)
            {
                GameManager.Instance.OnPlayerDeath();
            }
        }
    }

    /// <summary>
    /// Draws debug gizmos for the dual raycasts used in the ground check.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Compute the common origin with an upward offset.
        Vector3 originCenter = transform.position + Vector3.up * 0.05f;
        // Calculate front and back origins based on the z (forward) axis offset.
        Vector3 originFront = originCenter + transform.forward * groundCheckDepthOffset;
        Vector3 originBack = originCenter - transform.forward * groundCheckDepthOffset;

        // Set the Gizmo color for the ground check rays.
        Gizmos.color = Color.green;

        // Draw the front ray.
        Gizmos.DrawLine(originFront, originFront + Vector3.down * groundCheckDistance);
        // Draw the back ray.
        Gizmos.DrawLine(originBack, originBack + Vector3.down * groundCheckDistance);

        // Optionally, draw the origin points to see where they are.
        Gizmos.DrawSphere(originFront, 0.02f);
        Gizmos.DrawSphere(originBack, 0.02f);
    }
}
