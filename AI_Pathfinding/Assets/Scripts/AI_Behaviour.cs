using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AI_Behaviour : MonoBehaviour
{
    protected AI_Memory memory;
    protected AI_Vision vision;
    protected Inventory_Control inventory;
    private Rigidbody rb;

    [Serializable]
    public struct Personality_Data
    {
        public float scout;
        public float miner;
    }
    [SerializeField] protected Personality_Data personality = new Personality_Data();

    [Serializable]
    public struct Priority_Goal
    {
        public IEnumerator start_routine;
        public bool do_touch;
        public Vector3Int destination;
        public int goal_id;
        public float priority;
        public IEnumerator completion_routine;
    }
    [SerializeField] private List<Priority_Goal> goal_list = new List<Priority_Goal>();

    // State machine
    [SerializeField] protected IEnumerator current_state;
    [SerializeField] protected IEnumerator next_state;
    //

    protected bool does_copper_find = false;
    protected private bool does_nexus_return = false;
    protected private bool does_wall_build = false;

    private Vector3 rotation = new Vector3();

    [SerializeField] private GameObject wall_prefab;

    [SerializeField] protected float turn_speed;
    [SerializeField] protected float quick_turn_speed;
    private bool is_quick_turn_enabled;

    [SerializeField] protected float move_speed;
    bool is_look_in_progress = false;
    private IEnumerator move_routine;

    bool is_move_in_progress = false;

    private List<int> current_search_ids = new List<int>();

    [SerializeField] private bool DEBUG_draw_pathfinding_grid = false;

    private List<Vector3Int> waypoints = new List<Vector3Int>();
    private Vector3Int next_waypoint_pos;
    private Vector3Int target_pos = new Vector3Int();

    private Vector3Int destination;

    [SerializeField] private const int STARTING_RANGE = 1;

    void Start()
    {
        StartCoroutine(FSM());

        memory = GetComponent<AI_Memory>();
        vision = GetComponent<AI_Vision>();
        inventory = GetComponent<Inventory_Control>();
        rb = GetComponent<Rigidbody>();

        rotation.y = 1000.0f;

        On_Start();
    }

    protected virtual void On_Start()
    {

    }

    protected void Set_Quick_Turn(bool i_flag)
    {
        is_quick_turn_enabled = i_flag;
    }

    protected void Stop_Moving()
    {
        if (move_routine != null)
        {
            StopCoroutine(move_routine);
            is_move_in_progress = false;
        }
    }

    protected IEnumerator Check_Priorities()
    {
        if (goal_list.Count <= 0)
        {
            next_state = Search_For_Goal();
        }
        else
        {
            float highest_priority = 0.0f;
            Priority_Goal temp_goal = new Priority_Goal();

            foreach (var goal in goal_list)
            {
                if (goal.priority > highest_priority)
                {
                    temp_goal = goal;
                }
            }

            if (temp_goal.start_routine == null)
            {
                next_state = Move_To_Goal(temp_goal.destination, !temp_goal.do_touch);
            }
            else
            {
                next_state = temp_goal.start_routine;
            }

        }

        yield return null;
    }

    protected void Create_Goal(Vector3Int i_pos, float i_priority, bool i_do_touch, IEnumerator i_routine)
    {
        Priority_Goal temp_goal = new Priority_Goal();
        temp_goal.destination = i_pos;
        temp_goal.priority = i_priority;
        temp_goal.do_touch = i_do_touch;
        temp_goal.completion_routine = i_routine;

        goal_list.Add(temp_goal);
    }

    protected IEnumerator Extract_Ore(Vector3Int i_pos)
    {
        Block_Interactable temp_block = vision.Grab_Seen_Interactable(i_pos);

        if (memory.Check_Pos(i_pos) != 3)
        {
            does_copper_find = false;
            next_state = Check_Priorities();
            yield break;
        }

        while (temp_block == null)
        {
            Look_At_Point(i_pos);
            temp_block = vision.Grab_Seen_Interactable(i_pos);
            yield return null;
        }

        inventory.Set_Target(temp_block.Get_Inventory());
        inventory.Take_Item("Copper Ore", 1);
        does_copper_find = false;

        next_state = Check_Priorities();
        yield return null;
    }

    protected IEnumerator Deposit_Ore(Vector3Int i_pos)
    {
        Block_Interactable temp_block = vision.Grab_Seen_Interactable(i_pos);

        if (memory.Check_Pos(i_pos) != 2)
        {
            does_nexus_return = false;
            next_state = Check_Priorities();
            yield break;
        }

        while (temp_block == null)
        {
            Look_At_Point(i_pos);
            temp_block = vision.Grab_Seen_Interactable(i_pos);
            yield return null;
        }

        inventory.Set_Target(temp_block.Get_Inventory());
        inventory.Give_Item("Copper Ore", 1);
        does_nexus_return = false;

        next_state = Check_Priorities();
        yield return null;
    }

    protected IEnumerator Build(Vector3Int i_pos)
    {
        if ((inventory.Get_Item_Quantity() >= 1) && (vision.Is_Pos_Seen(i_pos)) && (memory.Check_Pos(i_pos) == 0))
        {
            Block_Interactable temp_block = vision.Grab_Seen_Interactable(i_pos);

            while (!vision.Is_Pos_Seen(i_pos))
            {
                Look_At_Point(i_pos);
                yield return null;
            }
        }
        else
        {
            does_wall_build = false;
            next_state = Check_Priorities();
            yield break;
        }

        Build_Wall(i_pos);

        does_wall_build = false;
        next_state = Check_Priorities();
        yield return null;
    }

    protected void Build_Wall(Vector3Int i_pos)
    {
        if (inventory.Get_Item_Quantity() >= 1)
        {
            Instantiate(wall_prefab, i_pos, new Quaternion());
            inventory.Consume_Item("Copper Ore", 1);
        }
    }

    protected void Move(Vector3 i_dir,float i_distance)
    {
        if (!is_move_in_progress)
        {
            move_routine = Move_Routine(i_dir, i_distance);
            StartCoroutine(move_routine);
        }
    }

    IEnumerator Move_Routine(Vector3 i_dir, float i_distance)
    {
        is_move_in_progress = true;
        float dist_count = 0.0f;

        Vector3 target_pos = transform.position + (i_dir * i_distance);

        Vector3Int rounded_target_pos = new Vector3Int();
        rounded_target_pos.x = Mathf.RoundToInt(target_pos.x);
        rounded_target_pos.z = Mathf.RoundToInt(target_pos.z);

        while (dist_count < i_distance)
        {
            rb.MovePosition(transform.position + (i_dir * Time.deltaTime * move_speed));

            dist_count += Time.deltaTime;
            yield return null;
        }
        is_move_in_progress = false;
        yield return null;
    }

    protected void Delete_Goal(Vector3Int i_pos, bool i_do_routine)
    {
        List<Priority_Goal> temp_goal_list = new List<Priority_Goal>();

        IEnumerator goal_routine = null;

        foreach (var goal in goal_list)
        {
            if (goal.destination != i_pos)
            {
                temp_goal_list.Add(goal);
            }
            else
            {
                goal_routine = goal.completion_routine;
            }
        }

        goal_list.Clear();
        goal_list = temp_goal_list;

        if (i_do_routine)
        {
            if (goal_routine != null)
            {
                next_state = goal_routine;
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach (var goal in goal_list)
        {
            Gizmos.DrawCube(goal.destination, new Vector3(1.0f, 0.3f, 1.0f));
        }
        if (waypoints.Count >= 2)
        {
            Gizmos.color = Color.blue;
            for (int j = 1; j < waypoints.Count; j++)
            {
                Gizmos.DrawLine(waypoints[j], waypoints[j - 1]);
            }
        }
        if (DEBUG_draw_pathfinding_grid)
        {
            foreach (var director in path_grid)
            {
                if (director.Value == DIRECTION.None)
                {
                    Gizmos.color = Color.white;
                }
                if (director.Value == DIRECTION.North)
                {
                    Gizmos.color = Color.red;
                }
                if (director.Value == DIRECTION.South)
                {
                    Gizmos.color = Color.blue;
                }
                if (director.Value == DIRECTION.East)
                {
                    Gizmos.color = Color.green;
                }
                if (director.Value == DIRECTION.West)
                {
                    Gizmos.color = Color.cyan;
                }

                Gizmos.DrawCube(director.Key + Vector3.up, new Vector3(1.0f, 0.1f, 1.0f));
            }
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(next_waypoint_pos, 0.5f);

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(target_pos, 0.5f);

    }

    IEnumerator Move_To_Goal(Vector3 i_goal, bool i_does_sight_only)
    {
        Vector3 chosen_goal = i_goal;
        Look_At_Point(chosen_goal);
        Vector3Int chosen_goal_rounded = new Vector3Int();
        chosen_goal_rounded.x = Mathf.RoundToInt(chosen_goal.x);
        chosen_goal_rounded.z = Mathf.RoundToInt(chosen_goal.z);


        target_pos = new Vector3Int();

        Reset_Pathfinding();
        Recalc_Path(chosen_goal_rounded);
        target_pos = Waypoint_Generate(chosen_goal_rounded);

        bool is_goal_complete = false;

        float look_timer = 0.0f;

        while (!is_goal_complete)
        {
            if ((memory.Check_Pos(target_pos) > 0) && (target_pos != chosen_goal_rounded))
            {
                Reset_Pathfinding();
                Recalc_Path(chosen_goal_rounded);
                target_pos = Waypoint_Generate(chosen_goal_rounded);
                Debug.Log("Recalculated");
            }

            if (i_does_sight_only)
            {
                is_goal_complete = vision.Is_Pos_Seen(chosen_goal_rounded);
            }
            else if (Vector3.Distance(chosen_goal_rounded, Round_Pos(transform.position)) < 1.4f)
            {
                is_goal_complete = true;
            }

            if (memory.Check_Memory_Update())
            {
                target_pos = Waypoint_Generate(chosen_goal_rounded);

            }
            else if (target_pos == Round_Pos(transform.position))
            {
                Stop_Moving();
                target_pos = Waypoint_Generate(chosen_goal_rounded);
            }

            if (Get_Dir_From_Pos(Round_Pos(transform.position)) == DIRECTION.None)
            {
                Recalc_Path(chosen_goal_rounded);
            }

            if (target_pos != Round_Pos(transform.position))
            {

                if (vision.Is_Pos_Seen(target_pos))
                {
                    Move((target_pos - transform.position).normalized, Vector3.Distance(target_pos, transform.position));
                }
                else if (vision.Is_Pos_Seen(next_waypoint_pos))
                {
                    Move((next_waypoint_pos - transform.position).normalized, Vector3.Distance(next_waypoint_pos, transform.position));
                }
                else
                {
                    target_pos = Waypoint_Generate(chosen_goal_rounded);
                }

            }

            if (Vector3.Dot(transform.forward, (next_waypoint_pos - transform.position).normalized) <= 0.0f)
            {
                Set_Quick_Turn(true);
            }
            else
            {
                Set_Quick_Turn(false);
            }


            if (!vision.Is_Pos_Seen(next_waypoint_pos))
            {
                Look_At_Point(next_waypoint_pos);
            }

            //if (!vision.Is_Pos_Seen(target_pos) && !vision.Is_Pos_Seen(next_waypoint_pos))
            //{
            //    look_timer += Time.deltaTime;
            //}
            //else
            //{
            //    look_timer = 0.0f;
            //}

            //if (look_timer > 2.0f)
            //{
            //    Vector3Int random_direction = new Vector3Int();
            //    random_direction.x += UnityEngine.Random.Range(-1, 1);
            //    random_direction.z += UnityEngine.Random.Range(-1, 1);

            //    Look_At_Point(Round_Pos(transform.position) + random_direction);
            //    look_timer = 0.0f;
            //}
            //else
            //{
            //    Look_At_Point(next_waypoint_pos);
            //    look_timer = 0.0f;
            //}
            //look_timer += Time.deltaTime;

            //if (look_timer > 2.0f)
            //{
            //    Vector3Int random_direction = new Vector3Int();
            //    random_direction.x += UnityEngine.Random.Range(-1, 1);
            //    random_direction.y += UnityEngine.Random.Range(-1, 1);

            //    Look_At_Point(Round_Pos(transform.position) + random_direction);

            //    look_timer = 0.0f;
            //}


            yield return null;
            //AI_Movement_Utils.Seek_Target(rb, target_pos, chase_force);
        }

        Stop_Moving();

        Delete_Goal(chosen_goal_rounded, true);

        if (next_state == null)
        {
            next_state = Check_Priorities();
        }

        yield return null;
    }

    public enum DIRECTION
    {
        None = 0x0,
        North = 0x1,
        East = 0x2,
        South = 0x4,
        West = 0x8,
        NorthEast = North | East,
        SouthEast = South | East,
        NorthWest = North | West,
        SouthWest = South | West,
    }

    Dictionary<Vector3Int, DIRECTION> path_grid = new Dictionary<Vector3Int, DIRECTION>();
    List<Vector3Int> to_calc_list = new List<Vector3Int>();

    private void Reset_Pathfinding()
    {
        path_grid.Clear();
        to_calc_list.Clear();
    }

    void Recalc_Path(Vector3Int i_dest)
    {
        Vector3Int rounded_pos = new Vector3Int();
        rounded_pos.x = Mathf.RoundToInt(transform.position.x);
        rounded_pos.z = Mathf.RoundToInt(transform.position.z);
        to_calc_list.Add(i_dest);

        destination = rounded_pos;

        int counter = 0;
        while (counter < 2)
        {
            to_calc_list.Sort(compare_dist);

            int calc_limit = 4;

            if (to_calc_list.Count < calc_limit)
            {
                calc_limit = to_calc_list.Count;
            }

            for (int i = 0; i < calc_limit; i++)
            {
                Vector3Int current_front_pos = to_calc_list[i];

                if (current_front_pos == i_dest)
                {
                    path_grid[current_front_pos] = DIRECTION.None;
                }

                if (path_grid.ContainsKey(current_front_pos + Vector3Int.right))
                {
                    path_grid[current_front_pos] = DIRECTION.East;
                }
                else if (path_grid.ContainsKey(current_front_pos - Vector3Int.right))
                {
                    path_grid[current_front_pos] = DIRECTION.West;
                }
                else if (path_grid.ContainsKey(current_front_pos + new Vector3Int(0, 0, 1)))
                {
                    path_grid[current_front_pos] = DIRECTION.North;
                }
                else if (path_grid.ContainsKey(current_front_pos - new Vector3Int(0, 0, 1)))
                {
                    path_grid[current_front_pos] = DIRECTION.South;
                }
            }

            to_calc_list.Clear();


            // Adds adjacent cells for pathfinding frontier to calculate
            foreach (KeyValuePair<Vector3Int, DIRECTION> entry in path_grid)
            {
                if ((!path_grid.ContainsKey(entry.Key + Vector3Int.right)) && (memory.Check_Pos(entry.Key + Vector3Int.right) < 1))
                {
                    to_calc_list.Add(entry.Key + Vector3Int.right);
                }
                if ((!path_grid.ContainsKey(entry.Key - Vector3Int.right)) && (memory.Check_Pos(entry.Key - Vector3Int.right) < 1))
                {
                    to_calc_list.Add(entry.Key - Vector3Int.right);
                }
                if ((!path_grid.ContainsKey(entry.Key + new Vector3Int(0, 0, 1))) && (memory.Check_Pos(entry.Key + new Vector3Int(0, 0, 1)) < 1))
                {
                    to_calc_list.Add(entry.Key + new Vector3Int(0, 0, 1));
                }
                if ((!path_grid.ContainsKey(entry.Key - new Vector3Int(0, 0, 1))) && (memory.Check_Pos(entry.Key - new Vector3Int(0, 0, 1)) < 1))
                {
                    to_calc_list.Add(entry.Key - new Vector3Int(0, 0, 1));
                }
            }
            counter++;
        }

        path_grid[i_dest] = DIRECTION.None;
    }

    public DIRECTION Get_Dir_From_Pos(Vector3 i_pos)
    {
        Vector3Int temp_pos = new Vector3Int();
        temp_pos.x = Mathf.RoundToInt(i_pos.x);
        temp_pos.z = Mathf.RoundToInt(i_pos.z);


        if (path_grid.ContainsKey(temp_pos))
        {
            return path_grid[temp_pos];
        }
        else
        {
            return DIRECTION.None;
        }

    }

    IEnumerator Debug_Routine()
    {
        Debug.Log("Debug_Routine hit");
        next_state = Check_Priorities();
        yield return null;
    }

    protected IEnumerator Search_For_Goal()
    {
        int range = STARTING_RANGE;
        int attempt_counter = 0;
        bool is_goal_found = false;

        while (!is_goal_found)
        {
            Vector3Int check_pos = new Vector3Int();

            check_pos.x = Mathf.RoundToInt(transform.position.x) + UnityEngine.Random.Range(-range, range);
            check_pos.z = Mathf.RoundToInt(transform.position.z) + UnityEngine.Random.Range(-range, range);

            if (memory.Check_Pos(check_pos) < 0)
            {
                if (memory.Check_If_Beside(check_pos, 0))
                {
                    attempt_counter = 0;
                    Create_Goal(check_pos, 10.0f, false, null);
                    is_goal_found = true;
                    next_state = Check_Priorities();
                }
                yield return null;
            }
            else
            {
                attempt_counter++;
                if (attempt_counter > 10)
                {
                    attempt_counter = 0;
                    range++;

                    if (range > 40)
                    {
                        range = 1;
                    }
                }
                yield return null;
            }
        }


        yield return null;
    }

    private Vector3Int Waypoint_Generate(Vector3Int i_destination)
    {
        waypoints.Clear();
        Vector3Int rounded_pos = new Vector3Int();
        rounded_pos.x = Mathf.RoundToInt(transform.position.x);
        rounded_pos.z = Mathf.RoundToInt(transform.position.z);

        Vector3Int last_waypoint_pos = new Vector3Int();
        Vector3Int waypoint_pos = rounded_pos;

        int counter = 0;
        DIRECTION dir = DIRECTION.North;
        bool is_waypoint_found = false;

        while ((dir != DIRECTION.None) && (!is_waypoint_found))
        {
            last_waypoint_pos = waypoint_pos;

            dir = Get_Dir_From_Pos(waypoint_pos);

            if (dir == DIRECTION.East)
            {
                waypoint_pos.x += 1;
            }
            if (dir == DIRECTION.West)
            {
                waypoint_pos.x -= 1;
            }
            if (dir == DIRECTION.North)
            {
                waypoint_pos.z += 1;
            }
            if (dir == DIRECTION.South)
            {
                waypoint_pos.z -= 1;
            }

            if (!vision.Is_Pos_Seen(waypoint_pos))
            {
                waypoints.Add(last_waypoint_pos);
                is_waypoint_found = true;
            }
            counter++;
        }
        next_waypoint_pos = waypoint_pos;
        return last_waypoint_pos;
        //waypoints.Add(waypoint_pos);
    }

    //When the frontier queue is sorted, this is used to sort the queue based on the lowest distance to the destination
    private int compare_dist(Vector3Int a, Vector3Int b)
    {
        int output_a = Mathf.Abs(a.x - destination.x) + Mathf.Abs(a.y - destination.y);
        int output_b = Mathf.Abs(b.x - destination.x) + Mathf.Abs(b.y - destination.y);

        if (Vector3Int.Distance(a, destination) < Vector3Int.Distance(b, destination))
        {
            return -1;
        }
        else
        {
            return 1;
        }

    }

    protected Vector3Int Round_Pos(Vector3 i_pos)
    {
        Vector3Int rounded_pos = new Vector3Int();
        rounded_pos.x = Mathf.RoundToInt(i_pos.x);
        rounded_pos.y = Mathf.RoundToInt(i_pos.y);
        rounded_pos.z = Mathf.RoundToInt(i_pos.z);

        return rounded_pos;
    }

    protected void Look_At_Point(Vector3 i_pos)
    {
        if (!is_look_in_progress)
        {
            StartCoroutine(Turn_Towards_Routine(i_pos));
        }
    }

    IEnumerator Turn_Towards_Routine(Vector3 i_pos)
    {
        is_look_in_progress = true;
        Vector3 target_look_dir = (i_pos - transform.position).normalized;

        float look_speed_local = turn_speed;
        if (is_quick_turn_enabled)
        {
            look_speed_local = quick_turn_speed;
        }

        while (Vector3.Dot(transform.forward,target_look_dir) < 0.9f )
        {
            Vector3 look_dir = Vector3.RotateTowards(transform.forward, target_look_dir, look_speed_local * Time.deltaTime, 0.0f);
            transform.rotation = Quaternion.LookRotation(look_dir);
            yield return null;
        }
        is_look_in_progress = false;
        yield return null;
    }

    private IEnumerator FSM()
    {
        yield return null;
        while (current_state != null)
        {
            yield return StartCoroutine(current_state);

            current_state = next_state;
            next_state = null;

        }
    }
}
