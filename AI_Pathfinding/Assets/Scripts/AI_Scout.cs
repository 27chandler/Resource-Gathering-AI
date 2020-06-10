using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Scout : AI_Behaviour
{
    

    [SerializeField] private bool is_builder = false;
    [SerializeField] private bool is_scout = false;
    private bool is_scout_in_progress = false;
    [SerializeField] private int scout_target = 5;


    override protected void On_Start()
    {
        current_state = Check_Priorities();
    }

    // Update is called once per frame
    void Update()
    {
        if (personality.scout > personality.miner)
        {
            Scout_Behaviour();
        }
        else
        {

        }

        if ((inventory.Get_Item_Quantity() <= 0) && (!does_copper_find))
        {
            AI_Memory.Pos_Data copper_ore_location = memory.Find_Nearest(3);

            if (copper_ore_location.is_valid)
            {
                does_copper_find = true;
                Create_Goal(copper_ore_location.pos, 70.0f, true, Extract_Ore(copper_ore_location.pos));
                Debug.Log("Random_Goal");
            }

        }
        else if ((inventory.Get_Item_Quantity() > 0) && (!does_nexus_return) && (!is_builder))
        {

            AI_Memory.Pos_Data nexus_location = memory.Find_Nearest(2);

            if (nexus_location.is_valid)
            {
                does_nexus_return = true;
                Create_Goal(nexus_location.pos, 60.0f, true, Deposit_Ore(nexus_location.pos));
            }

        }
        else if ((inventory.Get_Item_Quantity() > 0) && (!does_wall_build) && is_builder)
        {
            AI_Memory.Pos_Data danger_location = memory.Find_Nearest_Danger_Zone(Round_Pos(transform.position), 0.4f);

            if (danger_location.is_valid)
            {
                does_wall_build = true;
                Create_Goal(danger_location.pos, 90.0f, true, Build(danger_location.pos));
            }
        }

    }

    private void Check_For_Landmark()
    {
        // Find out if the AI knows where the copper ore is
    }

    private void Scout_Behaviour()
    {
        // Search for specified landmarks
        AI_Memory.Pos_Data scout_data = new AI_Memory.Pos_Data();
        scout_data = memory.Find_Nearest(scout_target);

        if (!memory.Find_Nearest(scout_target).is_valid)
        {
            is_scout_in_progress = true;
            next_state = Search_For_Goal();
        }
        else
        {
            is_scout_in_progress = false;
        }
    }
}
