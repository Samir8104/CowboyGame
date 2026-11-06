using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class playerMovementScript : MonoBehaviour
{
    [Header("Basic Initialization Stuff")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private float groundAngleThreshHold = 45f;
    [SerializeField] private LayerMask groundLayer;


    [SerializeField] private float fallGravityMultiplier = 5f;
    [SerializeField] private float lowJumpMultiplier = 3f;
    [SerializeField] private float riseGravityMultiplier = 2.2f;

    private Rigidbody rb;
    private CapsuleCollider col;
    private Vector2 moveInput;
    private Camera playerCamera;
    private RaycastHit hit;

    private bool isGrounded;
    private bool isSloped;
    private bool jumpQueued;
    private bool jumpHeld;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        col = GetComponent<CapsuleCollider>();
        playerCamera = Camera.main;
    }

    void Update()
    {
        CheckGround();
        if (jumpQueued && isGrounded)
        {
            jumpQueued = false;
            Jump();
        }
    }

    void FixedUpdate()
    {
        Move();
        BetterJumpFeel();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpQueued = true;
            jumpHeld = true;
        }
        if (context.canceled)
        {
            jumpQueued = false;
            jumpHeld = false;
        }
    }

    private void Move()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 desired = move * moveSpeed;

        if (isSloped)
        {
            if (isGrounded)
            {
                desired = Vector3.ProjectOnPlane(desired, hit.normal).normalized * moveSpeed;
                rb.useGravity = false;
                rb.linearVelocity = desired;
            }
            else
            {
                rb.linearVelocity = new Vector3(desired.x, Mathf.Min(rb.linearVelocity.y, -9f), desired.z);
                rb.useGravity = true;
            }  
        }
        else
        {
            rb.linearVelocity = new Vector3(desired.x, rb.linearVelocity.y, desired.z);
            rb.useGravity = true;
        }
            
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void CheckGround()
    {
        Vector3 origin = col.bounds.center;
        float radius = col.radius * 1.05f;
        float castDist = (col.bounds.extents.y - radius) + groundCheckDistance;
        if (Physics.SphereCast(origin, radius, Vector3.down, out hit, castDist, groundLayer))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle <= groundAngleThreshHold)
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
            if (angle != 0f)
            {
                isSloped = true;
            }
            else
            {
                isSloped = false;
            }
        }
        else
        {
            isGrounded = false;
            isSloped = false;
        }

    }

    private void BetterJumpFeel()
    {
        float yVel = rb.linearVelocity.y;
        if (yVel < 0f)
        {
            rb.AddForce(Physics.gravity * (fallGravityMultiplier - 1f), ForceMode.Acceleration);
        }
        else if (yVel > 0f)
        {
            float mult = jumpHeld ? riseGravityMultiplier : lowJumpMultiplier;
            rb.AddForce(Physics.gravity * (mult - 1f), ForceMode.Acceleration);
        }
    }

}
