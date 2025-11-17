using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SawbladeHazard2D : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Points the sawblade moves between, in order.")]
    public Transform[] waypoints;

    [Tooltip("Movement speed in units per second.")]
    public float moveSpeed = 5f;

    [Tooltip("Pause time at each waypoint.")]
    public float pauseAtPoint = 0f;

    [Tooltip("Ping-pong between ends instead of looping back to 0.")]
    public bool pingPong = true;

    [Header("Player")]
    public string playerTag = "Player";

    private int currentIndex = 0;
    private int direction = 1; // 1 forward, -1 backward for ping-pong
    private float pauseTimer = 0f;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        Transform target = waypoints[currentIndex];
        Vector3 currentPos = transform.position;
        Vector3 targetPos = target.position;

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(currentPos, targetPos, step);

        if (Vector3.Distance(transform.position, targetPos) <= 0.001f)
        {
            if (pauseAtPoint > 0f)
                pauseTimer = pauseAtPoint;

            AdvanceIndex();
        }
    }

    private void AdvanceIndex()
    {
        if (waypoints.Length <= 1) return;

        if (pingPong)
        {
            currentIndex += direction;

            if (currentIndex >= waypoints.Length)
            {
                currentIndex = waypoints.Length - 2;
                direction = -1;
            }
            else if (currentIndex < 0)
            {
                currentIndex = 1;
                direction = 1;
            }
        }
        else
        {
            currentIndex++;
            if (currentIndex >= waypoints.Length)
                currentIndex = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        var player = other.GetComponent<PlayerController>();
        if (player == null) player = PlayerController.Instance;

        if (player != null)
        {
            player.Die();
        }
        else
        {
            Debug.LogWarning("SawbladeHazard2D: Player hit but no PlayerController found.");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.1f);

            if (i + 1 < waypoints.Length && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
#endif
}
