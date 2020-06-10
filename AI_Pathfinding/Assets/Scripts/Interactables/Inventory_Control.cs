using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Control : MonoBehaviour
{
    [SerializeField] private string item_type;
    [SerializeField] protected int item_quantity;

    private Inventory_Control target;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string Get_Type()
    {
        return item_type;
    }

    public void Set_Target(Inventory_Control i_target)
    {
        target = i_target;
    }

    public int Get_Item_Quantity()
    {
        return item_quantity;
    }

    public bool Consume_Item(string i_type, int i_quantity)
    {
        if (i_type == item_type)
        {
            item_quantity -= i_quantity;
            return true;
        }

        return false;
    }

    public bool Give_Item(string i_type, int i_quantity)
    {
        if ((i_type == item_type) && (target.Get_Type() == i_type))
        {
            item_quantity -= i_quantity;
            target.item_quantity += i_quantity;
            return true;
        }
        return false;
    }

    public bool Take_Item(string i_type, int i_quantity)
    {
        if (target.Get_Item_Quantity() >= i_quantity)
        {
            if ((i_type == item_type) && (target.Get_Type() == i_type))
            {
                item_quantity += i_quantity;
                target.item_quantity -= i_quantity;
                return true;
            }
        }

        return false;
    }
}
