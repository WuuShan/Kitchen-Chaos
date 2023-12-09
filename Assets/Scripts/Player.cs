using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;

    private bool isWalking;

    private void Update()
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

    public bool IsWalking()
    {
        return isWalking;
    }
}