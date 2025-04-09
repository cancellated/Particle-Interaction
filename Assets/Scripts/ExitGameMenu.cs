using UnityEngine;

public class ExitGameMenu : MonoBehaviour
{
    public GameObject exitMenuUI;
    public GameObject targetObject; // 需要重置位置的物体
    private Vector3 initialPosition; // 初始位置
    private Quaternion initialRotation; // 初始旋转（可选）

    void Start()
    {
        // 记录目标物体的初始位置和旋转
        if (targetObject != null)
        {
            initialPosition = targetObject.transform.position;
            initialRotation = targetObject.transform.rotation;
        }

        // 确保菜单关闭
        exitMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        bool shouldActivate = !exitMenuUI.activeSelf;
        exitMenuUI.SetActive(shouldActivate);
        Time.timeScale = shouldActivate ? 0f : 1f;
    }

    // 重置特定物体位置的方法
    public void ResetTargetPosition()
    {
        if (targetObject != null)
        {
            // 重置位置和旋转
            targetObject.transform.position = initialPosition;
            targetObject.transform.rotation = initialRotation;

            // 如果有刚体，重置物理状态
            Rigidbody rb = targetObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // 恢复游戏时间并关闭菜单
        Time.timeScale = 1f;
        exitMenuUI.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
