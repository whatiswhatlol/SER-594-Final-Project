using UnityEngine;
using UnityEngine.Assertions;

public class PlayerBehavior : MonoBehaviour
{
    public GameObject[] levelSpawns;
    public int currentLevel;
    Vector3 spawn;

    PlayerController playerController;
    new Rigidbody2D rigidbody2D;
    float moveSpeed;
    float jumpForce;
    bool dead;
    bool glued;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assert.IsTrue(levelSpawns.Length != 0);
        foreach (GameObject levelSpawn in levelSpawns) {
            Assert.IsTrue(levelSpawn != null);
        }

        Assert.IsTrue(currentLevel < levelSpawns.Length);

        spawn = levelSpawns[currentLevel].transform.position;
        transform.position = spawn;

        playerController = GetComponent<PlayerController>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        moveSpeed = playerController.moveSpeed;
        jumpForce = playerController.jumpForce;
    }

    // Update is called once per frame
    void Update()
    {
        if (dead) {
             dead = false;
             transform.position = spawn;
        } else {
        	if (glued) {
		    rigidbody2D.linearDamping = 20f;
                    playerController.moveSpeed = 0.2f * moveSpeed;
		    playerController.jumpForce = 0.0f;
		} else {
		    rigidbody2D.linearDamping = 0f;
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
                if (currentLevel == levelSpawns.Length - 1)
                {
                    Debug.Log("last level");
                }
                else
                {
                    currentLevel++;
                    spawn = levelSpawns[currentLevel].transform.position;
                    transform.position = spawn;
                }
                break;
            case "Checkpoint":
                spawn = collider.transform.position;
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
