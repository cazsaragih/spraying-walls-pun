using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items.Actions;
using PaintIn3D;
using Photon.Pun;
using UnityEngine;

public class SprayNetworkCommand : MonoBehaviour
{
    private PhotonView photonView;
    private InventoryBase m_Inventory;

    private void Awake()
    {
        m_Inventory = gameObject.GetComponent<InventoryBase>();
    }

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }
    private ItemAction GetItemAction(int slotID, int actionID)
    {
        var item = m_Inventory.GetActiveItem(slotID);
        if (item == null)
        {
            return null;
        }
        return item.GetItemAction(actionID);
    }

    public void HandleHitPoint(ItemAction itemAction, bool preview, int priority, float pressure, int seed, Vector3 position, Quaternion rotation)
    {
        photonView.RPC("HandleHitPointRPC", RpcTarget.Others, itemAction.Item.SlotID, itemAction.ID, preview, priority, pressure, seed, position, rotation);
    }

    public void HandleHitLine(ItemAction itemAction, bool preview, int priority, float pressure, int seed, Vector3 position, Vector3 endPosition, Quaternion rotation, bool clip)
    {
        photonView.RPC("HandleHitLineRPC", RpcTarget.Others, itemAction.Item.SlotID, itemAction.ID, preview, priority, pressure, seed, position, endPosition, rotation, clip);
    }

    [PunRPC]
    private void HandleHitPointRPC(int slotID, int actionID, bool preview, int priority, float pressure, int seed, Vector3 position, Quaternion rotation)
    {
        var sprayItemAction = GetItemAction(slotID, actionID) as SprayItemAction;
        sprayItemAction.BroadcastHitPoint(preview, priority, pressure, seed, position, rotation);
    }

    [PunRPC]
    private void HandleHitLineRPC(int slotID, int actionID, bool preview, int priority, float pressure, int seed, Vector3 position, Vector3 endPosition, Quaternion rotation, bool clip)
    {
        var sprayItemAction = GetItemAction(slotID, actionID) as SprayItemAction;
        sprayItemAction.BroadcastHitLine(preview, priority, pressure, seed, position, endPosition, rotation, clip);
    }
}
