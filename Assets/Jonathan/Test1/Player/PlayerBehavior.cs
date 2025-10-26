using UnityEngine;
using UnityEngine.Assertions;

public class PlayerBehavior : MonoBehaviour
{
    GameObject[] levels;
    int currentLevel;

    PlayerController playerController;
    float moveSpeed;
    float jumpForce;
    bool dead;
    bool glued;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string[] levelNames = new string[]{
            "Level 1 Spawn",
            "Level 2 Spawn",
        };
        levels = new GameObject[2];
        Assert.IsTrue(levels.Length == levelNames.Length);
        for (int i = 0; i < levelNames.Length; i++)
        {
            levels[i] = GameObject.Find(levelNames[i]);
            Assert.IsTrue(levels[i] != null);
        }

        currentLevel = 0;

        playerController = GetComponent<PlayerController>();
        moveSpeed = playerController.moveSpeed;
        jumpForce = playerController.jumpForce;
    }

    // Update is called once per frame
    void Update()
    {
        if (dead) {
             dead = false;
             transform.position = levels[currentLevel].transform.position;
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
                if (currentLevel == levels.Length - 1)
                {
                    Debug.Log("last level");
                }
                else
                {
                    currentLevel++;
                    transform.position = levels[currentLevel].transform.position;
                }
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
