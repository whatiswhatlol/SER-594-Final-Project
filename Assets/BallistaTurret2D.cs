using UnityEngine;

/// Stationary 2D ballista that aims at the player and fires only with clear LOS.
public class BallistaTurret2D : MonoBehaviour
{
    [Header("Targeting")]
    public Transform target;                 // assign player; or leave null and it finds by tag "Player"
    public Transform firePoint;              // arrow spawn point (at the tip)
    public LayerMask losBlockers;            // e.g. Walls, Ground; layers that block line of sight
    public float maxSightDistance = 50f;

    [Header("Firing")]
    public BallistaArrow2D arrowPrefab;
    public float arrowSpeed = 24f;
    public float fireCooldown = 1.25f;

    [Header("Aiming")]
    public bool rotateZTowardTarget = true;  // rotates so +X faces target
    public float aimSmoothing = 0.1f;        // 0 = instant

    private float _nextFireTime;

    void Awake()
    {
        if (target == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) target = go.transform;
        }
        if (firePoint == null) firePoint = transform;
    }

    void Update()
    {
        if (target == null || arrowPrefab == null) return;

        Vector2 from = firePoint.position;
        Vector2 to = target.position;
        Vector2 dir = (to - from).normalized;

        // Aim (optional smooth)
        if (rotateZTowardTarget)
        {
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (aimSmoothing <= 0f)
                transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
            else
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.AngleAxis(ang, Vector3.forward), 1f - Mathf.Exp(-Time.deltaTime / aimSmoothing));
        }

        // LOS check (one ray; earliest hit must be the target)
        RaycastHit2D hit = Physics2D.Raycast(from, dir, maxSightDistance, losBlockers | (1 << target.gameObject.layer));
        bool hasLOS = hit && hit.transform == target;

        if (!hasLOS) return;
        if (Time.time < _nextFireTime) return;

        Fire(dir);
        _nextFireTime = Time.time + fireCooldown;
    }

    private void Fire(Vector2 dir)
    {
        var arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        // rotate arrow so its +X points along dir
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);

        // launch perfectly straight
        arrow.Launch(dir * arrowSpeed, shooter: GetComponent<Collider2D>());
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (firePoint == null || target == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(firePoint.position, target.position);
    }
#endif
}
