using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class CrossBowArrow : MonoBehaviour
{
    public float stickTime = 4f;
    public float destroyDelay = 0.1f;
    public LayerMask stickMask;

    private Rigidbody2D rb;
    private bool stuck;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (stuck) return;

        if ((stickMask.value & (1 << col.gameObject.layer)) != 0)
        {
            Stick(col);
        }
        else
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    void Stick(Collision2D col)
    {
        stuck = true;
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        rb.simulated = false;
        transform.SetParent(col.transform);
        StartCoroutine(UnstickAfterTime());
    }

    System.Collections.IEnumerator UnstickAfterTime()
    {
        yield return new WaitForSeconds(stickTime);
        if (this) Destroy(gameObject);
    }
}
