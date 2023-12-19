using UnityEngine;

/// <summary>
/// 包含可烹饪、烹饪完成的厨房物品数据和烹饪时长
/// </summary>
[CreateAssetMenu()]
public class FryingRecipeSO : ScriptableObject
{
    /// <summary>
    /// 可烹饪的厨房物品数据
    /// </summary>
    public KitchenObjectSO input;

    /// <summary>
    /// 烹饪完成的厨房物品数据
    /// </summary>
    public KitchenObjectSO output;

    /// <summary>
    /// 烹饪时长
    /// </summary>
    public float fryingTimerMax;
}