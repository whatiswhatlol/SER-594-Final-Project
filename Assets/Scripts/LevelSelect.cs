using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    public string[] levelSceneNames;

    [Header("Optional Fade")]
    public Image blackout;
    public float fadeDuration = 1f;



    public void PlaySelected(int index)
    {
        string scene = levelSceneNames[index];

        if (blackout != null)
        {
            blackout.DOFade(1f, fadeDuration).SetUpdate(true).OnComplete(() =>
            {
                SceneManager.LoadScene(scene);
            });
        }
        else
        {
            SceneManager.LoadScene(scene);
        }
    }


}
