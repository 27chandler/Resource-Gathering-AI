using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Basic_Movement : MonoBehaviour
{
    private AI_Memory memory;
    private AI_Vision vision;
    private Rigidbody rb;

    private Vector3 rotation = new Vector3();
    // Start is called before the first frame update
    void Start()
    {
        memory = GetComponent<AI_Memory>();
        vision = GetComponent<AI_Vision>();
        rb = GetComponent<Rigidbody>();

        rotation.y = 1000.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (memory.Check_Pos(transform.position + transform.forward) <= 0)
        {
            rb.MovePosition(transform.position + (transform.forward * 1.0f * Time.deltaTime));
        }
        else
        {
            Quaternion rotation_delta = Quaternion.Euler(rotation * Time.deltaTime);
            rb.MoveRotation(rb.rotation * rotation_delta);
        }
    }
}
