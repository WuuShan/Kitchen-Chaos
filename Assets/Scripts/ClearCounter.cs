using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 清理柜台的逻辑
/// </summary>
public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    /// <summary>
    /// 交互逻辑
    /// </summary>
    public override void Interact(Player player)
    {
    }
}