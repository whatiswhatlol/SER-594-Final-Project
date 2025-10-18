using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private float travelSpeed = 5f;

    private float timer = 0f;
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > lifetime)
        {
            Destroy(gameObject);
        }
        
        transform.Translate(Vector3.right * (travelSpeed * Time.deltaTime));
    }
}
