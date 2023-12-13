using System;

/// <summary>
/// 垃圾柜台
/// </summary>
public class TrashCounter : BaseCounter
{
    public static event EventHandler OnAnyObjectTrashed;

    public new static void ResetStaticData()
    {
        OnAnyObjectTrashed = null;
    }

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            player.GetKitchenObject().DestroySelf();

            OnAnyObjectTrashed?.Invoke(this, EventArgs.Empty);
        }
    }
}