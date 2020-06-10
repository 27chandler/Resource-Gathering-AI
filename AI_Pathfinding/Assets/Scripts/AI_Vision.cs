using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// The AI_Vision class is responsible for managing the AI's field of view
public class AI_Vision : MonoBehaviour
{
    public struct Hit_Data
    {
        public bool is_hit;
        public GameObject hit_object;
        public Vector3 hit_point;
        public string hit_tag;
    }

    private List<Vector3Int> seen_pos_list = new List<Vector3Int>();
    [SerializeField] private List<Block_Interactable> seen_interactables = new List<Block_Interactable>();

    [SerializeField] private int NUM_OF_ANGLES = 7;
    [SerializeField] private float angle_size;
    [SerializeField] private int max_look_dist;

    private AI_Memory mem;

    // Start is called before the first frame update
    void Start()
    {
        mem = GetComponent<AI_Memory>();
    }

    // This will scan the AI's vision for the nearest interactable tile and return it
    public Block_Interactable Grab_Nearest_Interactable(Vector3 i_pos)
    {
        float distance = 100.0f;
        Block_Interactable return_interactable = null;
        foreach (var interactable in seen_interactables)
        {
            float current_distance = Vector3.Distance(i_pos, interactable.transform.position);
            if (current_distance < distance)
            {
                return_interactable = interactable;
                distance = current_distance;
            }
        }
        return return_interactable;
    }

    public Block_Interactable Grab_Seen_Interactable(Vector3Int i_pos)
    {
        foreach (var interactable in seen_interactables)
        {
            if (interactable.transform.position == i_pos)
            {
                return interactable;
            }
        }
        return null;
    }

    // Check to see if a position can currently be seen
    public bool Is_Pos_Seen(Vector3Int i_check_pos)
    {
        if (seen_pos_list.FindIndex(d => d == i_check_pos) >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        seen_pos_list.Clear();
        seen_interactables.Clear();

        Vector3 starting_dir = Quaternion.AngleAxis(-(float)((NUM_OF_ANGLES/2) * angle_size), Vector3.up) * transform.forward;

        // Recursively searches for an object in the AI's define FOV
        for (int i = 0; i < NUM_OF_ANGLES; i++)
        {
            for (int j=0; j < max_look_dist * 5; j++)
            {
                Hit_Data temp_hit = Position_Check(starting_dir, (float)(j/5.0f));

                Vector3Int rounded_hit_pos = new Vector3Int();
                rounded_hit_pos.x = Mathf.RoundToInt(temp_hit.hit_point.x);
                rounded_hit_pos.y = 0;
                rounded_hit_pos.z = Mathf.RoundToInt(temp_hit.hit_point.z);

                if (temp_hit.is_hit)
                {
                    j = max_look_dist * 5;
                    seen_pos_list.Add(rounded_hit_pos);

                    if ((temp_hit.hit_tag == "Copper") || (temp_hit.hit_tag == "Red"))
                    {
                        // Adds the seen object to the list of seen objects
                        seen_interactables.Add(temp_hit.hit_object.GetComponent<Block_Interactable>());
                    }
                }
                else
                {
                    seen_pos_list.Add(rounded_hit_pos);
                }

                // If the object exists it will be added to the memory banks for this AI
                Add_Data_To_Memory(rounded_hit_pos, temp_hit.hit_tag);
            }


            starting_dir = Quaternion.AngleAxis(angle_size, Vector3.up) * starting_dir;
        }


    }

    private Hit_Data Position_Check(Vector3 i_dir,float i_dist)
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, i_dir, out hit, i_dist);

        Vector3 hit_pos;

        bool is_object_present = false;
        Hit_Data return_hit = new Hit_Data();

        if (hit.collider != null)
        {
            is_object_present = true;
            hit_pos = hit.collider.transform.position;
            return_hit.hit_tag = hit.collider.tag;
            return_hit.hit_object = hit.collider.gameObject;
        }
        else
        {
            hit_pos = transform.position + (i_dir * i_dist);
            return_hit.hit_tag = "";
        }

        Color ray_colour;
        if (is_object_present)
        {
            ray_colour = Color.green;
        }
        else
        {
            ray_colour = Color.white;
        }

        Debug.DrawLine(transform.position, hit_pos, ray_colour);


        return_hit.is_hit = is_object_present;
        return_hit.hit_point = hit_pos;
        

        return return_hit;
    }

    private void Add_Data_To_Memory(Vector3Int i_pos, string i_tag)
    {
        if (mem != null)
        {
            mem.Add_Data(i_pos, i_tag);
        }
        else
        {
            Debug.LogWarning("AI Memory is currently NULL, cannot add data");
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var pos in seen_pos_list)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawCube(pos + new Vector3(0.0f,0.2f,0.0f), new Vector3(1.0f, 0.9f, 1.0f));
        }
    }
}
