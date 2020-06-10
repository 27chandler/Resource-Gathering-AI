using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block_Interactable : MonoBehaviour
{
    private Inventory_Control inventory;
    // Start is called before the first frame update
    void Start()
    {
        inventory = GetComponent<Inventory_Control>();
    }

    public Inventory_Control Get_Inventory()
    {
        return inventory;
    }
}
