using UnityEngine;
using System.Collections;

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

    [Header("Death Settings")]
    public GameObject deathParticlePrefab;  // Assign a particle prefab here.
    public float deathDelay = 2.5f;           // Delay before calling OnPlayerDeath.

    private Rigidbody rb;
    private Transform firstChild;
    private bool isJumping = false;
    private bool isDead = false;
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
        // Only run movement and input processing if the game is in playing state and the player is not dead.
        if (GameManager.Instance.CurrentState != GameState.Playing || isDead)
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
            StartCoroutine(HandleDeath());
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.CurrentState == GameState.Playing && !isDead)
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
        isDead = false;

        // Reset Rigidbody constraints (freeze rotation if desired).
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Ensure the child is visible.
        if (firstChild != null)
            firstChild.gameObject.SetActive(true);

        UpdateJumpParameters();
    }

    /// <summary>
    /// Updates the jump parameters and recalculates the read-only fields.
    /// Also calculates horizontal distances for landing on platforms at different heights.
    /// </summary>
    private void UpdateJumpParameters()
    {
        // Symmetrical jump formulas:
        // customGravity = 8 * jumpHeight / (totalJumpTime^2)
        // initialJumpVelocity = 4 * jumpHeight / totalJumpTime
        customGravity = 8f * jumpHeight / (totalJumpTime * totalJumpTime);
        initialJumpVelocity = 4f * jumpHeight / totalJumpTime;

        // Calculate forward speed (units per second) based on BPM.
        float forwardSpeed = (bpm / 60f) * unitsPerTick;

        // Jump distance for landing at same level.
        jumpDistance = forwardSpeed * totalJumpTime;

        // Calculate jump distance for landing one unit above:
        float discriminantAbove = initialJumpVelocity * initialJumpVelocity - 2f * customGravity * 1f;
        if (discriminantAbove >= 0f)
        {
            float tAbove = (initialJumpVelocity + Mathf.Sqrt(discriminantAbove)) / customGravity;
            jumpDistanceAbove = forwardSpeed * tAbove;
        }
        else
        {
            jumpDistanceAbove = 0f;
        }

        // Calculate jump distance for landing one unit below:
        float discriminantBelow = initialJumpVelocity * initialJumpVelocity + 2f * customGravity * 1f;
        float tBelow = (initialJumpVelocity + Mathf.Sqrt(discriminantBelow)) / customGravity;
        jumpDistanceBelow = forwardSpeed * tBelow;
    }

    /// <summary>
    /// Handles collisions and triggers the death routine.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (isDead)
            return;

        bool collisionCausesDeath = false;

        if (collision.collider.CompareTag("Spike"))
        {
            collisionCausesDeath = true;
        }
        else if (collision.collider.CompareTag("Platform"))
        {
            const float sideAngleThreshold = 60f;
            foreach (ContactPoint contact in collision.contacts)
            {
                float angle = Vector3.Angle(contact.normal, Vector3.up);
                if (angle > sideAngleThreshold)
                {
                    collisionCausesDeath = true;
                    break;
                }
            }
        }

        if (collisionCausesDeath)
        {
            StartCoroutine(HandleDeath());
        }
    }

    /// <summary>
    /// Coroutine that freezes the player, hides the mesh, spawns a particle system,
    /// and waits for the specified delay before notifying the GameManager of death.
    /// </summary>
    private IEnumerator HandleDeath()
    {
        isDead = true;

        // Freeze physics: zero velocities and freeze Rigidbody constraints.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        // Hide the child mesh.
        if (firstChild != null)
        {
            firstChild.gameObject.SetActive(false);
        }

        // Spawn the death particle system, if assigned.
        if (deathParticlePrefab != null)
        {
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        }

        // Wait for the specified delay.
        yield return new WaitForSeconds(deathDelay);

        // Notify GameManager (or execute further death logic).
        GameManager.Instance.OnPlayerDeath();
    }

    /// <summary>
    /// Draws debug gizmos for ground check raycasts.
    /// </summary>
    private void OnDrawGizmos()
    {
        Vector3 originCenter = transform.position + Vector3.up * 0.05f;
        Vector3 originFront = originCenter + transform.forward * groundCheckDepthOffset;
        Vector3 originBack = originCenter - transform.forward * groundCheckDepthOffset;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(originFront, originFront + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(originBack, originBack + Vector3.down * groundCheckDistance);

        Gizmos.DrawSphere(originFront, 0.02f);
        Gizmos.DrawSphere(originBack, 0.02f);
    }
}
