// GameManager.cs
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameObject playerPrefab;
    public Vector3 spawnPosition;

    protected override void Awake()
    {
        base.Awake();
    }

    public void ResetGame()
    {
        // 销毁当前玩家
        GameObject currentPlayer = GameObject.FindWithTag("Player");
        if (currentPlayer)
            Destroy(currentPlayer);

        // 重新生成玩家
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }
}
