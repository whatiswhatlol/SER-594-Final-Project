using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    public Animator animator;

    [Header("Movement")]
    public float moveSpeed = 12f;
    public float acceleration = 80f;
    public float deceleration = 90f;
    public float airControlMultiplier = 0.7f;

    [Header("Jumping")]
    public float jumpForce = 16f;
    public float jumpCutMultiplier = 0.5f;
    public float jumpBufferTime = 0.15f;
    public float coyoteTime = 0.1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Animation Speed")]
    public float minAnimSpeed = 0.15f;   // don’t let the anim freeze
    public float maxAnimSpeed = 1.5f;    // speed at full run
    private const float AnimEpsilon = 0.001f;

    private Rigidbody2D rb;
    private bool isFacingRight = true;

    private float moveInput;
    private bool jumpPressed;
    private bool jumpHeld;

    private float lastGroundedTime;
    private float lastJumpPressedTime;
    private bool isJumping;

    private float impulseGraceTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        impulseGraceTimer -= Time.deltaTime;

        if (jumpPressed)
            lastJumpPressedTime = jumpBufferTime;

        if (IsGrounded())
            lastGroundedTime = coyoteTime;

        if (lastJumpPressedTime > 0f && lastGroundedTime > 0f && !isJumping)
            Jump();

        if (!jumpHeld && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        lastGroundedTime -= Time.deltaTime;
        lastJumpPressedTime -= Time.deltaTime;

        if (Mathf.Abs(moveInput) > 0.001f)
            Flip(moveInput);

        UpdateAnimator();
        UpdateAnimationSpeed();
    }

    void FixedUpdate()
    {
        Move(moveInput);
    }

    private void Move(float xInput)
    {
        if (impulseGraceTimer > 0f)
            return;

        float targetSpeed = xInput * moveSpeed;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        if (!IsGrounded()) accelRate *= airControlMultiplier;

        float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    }

    private void Jump()
    {
        isJumping = true;
        lastGroundedTime = 0f;
        lastJumpPressedTime = 0f;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        GrantImpulseGrace(0.05f);
    }

    private bool IsGrounded()
    {
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        if (grounded) isJumping = false;
        return grounded;
    }

    private void Flip(float dir)
    {
        bool shouldFaceRight = dir > 0f;
        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (isFacingRight ? 1f : -1f);
            transform.localScale = s;
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        float vx = rb.linearVelocity.x;
        float vy = rb.linearVelocity.y;

        if (Mathf.Abs(vx) < AnimEpsilon) vx = 0f;
        if (Mathf.Abs(vy) < AnimEpsilon) vy = 0f;

        animator.SetFloat("HSpeed", Mathf.Abs(vx));
        animator.SetFloat("VSpeed", vy);
        animator.SetBool("IsGrounded", IsGrounded());
    }

    private void UpdateAnimationSpeed()
    {
        if (animator == null) return;

        float hAbs = Mathf.Abs(rb.linearVelocity.x);

        if (!IsGrounded())
        {
            // In air: use normal speed
            animator.speed = 1f;
            return;
        }

        // Convert speed to 0–1
        float normalized = Mathf.Clamp01(hAbs / Mathf.Max(0.001f, moveSpeed));

        // Map to playback speed range
        float animSpeed = Mathf.Lerp(minAnimSpeed, maxAnimSpeed, normalized);
        animator.speed = animSpeed;
    }

    public void GrantImpulseGrace(float duration)
    {
        if (duration > impulseGraceTimer)
            impulseGraceTimer = duration;
    }

    // Input System
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>().x;
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.started) { jumpPressed = true; jumpHeld = true; }
        if (ctx.performed) { jumpHeld = true; }
        if (ctx.canceled) { jumpHeld = false; jumpPressed = false; }
    }
    public void Restart(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;       
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hazards"))
        {
            Debug.Log("Player Hit!");
        }
    }
}
