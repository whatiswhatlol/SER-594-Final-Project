using UnityEngine;

public class Ballista : MonoBehaviour
{
    [SerializeField] private GameObject arrow;
    [SerializeField] private float interval = 5f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            Instantiate(arrow, transform.position, Quaternion.identity);
        }

    }
}
