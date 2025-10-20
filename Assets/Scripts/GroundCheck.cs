using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public LayerMask groundLayer;
    public Transform checkPoint;
    public float checkRadius = 0.15f;

    public bool IsGrounded()
    {
        Vector2 pos = checkPoint ? (Vector2)checkPoint.position : (Vector2)transform.position + Vector2.down * 0.1f;
        return Physics2D.OverlapCircle(pos, checkRadius, groundLayer) != null;
    }

    void OnDrawGizmosSelected()
    {
        Vector2 pos = checkPoint ? (Vector2)checkPoint.position : (Vector2)transform.position + Vector2.down * 0.1f;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pos, checkRadius);
    }
}
