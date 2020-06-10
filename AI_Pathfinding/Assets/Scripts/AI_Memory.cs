using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AI_Memory : MonoBehaviour
{
    enum DEBUG_MEMORY_MODE {MENTAL_MAP,BOTS,ENEMIES,ALL_BOT_MERGED };
    [SerializeField] private DEBUG_MEMORY_MODE DEBUG_mode;

    [SerializeField] private Dictionary<Vector3Int, int> memory = new Dictionary<Vector3Int, int>();
    private Dictionary<Vector3Int, float> bot_position_memory = new Dictionary<Vector3Int, float>();
    private Dictionary<Vector3Int, float> enemy_position_memory = new Dictionary<Vector3Int, float>();
    private Dictionary<Vector3Int,int> landmarks = new Dictionary< Vector3Int,int>();

    private List<int> landmark_types = new List<int>() { 5,3,2 };

    private bool has_memory_updated = false;

    public struct Pos_Data
    {
        public Vector3Int pos;
        public bool is_valid;
    }

    public bool Check_Memory_Update()
    {
        bool return_bool = has_memory_updated;

        has_memory_updated = false;
        return return_bool;
    }

    private bool Is_Landmark_Type(int i_value)
    {
        foreach (var type in landmark_types)
        {
            if (type == i_value)
            {
                return true;
            }
        }
        return false;
    }

    public Pos_Data Find_Nearest(int i_id)
    {
        float nearest_distance = 1000000;

        Pos_Data temp_data = new Pos_Data();
        temp_data.pos = new Vector3Int();
        temp_data.is_valid = false;

        foreach (var mark in landmarks)
        {
            if (mark.Value == i_id)
            {
                float temp_distance = Vector3.Distance(mark.Key, transform.position);
                if (temp_distance < nearest_distance)
                {
                    temp_data.pos = mark.Key;
                    nearest_distance = temp_distance;
                    temp_data.is_valid = true;
                }
            }
        }

        return temp_data;
    }

    public Pos_Data Find_Nearest_Danger_Zone(Vector3Int i_search_pos,float i_threshold)
    {
        float closest_distance = 1000000.0f;

        Pos_Data return_data = new Pos_Data();
        return_data.pos = new Vector3Int();
        return_data.is_valid = false;

        foreach (var pos in enemy_position_memory)
        {
            float friendly_value = 0.0f;

            if (bot_position_memory.ContainsKey(pos.Key))
            {
                friendly_value = bot_position_memory[pos.Key];
            }

            if (pos.Value - friendly_value > i_threshold)
            {
                float current_distance = Vector3.Distance(i_search_pos, pos.Key);
                if (current_distance < closest_distance)
                {
                    closest_distance = current_distance;
                    return_data.pos = pos.Key;
                    return_data.is_valid = true;
                }
            }
        }

        return return_data;
    }

    private void Add_Bot_Positon_Data(Vector3Int i_pos)
    {
        if (!bot_position_memory.ContainsKey(i_pos))
        {
            bot_position_memory.Add(i_pos, 0.01f);
        }
        else
        {
            bot_position_memory[i_pos] += 0.01f;
        }
    }

    private void Add_Enemy_Positon_Data(Vector3Int i_pos)
    {
        if (!enemy_position_memory.ContainsKey(i_pos))
        {
            enemy_position_memory.Add(i_pos, 0.01f);
        }
        else
        {
            enemy_position_memory[i_pos] += 0.01f;
        }
    }

    public void Add_Data(Vector3Int i_pos, string i_tag)
    {
        int input_value = -1;
        if (i_tag == "")
        {
            input_value = 0;
        }
        else if (i_tag == "Wall")
        {
            input_value = 1;
        }
        else if (i_tag == "Red")
        {
            input_value = 2;
        }
        else if (i_tag == "Copper")
        {
            input_value = 3;
        }
        else if (i_tag == "Dry Copper")
        {
            input_value = 4;
        }
        else if (i_tag == "Blue")
        {
            input_value = 5;
        }
        else if (i_tag == "AI")
        {
            Add_Bot_Positon_Data(i_pos);
        }
        else if (i_tag == "Enemy_AI")
        {
            Add_Enemy_Positon_Data(i_pos);
        }
        // Adding new memory tile
        if (!memory.ContainsKey(i_pos))
        {
            if (Is_Landmark_Type(input_value))
            {
                landmarks.Add(i_pos, input_value);
            }

            has_memory_updated = true;
            memory.Add(i_pos, input_value);
        }
        else // Updating old memory tile
        {
            if (memory[i_pos] != input_value)
            {
                if (landmarks.ContainsKey(i_pos))
                {
                    if (Is_Landmark_Type(input_value))
                    {
                        landmarks[i_pos] = input_value;
                    }
                    else
                    {
                        landmarks.Remove(i_pos);
                    }
                    
                }
                else if (Is_Landmark_Type(input_value))
                {
                    landmarks.Add(i_pos, input_value);
                }


                has_memory_updated = true;
                memory[i_pos] = input_value;
            }
        }
    }

    public int Check_Pos(Vector3 i_check_pos)
    {
        Vector3Int rounded_check_pos = new Vector3Int();
        rounded_check_pos.x = Mathf.RoundToInt(i_check_pos.x);
        rounded_check_pos.y = Mathf.RoundToInt(i_check_pos.y);
        rounded_check_pos.z = Mathf.RoundToInt(i_check_pos.z);

        if (memory.ContainsKey(rounded_check_pos))
        {
            return memory[rounded_check_pos];
        }
        else
        {
            return -1;
        }
    }

    public bool Check_If_Beside(Vector3Int i_check_pos, int i_target_value)
    {
        bool is_adjacent_to_target = false;

        Vector3Int check_forward = i_check_pos;
        check_forward.z += 1;

        Vector3Int check_back = i_check_pos;
        check_back.z -= 1;

        Vector3Int check_left = i_check_pos;
        check_left.x -= 1;

        Vector3Int check_right = i_check_pos;
        check_right.x += 1;

        if (memory.ContainsKey(check_forward))
        {
            if (memory[check_forward] == i_target_value)
            {
                is_adjacent_to_target = true;
            }
        }

        if (memory.ContainsKey(check_back))
        {
            if (memory[check_back] == i_target_value)
            {
                is_adjacent_to_target = true;
            }
        }

        if (memory.ContainsKey(check_left))
        {
            if (memory[check_left] == i_target_value)
            {
                is_adjacent_to_target = true;
            }
        }

        if (memory.ContainsKey(check_right))
        {
            if (memory[check_right] == i_target_value)
            {
                is_adjacent_to_target = true;
            }
        }

        return is_adjacent_to_target;
    }





    private void OnDrawGizmosSelected()
    {
        if (DEBUG_mode == DEBUG_MEMORY_MODE.MENTAL_MAP)
        {
            foreach (var box in memory)
            {
                if (box.Value == 1)
                {
                    Gizmos.color = Color.green;
                }
                else if (box.Value == 2)
                {
                    Gizmos.color = Color.cyan;
                }
                else
                {
                    Gizmos.color = Color.grey;
                }

                Gizmos.DrawCube(box.Key, new Vector3(1.0f, 0.1f, 1.0f));
            }

            Gizmos.color = Color.yellow;
            foreach (var mark in landmarks)
            {
                Gizmos.DrawCube(mark.Key + Vector3.up * 2, new Vector3(0.1f, 4.0f, 0.1f));
            }
        }
        else if (DEBUG_mode == DEBUG_MEMORY_MODE.BOTS)
        {
            foreach (var box in bot_position_memory)
            {
                Color temp_colour = new Color(0.0f, 1.0f, 0.0f, box.Value);
                Gizmos.color = temp_colour;

                Gizmos.DrawCube(box.Key, new Vector3(1.0f, 0.1f, 1.0f));
            }
        }
        else if (DEBUG_mode == DEBUG_MEMORY_MODE.ENEMIES)
        {
            foreach (var box in enemy_position_memory)
            {
                Color temp_colour = new Color(1.0f, 0.0f, 0.0f, box.Value);
                Gizmos.color = temp_colour;

                Gizmos.DrawCube(box.Key, new Vector3(1.0f, 0.1f, 1.0f));
            }
        }
        else if (DEBUG_mode == DEBUG_MEMORY_MODE.ALL_BOT_MERGED)
        {
            foreach (var box in enemy_position_memory)
            {
                float temp_green = 0.0f;
                if (bot_position_memory.ContainsKey(box.Key))
                {
                    temp_green = bot_position_memory[box.Key];
                }

                Color temp_colour = new Color(box.Value, temp_green, 0.0f);
                Gizmos.color = temp_colour;

                Gizmos.DrawCube(box.Key, new Vector3(1.0f, 0.1f, 1.0f));
            }

            foreach (var box in bot_position_memory)
            {
                if (!enemy_position_memory.ContainsKey(box.Key))
                {
                    Color temp_colour = new Color(0.0f, box.Value, 0.0f);
                    Gizmos.color = temp_colour;

                    Gizmos.DrawCube(box.Key, new Vector3(1.0f, 0.1f, 1.0f));
                }
            }
        }
    }
}
