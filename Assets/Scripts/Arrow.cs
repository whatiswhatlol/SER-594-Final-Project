using UnityEngine;

/// Attach to your Arrow prefab:
/// - Rigidbody2D (Dynamic)
/// - Collider2D (set as Trigger while flying)
/// This script makes the arrow fly, rotate to velocity, and on first hit:
///  - sticks in place, becomes solid (non-trigger), and moves to the 'stuckLayer'
///  - optionally parents to the hit object so it moves with platforms, etc.
public class Arrow : MonoBehaviour
{
    [Header("Physics")]
    public Rigidbody2D rb;
    public Collider2D arrowCollider;

    [Tooltip("Layer to switch to when the arrow sticks (include this in player's ground mask).")]
    public string stuckLayer = "Ground";

    [Tooltip("Parent the arrow to what it hit so it rides moving platforms.")]
    public bool parentToHit = true;

    [Tooltip("Time to ignore collision with the shooter to avoid instant self-hit.")]
    public float ignoreOwnerCollisionTime = 0.15f;

    [Tooltip("Rotate to match velocity while flying.")]
    public bool rotateToVelocity = true;

    private bool stuck = false;
    private int originalLayer = -1;
    private float ignoreTimer = 0f;
    private Collider2D ownerCollider;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        arrowCollider = GetComponent<Collider2D>();
    }

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (arrowCollider == null) arrowCollider = GetComponent<Collider2D>();
        originalLayer = gameObject.layer;
    }

    public void Launch(Vector2 velocity, Collider2D owner)
    {
        ownerCollider = owner;

        // set initial state for flight
        stuck = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = false;
        rb.linearVelocity = velocity;

        if (arrowCollider != null)
            arrowCollider.isTrigger = true; // fly-through, detect hits via trigger

        // briefly ignore the shooter's collider to avoid immediate collision
        if (ownerCollider != null && arrowCollider != null)
        {
            Physics2D.IgnoreCollision(arrowCollider, ownerCollider, true);
            ignoreTimer = ignoreOwnerCollisionTime;
        }
    }

    private void Update()
    {
        // re-enable owner collision after grace period
        if (ignoreTimer > 0f)
        {
            ignoreTimer -= Time.deltaTime;
            if (ignoreTimer <= 0f && ownerCollider != null && arrowCollider != null)
            {
                Physics2D.IgnoreCollision(arrowCollider, ownerCollider, false);
                ownerCollider = null;
            }
        }

        // Keep the arrow visually aligned to its flight direction
        if (!stuck && rotateToVelocity && rb != null)
        {
            Vector2 v = rb.linearVelocity;
            if (v.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
                rb.MoveRotation(angle);
            }
        }
    }

    // Use trigger while flying to detect the first impact cleanly
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (stuck) return;

        // Ignore the shooter during grace period
        if (other == ownerCollider) return;

        StickInto(other);
    }

    // If you prefer solid collisions during flight,
    // you can also handle OnCollisionEnter2D similarly:
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (stuck) return;
        // If you use non-trigger collider during flight, call StickInto(collision.collider) here.
    }

    private void StickInto(Collider2D hitCol)
    {
        if (rb == null || arrowCollider == null) return;

        stuck = true;

        // Stop motion and freeze
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;

        // Make it solid
        arrowCollider.isTrigger = false;

        // Move to the chosen ground layer so the player can stand on it
        if (!string.IsNullOrEmpty(stuckLayer))
        {
            int layer = LayerMask.NameToLayer(stuckLayer);
            if (layer != -1) gameObject.layer = layer;
        }

        // Optionally parent to what we hit (so it moves with moving platforms)
        if (parentToHit && hitCol != null)
        {
            transform.SetParent(hitCol.transform, true);
        }

        // Optional: nudge the arrow slightly into the surface so there’s no seam
        // (uncomment if needed)
        // transform.position += transform.right * 0.01f;
    }
}
