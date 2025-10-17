using UnityEngine;
using UnityEngine.Assertions;

public class LeverBehavior : MonoBehaviour
{
    bool activated;
    float activateTime;

    SpriteRenderer spriteRenderer;

    public float activateDuration = 5.0f;
    public GameObject door;
    public Sprite activatedSprite;
    Sprite deactivatedSprite;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        activated = false;
        door.SetActive(!activated);
        spriteRenderer = GetComponent<SpriteRenderer>();
        deactivatedSprite = spriteRenderer.sprite;

        Assert.IsTrue(spriteRenderer != null);
        Assert.IsTrue(door != null);
        Assert.IsTrue(activatedSprite != null);
        Assert.IsTrue(deactivatedSprite != null);
    }

    // Update is called once per frame
    void Update()
    {
        if (activated)
        {
            if (Time.time - activateTime >= activateDuration)
            {
                activated = false;
            }
        }

        if (activated)
        {
            spriteRenderer.sprite = activatedSprite; 
        }
        else
        {
            spriteRenderer.sprite = deactivatedSprite;
        }

        door.SetActive(!activated);
    }
    
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            activated = true;
            activateTime = Time.time;
        }
    }
}
