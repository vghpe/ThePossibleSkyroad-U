using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Reset Position")] 
    public float playerProgress = 0f;
    
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
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float groundCheckDepthOffset = 0.45f;

    [Header("Readonly Runtime Debug")]
    [SerializeField] private float customGravity;
    [SerializeField] private float initialJumpVelocity;
    [SerializeField] private float jumpDistance;         // Horizontal distance for landing at same level.
    [SerializeField] private float jumpDistanceAbove;    // Horizontal distance if landing 1 unit above.
    [SerializeField] private float jumpDistanceBelow;    // Horizontal distance if landing 1 unit below.

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

        // BPM-based forward movement.
        float forwardSpeed = (bpm / 60f) * unitsPerTick;
        float distThisFrame = forwardSpeed * Time.deltaTime;
        transform.Translate(Vector3.forward * distThisFrame, Space.World);
        distanceSinceLastTick += distThisFrame;
        if (distanceSinceLastTick >= unitsPerTick)
        {
            distanceSinceLastTick -= unitsPerTick;
            Debug.Log("Tick!");
        }

        // Jump input only when grounded.
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            Jump();
        }

        // Debug: Force death.
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.Instance.OnPlayerDeath();
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            // Apply custom gravity.
            Vector3 velocity = rb.linearVelocity;
            velocity.y -= customGravity * Time.fixedDeltaTime;
            rb.linearVelocity = velocity;

            // Check if actually grounded.
            bool grounded = IsGrounded();

            if (grounded)
            {
                isJumping = false;
                // Optionally allow auto-jump if space is held.
                if (Input.GetKey(KeyCode.Space))
                {
                    Jump();
                }
            }

            // Rotate child (visual) only when in air.
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
    /// Rays are cast from two edges along the player's forward (z) axis.
    /// </summary>
    private bool IsGrounded()
    {
        Vector3 originCenter = transform.position + Vector3.up * 0.05f;
        Vector3 originFront = originCenter + transform.forward * groundCheckDepthOffset;
        Vector3 originBack = originCenter - transform.forward * groundCheckDepthOffset;

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
        transform.position = new Vector3(0, 1, playerProgress);
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        distanceSinceLastTick = 0f;
        isJumping = false;
        UpdateJumpParameters();
    }

    /// <summary>
    /// Updates the jump parameters and recalculates the read-only fields.
    /// Also calculates horizontal distances when landing on platforms 
    /// at the same level, one unit above, and one unit below.
    /// </summary>
    private void UpdateJumpParameters()
    {
        // Calculate custom gravity and initial jump velocity using symmetrical jump formulas.
        customGravity = 8f * jumpHeight / (totalJumpTime * totalJumpTime);
        initialJumpVelocity = 4f * jumpHeight / totalJumpTime;

        // Calculate forward speed (units per second) based on BPM.
        float forwardSpeed = (bpm / 60f) * unitsPerTick;

        // Jump distance for landing at same level.
        jumpDistance = forwardSpeed * totalJumpTime;

        // Calculate jump distance for landing one unit above:
        // Solve: 0.5 * customGravity * t^2 - initialJumpVelocity * t + 1 = 0  (Δy = +1)
        float discriminantAbove = initialJumpVelocity * initialJumpVelocity - 2f * customGravity * 1f;
        if (discriminantAbove >= 0f)
        {
            // Use the positive root.
            float tAbove = (initialJumpVelocity + Mathf.Sqrt(discriminantAbove)) / customGravity;
            jumpDistanceAbove = forwardSpeed * tAbove;
        }
        else
        {
            jumpDistanceAbove = 0f;  // No valid flight time if the jump cannot reach a platform 1 unit higher.
        }

        // Calculate jump distance for landing one unit below:
        // Solve: 0.5 * customGravity * t^2 - initialJumpVelocity * t - 1 = 0  (Δy = -1)
        float discriminantBelow = initialJumpVelocity * initialJumpVelocity + 2f * customGravity * 1f;
        float tBelow = (initialJumpVelocity + Mathf.Sqrt(discriminantBelow)) / customGravity;
        jumpDistanceBelow = forwardSpeed * tBelow;
    }

    /// <summary>
    /// Detects collisions and triggers death on side collisions or if hitting a spike.
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
        Vector3 originCenter = transform.position + Vector3.up * 0.05f;
        Vector3 originFront = originCenter + transform.forward * groundCheckDepthOffset;
        Vector3 originBack = originCenter - transform.forward * groundCheckDepthOffset;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(originFront, originFront + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(originBack, originBack + Vector3.down * groundCheckDistance);

        // Draw spheres at the origin points.
        Gizmos.DrawSphere(originFront, 0.02f);
        Gizmos.DrawSphere(originBack, 0.02f);
    }
}
