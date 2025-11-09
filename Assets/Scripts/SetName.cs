using DG.Tweening;
using TMPro;
using UnityEngine;

public class SetName : MonoBehaviour
{
    public LetterSelect[] letterSelects;
    public string currentName = "     ";


    public TMP_Text warningText;

    private bool concreted = false;

    public CanvasGroup NameSelect, LevelSelect;

    public void PressPlay()
    {
        bool attempt = false;
        foreach (char letter in currentName.ToCharArray())
        {

            if (letter != ' ')
            {
                attempt = true;
            }
        }

        if (attempt)
        {
                ChangeMenu();
                ConcreteName();
        }
        else
        {
            warningText.DOFade(1, 3f).OnComplete(() =>
            {
                warningText.DOFade(0, 2f);
            });
        }
    }


    public void updateCurrentName()
    {
        if (!concreted)
        {

            string newName = "";
            for (int i = 0; i < letterSelects.Length; i++)
            {

                newName += letterSelects[i].GetCurrentLetter();
            }
            currentName = newName;
        }
    }


    private void ConcreteName()
    {
        concreted = true;
        PlayerSession.PlayerName = currentName;
    }

    private void ChangeMenu()
    {
        NameSelect.DOFade(0, 0.75f).OnComplete(() =>
        {
            NameSelect.interactable = false;
            NameSelect.blocksRaycasts = false;
        });

        LevelSelect.DOFade(1, 0.75f).OnComplete(() =>
        {
            LevelSelect.interactable = true;
            LevelSelect.blocksRaycasts = true;
        });
    }


}
