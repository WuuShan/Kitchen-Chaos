using UnityEngine;

/// <summary>
/// 切割食谱
/// </summary>
[CreateAssetMenu()]
public class CuttingRecipeSO : ScriptableObject
{
    /// <summary>
    /// 被切的厨房物品
    /// </summary>
    public KitchenObjectSO input;

    /// <summary>
    /// 切好的厨房物品
    /// </summary>
    public KitchenObjectSO output;

    /// <summary>
    /// 需要切的次数
    /// </summary>
    public int cuttingProgressMax;
}