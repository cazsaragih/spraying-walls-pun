using Opsive.UltimateCharacterController.Items.Actions;
using UnityEngine;
using PaintIn3D;
using Opsive.UltimateCharacterController.Camera;
using Opsive.Shared.Inventory;
using Opsive.Shared.Events;
using Opsive.UltimateCharacterController.Character.Abilities.Items;

public class SprayItemAction : UsableItem
{
    [SerializeField] private Color m_PaintColor;
    [SerializeField] private Camera m_Camera;
    [SerializeField] private float m_SprayDistance = 3.5f;
    [SerializeField] protected ItemDefinitionBase m_ConsumableItemDefinition;
    public Color PaintColor { get { return m_PaintColor; } set { m_PaintColor = value; } }

    private GameObject m_PaintObject;
    private P3dHitScreen m_P3dHitScreen;
    private P3dPaintSphere m_P3dPaintSphere;
    private CameraController m_CameraController;
    private IItemIdentifier m_ConsumableItemIdentifier;
    private int m_FluidRemaining;
    private bool m_IsInSprayingRange;
    
    public int FluidRemaining 
    { 
        get { return m_FluidRemaining; } 
        private set 
        { 
            m_FluidRemaining = value;
            EventHandler.ExecuteEvent(m_Character, "OnItemUseConsumableItemIdentifier", m_Item, m_ConsumableItemIdentifier, m_FluidRemaining);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        if (m_Camera == null)
            m_Camera = Camera.main;

        if (m_ConsumableItemDefinition != null)
            m_ConsumableItemIdentifier = m_ConsumableItemDefinition.CreateItemIdentifier();

        m_PaintObject = new GameObject("Paint");
        m_P3dHitScreen = m_PaintObject.AddComponent<P3dHitScreen>();
        m_P3dPaintSphere = m_PaintObject.AddComponent<P3dPaintSphere>();
        m_P3dPaintSphere.Color = m_PaintColor;
        m_CameraController = m_Camera.GetComponent<CameraController>();

        EventHandler.RegisterEvent<IItemIdentifier, int, bool, bool>(m_Character, "OnInventoryPickupItemIdentifier", OnPickupItemIdentifier);
    }

    protected override void Start()
    {
        base.Start();

        DisableP3dHitScreen();
    }

    private void OnPickupItemIdentifier(IItemIdentifier itemIdentifier, int amount, bool immediatePickup, bool forceEquip) 
    {
        FluidRemaining += amount;
    }

    private void FixedUpdate()
    {
        RaycastHit hitInfo;
        Vector3 startPos = m_Camera.transform.position;
        Vector3 forward = m_Camera.transform.forward;
        float distance = m_SprayDistance - m_CameraController.AnchorOffset.z;
        if (Physics.Raycast(startPos, forward, out hitInfo, distance))
        {
            P3dPaintable p3d = hitInfo.collider.GetComponent<P3dPaintable>();
            if (p3d != null)
                m_IsInSprayingRange = true;
        }
        else
            m_IsInSprayingRange = false;

        Debug.DrawRay(startPos, forward * distance);
    }

    private void EnableP3dHitScreen()
    {
        m_P3dHitScreen.enabled = true;
    }

    private void DisableP3dHitScreen()
    {
        m_P3dHitScreen.enabled = false;
    }

    public override void StartItemUse(ItemAbility useAbility)
    {
        base.StartItemUse(useAbility);
        Debug.Log("StartItemUse");
        EnableP3dHitScreen();
    }

    public override void StopItemUse()
    {
        base.StopItemUse();
        Debug.Log("StopItemUse");
        DisableP3dHitScreen();
    }

    public override void UseItem()
    {
        base.UseItem();

        if (m_ConsumableItemIdentifier != null)
            FluidRemaining--;
    }

    public override bool CanUseItem(ItemAbility itemAbility, UseAbilityState abilityState)
    {
        if (!base.CanUseItem(itemAbility, abilityState, false))
            return false;

        if (m_FluidRemaining == 0)
            return false;

        if (m_IsInSprayingRange == false)
            return false;

        return true;
    }
}
