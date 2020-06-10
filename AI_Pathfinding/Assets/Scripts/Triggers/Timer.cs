using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : Trigger
{
    [SerializeField] private float activation_time;
    private float timer = 0.0f;

    override protected bool Trigger_Process()
    {
        timer += Time.deltaTime;

        if (timer > activation_time)
        {
            return true;
        }
        return false;
    }
}
