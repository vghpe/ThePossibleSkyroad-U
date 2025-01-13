using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;

    private Rigidbody rb;

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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // We’ll do a basic jump for testing 
                // (In a real scenario, you'd check if grounded, etc.)
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            }

            // Debug test: Press K to simulate death
            if (Input.GetKeyDown(KeyCode.K))
            {
                GameManager.Instance.OnPlayerDeath();
            }
        }
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
    // (You can replace with OnTriggerEnter or more advanced checks)
    private void OnCollisionEnter(Collision collision)
    {
        // If we collide with something lethal (spike), call death
        if (collision.collider.tag == "Spike")
        {
            GameManager.Instance.OnPlayerDeath();
        }
    }
}
