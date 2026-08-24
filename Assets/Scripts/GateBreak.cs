using UnityEngine;

public class GateBreak : MonoBehaviour
{

    Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Excavator"))
        {
            rb.isKinematic = false;
        }
    }
}

