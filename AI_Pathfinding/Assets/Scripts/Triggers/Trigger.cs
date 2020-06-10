using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    [SerializeField] private Action activation_action;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Trigger_Process())
        {
            Trigger_Activate();
        }
    }

    protected virtual bool Trigger_Process()
    {
        return false;
    }

    private void Trigger_Activate()
    {
        activation_action.Activate();
    }

}
