using System;
using UnityEngine;

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

    #region 对话API事件
    public static event Action<string> OnAIDialogueStart;
    public static event Action<string> OnAIDialogueComplete;
    
    public static void TriggerAIDialogueComplete(string response)
    {
        Debug.Log("[GameEvents] 触发AI对话完成事件");
        OnAIDialogueComplete?.Invoke(response);
    }
    #endregion
}
