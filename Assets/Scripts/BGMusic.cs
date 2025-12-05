using UnityEngine;

public class BGMusic : MonoBehaviour
{
    void Start()
    {
        GameObject backgroundMusic = GameObject.Find("BackgroundMusic");
        if (backgroundMusic != gameObject) {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(transform.gameObject);
    }
}
