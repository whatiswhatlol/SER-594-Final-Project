using Dan.Main;
using DG.Tweening;
using UnityEngine;

public class RemoveMainMenu : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public CanvasGroup nameselect;


public void Activate()
    {
        LeaderboardCreator.ResetPlayer();

        canvasGroup.DOFade(0, 0.75f).OnComplete(() =>
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
        });

        nameselect.DOFade(1, 0.75f).OnComplete(() =>
        {
            nameselect.interactable = true;
            nameselect.blocksRaycasts = true;
        });

    }
}
