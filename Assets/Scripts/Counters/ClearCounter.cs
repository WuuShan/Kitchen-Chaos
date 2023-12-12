using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 清理柜台的逻辑
/// </summary>
public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())    // 这里没有厨房物品
        {
            if (player.HasKitchenObject())  // 玩家携带着东西
            {
                player.GetKitchenObject().SetKitchenObjectParent(this);
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
                        GetKitchenObject().DestroySelf();
                    }
                }
                else    // 玩家携带的不是盘子而是其他东西
                {
                    if (GetKitchenObject().TryGetPlate(out plateKitchenObject))  // 柜台上有一个盘子
                    {
                        if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                        {
                            player.GetKitchenObject().DestroySelf();
                        }
                    }
                }
            }
            else // 玩家没有携带任何东西
            {
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }
}