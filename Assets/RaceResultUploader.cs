// RaceResultUploader.cs
using UnityEngine;
using TMPro;
using Dan.Main;
using DG.Tweening;
using UnityEngine.UI;  // Leaderboard Creator namespace

public class RaceResultUploader : MonoBehaviour
{
    [Header("Links")]
    public Stopwatch stopwatch;           // your existing Stopwatch component
    [Tooltip("Leaderboard public key from Danqzq's dashboard")]
    public string publicKey;
    
    public CanvasGroup canvasGroup;
    public Image BlackOut;

    [Header("Behavior")]
    [Tooltip("If your leaderboard is configured ASCENDING (lower = better), leave this OFF. Turn ON only if the board is DESCENDING and you cannot change it.")]
    public bool invertForDescendingBoards = false;

    [Header("UI (optional)")]
    public LeaderboardUI leaderboardUI;
    public TMP_Text statusText;

    public void SubmitFinalTime()
    {
        if (stopwatch == null || string.IsNullOrEmpty(publicKey))
        {
            SetStatus("Missing stopwatch or public key.");
            return;
        }

        float totalSeconds = stopwatch.GetTotalTime();
        int centiseconds = Mathf.RoundToInt(totalSeconds * 100f);

        int scoreToUpload = invertForDescendingBoards ? -centiseconds : centiseconds;
        string playerName = string.IsNullOrWhiteSpace(PlayerSession.PlayerName)
            ? "Player" : PlayerSession.PlayerName;

        SetStatus("Submitting...");

        LeaderboardCreator.UploadNewEntry(
            publicKey,
            playerName,
            scoreToUpload,
            (ok) => SetStatus(ok ? "Submit OK" : "Submit failed"),
            (err) => SetStatus("Error: " + err)
        );

        BlackOut
            .DOFade(1f, 1f)
            .SetUpdate(true)                  
            .OnComplete(() =>
            {
                leaderboardUI.Refresh();

                canvasGroup
                    .DOFade(1f, 1f)
                    .SetUpdate(true).OnComplete(()=>
                    {
                        Time.timeScale = 1f;
                    });         
            });
    }

    private void SetStatus(string s)
    {
        if (statusText != null) statusText.text = s;
        else Debug.Log("[RaceResultUploader] " + s);
    }
}
