// Description: Controls the blackout screens on both the VR and non-VR players
// It makes use of layers and custom shaders to ensure that the blackout screen is rendered properly
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class BlackoutController : MonoBehaviour
{
    private Image blackoutImage;

    private void Awake()
    {
        blackoutImage = GetComponentInChildren<Image>();
        if (blackoutImage == null)
        {
            Debug.LogError("Image component not found in the children of the Canvas.");
            return;
        }

        SetImageAlpha(1);
    }

    public void FadeIn()
    {
        StartCoroutine(DoFade(1));
    }

    public void FadeOut()
    {
        StartCoroutine(DoFade(0));
    }


    public void FadeIn(string sceneToLoad)
    {
        StartCoroutine(DoFadeAndLoadScene(1, sceneToLoad));
    }

    private IEnumerator DoFade(float targetAlpha)
    {
        float startAlpha = blackoutImage.color.a;
        float time = 0f;

        while (time < PersistentDataManager.Instance.BlackoutFadeDuration)
        {
            time += Time.deltaTime;
            SetImageAlpha(Mathf.Lerp(startAlpha, targetAlpha, time / PersistentDataManager.Instance.BlackoutFadeDuration));
            yield return null;
        }

        SetImageAlpha(targetAlpha);
    }

    private IEnumerator DoFadeAndLoadScene(float targetAlpha, string sceneToLoad)
    {
        // This code is used to ensure that when transitioning scenes, the canvas is on top of other canvas elements
        // Normally, instructions are still visible on top of the canvas in game, but this lets us perform a true 'fade to black' effect during scene transitions
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            if (PersistentDataManager.Instance.IsVR)
            {
                canvas.sortingLayerName = "Default";
            }
            else
            {
                canvas.sortingOrder = 2;
            }
        }
        else
        {
            Debug.LogError("Canvas component not found in the children of the Canvas.");
        }

        yield return StartCoroutine(DoFade(targetAlpha));

        SceneManager.LoadScene(sceneToLoad);
    }

    // Used for testing, currently not in use
    private void SetImageAlpha(float alpha)
    {
        blackoutImage.color = new Color(blackoutImage.color.r, blackoutImage.color.g, blackoutImage.color.b, alpha);
    }

    public void RemoveBlackoutInstantly()
    {
        SetImageAlpha(0);
    }

    public void ReplaceBlackoutInstantly()
    {
        SetImageAlpha(1);
    }

}