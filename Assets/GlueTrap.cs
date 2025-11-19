using UnityEngine;

public class GlueTrap : MonoBehaviour
{
    [Header("Slow Amount")]
    [Tooltip("Multiplier for movement speed while stuck in glue (0.3 = 70% slower).")]
    public float speedMultiplier = 0.3f;

    [Tooltip("Multiplier for acceleration and deceleration.")]
    public float accelMultiplier = 0.5f;

    private bool playerInside = false;

    // Cache original values
    private float originalSpeed;
    private float originalAccel;
    private float originalDecel;

    private PlayerController player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playerInside) return;
        if (!other.TryGetComponent(out player)) return;

        playerInside = true;

        // Cache original stats
        originalSpeed = player.moveSpeed;
        originalAccel = player.acceleration;
        originalDecel = player.deceleration;

        // Apply slow values
        player.moveSpeed *= speedMultiplier;
        player.acceleration *= accelMultiplier;
        player.deceleration *= accelMultiplier;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!playerInside) return;
        if (other.TryGetComponent<PlayerController>(out var p) && p == player)
        {
            Restore();
        }
    }

    private void OnDisable()
    {
        // Safety: scene reloads or disables will restore the player
        if (playerInside && player != null)
            Restore();
    }

    private void Restore()
    {
        if (player == null) return;

        player.moveSpeed = originalSpeed;
        player.acceleration = originalAccel;
        player.deceleration = originalDecel;

        playerInside = false;
        player = null;
    }
}
