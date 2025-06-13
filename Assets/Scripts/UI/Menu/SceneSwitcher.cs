using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景切换器，支持加载指定场景并配合LoadingScreen淡入淡出
/// </summary>
public class SceneSwitcher : MonoBehaviour
{
    public float loadingDelay = 0.5f; // 可根据LoadingScreen淡入时长调整

    public void SwitchToScene(string sceneName)
    {
        StartCoroutine(SwitchSceneCoroutine(sceneName));
    }

    private IEnumerator SwitchSceneCoroutine(string sceneName)
    {
        // 触发LoadingScreen淡入
        GameEvents.TriggerLoading(true);
         
        yield return new WaitForSeconds(loadingDelay);

        // 异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 触发LoadingScreen淡出
        GameEvents.TriggerLoading(false);
    }
}
