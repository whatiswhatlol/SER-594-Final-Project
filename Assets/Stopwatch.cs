using UnityEngine;
using TMPro;

public class Stopwatch : MonoBehaviour
{
    public TMP_Text timerText;

    private bool running = false;
    private float startTime = 0f;
    private float storedTime = 0f;

    void Update()
    {
        if (running)
        {
            float current = Time.time - startTime + storedTime;
            UpdateDisplay(current);
        }
        else
        {
            UpdateDisplay(storedTime);
        }
    }

    void UpdateDisplay(float total)
    {
        int minutes = Mathf.FloorToInt(total / 60f);
        int seconds = Mathf.FloorToInt(total % 60f);
        int ms = Mathf.FloorToInt((total - Mathf.Floor(total)) * 100f); // 2 digits

        if (timerText != null)
            timerText.text = $"{minutes:00}:{seconds:00}:{ms:00}";
    }

    // ----- Public Controls -----
    public void StartTimer()
    {
        if (!running)
        {
            running = true;
            startTime = Time.time;
        }
    }

    public void StopTimer()
    {
        if (running)
        {
            running = false;
            storedTime += Time.time - startTime;
        }
    }

    public void ResetTimer()
    {
        running = false;
        storedTime = 0f;
    }

    public float GetTotalTime()
    {
        if (running)
            return Time.time - startTime + storedTime;
        else
            return storedTime;
    }
}
