using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家逻辑
/// </summary>
public class Player : MonoBehaviour
{
    /// <summary>
    /// 移动速度
    /// </summary>
    [SerializeField] private float moveSpeed = 7f;

    /// <summary>
    /// 获取玩家的输入
    /// </summary>
    [SerializeField] private GameInput gameInput;

    /// <summary>
    /// 柜台图层蒙版，用于射线单一检测
    /// </summary>
    [SerializeField] private LayerMask countersLayerMask;

    /// <summary>
    /// 判断是否在移动中
    /// </summary>
    private bool isWalking;

    /// <summary>
    /// 最后一次交互的方向
    /// </summary>
    private Vector3 lastInteractDir;

    private void Update()
    {
        HandleMovement();
        HandleInteractions();
    }

    /// <summary>
    /// 获取当前是否在移动中
    /// </summary>
    /// <returns></returns>
    public bool IsWalking()
    {
        return isWalking;
    }

    /// <summary>
    /// 处理交互逻辑
    /// </summary>
    private void HandleInteractions()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero)    // 如果没有移动方向的输入
        {
            lastInteractDir = moveDir;
        }

        float interactDistance = 2; // 检测交互的距离
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out ClearCounter clearCounter))
            {
                // Has ClearCounter
                clearCounter.Interact();
            }
        }
    }

    /// <summary>
    /// 处理移动逻辑
    /// </summary>
    private void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;    // 移动距离
        float playerRadius = 0.7f;  // 胶囊体的半径。
        float playerHeight = 2f;    // 胶囊体的高度。
        bool canMove = !Physics.CapsuleCast(                // 创建胶囊体投射出去扫描
            transform.position,                             // 胶囊体在 start 处的球体中心。
            transform.position + Vector3.up * playerHeight, // 胶囊体在 end 处的球体中心。
            playerRadius, moveDir, moveDistance);           // 扫描胶囊体的半径、方向、最大距离。

        if (!canMove)   // 无法朝移动方向移动
        {
            // 尝试在 X 轴上移动
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);

            if (canMove)    // 只能在 X 轴上移动
            {
                moveDir = moveDirX;
            }
            else   // 不能在 X 轴上移动
            {
                // 尝试在 Z 轴上移动
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);

                if (canMove)    // 只能在 Z 轴上移动
                {
                    moveDir = moveDirZ;
                }
                else    // 无法向任何方向移动
                {
                }
            }
        }

        if (canMove)    // 即将移动的位置上没有阻碍则移动到该位置
        {
            transform.position += moveDir * moveDistance;
        }

        isWalking = moveDir != Vector3.zero;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
    }
}