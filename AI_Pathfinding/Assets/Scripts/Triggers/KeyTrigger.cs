using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTrigger : Trigger
{
    [SerializeField] private KeyCode trigger_key;

    override protected bool Trigger_Process()
    {
        if (Input.GetKey(trigger_key))
        {
            return true;
        }
        return false;
    }
}
