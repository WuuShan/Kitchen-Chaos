using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 炉灶柜台，烹饪厨房物品
/// </summary>
public class StoveCounter : BaseCounter, IHasProgress
{
    /// <summary>
    /// 烹饪进度变化事件
    /// </summary>
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    /// <summary>
    /// 烹饪状态变化事件
    /// </summary>
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;

    /// <summary>
    /// 烹饪状态事件参数
    /// </summary>
    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }

    /// <summary>
    /// 烹饪状态枚举
    /// </summary>
    public enum State
    {
        /// <summary>
        /// 等待烹饪
        /// </summary>
        Idle,

        /// <summary>
        /// 正在烹饪
        /// </summary>
        Frying,

        /// <summary>
        /// 完成烹饪
        /// </summary>
        Fried,

        /// <summary>
        /// 已烧焦
        /// </summary>
        Burned,
    }

    /// <summary>
    /// 烹饪食谱数据数组
    /// </summary>
    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;

    /// <summary>
    /// 烧焦食谱数据数组
    /// </summary>
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    /// <summary>
    /// 炉灶的烹饪状态 需要保证只在服务器上写入
    /// </summary>
    private NetworkVariable<State> state = new(State.Idle);

    /// <summary>
    /// 烹饪计时器 需要保证只在服务器上写入
    /// <para>用于记录 <see cref="State.Frying"/> 到 <see cref="State.Fried"/> 状态变换时间</para>
    /// </summary>
    private NetworkVariable<float> fryingTimer = new(0f);

    /// <summary>
    /// 当前烹饪的食谱数据
    /// </summary>
    private FryingRecipeSO fryingRecipeSO;

    /// <summary>
    /// 烧焦计时器 需要保证只在服务器上写入
    /// <para>用于记录 <see cref="State.Fried"/> 到 <see cref="State.Burned"/> 状态变换时间</para>
    /// </summary>
    private NetworkVariable<float> burningTimer = new(0f);

    /// <summary>
    /// 当前烧焦的食谱数据
    /// </summary>
    private BurningRecipeSO burningRecipeSO;

    public override void OnNetworkSpawn()
    {
        fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, new() { state = state.Value });

        if (state.Value == State.Burned || state.Value == State.Idle)
        {
            OnProgressChanged?.Invoke(this, new() { progressNormalized = 0f });
        }
    }

    private void FryingTimer_OnValueChanged(float previousValue, float newValue)
    {
        float fryingTimerMax = fryingRecipeSO != null ? fryingRecipeSO.fryingTimerMax : 1f;

        OnProgressChanged?.Invoke(this, new()
        {
            progressNormalized = fryingTimer.Value / fryingTimerMax
        });
    }

    private void BurningTimer_OnValueChanged(float previousValue, float newValue)
    {
        float burningTimerMax = burningRecipeSO != null ? burningRecipeSO.burningTimerMax : 1f;

        OnProgressChanged?.Invoke(this, new()
        {
            progressNormalized = burningTimer.Value / burningTimerMax
        });
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (HasKitchenObject())
        {
            switch (state.Value)
            {
                case State.Idle:
                    break;

                case State.Frying:
                    fryingTimer.Value += Time.deltaTime;

                    if (fryingTimer.Value > fryingRecipeSO.fryingTimerMax)    // 烹饪完成
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

                        state.Value = State.Fried;

                        burningTimer.Value = 0f;
                        SetBurningRecipeSOClientRpc(
                            KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectSO())
                            ); ;
                    }
                    break;

                case State.Fried:
                    burningTimer.Value += Time.deltaTime;

                    if (burningTimer.Value > burningRecipeSO.burningTimerMax) // 已烧焦
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

                        state.Value = State.Burned;
                    }
                    break;

                case State.Burned:
                    break;

                default:
                    break;
            }
        }
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())    // 这里没有厨房物品
        {
            if (player.HasKitchenObject())  // 玩家携带着东西
            {
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) // 玩家携带可以煎炒的东西
                {
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);

                    InteractLogicPlaceObjectOnCounterServerRpc(
                        KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectSO())
                        );
                }
            }
            else    // 玩家没有携带任何东西
            {
            }
        }
        else    // 这里有一个厨房物品
        {
            if (player.HasKitchenObject())  // 玩家携带着东西
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) // 玩家拿着一个盘子
                {
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        SetStateIdleServerRpc();
                    }
                }
            }
            else // 玩家没有携带任何东西
            {
                GetKitchenObject().SetKitchenObjectParent(player);

                SetStateIdleServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRpc()
    {
        state.Value = State.Idle;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSOIndex)
    {
        fryingTimer.Value = 0f;
        state.Value = State.Frying;

        SetFryingRecipeSOClientRpc(kitchenObjectSOIndex);
    }

    [ClientRpc]
    private void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);

        fryingRecipeSO = GetFryingRecipeSOWithInput(kitchenObjectSO);
    }

    [ClientRpc]
    private void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        burningRecipeSO = GetBurningRecipeSOWithInput(kitchenObjectSO);
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);

        return fryingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);

        if (fryingRecipeSO != null)
        {
            return fryingRecipeSO.output;
        }
        else
        {
            return null;
        }
    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (FryingRecipeSO fryingRecipeSO in fryingRecipeSOArray)
        {
            if (fryingRecipeSO.input == inputKitchenObjectSO)
            {
                return fryingRecipeSO;
            }
        }
        return null;
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray)
        {
            if (burningRecipeSO.input == inputKitchenObjectSO)
            {
                return burningRecipeSO;
            }
        }
        return null;
    }

    public bool IsFried()
    {
        return state.Value == State.Fried;
    }
}