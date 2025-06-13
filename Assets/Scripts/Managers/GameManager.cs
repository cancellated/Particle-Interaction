// Assets\Scripts\Managers\GameManager.cs
using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameObject playerPrefab;
    public Vector3 spawnPosition;

    #region 生命周期管理
    protected override void Awake()
    {
        base.Awake();
        // 订阅游戏重置事件
        GameEvents.OnGameReset += HandleGameReset;
    }

    private void OnDestroy()
    {
        // 取消订阅
        GameEvents.OnGameReset -= HandleGameReset;
    }

    #endregion

    #region 事件处理
    public void ResetGame()
    {
        // 触发游戏重置事件
        GameEvents.TriggerGameReset();
    }

    private void HandleGameReset()
    {
        // 销毁当前玩家
        GameObject currentPlayer = GameObject.FindWithTag("Player");
        if (currentPlayer)
            Destroy(currentPlayer);

        // 重新生成玩家
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }

    #endregion
}