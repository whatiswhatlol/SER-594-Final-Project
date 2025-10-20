using UnityEngine;

public class FlipSpriteWithPlayer : MonoBehaviour
{
    private Transform player;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;
    }

    void LateUpdate()
    {
        if (!player || !sr) return;
        sr.flipX = player.localScale.x < 0;
    }
}
