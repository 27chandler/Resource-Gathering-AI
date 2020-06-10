using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nexus_Control : Block_Interactable
{
    [SerializeField] private GameObject ai_obj;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if (Get_Inventory().Get_Item_Quantity() >= 5)
        {
            Get_Inventory().Consume_Item("Copper Ore", 5);
            Instantiate(ai_obj, transform.position + new Vector3(1.0f, 0.0f, 0.0f), transform.rotation);
        }
    }
}
