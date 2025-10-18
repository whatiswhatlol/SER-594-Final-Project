using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    PlayerController playerController;
    Vector3 startPosition;
    float moveSpeed;
    float jumpForce;
    bool dead;
    bool glued;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        startPosition = transform.position;
        moveSpeed = playerController.moveSpeed;
        jumpForce = playerController.jumpForce;
    }

    // Update is called once per frame
    void Update()
    {
        if (dead) {
             dead = false;
             transform.position = startPosition;
        } else {
        	if (glued) {
		    playerController.moveSpeed = 0.2f * moveSpeed;
		    playerController.jumpForce = 0.0f;
		} else {
		    playerController.moveSpeed = moveSpeed;
		    playerController.jumpForce = jumpForce;
		}
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        switch (collider.transform.tag)
        {
            case "Spike":
            case "Ballista Arrow":
            case "Sawblade":
                dead = true;
                break;
            case "Glue":
                glued = true;
                break;
            case "Portal":
                Debug.Log("pass level");
                break;
        }
    }
    
    void OnTriggerExit2D(Collider2D collider)
    {
        switch (collider.transform.tag)
        {
            case "Glue":
                glued = false;
                break;
        }
    }
}
