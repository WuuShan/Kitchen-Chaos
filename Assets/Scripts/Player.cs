using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// 玩家逻辑
/// </summary>
public class Player : NetworkBehaviour, IKitchenObjectParent
{
    /// <summary>
    /// 任意玩家生成事件
    /// </summary>
    public static event EventHandler OnAnyPlayerSpawned;

    /// <summary>
    /// 任意拾取东西事件
    /// </summary>
    public static event EventHandler OnAnyPickedSomething;

    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
    }

    /// <summary>
    /// 玩家实例
    /// </summary>
    public static Player LocalInstance { get; private set; }

    /// <summary>
    /// 拾取东西事件
    /// </summary>
    public event EventHandler OnPickedSomething;

    /// <summary>
    /// 选中柜台更改事件
    /// </summary>
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;

    /// <summary>
    /// 选中柜台更改事件参数
    /// </summary>
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    /// <summary>
    /// 移动速度
    /// </summary>
    [SerializeField] private float moveSpeed = 7f;

    /// <summary>
    /// 柜台图层蒙版，用于射线单一检测
    /// </summary>
    [SerializeField] private LayerMask countersLayerMask;

    [SerializeField] private LayerMask collisionsLayerMask;

    /// <summary>
    /// 厨房物品拿住位置
    /// </summary>
    [SerializeField] private Transform kitchenObjectHoldPoint;

    [SerializeField] private List<Vector3> spawnPositionList;

    /// <summary>
    /// 判断是否在移动中
    /// </summary>
    private bool isWalking;

    /// <summary>
    /// 最后一次交互的方向
    /// </summary>
    private Vector3 lastInteractDir;

    /// <summary>
    /// 玩家选中的柜台
    /// </summary>
    private BaseCounter selectedCounter;

    /// <summary>
    /// 玩家拾取的厨房物品
    /// </summary>
    private KitchenObject kitchenObject;

    /// <summary>
    /// 当 <see cref="NetworkObject"/> 生成时被调用，消息处理程序已准备好注册并且网络已设置。
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }

        transform.position = spawnPositionList[(int)OwnerClientId];

        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == OwnerClientId && HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
    }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            return;
        }

        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            return;
        }

        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
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
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDir = new(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero)    // 如果没有移动方向的输入
        {
            lastInteractDir = moveDir;
        }

        float interactDistance = 2; // 检测交互的距离
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                // Has ClearCounter
                if (baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

    /// <summary>
    /// 处理移动逻辑
    /// </summary>
    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDir = new(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;    // 移动距离
        float playerRadius = 0.7f;  // 胶囊体的半径。
        bool canMove = !Physics.BoxCast(                // 创建胶囊体投射出去检测
            transform.position,                             // 胶囊体在 start 处的球体中心。
            Vector3.one * playerRadius, // 胶囊体在 end 处的球体中心。
            moveDir, Quaternion.identity, moveDistance,            // 检测胶囊体的半径、方向、最大距离。
            collisionsLayerMask);                           // 检测的目标图层蒙版

        if (!canMove)   // 无法朝移动方向移动
        {
            // 尝试在 X 轴上移动
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = (moveDir.x < -0.5f || moveDir.x > 0.5f) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirX, Quaternion.identity, moveDistance, collisionsLayerMask);

            if (canMove)    // 只能在 X 轴上移动
            {
                moveDir = moveDirX;
            }
            else   // 不能在 X 轴上移动
            {
                // 尝试在 Z 轴上移动
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = (moveDir.z < -0.5f || moveDir.z > 0.5f) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirZ, Quaternion.identity, moveDistance, collisionsLayerMask);

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

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new() { selectedCounter = selectedCounter });
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
            OnAnyPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}