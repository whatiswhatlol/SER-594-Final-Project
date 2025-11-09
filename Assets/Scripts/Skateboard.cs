using UnityEngine;
using UnityEngine.InputSystem;

public class Skateboard : WeaponBase
{
    [Header("References")]
    public Rigidbody2D playerRb;
    public PlayerInput input;                  // optional; auto-finds if null

    [Header("Input")]
    public string moveActionName = "Move";     // Value/Vector2 (x used)
    public float turnDeadzone = 0.25f;         // how hard the stick/keys must be in opposite dir to trigger a turn

    [Header("Behavior")]
    public bool onlyWhenGrounded = true;
    public bool allowKickStart = true;

    [Header("Speed Control")]
    public float topSpeed = 14f;               // target max horizontal speed
    public float accelPerSecond = 6f;          // how quickly we approach top speed
    public float kickStartSpeed = 2f;          // small push when starting from rest
    public float turnKickSpeed = 2.5f;         // small push applied immediately after a turn

    [Header("Ground Check (if onlyWhenGrounded=true)")]
    public Transform groundCheck;
    public float groundRadius = 0.12f;
    public LayerMask groundLayer;

    [Header("Physics While Skating")]
    public float skatingDamping = 0f;          // 0 = frictionless feel

    // --- internals ---
    private float defaultDamping;
    private bool defaultsCached = false;
    private float bestAbsX = 0f;                // max horizontal speed achieved while active

    private InputAction moveAction;

    private void Awake()
    {
        if (playerRb == null)
            playerRb = GetComponentInParent<Rigidbody2D>();

        if (input == null)
            input = GetComponentInParent<PlayerInput>();

        if (input != null && !string.IsNullOrEmpty(moveActionName))
            moveAction = input.actions[moveActionName];

        CacheDefaults();
    }

    private void OnEnable()
    {
        CacheDefaults();
        bestAbsX = Mathf.Abs(playerRb != null ? playerRb.linearVelocity.x : 0f);
        ApplySkateDamping();
    }

    private void OnDisable()
    {
        RestoreDefaults();
        bestAbsX = 0f;
    }

    private void CacheDefaults()
    {
        if (playerRb == null || defaultsCached) return;
        defaultDamping = playerRb.linearDamping;
        defaultsCached = true;
    }

    private void ApplySkateDamping()
    {
        if (playerRb == null) return;
        playerRb.linearDamping = skatingDamping;
    }

    private void RestoreDefaults()
    {
        if (playerRb == null || !defaultsCached) return;
        playerRb.linearDamping = defaultDamping;
    }

    private bool IsGrounded()
    {
        if (!onlyWhenGrounded) return true;
        if (groundCheck == null) return true; // assume grounded if not provided
        return Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    private void FixedUpdate()
    {
        if (playerRb == null) return;

        // Keep your PlayerController from immediately steering us away this tick
        var controller = playerRb.GetComponent<PlayerController>();
        if (controller != null) controller.GrantImpulseGrace(0.05f);

        bool grounded = IsGrounded();

        // Read desired move direction from Input System
        float desiredDir = ReadDesiredDir(); // -1, 0, or +1 (approx)
        Vector2 v = playerRb.linearVelocity;

        if (onlyWhenGrounded && !grounded)
        {
            // In-air: just preserve current best speed, but allow turning intent to set future sign
            PreserveAndAccelerate(ref v, desiredDir, noAcceleration: true);
            playerRb.linearVelocity = v;
            ApplySkateDamping();
            return;
        }

        // If player is steering opposite to current horizontal movement (beyond deadzone), TURN:
        if (ShouldTurn(desiredDir, v.x))
        {
            // Reset preserved speed so we don't fight the turn
            bestAbsX = 0f;

            // Hard set a small kick in the new direction to start rolling immediately
            if (desiredDir != 0f)
                v.x = desiredDir * turnKickSpeed;
            else
                v.x = 0f; // no direction, just stop

            playerRb.linearVelocity = v;
            ApplySkateDamping();
            return; // next frame we'll accelerate in that direction
        }

        // Normal skating: preserve and accelerate up to top speed
        PreserveAndAccelerate(ref v, desiredDir, noAcceleration: false);
        playerRb.linearVelocity = v;

        ApplySkateDamping();
    }

    private float ReadDesiredDir()
    {
        if (moveAction == null) return 0f;
        Vector2 mv = moveAction.ReadValue<Vector2>();
        float x = mv.x;

        if (x > +turnDeadzone) return +1f;
        if (x < -turnDeadzone) return -1f;
        return 0f;
    }

    private bool ShouldTurn(float desiredDir, float currentX)
    {
        if (desiredDir == 0f) return false;                 // no intent to turn
        if (Mathf.Abs(currentX) < 0.05f) return false;      // already nearly stopped; no need to "turn"
        float signVel = Mathf.Sign(currentX);
        return desiredDir != 0f && Mathf.Sign(desiredDir) != signVel; // opposite directions
    }

    private void PreserveAndAccelerate(ref Vector2 v, float desiredDir, bool noAcceleration)
    {
        // Current stats
        float absX = Mathf.Abs(v.x);

        // Track best speed so far this session (prevents passive loss)
        bestAbsX = Mathf.Max(bestAbsX, absX);

        // Give a kick if we're nearly stopped and a direction is requested
        if (allowKickStart && absX < 0.05f && desiredDir != 0f)
        {
            absX = Mathf.Max(absX, kickStartSpeed);
            v.x = desiredDir * absX;
        }

        // Accelerate toward top speed if allowed this tick
        float targetAbs = noAcceleration
            ? absX
            : Mathf.MoveTowards(absX, topSpeed, accelPerSecond * Time.fixedDeltaTime);

        // Never drop below the best preserved speed (unless turning logic reset it)
        targetAbs = Mathf.Max(targetAbs, bestAbsX);
        targetAbs = Mathf.Clamp(targetAbs, 0f, topSpeed);

        // Apply sign: if we have input, use that; else keep whatever sign we currently have
        float sign = desiredDir != 0f ? Mathf.Sign(desiredDir)
                                      : (Mathf.Approximately(v.x, 0f) ? 1f : Mathf.Sign(v.x));

        v.x = targetAbs * sign;
    }

    // Passive weapon (selected = active); Fire not used.
    public override void Fire() { }
}
