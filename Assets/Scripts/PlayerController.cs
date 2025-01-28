using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Reset Position")] 
    public float playerProgress = 0f;

    [Header("BPM-Based Movement")]
    [Range(30f, 300f)] public float bpm = 120f;
    [Range(0.1f, 10f)] public float unitsPerTick = 4f;

    [Header("Symmetrical Jump Settings")]
    [Range(0.5f, 5f)] public float jumpHeight = 2f;
    [Range(0.2f, 3f)] public float totalJumpTime = 1f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 100f;

    [Header("Ground Check Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float groundCheckDepthOffset = 0.45f;

    [Header("Snapping Settings")]
    public float snapThreshold = 0.1f; 
    public float snapIncrement = 1.0f; 

    [Header("Death Settings")]
    public GameObject deathParticlePrefab;
    public float deathDelay = 2.5f;    
    [SerializeField] private AudioClip deathSoundClip; 
    [SerializeField] private AudioSource audioSource;         

    [Header("Readonly Runtime Debug")]
    [SerializeField] private float customGravity;
    [SerializeField] private float initialJumpVelocity;
    [SerializeField] private float jumpDistance;
    [SerializeField] private float jumpDistanceAbove;
    [SerializeField] private float jumpDistanceBelow;

    private Rigidbody rb;
    private Transform firstChild;
    private bool isJumping = false;
    private bool isDead = false;
    private InputAction jumpAction;
    private float jumpStartZ = 0f; // Track Z-position at start of jump

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (transform.childCount > 0)
        {
            firstChild = transform.GetChild(0);
        }
    }

    private void Start()
    {
        UpdateJumpParameters();

        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            jumpAction = playerInput.actions["Jump"];
            jumpAction.performed += OnJumpPerformed;
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateJumpParameters();
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (IsGrounded())
        {
            Jump();
        }
    }

    private void Update()
    {
        // Run movement/input only if game is playing and player is alive
        if (GameManager.Instance.CurrentState != GameState.Playing || isDead)
            return;

        // BPM-based forward movement
        float forwardSpeed = (bpm / 60f) * unitsPerTick;
        float moveThisFrame = forwardSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + Vector3.forward * moveThisFrame);

        // Debug: Force death
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(HandleDeath());
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.CurrentState == GameState.Playing && !isDead)
        {
            // Apply custom gravity
            Vector3 velocity = rb.linearVelocity;
            velocity.y -= customGravity * Time.fixedDeltaTime;
            rb.linearVelocity = velocity;

            bool grounded = IsGrounded();

            // Landed check
            if (grounded && isJumping)
            {
                float distanceJumped = rb.position.z - jumpStartZ;
                Debug.Log($"Jump distance: {distanceJumped:F2} units");

                // Snap to nearest multiple of snapIncrement if within threshold
                float snappedDistance = Mathf.Round(distanceJumped / snapIncrement) * snapIncrement;
                float diff = Mathf.Abs(distanceJumped - snappedDistance);

                if (diff < snapThreshold)
                {
                    Debug.Log($"Snapping to {snappedDistance} (from {distanceJumped:F2})");
                    Vector3 pos = rb.position;
                    pos.z = jumpStartZ + snappedDistance;
                    rb.position = pos;
                }
                else
                {
                    Debug.Log($"No snapping: diff {diff:F2} > {snapThreshold}");
                }

                isJumping = false;

                // Check auto-jump
                if (jumpAction != null && jumpAction.ReadValue<float>() > 0f)
                {
                    Jump();
                }
            }

            // Rotate visuals if in air
            if (firstChild != null)
            {
                if (!grounded)
                    firstChild.Rotate(Vector3.right * rotationSpeed * Time.fixedDeltaTime);
                else
                    firstChild.localRotation = Quaternion.identity;
            }
        }
    }

    private bool IsGrounded()
    {
        Vector3 center = transform.position + Vector3.up * 0.05f;
        Vector3 front = center + transform.forward * groundCheckDepthOffset;
        Vector3 back = center - transform.forward * groundCheckDepthOffset;

        bool hitFront = Physics.Raycast(front, Vector3.down, groundCheckDistance, groundLayer);
        bool hitBack = Physics.Raycast(back, Vector3.down, groundCheckDistance, groundLayer);

        return hitFront || hitBack;
    }

    private void Jump()
    {
        jumpStartZ = rb.position.z;
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

        isJumping = false;
        isDead = false;

        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (firstChild != null)
            firstChild.gameObject.SetActive(true);

        UpdateJumpParameters();
    }

    private void UpdateJumpParameters()
    {
        // Symmetrical jump: custom gravity and initial velocity
        customGravity = 8f * jumpHeight / (totalJumpTime * totalJumpTime);
        initialJumpVelocity = 4f * jumpHeight / totalJumpTime;

        // For debugging/tracking possible horizontal distances
        float forwardSpeed = (bpm / 60f) * unitsPerTick;
        jumpDistance = forwardSpeed * totalJumpTime;

        float discAbove = initialJumpVelocity * initialJumpVelocity - 2f * customGravity * 1f;
        jumpDistanceAbove = discAbove >= 0f
            ? forwardSpeed * (initialJumpVelocity + Mathf.Sqrt(discAbove)) / customGravity
            : 0f;

        float discBelow = initialJumpVelocity * initialJumpVelocity + 2f * customGravity * 1f;
        jumpDistanceBelow = forwardSpeed * (initialJumpVelocity + Mathf.Sqrt(discBelow)) / customGravity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        bool collisionCausesDeath = false;

        if (collision.collider.CompareTag("Spike"))
        {
            collisionCausesDeath = true;
        }
        else if (collision.collider.CompareTag("Platform"))
        {
            // Check angles for side collisions
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

    private IEnumerator HandleDeath()
    {
        isDead = true;

        // Freeze physics
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        // Stop the music
        if (GameManager.Instance != null && GameManager.Instance.AudioSource != null)
        {
            GameManager.Instance.AudioSource.Stop();
        }

        // Play death sound
        if (audioSource != null && deathSoundClip != null)
        {
            audioSource.PlayOneShot(deathSoundClip);
        }

        // Hide the child mesh
        if (firstChild != null)
        {
            firstChild.gameObject.SetActive(false);
        }

        // Spawn death particle
        if (deathParticlePrefab != null)
        {
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        }

        // Wait, then notify GM
        yield return new WaitForSeconds(deathDelay);
        GameManager.Instance.OnPlayerDeath();
    }
}
