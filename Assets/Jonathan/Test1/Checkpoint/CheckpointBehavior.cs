using UnityEngine;

public class CheckpointBehavior : MonoBehaviour
{
    public Sprite setSprite;
    public Sprite unsetSprite;

    SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = unsetSprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        switch (collider.transform.tag)
        {
            case "Player":
                spriteRenderer.sprite = setSprite;
                break;
        }
    }
}
