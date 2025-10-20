using System;
using UnityEngine;
using UnityEngine.InputSystem; // New Input System

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{

    public Animator animator;
    [Header("Movement")]
    public float moveSpeed = 12f;
    public float acceleration = 60f;
    public float deceleration = 70f;
    public float airControlMultiplier = 0.7f;

    [Header("Jumping")]
    public float jumpForce = 16f;
    public float jumpCutMultiplier = 0.5f;
    public float jumpBufferTime = 0.15f;
    public float coyoteTime = 0.1f;

    private Rigidbody2D rb;
    private bool isFacingRight = true;

    private float moveInput;
    private bool jumpPressed;
    private bool jumpHeld;

    private float lastGroundedTime;
    private float lastJumpPressedTime;
    private bool isJumping;

    [Header("Checks")]
    public Transform groundCheck;
    public float groundRadius = 0.1f;
    public LayerMask groundLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Track jump buffer
        if (jumpPressed)
            lastJumpPressedTime = jumpBufferTime;

        if (IsGrounded())
            lastGroundedTime = coyoteTime;

        // Jump logic
        if (lastJumpPressedTime > 0 && lastGroundedTime > 0 && !isJumping)
            Jump();

        // Variable jump height
        if (!jumpHeld && rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        lastGroundedTime -= Time.deltaTime;
        lastJumpPressedTime -= Time.deltaTime;

        // Flip sprite
        if (moveInput != 0)
            Flip(moveInput);

        // Update animator
        animator.SetFloat("HSpeed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetFloat("VSpeed", rb.linearVelocity.y);
        animator.SetBool("IsGrounded", IsGrounded());

    }

    void FixedUpdate()
    {
        Move(moveInput);
    }

    private void Move(float xInput)
    {
        float targetSpeed = xInput * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;

        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        if (!IsGrounded()) accelRate *= airControlMultiplier;

        float movement = speedDiff * accelRate;
        rb.AddForce(movement * Vector2.right);
    }

    private void Jump()
    {
        isJumping = true;
        lastGroundedTime = 0;
        lastJumpPressedTime = 0;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private bool IsGrounded()
    {
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        if (grounded) isJumping = false;
        return grounded;
    }

    private void Flip(float dir)
    {
        if ((dir > 0 && !isFacingRight) || (dir < 0 && isFacingRight))
        {
            isFacingRight = !isFacingRight;
            transform.localScale = new Vector3(isFacingRight ? 1 : -1, 1, 1);
        }
    }

    // =====================
    // INPUT SYSTEM CALLBACKS
    // =====================
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>().x;
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        Debug.Log($"[Jump] ctx={ctx.phase} grounded={IsGrounded()}");

        if (ctx.started) jumpPressed = true;
        if (ctx.canceled) jumpHeld = false;
        if (ctx.performed) jumpHeld = true;

        if (ctx.canceled) jumpPressed = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hazards"))
        {
            Debug.Log("Player Hit!");
        }
    }
}
