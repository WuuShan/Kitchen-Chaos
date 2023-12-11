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
            }
            else // 玩家没有携带任何东西
            {
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }
}