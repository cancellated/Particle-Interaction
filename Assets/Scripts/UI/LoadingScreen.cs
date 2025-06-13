using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 加载界面控制脚本，监听GameEvents.OnLoading事件，根据参数淡入淡出加载界面
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    #region 参数配置
    public CanvasGroup canvasGroup; // 关联UI的CanvasGroup组件，用于控制透明度
    public float fadeDuration = 0.5f; // 淡入淡出持续时间（秒）
    #endregion

    #region 生命周期管理
    private void Awake()
    {
        canvasGroup.alpha = 0f;
    }
    /// <summary>
    /// 组件启用时，注册事件监听
    /// </summary>
    private void OnEnable()
    {
        GameEvents.OnLoading += OnLoadingChanged;
    }

    /// <summary>
    /// 组件禁用时，注销事件监听
    /// </summary>
    private void OnDisable()
    {
        GameEvents.OnLoading -= OnLoadingChanged;
    }
    #endregion

    #region 事件响应
    /// <summary>
    /// 响应加载事件，根据show参数决定淡入或淡出
    /// </summary>
    /// <param name="show">true为淡入，false为淡出</param>
    private void OnLoadingChanged(bool show)
    {
        StopAllCoroutines(); // 停止当前所有淡入淡出协程，防止冲突
        if (show)
            StartCoroutine(FadeCanvas(1)); // 淡入（显示）
        else
            StartCoroutine(FadeCanvas(0)); // 淡出（隐藏）
    }
    #endregion

    #region 协程
    /// <summary>
    /// 协程：平滑改变CanvasGroup的alpha，实现淡入淡出
    /// </summary>
    /// <param name="targetAlpha">目标透明度（1为全显，0为全隐）</param>
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
        canvasGroup.alpha = targetAlpha; // 确保最终值精确
    }
    #endregion
}
