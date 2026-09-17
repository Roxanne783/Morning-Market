using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CUBE : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float maxPickUpDistance = 2f;

    private Rigidbody rb;
    private bool isGrounded;
    private GameObject objectBeingHeld;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Move();
        Jump();
        Grab();
    }

    void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.MovePosition(transform.position + movement * moveSpeed * Time.deltaTime);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void Grab()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (objectBeingHeld == null)
            {
                TryGrabObject();
            }
            else
            {
                ReleaseObject();
            }
        }
    }

    void TryGrabObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxPickUpDistance))
        {
            if (hit.collider.gameObject.CompareTag("PickUp"))
            {
                objectBeingHeld = hit.collider.gameObject;
                objectBeingHeld.transform.SetParent(transform);
                objectBeingHeld.GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }

    void ReleaseObject()
    {
        if (objectBeingHeld != null)
        {
            objectBeingHeld.GetComponent<Rigidbody>().isKinematic = false;
            objectBeingHeld.transform.SetParent(null);
            objectBeingHeld = null;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}