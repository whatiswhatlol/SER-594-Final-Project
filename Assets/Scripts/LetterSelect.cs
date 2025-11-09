using UnityEngine;
using TMPro;

public class LetterSelect : MonoBehaviour
{
    [Header("UI Target")]
    public TMP_Text displayText;

    // Characters: A–Z + space
    private readonly char[] characters =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ ".ToCharArray();

    // Current index in character list
    private int index = 26;
    private void Start()
    {
        UpdateText();
    }
    // Call this from a button
    public void NextLetter()
    {
        index++;
        if (index >= characters.Length)
            index = 0;

        UpdateText();
    }

    // Call this from another button
    public void PreviousLetter()
    {
        index--;
        if (index < 0)
            index = characters.Length - 1;

        UpdateText();
    }

    private void UpdateText()
    {
        if (displayText != null)
            displayText.text = characters[index].ToString();
    }

    // Optional: get current letter if needed
    public string GetCurrentLetter()
    {
        return characters[index].ToString();
    }
}
