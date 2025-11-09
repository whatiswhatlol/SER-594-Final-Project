using UnityEngine;
using UnityEngine.InputSystem;

public class Umbrella : WeaponBase
{
    [Header("References")]
    public Rigidbody2D playerRb;
    public PlayerInput input;                      

    [Header("Input")]
    public string holdActionName = "Attack";        
    public bool requireDescending = true;           

    [Header("Gravity Mapping (by horizontal speed)")]
    [Tooltip("Horizontal speed at/above which we reach full fast-fall settings.")]
    public float maxHorizSpeedForScale = 12f;

    [Tooltip("Gravity scale when moving SLOW horizontally (floaty).")]
    public float slowFallGravityScale = 0.25f;

    [Tooltip("Gravity scale when moving FAST horizontally (heavier).")]
    public float fastFallGravityScale = 1.2f;

    [Header("Terminal Velocity Clamp (optional)")]
    [Tooltip("Enable clamping vertical fall speed per-speed curve.")]
    public bool clampTerminalFall = true;

    [Tooltip("Max downward speed when moving SLOW horizontally (floaty).")]
    public float slowFallTerminal = -2.5f;

    [Tooltip("Max downward speed when moving FAST horizontally (heavier).")]
    public float fastFallTerminal = -10f;

    [Header("Drag While Gliding (optional)")]
    [Tooltip("Extra damping while gliding for stability. Set 0 for none.")]
    public float glideDamping = 2f;

    private InputAction holdAction;
    private bool umbrellaOpen = false;

    private float defaultGravityScale;
    private float defaultDamping;
    private bool defaultsCached = false;

    private void Awake()
    {
        if (playerRb == null)
            playerRb = GetComponentInParent<Rigidbody2D>();

        if (input == null)
            input = GetComponentInParent<PlayerInput>();

        if (input != null && !string.IsNullOrEmpty(holdActionName))
        {
            holdAction = input.actions[holdActionName];
        }

        CacheDefaults();
    }

    private void OnEnable()
    {
        CacheDefaults();

        if (holdAction != null)
        {
            holdAction.started += OnHoldStarted;
            holdAction.canceled += OnHoldCanceled;
        }
    }

    private void OnDisable()
    {
        if (holdAction != null)
        {
            holdAction.started -= OnHoldStarted;
            holdAction.canceled -= OnHoldCanceled;
        }

        umbrellaOpen = false;
        RestoreDefaults();
    }

    private void OnHoldStarted(InputAction.CallbackContext _)
    {
        umbrellaOpen = true;
    }

    private void OnHoldCanceled(InputAction.CallbackContext _)
    {
        umbrellaOpen = false;
        RestoreDefaults();
    }

    private void CacheDefaults()
    {
        if (playerRb == null) return;
        if (!defaultsCached)
        {
            defaultGravityScale = playerRb.gravityScale;
            defaultDamping = playerRb.linearDamping;
            defaultsCached = true;
        }
    }

    private void RestoreDefaults()
    {
        if (playerRb == null || !defaultsCached) return;
        playerRb.gravityScale = defaultGravityScale;
        playerRb.linearDamping = defaultDamping;
    }

    private void FixedUpdate()
    {
        if (playerRb == null) return;

        if (!umbrellaOpen)
        {
            RestoreDefaults();
            return;
        }

        if (requireDescending && playerRb.linearVelocity.y > 0.05f)
        {
            playerRb.gravityScale = defaultGravityScale;
            playerRb.linearDamping = defaultDamping;
            return;
        }

        float hAbs = Mathf.Abs(playerRb.linearVelocity.x);
        float t = Mathf.Clamp01(hAbs / Mathf.Max(0.001f, maxHorizSpeedForScale));

        float targetGravity = Mathf.Lerp(slowFallGravityScale, fastFallGravityScale, t);
        playerRb.gravityScale = targetGravity;

        playerRb.linearDamping = glideDamping;

        if (clampTerminalFall)
        {
            float maxDownSpeed = Mathf.Lerp(slowFallTerminal, fastFallTerminal, t);
            Vector2 v = playerRb.linearVelocity;
            if (v.y < maxDownSpeed) 
            {
                v.y = maxDownSpeed;
                playerRb.linearVelocity = v;
            }
        }
    }

    public override void Fire() { }
}
