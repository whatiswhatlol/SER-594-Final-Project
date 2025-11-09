// Portal2D.cs (if your game is 2D)
using UnityEngine;

public class Portal2D : MonoBehaviour
{
    public Stopwatch stopwatch;
    public RaceResultUploader uploader;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        stopwatch.StopTimer();
        uploader.SubmitFinalTime();
        Time.timeScale = 0f;

    }
}
