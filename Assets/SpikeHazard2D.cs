using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpikeHazard2D : MonoBehaviour
{
    [Tooltip("Tag used to identify the player object.")]
    public string playerTag = "Player";

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
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
            Debug.LogWarning("SpikeHazard2D: Player hit but no PlayerController found.");
        }
    }
}
