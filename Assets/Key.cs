using UnityEngine;

public class Key : MonoBehaviour
{

    public Portal2D Portal;
    public SpriteRenderer Renderer;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Portal.unlock();
            Renderer.enabled = false;
        }
    }
}
