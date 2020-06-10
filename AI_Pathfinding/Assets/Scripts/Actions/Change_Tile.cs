using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Change_Tile : Action
{
    [SerializeField] private GameObject tile_prefab;
    override public void Activate()
    {
        Instantiate(tile_prefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
