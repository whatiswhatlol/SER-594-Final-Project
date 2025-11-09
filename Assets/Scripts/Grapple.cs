using UnityEngine;
using UnityEngine.InputSystem;

/// Grapple rope similar to Worms W.M.D.
/// - Fire() shoots a ray from firePoint in your aim direction and attaches if it hits.
/// - While attached: 
///     * Up/Down (Move.y) retracts/extends rope
///     * Left/Right (Move.x) pumps swing by adding tangential force
/// - Fire() again to detach
/// - Renders rope with a LineRenderer
public class Grapple : WeaponBase
{
    [Header("Refs")]
    public Rigidbody2D playerRb;              // Player rigidbody
    public Transform firePoint;               // Where rope originates (weapon muzzle)
    public LineRenderer line;                 // LineRenderer for rope
    public PlayerInput input;                 // For "Look" and "Move" actions
    public Camera cam;                        // If null uses Camera.main

    [Header("Input Actions")]
    public string lookActionName = "Look";    // Value/Vector2 (Pointer.position / rightStick)
    public string moveActionName = "Move";    // Value/Vector2 (WASD / leftStick)

    [Header("Grapple Settings")]
    public LayerMask grappleMask = ~0;        // What the rope can attach to
    public float maxGrappleDistance = 25f;    // Max raycast distance
    public float initialPullFraction = 0.95f; // Set joint length to this fraction of hit distance (little snap)
    public float retractSpeed = 12f;          // Units/sec rope gets shorter
    public float extendSpeed = 12f;          // Units/sec rope gets longer
    public float minRopeLength = 1.5f;        // Shortest allowed
    public float maxRopeLength = 30f;         // Hard cap (also clamped to hit distance)

    [Header("Swing Assist")]
    public float swingForce = 22f;            // Tangential force from Move.x while attached
    public float airControlMultiplier = 1.0f; // Scale swing input effect

    [Header("Cooldown")]
    public float fireCooldown = 0.15f;

    private InputAction lookAction;
    private InputAction moveAction;

    private DistanceJoint2D joint;            // Created at runtime on the player
    private Vector2 hookPoint;
    private bool attached = false;
    private float ropeLength = 0f;
    private float nextFireTime = 0f;

    private void Awake()
    {
        if (playerRb == null) playerRb = GetComponentInParent<Rigidbody2D>();
        if (cam == null) cam = Camera.main;
        if (input == null) input = GetComponentInParent<PlayerInput>();

        if (input != null)
        {
            if (!string.IsNullOrEmpty(lookActionName))
            {
                lookAction = input.actions[lookActionName];
                lookAction?.Enable();
            }
            if (!string.IsNullOrEmpty(moveActionName))
            {
                moveAction = input.actions[moveActionName];
                moveAction?.Enable();
            }
        }

        if (line == null)
        {
            line = gameObject.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.widthMultiplier = 0.05f;
            line.enabled = false;
            // Assign a simple material if needed at runtime:
            line.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    private void Start()
    {
        // Prepare joint on the player (disabled by default)
        joint = playerRb.GetComponent<DistanceJoint2D>();
        if (joint == null) joint = playerRb.gameObject.AddComponent<DistanceJoint2D>();
        joint.autoConfigureConnectedAnchor = false;
        joint.enableCollision = true;           // lets rope collide with things
        joint.maxDistanceOnly = false;          // full rope constraint
        joint.enabled = false;
    }

    private void Update()
    {
        // Draw rope if attached
        if (attached && line != null)
        {
            line.SetPosition(0, firePoint != null ? (Vector3)firePoint.position : playerRb.transform.position);
            line.SetPosition(1, hookPoint);
        }
    }

    private void FixedUpdate()
    {
        if (!attached) return;

        // Retract/extend using Move.y
        Vector2 mv = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        float yInput = mv.y;

        if (yInput > 0.1f) // retract
            ropeLength -= retractSpeed * Time.fixedDeltaTime * yInput;
        else if (yInput < -0.1f) // extend
            ropeLength += extendSpeed * Time.fixedDeltaTime * (-yInput);

        ropeLength = Mathf.Clamp(ropeLength, minRopeLength, maxRopeLength);
        joint.distance = ropeLength;

        // Swing assist: add tangential force relative to rope direction
        if (Mathf.Abs(mv.x) > 0.05f && swingForce > 0f)
        {
            Vector2 toHook = (hookPoint - playerRb.position);
            if (toHook.sqrMagnitude > 0.0001f)
            {
                Vector2 along = toHook.normalized;
                Vector2 tangent = new Vector2(-along.y, along.x); // 90° CCW
                float strength = mv.x * swingForce * airControlMultiplier;
                playerRb.AddForce(tangent * strength, ForceMode2D.Force);
            }
        }
    }

    public override void Fire()
    {
        if (Time.time < nextFireTime) return;

        if (!attached)
        {
            TryAttach();
        }
        else
        {
            Detach();
        }

        nextFireTime = Time.time + fireCooldown;
    }

    private void TryAttach()
    {
        if (playerRb == null) return;

        Vector2 origin = firePoint != null ? (Vector2)firePoint.position : playerRb.position;
        Vector2 aimDir = GetAimDirection(origin);
        if (aimDir.sqrMagnitude < 1e-6f) aimDir = Vector2.right;

        RaycastHit2D hit = Physics2D.Raycast(origin, aimDir, maxGrappleDistance, grappleMask);
        if (!hit.collider) return; // nothing to attach

        hookPoint = hit.point;

        // Set up joint
        joint.connectedAnchor = hookPoint;
        float dist = Vector2.Distance(playerRb.position, hookPoint);
        ropeLength = Mathf.Clamp(dist * initialPullFraction, minRopeLength, Mathf.Min(maxRopeLength, dist));
        joint.distance = ropeLength;
        joint.enabled = true;

        // Line visuals
        if (line != null)
        {
            line.positionCount = 2;
            line.enabled = true;
            line.SetPosition(0, origin);
            line.SetPosition(1, hookPoint);
        }

        attached = true;
    }

    private void Detach()
    {
        attached = false;
        if (joint != null) joint.enabled = false;
        if (line != null) line.enabled = false;
    }

    private Vector2 GetAimDirection(Vector2 from)
    {
        // Gamepad: right stick gives a direction
        string scheme = input != null ? (input.currentControlScheme ?? "") : "";
        if (scheme.Contains("Gamepad") && lookAction != null)
        {
            Vector2 stick = lookAction.ReadValue<Vector2>();
            if (stick.sqrMagnitude > 0.0001f) return stick.normalized;
        }

        // Mouse: screen position -> world
        Vector2 screen;
        if (Mouse.current != null)
            screen = Mouse.current.position.ReadValue();
        else
            screen = lookAction != null ? lookAction.ReadValue<Vector2>() : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        if (cam == null) cam = Camera.main;
        if (cam == null) return Vector2.right;

        float z = Mathf.Abs((playerRb ? playerRb.transform.position.z : 0f) - cam.transform.position.z);
        if (cam.orthographic) z = Mathf.Abs(cam.transform.position.z);

        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, z));
        return ((Vector2)world - from).normalized;
    }

    // Optional: expose manual detach if you want to bind it separately
    public void ForceDetach() => Detach();

    private void OnDisable()
    {
        // Clean up if weapon is switched while attached
        Detach();
    }
}
