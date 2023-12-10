using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKitchenObjectParent
{
    /// <summary>
    /// 获取厨房物品跟随位置
    /// </summary>
    /// <returns></returns>
    public Transform GetKitchenObjectFollowTransform();

    /// <summary>
    /// 设置厨房物品
    /// </summary>
    /// <param name="kitchenObject"></param>
    public void SetKitchenObject(KitchenObject kitchenObject);

    /// <summary>
    /// 获取厨房物品
    /// </summary>
    /// <returns></returns>
    public KitchenObject GetKitchenObject();

    /// <summary>
    /// 清空的厨房物品
    /// </summary>
    public void ClearKitchenObject();

    /// <summary>
    /// 检查是否有厨房物品
    /// </summary>
    /// <returns></returns>
    public bool HasKitchenObject();
}