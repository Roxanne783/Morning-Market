using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPHERE : MonoBehaviour
{
    public float rotationSpeed = 50f; // ��ת���ٶ�

    // ÿ֡����
    void Update()
    {
        // ������Χ������������ת
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    // �����������巢����ײʱ���ô˺���
    void OnCollisionEnter(Collision collision)
    {
        // ���ٵ�ǰ�������壩
        Destroy(gameObject);
    }
}