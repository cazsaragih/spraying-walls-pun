using Opsive.UltimateCharacterController.Items.Actions;
using UnityEngine;

public class SprayItemAction : UsableItem
{
    public override void UseItem()
    {
        Debug.Log("USE ITEM");
        base.UseItem();
    }

}
