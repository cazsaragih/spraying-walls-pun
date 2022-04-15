using Opsive.Shared.Events;
using Opsive.Shared.Inventory;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Items.Actions;
using PaintIn3D;
using UnityEngine;

public class SprayItemAction : UsableItem, IHitPoint, IHitLine
{
    [SerializeField] private Color m_PaintColor;
    [SerializeField] private Camera m_Camera;
    [SerializeField] private float m_SprayDistance = 3.5f;
    [SerializeField] protected ItemDefinitionBase m_ConsumableItemDefinition;

    private P3dHitScreen m_P3dHitScreen;
    private P3dPaintSphere m_P3dPaintSphere;
    private CameraController m_CameraController;
    private IItemIdentifier m_ConsumableItemIdentifier;
    private int m_FluidRemaining;
    private bool m_IsInSprayingRange;
    private SprayNetworkCommand m_SprayNetworkCommand;
    private IHitPoint[] m_HitPoints;
    private IHitLine[] m_HitLines;

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

        m_P3dHitScreen = gameObject.AddComponent<P3dHitScreen>();
        m_P3dPaintSphere = gameObject.AddComponent<P3dPaintSphere>();
        m_P3dPaintSphere.Color = m_PaintColor;
        m_CameraController = m_Camera.GetComponent<CameraController>();
        m_SprayNetworkCommand = m_Character.GetComponent<SprayNetworkCommand>();
        m_HitPoints = gameObject.GetComponentsInChildren<IHitPoint>();
        m_HitLines = gameObject.GetComponentsInChildren<IHitLine>();

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

        EnableP3dHitScreen();
    }

    public override void StopItemUse()
    {
        base.StopItemUse();

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

    protected override void OnDestroy()
    {
        base.OnDestroy();

        EventHandler.UnregisterEvent<IItemIdentifier, int, bool, bool>(m_Character, "OnItemUseConsumableItemIdentifier", OnPickupItemIdentifier);
    }

    public void HandleHitPoint(bool preview, int priority, float pressure, int seed, Vector3 position, Quaternion rotation)
    {
        if (m_NetworkInfo == null || m_NetworkInfo.IsLocalPlayer())
        {
            if (m_NetworkCharacter != null && m_SprayNetworkCommand != null)
            {
                m_SprayNetworkCommand.HandleHitPoint(this, preview, priority, pressure, seed, position, rotation);
            }
        }
    }

    public void HandleHitLine(bool preview, int priority, float pressure, int seed, Vector3 position, Vector3 endPosition, Quaternion rotation, bool clip)
    {
        if (m_NetworkInfo == null || m_NetworkInfo.IsLocalPlayer())
        {
            if (m_NetworkCharacter != null && m_SprayNetworkCommand != null)
            {
                m_SprayNetworkCommand.HandleHitLine(this, preview, priority, pressure, seed, position, endPosition, rotation, clip);
            }
        }
    }

    public void BroadcastHitPoint(bool preview, int priority, float pressure, int seed, Vector3 position, Quaternion rotation)
    {
        // Loop through all components that implement IHitPoint
        foreach (var hitPoint in m_HitPoints)
        {
            // Ignore this one so we don't recursively paint
            if ((Object)hitPoint != this)
            {
                // Submit the hit point
                hitPoint.HandleHitPoint(preview, priority, pressure, seed, position, rotation);
            }
        }
    }

    public void BroadcastHitLine(bool preview, int priority, float pressure, int seed, Vector3 position, Vector3 endPosition, Quaternion rotation, bool clip)
    {
        // Loop through all components that implement IHitLine
        foreach (var hitLine in m_HitLines)
        {
            // Ignore this one so we don't recursively paint
            if ((Object)hitLine != this)
            {
                // Submit the hit line
                hitLine.HandleHitLine(preview, priority, pressure, seed, position, endPosition, rotation, clip);
            }
        }
    }
}
