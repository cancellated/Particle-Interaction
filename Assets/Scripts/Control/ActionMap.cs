using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActionMap : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private FreeFloatController movementController;

    private void Awake()
    {
        // 确保Input System已启用
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }
    }

    // 启用/禁用控制模式
    public void SetControlEnabled(bool enabled)
    {
        playerInput.enabled = enabled;
    }
}
