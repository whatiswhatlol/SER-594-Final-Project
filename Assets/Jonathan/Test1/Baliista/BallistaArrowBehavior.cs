using UnityEngine;

public class BallistaArrowBehavior : MonoBehaviour
{
    new Rigidbody2D rigidbody;
    public float force = 3.0f * 9.8f;
    float spawnTime;
    float lifeDuration = 30.0f;																		

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.AddForce(force * transform.up, ForceMode2D.Impulse);
        spawnTime = Time.time;
        Destroy(gameObject, lifeDuration);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        bool destroy = false;

        switch (collider.transform.tag) {
            case "Player":
                destroy = true;
                break;
            default:

                if (LayerMask.LayerToName(collider.gameObject.layer) == "Terrain") {
                    destroy = true;
                }
                break;
        }

        if (destroy) {
            Destroy(gameObject);
        }
    }
}
