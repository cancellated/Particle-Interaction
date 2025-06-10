using System;
using UnityEngine;
using GestureControl.Data;

/// <summary>
/// 全局事件系统，用于模块间通信
/// </summary>
public static class GameEvents
{
    #region 游戏事件
    public static event Action OnGameReset;
    
    // 触发游戏重置事件
    public static void TriggerGameReset()
    {
        Debug.Log("[GameEvents] 触发游戏重置事件");
        OnGameReset?.Invoke();
    }
    #endregion

    #region 对话流程事件
    public static event Action OnAIDialogueStart;
    public static event Action<string> OnAIDialogueResponse;
    public static event Action OnAIDialogueComplete;


    // 触发AI对话开始事件
    public static void TriggerAIDialogueStart()
    {
        Debug.Log("[GameEvents] 触发AI对话流程开始");
        OnAIDialogueStart?.Invoke();
    }

    // 触发AI对话响应事件
        public static void TriggerAIDialogueResponse(string message)
    {
        OnAIDialogueResponse?.Invoke(message);
    }
    
    // 触发AI对话完成事件
    public static void TriggerAIDialogueComplete()
    {
        Debug.Log("[GameEvents] 触发AI对话流程完成");
        OnAIDialogueComplete?.Invoke();
    }
    #endregion

    #region 手部手势事件
    public static event Action<HandData> OnHandGestureDetected;
    public static event Action<HandData> OnHandGestureStarted;
    public static event Action<HandData> OnHandGestureEnded;

    // 触发手势检测事件
    public static void TriggerHandGestureDetected(HandData data)
    {
        OnHandGestureDetected?.Invoke(data);
    }

    // 触发手势开始事件
    public static void TriggerHandGestureStarted(HandData data)
    {
        OnHandGestureStarted?.Invoke(data);
    }

    // 触发手势结束事件
    public static void TriggerHandGestureEnded(HandData data)
    {
        OnHandGestureEnded?.Invoke(data);
    }
    #endregion
}
