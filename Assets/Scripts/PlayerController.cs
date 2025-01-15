using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;

    private Rigidbody rb;
    private bool isJumping = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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

                isJumping = true;
            }

            // Debug test: Press K to simulate death
            if (Input.GetKeyDown(KeyCode.K))
            {
                GameManager.Instance.OnPlayerDeath();
            }
        }
        Debug.Log(isJumping);
    }

    public void ResetPlayer()
    {
        // Example reset: Place at origin and clear velocity
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // Example collision detection for future
    private void OnCollisionEnter(Collision collision)
    {
        // If it's a platform
        if (collision.collider.CompareTag("Platform"))
        {
            // Check each contact point
            foreach (ContactPoint contact in collision.contacts)
            {
                // If the normal is mostly "up" (y > some threshold), we treat it as a top landing
                if (contact.normal.y > 0.5f)
                {
                    isJumping = false;
                    return;
                }
            }
            // If we never found a normal pointing up, it must be a side -> death
            GameManager.Instance.OnPlayerDeath();
        }
        
        if (collision.collider.tag == "Spike")
        {
            GameManager.Instance.OnPlayerDeath();
        }
        else if (collision.collider.tag == "Ground")
        {
            isJumping = false;
        }
    }
}