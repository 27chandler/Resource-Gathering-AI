using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private float movement_speed;
    [Space]
    [SerializeField] private KeyCode left_move;
    [SerializeField] private KeyCode right_move;
    [SerializeField] private KeyCode forward_move;
    [SerializeField] private KeyCode backward_move;

    private float rotation_x;
    

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    
    }

    private void Movement()
    {
        if (Input.GetKey(left_move))
        {
            rb.AddForce(transform.right * -movement_speed);
        }

        if (Input.GetKey(right_move))
        {
            rb.AddForce(transform.right * movement_speed);
        }

        if (Input.GetKey(forward_move))
        {
            rb.AddForce(transform.forward * movement_speed);
        }

        if (Input.GetKey(backward_move))
        {
            rb.AddForce(transform.forward * -movement_speed);
        }
    }

    private void Update()
    {
        Movement();
    }
}
