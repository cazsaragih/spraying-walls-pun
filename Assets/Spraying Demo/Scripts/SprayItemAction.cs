using Opsive.UltimateCharacterController.Items.Actions;
using UnityEngine;
using PaintIn3D;
using Opsive.UltimateCharacterController.Camera;

public class SprayItemAction : UsableItem
{
    [SerializeField] private Color m_PaintColor;
    [SerializeField] private Camera m_Camera;
    [SerializeField] private float m_SprayDistance = 3.5f;

    public Color PaintColor { get { return m_PaintColor; } set { m_PaintColor = value; } }

    private GameObject m_PaintObject;
    private P3dHitScreen m_P3dHitScreen;
    private P3dPaintSphere m_P3dPaintSphere;
    private CameraController m_CameraController;

    protected override void Awake()
    {
        base.Awake();

        if (m_Camera == null)
            m_Camera = Camera.main;

        m_PaintObject = new GameObject("Paint");
        m_P3dHitScreen = m_PaintObject.AddComponent<P3dHitScreen>();
        m_P3dPaintSphere = m_PaintObject.AddComponent<P3dPaintSphere>();
        m_P3dPaintSphere.Color = m_PaintColor;
        m_CameraController = m_Camera.GetComponent<CameraController>();
    }

    protected override void Start()
    {
        base.Start();

        m_P3dHitScreen.enabled = false;
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
                EnableP3dHitScreen();
        }
        else
            DisableP3dHitScreen();

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

    public override void UseItem()
    {
        Debug.Log("USE ITEM");
        base.UseItem();
    }
}
