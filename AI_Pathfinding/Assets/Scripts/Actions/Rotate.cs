using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : Action
{
    private Rigidbody rb;

    [SerializeField] private Quaternion rotation;
    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();

    }

    public override void Activate()
    {
        rb.MoveRotation(rotation);
    }
}
