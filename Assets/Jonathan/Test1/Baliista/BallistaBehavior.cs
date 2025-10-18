using UnityEngine;
using UnityEngine.Assertions;

public class BallistaBehavior : MonoBehaviour
{
    GameObject player;
    float lastFireTime;

    public GameObject arrow;
    public float fireCooldown = 1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player");
        lastFireTime = Time.time;

        Assert.IsTrue(player != null);
        Assert.IsTrue(arrow != null);
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, player.transform.position) < 10.0f) {
	    transform.up = player.transform.position - transform.position;

            if (Time.time - lastFireTime >= fireCooldown) {
                Instantiate(arrow, transform.position + 0.5f * transform.up, transform.rotation);
                lastFireTime = Time.time;
            }
        }
    }
}
