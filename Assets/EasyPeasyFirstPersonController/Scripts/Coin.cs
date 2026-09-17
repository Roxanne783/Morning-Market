using UnityEngine;

public class Coin : MonoBehaviour
{
    public float rotateSpeed = 80f; // 自转速度

    void Update()
    {
        // 让金币（Sphere）绕Y轴自转
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }

    // 触发碰撞（需要Sphere添加Collider并勾选IsTrigger）
    private void OnTriggerEnter(Collider other)
    {
        // 判断撞到的对象是否是主角（立方体）
        if (other.CompareTag("Player"))
        {
            // 金币消失
            Destroy(gameObject);
        }
    }
}
