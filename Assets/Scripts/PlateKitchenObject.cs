using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 盘子厨房物品
/// </summary>
public class PlateKitchenObject : KitchenObject
{
    /// <summary>
    /// 添加配菜事件
    /// </summary>
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;

    /// <summary>
    /// 添加配菜事件参数
    /// </summary>
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSO;
    }

    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;

    private List<KitchenObjectSO> kitchenObjectSOList;

    protected override void Awake()
    {
        base.Awake();

        kitchenObjectSOList = new();
    }

    /// <summary>
    /// 尝试添加配菜
    /// </summary>
    /// <param name="kitchenObjectSO"></param>
    /// <returns></returns>
    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (!validKitchenObjectSOList.Contains(kitchenObjectSO))    // 不是食谱中的配菜
        {
            return false;
        }
        if (kitchenObjectSOList.Contains(kitchenObjectSO))  // 已经包含该配菜
        {
            return false;
        }
        else
        {
            int kitchenObjectSOIndex = KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObjectSO);
            AddIngredientServerRpc(kitchenObjectSOIndex);

            return true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(int kitchenObjectSOIndex)
    {
        AddIngredientClientRpc(kitchenObjectSOIndex);
    }

    [ClientRpc]
    private void AddIngredientClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);

        kitchenObjectSOList.Add(kitchenObjectSO);

        OnIngredientAdded?.Invoke(this, new() { kitchenObjectSO = kitchenObjectSO });
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOList;
    }
}