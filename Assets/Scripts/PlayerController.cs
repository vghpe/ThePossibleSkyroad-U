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

    [Header("Readonly Runtime Debug")]
    [SerializeField] private float customGravity;
    [SerializeField] private float initialJumpVelocity;

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
        if (Input.GetKeyDown(KeyCode.K))
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
    /// Uses a raycast to determine if the player is on the ground.
    /// With pivot-at-bottom, cast from transform.position with a small upward offset.
    /// </summary>
    private bool IsGrounded()
    {
        // Since the pivot is at the bottom, we start at transform.position.
        // Add a small offset upward so the ray starts inside the collider rather than exactly at the edge.
        Vector3 origin = transform.position + Vector3.up * 0.05f;
        // Cast downwards
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayer);
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

    private void UpdateJumpParameters()
    {
        // Using a symmetrical jump formula:
        // gravity = 8 * jumpHeight / (totalJumpTime^2)
        // initialVelocity = 4 * jumpHeight / totalJumpTime
        customGravity = 8f * jumpHeight / (totalJumpTime * totalJumpTime);
        initialJumpVelocity = 4f * jumpHeight / totalJumpTime;
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
}
