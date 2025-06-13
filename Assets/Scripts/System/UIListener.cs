using UnityEngine;
using UnityEngine.EventSystems;

public class UIListener : MonoBehaviour
{
    [Header("拖入第一人称控制器对象（包含控制脚本）")]
    public GameObject firstPersonController;

    [Header("是否启用监听")]
    public bool enableListening = true;

    private MonoBehaviour[] controllerScripts;
    private bool uiIsActive = false;

    void Start()
    {
        if (firstPersonController != null)
        {
            controllerScripts = firstPersonController.GetComponents<MonoBehaviour>();
        }
    }

    void Update()
    {
        if (!enableListening) return;

        // 判断当前是否有 UI 在响应
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (!uiIsActive)
            {
                DisableFirstPersonController();
                uiIsActive = true;
            }
        }
        else
        {
            if (uiIsActive)
            {
                EnableFirstPersonController();
                uiIsActive = false;
            }
        }
    }

    void DisableFirstPersonController()
    {
        if (controllerScripts == null) return;
        foreach (var script in controllerScripts)
        {
            script.enabled = false;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void EnableFirstPersonController()
    {
        if (controllerScripts == null) return;
        foreach (var script in controllerScripts)
        {
            script.enabled = true;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
