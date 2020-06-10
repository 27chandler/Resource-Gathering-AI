using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Depletion_Action : MonoBehaviour
{
    private Inventory_Control inventory;
    [SerializeField] private GameObject replacement_obj;

    private enum ACTION {REGEN = 0, DESTROY = 1 }

    [SerializeField] private ACTION depletion_action;
    // Start is called before the first frame update
    void Start()
    {
        inventory = GetComponent<Inventory_Control>();
    }

    // Update is called once per frame
    void Update()
    {

        if (inventory.Get_Item_Quantity() <= 0)
        {
            switch (depletion_action)
            {
                case ACTION.REGEN:
                    {
                        Regen_Action();
                        break;
                    }
                case ACTION.DESTROY:
                    {
                        Destroy_Action();
                        break;
                    }
                default:
                    {
                        Debug.LogWarning("Invalid depletion action detected! This is not supposed to happen!");
                        break;
                    }
            }
        }
    }

    private void Destroy_Action()
    {
        Destroy(gameObject);
    }

    private void Regen_Action()
    {
        Instantiate(replacement_obj, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
