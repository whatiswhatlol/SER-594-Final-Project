using UnityEngine;

/// Fast straight arrow:
/// - gravity off, velocity fixed
/// - sticks on first hit, becomes solid & harmless
public class BallistaArrow2D : MonoBehaviour
{
    [Header("Refs")]
    public Rigidbody2D rb;
    public Collider2D arrowCollider;

    [Header("Stick Behavior")]
    public bool parentToHit = true;
    public string stuckLayerName = "Ground"; // put player ground layer here if you want to stand on arrows

    [Header("Flight")]
    public bool rotateToVelocity = true;

    private bool _stuck;
    private Collider2D _shooterToIgnore; // to avoid hitting the turret instantly

    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        arrowCollider = GetComponent<Collider2D>();
    }

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (arrowCollider == null) arrowCollider = GetComponent<Collider2D>();
    }

    public void Launch(Vector2 velocity, Collider2D shooter)
    {
        _stuck = false;
        _shooterToIgnore = shooter;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;        // straight flight
        rb.linearDamping = 0f;
        rb.freezeRotation = true;
        rb.linearVelocity = velocity;

        if (arrowCollider != null)
        {
            arrowCollider.isTrigger = true;  // detect first impact via trigger, then become solid
            if (_shooterToIgnore != null)
                Physics2D.IgnoreCollision(arrowCollider, _shooterToIgnore, true);
        }
    }

    void Update()
    {
        if (!_stuck && rotateToVelocity && rb != null)
        {
            Vector2 v = rb.linearVelocity;
            if (v.sqrMagnitude > 0.0001f)
            {
                float ang = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
            }
        }
    }



    private void OnTriggerEnter2D(Collider2D c)
    {
        if (_stuck) return;
        if (c == _shooterToIgnore) return;

        // If you decide to start with non-trigger collider, this also catches the first hit:
        if(c.gameObject == PlayerController.Instance.gameObject)
        {
            PlayerController.Instance.Die();
        }
        StickInto(c);
    }

    private void StickInto(Collider2D hitCol)
    {
        _stuck = true;

        // Stop & freeze
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Become solid + harmless
        if (arrowCollider != null)
        {
            if (_shooterToIgnore != null)
                Physics2D.IgnoreCollision(arrowCollider, _shooterToIgnore, false);

            arrowCollider.isTrigger = false;

            if (!string.IsNullOrEmpty(stuckLayerName))
            {
                int layer = LayerMask.NameToLayer(stuckLayerName);
                if (layer != -1) gameObject.layer = layer;
            }
        }

        // Ride moving platforms if desired
        if (parentToHit && hitCol != null)
            transform.SetParent(hitCol.transform, true);
    }
}
