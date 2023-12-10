using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 处理玩家的各种输入
/// </summary>
public class GameInput : MonoBehaviour
{
    public event EventHandler OnInteractAction;

    /// <summary>
    /// 玩家输入操作集
    /// </summary>
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        playerInputActions.Player.Interact.performed += Interact_performed;
    }

    private void Interact_performed(InputAction.CallbackContext context)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 获得归一化的移动方向
    /// </summary>
    /// <returns>向量长度为 1 的方向</returns>
    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        inputVector = inputVector.normalized;   // 归一化向量，使得向量长度保持为 1，避免斜走移动速度异常

        return inputVector;
    }
}