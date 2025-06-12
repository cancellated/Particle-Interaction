using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    public CanvasGroup canvasGroup; // 关联UI的CanvasGroup组件
    public float fadeDuration = 0.5f;

    private void OnEnable()
    {
        GameEvents.OnGameReset += ShowLoading;
    }

    private void OnDisable()
    {
        GameEvents.OnGameReset -= ShowLoading;
    }

    private void ShowLoading()
    {
        StopAllCoroutines();
        StartCoroutine(FadeCanvas(1));
    }

    private void HideLoading()
    {
        StopAllCoroutines();
        StartCoroutine(FadeCanvas(0));
    }

    private IEnumerator FadeCanvas(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;
        while (time < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }
}
