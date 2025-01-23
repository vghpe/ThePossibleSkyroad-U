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
    public float rotationSpeed = 100f; // Rotation speed in degrees per second

    [Header("Ground Check Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float groundCheckDepthOffset = 0.45f;

    [Header("Death Settings")]
    public GameObject deathParticlePrefab;  // Assign a particle prefab here.
    public float deathDelay = 2.5f;           // Delay before calling OnPlayerDeath.
    [SerializeField] private AudioClip deathSoundClip; // Assign a death sound effect.
    [SerializeField] private AudioSource audioSource;         // Player's AudioSource for sound effects.

    [Header("Readonly Runtime Debug")]
    [SerializeField] private float customGravity;
    [SerializeField] private float initialJumpVelocity;
    [SerializeField] private float jumpDistance;         // Horizontal distance for landing at same level.
    [SerializeField] private float jumpDistanceAbove;    // Horizontal distance if landing 1 unit above.
    [SerializeField] private float jumpDistanceBelow;    // Horizontal distance if landing 1 unit below.

    private Rigidbody rb;
    private Transform firstChild;
    private bool isJumping = false;
    private bool isDead = false;
    private float distanceSinceLastTick = 0f;
    private InputAction jumpAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>(); // Ensure AudioSource is attached to the player.

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
            // Make sure "Fire" matches the name of the action in your Input Actions asset
            jumpAction = playerInput.actions["Jump"];

            // Subscribe to "performed" so when the user clicks or taps, we can jump
            jumpAction.performed += OnJumpPerformed;
        }
        
    }
    
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        // Only jump if grounded
        if (IsGrounded())
        {
            Jump();
        }
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
        Vector3 newPosition = rb.position + Vector3.forward * distThisFrame;
        rb.MovePosition(newPosition);
        if (distanceSinceLastTick >= unitsPerTick)
        {
            distanceSinceLastTick -= unitsPerTick;
            Debug.Log("Tick!");
        }

        // Jump input only when grounded.
        //if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        //{
        //    Jump();
        //}

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
                
                if (jumpAction != null && jumpAction.ReadValue<float>() > 0f)
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

    private bool IsGrounded()
    {
        Vector3 originCenter = transform.position + Vector3.up * 0.05f;
        Vector3 originFront = originCenter + transform.forward * groundCheckDepthOffset;
        Vector3 originBack = originCenter - transform.forward * groundCheckDepthOffset;

        bool hitFront = Physics.Raycast(originFront, Vector3.down, groundCheckDistance, groundLayer);
        bool hitBack = Physics.Raycast(originBack, Vector3.down, groundCheckDistance, groundLayer);

        return hitFront || hitBack;
    }

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

        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (firstChild != null)
            firstChild.gameObject.SetActive(true);

        UpdateJumpParameters();
    }

    private void UpdateJumpParameters()
    {
        customGravity = 8f * jumpHeight / (totalJumpTime * totalJumpTime);
        initialJumpVelocity = 4f * jumpHeight / totalJumpTime;

        float forwardSpeed = (bpm / 60f) * unitsPerTick;
        jumpDistance = forwardSpeed * totalJumpTime;

        float discriminantAbove = initialJumpVelocity * initialJumpVelocity - 2f * customGravity * 1f;
        jumpDistanceAbove = discriminantAbove >= 0f
            ? forwardSpeed * (initialJumpVelocity + Mathf.Sqrt(discriminantAbove)) / customGravity
            : 0f;

        float discriminantBelow = initialJumpVelocity * initialJumpVelocity + 2f * customGravity * 1f;
        jumpDistanceBelow = forwardSpeed * (initialJumpVelocity + Mathf.Sqrt(discriminantBelow)) / customGravity;
    }

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

        // Play the death sound
        if (audioSource != null && deathSoundClip != null)
        {
            audioSource.PlayOneShot(deathSoundClip);
        }

        // Hide the child mesh
        if (firstChild != null)
        {
            firstChild.gameObject.SetActive(false);
        }

        // Spawn the death particle system
        if (deathParticlePrefab != null)
        {
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        }

        // Wait for the specified delay
        yield return new WaitForSeconds(deathDelay);

        // Notify GameManager of death
        GameManager.Instance.OnPlayerDeath();
    }
}
