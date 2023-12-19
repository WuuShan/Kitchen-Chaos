using System;
using System.Collections;
using System.Collections.Generic;
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
        if (!validKitchenObjectSOList.Contains(kitchenObjectSO))
        {
            // Not a valid ingredient
            return false;
        }
        if (kitchenObjectSOList.Contains(kitchenObjectSO))
        {
            // Already has this type
            return false;
        }
        else
        {
            kitchenObjectSOList.Add(kitchenObjectSO);

            OnIngredientAdded?.Invoke(this, new() { kitchenObjectSO = kitchenObjectSO });

            return true;
        }
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOList;
    }
}