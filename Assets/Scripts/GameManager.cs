// // GameManager.cs
// using UnityEngine;

// public class GameManager : MonoBehaviour
// {
//     public static GameManager Instance;

//     public GameObject playerPrefab;
//     public Vector3 spawnPosition;

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     public void ResetGame()
//     {
//         // 销毁当前玩家
//         GameObject currentPlayer = GameObject.FindWithTag("Player");
//         if (currentPlayer)
//             Destroy(currentPlayer);

//         // 重新生成玩家
//         Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
//     }
// }
