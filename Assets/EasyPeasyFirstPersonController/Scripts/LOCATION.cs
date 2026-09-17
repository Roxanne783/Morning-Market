using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ControllerRayGrab : MonoBehaviour
{
    [Header("Input")]
    // 如果你想用特定按键，可以改 CommonUsages.triggerButton 为你需要的
    public InputFeatureUsage<bool> grabButton = CommonUsages.triggerButton;

    [Header("Raycast")]
    public float rayDistance = 5f;
    public LayerMask grabbableLayer; // 指向包含 Grabbable 的层（可选）

    [Header("Refs")]
    public Transform holdParent; // 通常设置为控制器自身或手柄点

    private InputDevice device;
    private bool isGrabbing = false;

    private GameObject grabbedObject;
    private Rigidbody grabbedRb;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    void Start()
    {
        // 尝试获取和该 transform 对应的 XR InputDevice（基于特性）
        InitializeDevice();
    }

    void InitializeDevice()
    {
        // 简单策略：通过 role 判断左右
        XRNode node = XRNode.RightHand;
        if (name.ToLower().Contains("left")) node = XRNode.LeftHand;
        device = InputDevices.GetDeviceAtXRNode(node);
    }

    void Update()
    {
        if (!device.isValid)
        {
            InitializeDevice();
        }

        bool pressed = false;
        if (device.TryGetFeatureValue(grabButton, out pressed) && pressed)
        {
            if (!isGrabbing)
            {
                TryGrab();
            }
            else
            {
                // update held object's transform (跟随手柄)
                if (grabbedObject != null)
                {
                    grabbedObject.transform.position = holdParent.position;
                    grabbedObject.transform.rotation = holdParent.rotation;
                }
            }
        }
        else
        {
            if (isGrabbing)
            {
                Release();
            }
        }

        // 可视化射线（调试）
        Debug.DrawRay(holdParent.position, holdParent.forward * rayDistance, Color.green);
    }

    void TryGrab()
    {
        Ray r = new Ray(holdParent.position, holdParent.forward);
        if (Physics.Raycast(r, out RaycastHit hit, rayDistance, grabbableLayer))
        {
            GameObject go = hit.collider.gameObject;
            if (go.CompareTag("Grabbable"))
            {
                grabbedObject = go;
                grabbedRb = go.GetComponent<Rigidbody>();
                if (grabbedRb) grabbedRb.isKinematic = true;
                go.transform.SetParent(holdParent, true);
                isGrabbing = true;
            }
        }
    }

    void Release()
    {
        if (grabbedObject != null)
        {
            // 在释放时判断是否与放置区重叠 / 命中
            Collider[] hits = Physics.OverlapSphere(grabbedObject.transform.position, 0.3f);
            bool snapped = false;
            foreach (var c in hits)
            {
                if (c.CompareTag("PlacementZone"))
                {
                    // 使用放置区 transform 做吸附（可以自定义吸附逻辑）
                    grabbedObject.transform.position = c.transform.position;
                    grabbedObject.transform.rotation = c.transform.rotation;
                    snapped = true;

                    // 如果希望放置后不再受物理影响：
                    var rb = grabbedObject.GetComponent<Rigidbody>();
                    if (rb) { rb.isKinematic = true; rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
                    break;
                }
            }

            if (!snapped)
            {
                // 恢复物理行为
                if (grabbedRb)
                {
                    grabbedRb.isKinematic = false;
                    // 可尝试给一点手柄速度做投掷效果（可扩展：读 controller velocity）
                }
                grabbedObject.transform.SetParent(null, true);
            }
        }

        grabbedObject = null;
        grabbedRb = null;
        isGrabbing = false;
    }
}
