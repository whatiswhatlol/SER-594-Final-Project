// Portal2D.cs (if your game is 2D)
using UnityEngine;

public class Portal2D : MonoBehaviour
{
    public Stopwatch stopwatch;
    public RaceResultUploader uploader;
    public bool isUnlocked = true;
    public SpriteRenderer renderer;
    public Sprite unlockedSprite;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isUnlocked)
        {
            stopwatch.StopTimer();
            uploader.SubmitFinalTime();
            Time.timeScale = 0f;
        }
    }

    public void unlock()
    {
        isUnlocked = true;
        renderer.sprite = unlockedSprite;
    }
}
