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

    [Header("Readonly Runtime Debug")]
    [SerializeField] private float customGravity;
    [SerializeField] private float initialJumpVelocity;

    private Rigidbody rb;
    private bool isJumping = false;

    private float distanceSinceLastTick = 0f;
    private Transform firstChild;

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
        // Calculate once at start
        UpdateJumpParameters();
    }

    private void OnValidate()
    {
        // Called in Editor when fields change.
        // If the game is running, let's recalc to apply changes immediately.
        if (Application.isPlaying)
        {
            UpdateJumpParameters();
        }
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing)
            return;

        // BPM-based forward speed
        float forwardSpeed = (bpm / 60f) * unitsPerTick;
        float distThisFrame = forwardSpeed * Time.deltaTime;
        transform.Translate(Vector3.forward * distThisFrame, Space.World);

        // Check "tick"
        distanceSinceLastTick += distThisFrame;
        if (distanceSinceLastTick >= unitsPerTick)
        {
            distanceSinceLastTick -= unitsPerTick;
            Debug.Log("Tick!");
        }

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            Jump();
        }

        // Debug death
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

            // Handle child rotation
            if (firstChild != null)
            {
                if (isJumping)
                {
                    // Rotate the child while jumping or falling
                    firstChild.Rotate(Vector3.right * rotationSpeed * Time.fixedDeltaTime);
                }
                else
                {
                    // Reset rotation when grounded
                    firstChild.localRotation = Quaternion.identity;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Platform"))
        {
            // Check if top collision
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isJumping = false;

                    // Re-add "auto-jump if still holding space"
                    if (Input.GetKey(KeyCode.Space))
                    {
                        Jump();
                    }
                    return;
                }
            }
            // Side collision => death
            GameManager.Instance.OnPlayerDeath();
        }
        else if (collision.collider.CompareTag("Spike"))
        {
            GameManager.Instance.OnPlayerDeath();
        }
        else if (collision.collider.CompareTag("Ground"))
        {
            isJumping = false;

            // Auto-jump if still holding
            if (Input.GetKey(KeyCode.Space))
            {
                Jump();
            }
        }
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
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        rb.linearVelocity        = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        distanceSinceLastTick = 0f;
        isJumping = false;

        // Recalc in case user tweaked jumpHeight or totalJumpTime
        UpdateJumpParameters();
    }

    private void UpdateJumpParameters()
    {
        customGravity       = 8f * jumpHeight / (totalJumpTime * totalJumpTime);
        initialJumpVelocity = 4f * jumpHeight / totalJumpTime;
    }
}
