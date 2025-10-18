using UnityEngine;
using UnityEngine.Assertions;

public class KeyBehavior : MonoBehaviour
{
    public GameObject door;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assert.IsTrue(door != null);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.transform.tag == "Player")
        {
            door.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
