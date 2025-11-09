using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Rigidbody2D targetRb2D;          // Optional: for better look-ahead (auto-found if null)

    [Header("Offsets / View")]
    [Tooltip("Fixed Z offset for 2D side view (camera stays at this Z).")]
    public float zOffset = -10f;
    [Tooltip("Static offset added to the follow position (X,Y).")]
    public Vector2 offsetXY = Vector2.zero;

    [Header("Smoothing")]
    [Tooltip("Smoothing time for horizontal motion (lower = snappier).")]
    public float smoothTimeX = 0.12f;
    [Tooltip("Smoothing time for vertical motion (lower = snappier).")]
    public float smoothTimeY = 0.18f;

    [Header("Dead Zone (Soft Zone)")]
    [Tooltip("Half-size of a box (in world units) around the camera center. Target can move inside without moving the camera.")]
    public Vector2 deadZoneHalfSize = new Vector2(2.0f, 1.0f);
    public bool useDeadZone = true;

    [Header("Look-Ahead")]
    [Tooltip("How much to lead the camera in the movement direction.")]
    public float lookAheadDistance = 2.0f;
    [Tooltip("How quickly the look-ahead catches up when you change direction.")]
    public float lookAheadSmoothing = 0.25f;
    [Tooltip("Minimum horizontal speed before we start looking ahead.")]
    public float lookAheadVelocityThreshold = 0.5f;

    [Header("Vertical Damping Booster")]
    [Tooltip("Extra smoothing while the player is moving vertically (helps soften jumps/falls).")]
    public float airborneSmoothBoost = 0.1f;
    [Tooltip("Consider player 'airborne' when vertical speed magnitude exceeds this.")]
    public float airborneVyThreshold = 0.2f;

    [Header("World Bounds (Optional)")]
    public bool clampToWorldBounds = false;
    [Tooltip("World-space rectangle that the CAMERA CENTER must remain inside.")]
    public Rect worldBounds = new Rect(-50, -10, 100, 30);

    // Internal
    private Vector2 _currentVelocity;     // SmoothDamp velocity holder (x in .x, y in .y)
    private float _lookAheadX;            // current look-ahead on X
    private float _lookAheadXVel;         // smoothing velocity for look-ahead
    private Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        if (target != null && targetRb2D == null)
            targetRb2D = target.GetComponent<Rigidbody2D>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Base desired XY (target + static offset)
        Vector2 desired = (Vector2)target.position + offsetXY;

        // --- Look-ahead on X ---
        float vx = 0f;
        if (targetRb2D != null)
            vx = targetRb2D.linearVelocity.x;
        else
            vx = (desired.x - transform.position.x) / Mathf.Max(Time.deltaTime, 0.0001f);

        float targetLookAhead = Mathf.Abs(vx) > lookAheadVelocityThreshold
            ? Mathf.Sign(vx) * lookAheadDistance
            : 0f;

        _lookAheadX = Mathf.SmoothDamp(_lookAheadX, targetLookAhead, ref _lookAheadXVel, lookAheadSmoothing);
        desired.x += _lookAheadX;

        // --- Dead zone (soft zone) ---
        Vector2 camXY = new Vector2(transform.position.x, transform.position.y);
        if (useDeadZone)
        {
            Vector2 delta = desired - camXY;
            // If inside dead zone, suppress movement along that axis
            if (Mathf.Abs(delta.x) < deadZoneHalfSize.x) desired.x = camXY.x;
            if (Mathf.Abs(delta.y) < deadZoneHalfSize.y) desired.y = camXY.y;
        }

        // --- Vertical booster (soften jumps/falls) ---
        float vy = targetRb2D != null ? targetRb2D.linearVelocity.y : 0f;
        float extraY = (Mathf.Abs(vy) > airborneVyThreshold) ? airborneSmoothBoost : 0f;

        // --- SmoothDamp toward desired XY ---
        float targetX = Mathf.SmoothDamp(camXY.x, desired.x, ref _currentVelocity.x, smoothTimeX);
        float targetY = Mathf.SmoothDamp(camXY.y, desired.y, ref _currentVelocity.y, smoothTimeY + extraY);

        Vector3 finalPos = new Vector3(targetX, targetY, zOffset);

        // --- Clamp to world bounds (camera center) ---
        if (clampToWorldBounds)
        {
            finalPos.x = Mathf.Clamp(finalPos.x, worldBounds.xMin, worldBounds.xMax);
            finalPos.y = Mathf.Clamp(finalPos.y, worldBounds.yMin, worldBounds.yMax);
        }

        transform.position = finalPos;

        // Side-scrollers usually don’t need LookAt; remove to avoid roll/tilt jitter.
        // If you’re in 3D and want the camera angled, set rotation in Inspector instead.
    }

    /// Call this after teleporting the player (or at scene start) to avoid big camera catch-up.
    public void SnapToTarget()
    {
        if (target == null) return;
        _currentVelocity = Vector2.zero;
        _lookAheadX = 0f;
        Vector3 p = target.position + (Vector3)offsetXY;
        p.z = zOffset;
        if (clampToWorldBounds)
        {
            p.x = Mathf.Clamp(p.x, worldBounds.xMin, worldBounds.xMax);
            p.y = Mathf.Clamp(p.y, worldBounds.yMin, worldBounds.yMax);
        }
        transform.position = p;
    }

    /// Optional: quick, lightweight camera shake.
    public void Shake(float amplitude = 0.2f, float duration = 0.15f)
    {
        StartCoroutine(ShakeCo(amplitude, duration));
    }
    private System.Collections.IEnumerator ShakeCo(float amp, float dur)
    {
        Vector3 basePos = transform.position;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float x = (Random.value * 2f - 1f) * amp;
            float y = (Random.value * 2f - 1f) * amp * 0.6f;
            transform.position = basePos + new Vector3(x, y, 0f);
            yield return null;
        }
        transform.position = basePos;
    }
}
