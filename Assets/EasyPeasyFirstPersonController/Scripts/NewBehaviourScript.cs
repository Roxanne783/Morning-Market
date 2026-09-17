using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;        // 移动速度
    public float jumpForce = 6f;        // 跳跃力度

    [Header("Ground Check")]
    public float groundCheckDistance = 0.1f; // 地面检测射线长度（从角色底部向下）
    public LayerMask groundLayers = ~0;      // 哪些层视为地面（默认所有层）

    [Header("Popup (Collision)")]
    public GameObject popupPrefab;      // 弹窗预设（UI 或 world prefab）
    public float popupDuration = 2.5f;  // 弹窗存在时间（秒）
    public bool popupIsUI = true;       // 如果为 UI：会查找场景中的 Canvas 并把实例设为其子物体
    public Transform popupWorldSpawnPoint; // 若 popupIsUI == false，允许指定世界生成点（可为空，若为空则在碰撞位置生成）

    // 内部
    Rigidbody rb;
    Vector3 moveInput;
    bool wantJump = false;
    bool isGrounded = false;

    // 防止在短时间内重复生成弹窗（可按需调整或移除）
    float lastPopupTime = -10f;
    public float popupCooldown = 0.2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // 推荐 Rigidbody 设置：重力开启，Freeze Rotation (X,Y,Z) 如果你不想角色翻滚
    }

    void Update()
    {
        // 读输入（在 Update 中读取输入，物理在 FixedUpdate 中执行）
        float h = Input.GetAxis("Horizontal"); // A/D or 左右箭头
        float v = Input.GetAxis("Vertical");   // W/S or 上下箭头

        // 将输入映射到世界/朝向移动（这里使用相机方向进行移动是更常用的方式）
        // 如果你想用人物局部方向改为 transform.forward/transform.right
        Vector3 camForward = Camera.main ? Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized : Vector3.forward;
        Vector3 camRight = Camera.main ? Camera.main.transform.right : Vector3.right;
        moveInput = (camForward * v + camRight * h).normalized;

        // 跳跃按键（空格）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            wantJump = true;
        }
    }

    void FixedUpdate()
    {
        // 先做地面检测（射线从角色底部往下）
        isGrounded = CheckGrounded();

        // 移动 — 使用 Rigidbody.velocity 控制横向速度（保留 y 速度）
        Vector3 velocity = rb.velocity;
        Vector3 desiredVel = moveInput * moveSpeed;
        velocity.x = desiredVel.x;
        velocity.z = desiredVel.z;
        rb.velocity = velocity;

        // 跳跃（只有在接地时响应）
        if (wantJump && isGrounded)
        {
            // 先清除当前垂直速度再加力，保证跳跃一致性
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }

        wantJump = false;
    }

    bool CheckGrounded()
    {
        // 从角色中心底部稍内收一点向下射线检测
        float radius = 0.2f;
        Vector3 origin = transform.position + Vector3.up * 0.1f; // 角色底部稍上
        // 使用短射线检测或球形投射更稳健
        bool hit = Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hitInfo, groundCheckDistance + 0.1f, groundLayers, QueryTriggerInteraction.Ignore);
        return hit;
    }

    void OnCollisionEnter(Collision collision)
    {
        // 当发生碰撞时触发弹窗（如果 cooldown 允许）
        if (Time.time - lastPopupTime < popupCooldown) return;
        lastPopupTime = Time.time;

        // 你可以根据 collision.collider.tag 或 name 做过滤。例如只对特定 tag 触发：
        // if (collision.collider.CompareTag("Pickup")) { ... }

        // 生成弹窗
        SpawnPopup(collision);
    }

    void SpawnPopup(Collision collision)
    {
        if (popupPrefab == null) return;

        if (popupIsUI)
        {
            // 找到场景中的 Canvas（优先找到有 Canvas component 的 active 对象）
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("PlayerController: 找不到 Canvas。若弹窗为 UI，请在场景中添加 Canvas 或将 popupIsUI 设为 false。");
                // 作为回退：直接在世界中生成
                Instantiate(popupPrefabAtWorldPosition(collision));
                return;
            }

            // 实例化到 Canvas 下，重置局部 transform 以便 UI 位置为中心
            GameObject popupInstance = Instantiate(popupPrefab, canvas.transform);

            // 如果 popupPrefab 是一个具有 RectTransform 的 UI 元件，可以设置其锚点位于屏幕中心或碰撞点附近
            RectTransform rt = popupInstance.GetComponent<RectTransform>();
            if (rt != null)
            {
                // 下面设到屏幕中心（你可以改成将屏幕位置设为碰撞点）
                rt.anchoredPosition = Vector2.zero;
            }

            // 自动销毁
            Destroy(popupInstance, popupDuration);
        }
        else
        {
            // world-space 生成：在给定 spawn point 或碰撞点生成
            GameObject instance = Instantiate(popupPrefabAtWorldPosition(collision));
            Destroy(instance, popupDuration);
        }
    }

    GameObject popupPrefabAtWorldPosition(Collision collision)
    {
        Vector3 spawnPos;
        if (popupWorldSpawnPoint != null)
        {
            spawnPos = popupWorldSpawnPoint.position;
        }
        else
        {
            // 使用碰撞接触点的第一个点稍微向外偏移一点，防止重叠
            if (collision.contacts.Length > 0)
            {
                spawnPos = collision.contacts[0].point + collision.contacts[0].normal * 0.5f;
            }
            else
            {
                spawnPos = transform.position + transform.forward * 1f;
            }
        }

        GameObject instance = Instantiate(popupPrefab, spawnPos, Quaternion.identity);
        return instance;
    }

    // 可选：在编辑器里绘制地面检测帮助线
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawLine(origin, origin + Vector3.down * (groundCheckDistance + 0.1f));
        Gizmos.DrawWireSphere(origin + Vector3.down * (groundCheckDistance + 0.1f), 0.1f);
    }
}
