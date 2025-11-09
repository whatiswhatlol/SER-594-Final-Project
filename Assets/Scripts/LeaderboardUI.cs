// LeaderboardUI.cs
using UnityEngine;
using TMPro;
using Dan.Main;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Leaderboard")]
    public string publicKey;
    [Tooltip("Set TRUE only if you uploaded negative scores to simulate ascending.")]
    public bool invertForDescendingBoards = false;

    [Header("UI")]
    public Transform contentParent;     // e.g., a Vertical Layout Group
    public GameObject rowPrefab;        // prefab with a TMP_Text component

    public void Refresh()
    {
        if (string.IsNullOrEmpty(publicKey) || contentParent == null || rowPrefab == null)
        {
            Debug.LogError("[LeaderboardUI] Missing setup.");
            return;
        }

        LeaderboardCreator.GetLeaderboard(
            publicKey,
            entries =>
            {
                foreach (Transform t in contentParent) Destroy(t.gameObject);
                foreach (var e in entries)
                {
                    int raw = e.Score;
                    int cs = invertForDescendingBoards ? Mathf.Abs(raw) : raw;
                    string timeText = FormatCentiseconds(cs);
                    var go = Instantiate(rowPrefab, contentParent);
                    var txt = go.GetComponentInChildren<TMP_Text>();
                    txt.text = $"{e.Rank,2}. {e.Username}  —  {timeText}";
                }
            },
            error => Debug.LogError("[LeaderboardUI] " + error)
        );
    }

    public static string FormatCentiseconds(int cs)
    {
        int totalSeconds = cs / 100;
        int centi = cs % 100;
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:00}:{seconds:00}:{centi:00}";
    }
}
