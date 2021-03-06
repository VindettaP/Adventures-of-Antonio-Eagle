using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

enum TileType
{
    WALL = 1,
    FLOOR = 2,
    AIR_PLAT = 0,
    GRAPPLE_POINT = 4,
    VIRUS = 3,
}

struct Coord
{
    public int x;
    public int y;

    public Coord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

struct Node
{
    public float g;
    public float h;
    public float f;
    public Coord parent;
    public int key;

    public Node(float g, float h, float f, Coord parent)
    {
        this.g = g;
        this.h = h;
        this.f = f;
        this.parent = parent;
        this.key = this.parent.x + (100 * this.parent.y); // give every node a unique key for Q dictionary
    }
}

public class Level : MonoBehaviour
{
    public int width = 16;   // size of level (default 16 x 16 blocks)
    public int length = 16;
    public float story_height = 2.5f;   // height of walls
    public float air_platform_height = 3.0f;
    public float grapple_point_height = 10.0f;
    public int grapple_point_spacing_w = 3;
    public int grapple_point_spacing_l = 3;
    public int num_grapples = 4;
    public GameObject player_prefab;
    public GameObject win_popup;
    public GameObject lose_popup;
    public Text clock_text;
    public Camera overhead_cam;
    public GameObject ceiling;
    public Material border_wall_mat;
    public Material air_plat_mat;
    public Material goal_wall_mat;
    public AudioClip victory;
    public AudioClip splash;
    public AudioClip bgMusic;

    // fields/variables accessible from other scripts
    internal GameObject player;
    internal bool player_entered_goal = false;

    // fields/variables needed only from this script
    private Bounds bounds;                   // size of ground plane in world space coordinates 
    private int function_calls = 0;          // number of function calls during backtracking for solving the CSP
    private List<int[]> pos_grapples;
    private float playerX = -1;
    private float playerZ = -1;
    private int hW = -1;
    private int hL = -1;
    private float timeSpent = 0.0f;
    private float finalTime = 0.0f;
    private List<TileType>[,] sol;
    private GameObject cursor;
    private bool playedJingle = false;
    private bool playedSplash = false;
    private int playerW, playerL;


    private void Shuffle<T>(ref List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    // helper to check nearby spots on the grid and see if any are of specified type, true if any of type are within rad squares of start_w, start_l position
    private bool CheckRadius(List<TileType>[,] grid, int start_w, int start_l, int w_dist, int l_dist, TileType type)
    {
        for (int w = start_w - w_dist; w <= start_w + w_dist; w++)
        {
            for (int l = start_l - l_dist; l <= start_l + l_dist; l++)
            {
                // skip any l's or w's outside of the grid
                if (l < 1 || l > length - 1 || w < 1 || w > length - 1)
                    continue;
                else if (grid[w, l] == null)
                    continue;
                else if (grid[w, l][0] == type && grid[w, l].Count == 1)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Use this for initialization
    void Start()
    {
        // reset jingle
        playedJingle = false;

        // Make floor plane blue
        GetComponent<Renderer>().material.color = new Color(0.5f, 5.5f, 5.0f); // Make floor blue (for water)

        // Make ceiling
        ceiling.transform.position = new Vector3(ceiling.transform.position.x, grapple_point_height - 0.5f, ceiling.transform.position.z);

        // Grab aiming cursor
        cursor = GameObject.Find("AimingCursor");

        // Disable overhead cam
        overhead_cam.gameObject.SetActive(false);

        // initialize internal/private variables
        bounds = GetComponent<Collider>().bounds;

        // disable UI elements
        win_popup.gameObject.SetActive(false);
        lose_popup.gameObject.SetActive(false);

        // initialize 2D grid
        List<TileType>[,] grid = new List<TileType>[width, length];
        // useful to keep variables that are unassigned so far
        List<int[]> unassigned = new List<int[]>();

        // Place some random grapple points to start
        pos_grapples = new List<int[]>();

        bool success = false;
        while (!success)
        {
            int wr, lr;
            // place grapple points
            for (int g = 0; g < num_grapples; g++)
            {
                int i = 0;
                while (true)
                {
                    if (i > 10)
                        break;
                    i++;

                    wr = Random.Range(1, width - 1);
                    lr = Random.Range(1, length - 1);

                    if (!CheckRadius(grid, wr, lr, grapple_point_spacing_w, grapple_point_spacing_l, TileType.GRAPPLE_POINT))
                    {
                        grid[wr, lr] = new List<TileType> { TileType.GRAPPLE_POINT };
                        pos_grapples.Add(new int[2] { wr, lr });
                        break;
                    }
                }
            }

            // Add some random air platforms and random walls to pass initial constraints
            int p = 0;
            int wall = 0;
            while (p < (width * length) / 50)
            {
                wr = Random.Range(1, width - 1);
                lr = Random.Range(1, length - 1);
                if (grid[wr, lr] == null)
                {
                    grid[wr, lr] = new List<TileType> { TileType.AIR_PLAT };
                    p++;
                }
            }
            while (wall < (width * length) / 25)
            {
                wr = Random.Range(1, width - 1);
                lr = Random.Range(1, length - 1);
                if (grid[wr, lr] == null)
                {
                    grid[wr, lr] = new List<TileType> { TileType.WALL };
                    wall++;
                }
            }

            // Place border walls, allocate unassigned
            for (int w = 0; w < width; w++)
                for (int l = 0; l < length; l++)
                    if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                        grid[w, l] = new List<TileType> { TileType.WALL };
                    else
                    {
                        if (grid[w, l] == null) 
                        {
                            List<TileType> candidate_assignments = new List<TileType> { TileType.WALL, TileType.FLOOR, TileType.AIR_PLAT };
                            Shuffle<TileType>(ref candidate_assignments);

                            grid[w, l] = candidate_assignments;
                            unassigned.Add(new int[] { w, l });
                        }
                    }

            success = BackTrackingSearch(grid, unassigned);
            if (!success)
            {
                Debug.Log("Could not find valid solution - will try again");
                unassigned.Clear();
                grid = new List<TileType>[width, length];
                function_calls = 0;
            }
        }

        int[] assigned = new int[] { 0, 0, 0, 0, 0 };
        for (int w = 0; w < width; w++)
        {
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                if (grid[w, l].Count == 1)
                    assigned[(int)grid[w, l][0]]++;
            }
        }

        DrawDungeon(grid);
        sol = grid;
    }

    // Constraints
    bool TooManyWallsOrPlats(List<TileType>[,] grid)
    {
        int[] assigned = new int[] { 0, 0, 0, 0, 0 };
        for (int w = 0; w < width; w++)
        {
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                if (grid[w, l].Count == 1)
                    assigned[(int)grid[w, l][0]]++;
            }
        }

        if ((assigned[(int)TileType.WALL] > width * length / 14) ||
            assigned[(int)TileType.AIR_PLAT] > width * length / 14)
            return true;
        else
            return false;
    }

    bool TooFewWallsOrPlats(List<TileType>[,] grid)
    {
        int[] assigned = new int[] { 0, 0, 0, 0, 0 };
        for (int w = 0; w < width; w++)
        {
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                if (grid[w, l].Count == 1)
                    assigned[(int)grid[w, l][0]]++;
            }
        }

        if (assigned[(int)TileType.AIR_PLAT] < width * length / 20 ||
            assigned[(int)TileType.WALL] < width * length / 20)
            return true;
        else
            return false;
    }

    bool WallsNearGrapples(List<TileType>[,] grid)
    {
        for (int w = 0; w < width; w++)
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;  // Case where square is exterior wall, ignore
                if (w == 1 || l == 1 || w == width - 2 || l == length - 2)
                    continue;  // Case where square must be next to exterior wall, ignore
                if (grid[w, l].Count > 1 || grid[w, l][0] != TileType.GRAPPLE_POINT)
                    continue;  // Ignore unassigned tiles or tiles with something other than a virus
                if (grid[w - 1, l][0] == TileType.WALL || grid[w + 1, l][0] == TileType.WALL || grid[w, l - 1][0] == TileType.WALL || grid[w, l + 1][0] == TileType.WALL ||
                    grid[w - 1, l - 1][0] == TileType.WALL || grid[w - 1, l + 1][0] == TileType.WALL || grid[w + 1, l - 1][0] == TileType.WALL || grid[w + 1, l + 1][0] == TileType.WALL)
                    return true;  // if any walls nearby return false
            }
        return false;
    }

    bool TooLongWall(List<TileType>[,] grid)
    {
        bool oneAbove = false;
        bool oneBelow = false;
        bool oneLeft = false;
        bool oneRight = false;
        /*** implement the rest ! */
        for (int w = 0; w < width; w++)
        {
            for (int l = 0; l < length; l++)
            {
                oneAbove = false;
                oneBelow = false;
                oneLeft = false;
                oneRight = false;  // reset flags

                // Ignore edges and non-walls
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;  // Case where square is exterior wall, ignore
                if (grid[w, l].Count > 1 || grid[w, l][0] != TileType.WALL)
                    continue;  // Ignore unassigned tiles or tiles with something other than an interior wall

                // Check vertical
                // Check above
                if (grid[w, l - 1].Count == 1 && grid[w, l - 1][0] == TileType.WALL && l > 1)
                    oneAbove = true;
                // Check below
                if (l < length - 2 && grid[w, l + 1].Count == 1 && grid[w, l + 1][0] == TileType.WALL)
                    oneBelow = true;
                if (oneAbove && oneBelow)
                    return true;

                // Check horizontal
                // Check left
                if (w > 1 && grid[w - 1, l].Count == 1 && grid[w - 1, l][0] == TileType.WALL)
                    oneLeft = true;
                // Check right
                if (w < width - 2 && grid[w + 1, l].Count == 1 && grid[w + 1, l][0] == TileType.WALL)
                    oneRight = true;
                if (oneLeft && oneRight)
                    return true;
            }
        }
        return false;
    }

    // check if attempted assignment is consistent with the constraints or not
    bool CheckConsistency(List<TileType>[,] grid, int[] cell_pos, TileType t)
    {
        int w = cell_pos[0];
        int l = cell_pos[1];

        List<TileType> old_assignment = new List<TileType>();
        old_assignment.AddRange(grid[w, l]);
        grid[w, l] = new List<TileType> { t };

        // note that we negate the functions here i.e., check if we are consistent with the constraints we want
        bool areWeConsistent = !TooLongWall(grid) && !TooManyWallsOrPlats(grid);// && !WallsNearGrapples(grid); //&& !TooFewWallsOrPlats(grid);

        grid[w, l] = new List<TileType>();
        grid[w, l].AddRange(old_assignment);
        return areWeConsistent;
    }

    // euclidian distance from point to other point rounded down
    float heur(int sw, int sl, int gw, int gl)
    {
        float diffw = (float)gw - (float)sw;
        float diffl = (float)gl - (float)sl;
        return Mathf.Sqrt((diffw * diffw) + (diffl * diffl));
    }

    bool BackTrackingSearch(List<TileType>[,] grid, List<int[]> unassigned)
    {
        if (function_calls++ > 100000)
            return false;

        // we are done!
        if (unassigned.Count == 0)
            return true;

        int index = Random.Range(0, unassigned.Count);
        int[] nextVar = unassigned[index]; // choose random unassigned grid location to try next
        bool result = false;

        for (int i = 0; i < 3; i++)
        {
            if (CheckConsistency(grid, nextVar, (TileType)i))
            {
                function_calls++;
                grid[nextVar[0], nextVar[1]] = new List<TileType> { (TileType)i };
                // remove a value from unassigned and recurse on it
                unassigned.RemoveAt(index);
                result = BackTrackingSearch(grid, unassigned);
                if (result)
                    return true; // end if recursion assigns all values
                // if we got here recursion turned up a failure caused by the assignment, readd unassigned position
                unassigned.Insert(0, nextVar);

                // unassign the grid space
                List<TileType> candidate_assignments = new List<TileType> { TileType.WALL, TileType.FLOOR, TileType.AIR_PLAT };
                Shuffle<TileType>(ref candidate_assignments);
                grid[nextVar[0], nextVar[1]] = candidate_assignments;
            }
        }

        return false; // if we got here, recursion failed
    }


    // places the primitives/objects according to the grid assignents
    // you will need to edit this function (see below)
    void DrawDungeon(List<TileType>[,] solution)
    {
        int wr = 0;
        int lr = 0;

        wr = Random.Range(1, width - 1);
        wr = 1; // Hard set player to edge of arena
        lr = Random.Range(1, length - 1);
        solution[wr, lr][0] = TileType.AIR_PLAT;

        float xp = bounds.min[0] + (float)wr * (bounds.size[0] / (float)width);
        float zp = bounds.min[2] + (float)lr * (bounds.size[2] / (float)length);

        //**********INSTANTIATE PLAYER HERE*********************
        playerX = xp;
        playerZ = zp;
        playerW = wr;
        playerL = lr;

        player = Instantiate(player_prefab,
            new Vector3(playerX + bounds.size[0] / (2 * (float)width), bounds.min[1] + 3.0f + (3.0f * air_platform_height), playerZ + bounds.size[2] / (2 * (float)length)),
            Quaternion.Euler(0, 90, 0));
        player.name = "PlayerBody";

        // Play background music
        player.GetComponent<AudioSource>().PlayOneShot(bgMusic, 0.5f);
        //*********************************************************

        // Choose goal location here
        int wee = -1;
        int lee = -1;

        wee = width - 1;
        lee = Random.Range(1, length - 1);

        hW = wee;
        hL = lee;
        ///


        // initialize table of nodes, costs, and parents
        Node[,] lookTable = new Node[width, length];  // each entry will contain [cost, parent w, parent l]
        for (int wid = 0; wid < width; wid++)
            for (int len = 0; len < length; len++)
                lookTable[wid, len] = new Node(float.MaxValue, float.MaxValue, float.MaxValue, new Coord(-1, -1));  // initialize all costs to max

        Coord start = new Coord(wr, lr);  // get player's starting location
        Dictionary<int, Coord> q = new Dictionary<int, Coord>();  // make q a list of coordinates to consider
        q.Add(wr + (100 * lr), start);  // add start to q

        lookTable[wr, lr] = new Node(0.0f, heur(wr, lr, wee, lee), heur(wr, lr, wee, lee), new Coord(-1, -1)); // g = 0, h = heuristic, f = heuristic

        int current = -1; // current tracks the key of the current node in q
        Coord final = new Coord(-1, -1);

        int loopCount = 0;
        while (q.Count > 0)  // begin a* search
        {
            loopCount++;

            float minf = float.MaxValue; // initialize with high cost
            foreach (Coord coord in q.Values)
            {
                if (lookTable[coord.x, coord.y].f < minf)
                {
                    minf = lookTable[coord.x, coord.y].f;
                    current = coord.x + (100 * coord.y); // make a key with simple formula
                }
            }

            // check if current is goal
            if (q[current].x == wee && q[current].y == lee)
            {
                final = new Coord(q[current].x, q[current].y);
                break;
            }

            Coord cur = q[current];

            q.Remove(current);
            Coord toCheck;

            for (int i = 0; i < 4; i++) // check all 4 possible neighbors of current node
            {

                if (i == 0)
                    toCheck = new Coord(cur.x - 1, cur.y);
                else if (i == 1)
                    toCheck = new Coord(cur.x, cur.y - 1);
                else if (i == 2)
                    toCheck = new Coord(cur.x + 1, cur.y);
                else
                    toCheck = new Coord(cur.x, cur.y + 1);

                float gScore = 0.0f;
                // check to see if point to check is valid
                if (toCheck.x < 0 || toCheck.x >= width || toCheck.y < 0 || toCheck.y >= length)
                    continue;  // don't add anything outside of grid
                else  // anything that reaches this point must be okay
                {
                    // check gScore of neighbor
                    if (solution[toCheck.x, toCheck.y][0] != TileType.WALL)
                        gScore = lookTable[cur.x, cur.y].g + 1.0f; // if neighbor is not a wall, just add 1 for movement
                    else if (toCheck.x == wee && toCheck.y == lee)
                        gScore = 1.0f; // add 1 to move to goal
                    else if (toCheck.x == 0 || toCheck.x == width - 1 || toCheck.y == 0 || toCheck.y == length - 1)
                        gScore = 100000000.0f; // make gScore for outer walls prohibitive
                    else
                        gScore = lookTable[cur.x, cur.y].g + 100.0f;  // make walls much more expensive to move through

                    // if lower, update neighbor's min gScore
                    if (gScore < lookTable[toCheck.x, toCheck.y].g)
                    {
                        lookTable[toCheck.x, toCheck.y].parent = new Coord(cur.x, cur.y);
                        lookTable[toCheck.x, toCheck.y].g = gScore;
                        lookTable[toCheck.x, toCheck.y].f = gScore + heur(toCheck.x, toCheck.y, wee, lee);
                        lookTable[toCheck.x, toCheck.y].key = toCheck.x + (100 * toCheck.y);

                        // check if node is in q
                        if (!q.ContainsKey(lookTable[toCheck.x, toCheck.y].key))
                            q.Add(lookTable[toCheck.x, toCheck.y].key, new Coord(toCheck.x, toCheck.y));  // add to q if not in q

                    }
                }

            }
        }

        // at end of search, q[final] is the destination and lookTable has all the parents in path we need to follow
        Coord curr = new Coord(final.x, final.y); // start path at goal node

        bool reachedSource = false;
        while (!reachedSource)
        {
            curr = lookTable[curr.x, curr.y].parent; // switch to next node in path (skips goal node on purpose)
            if (curr.x < 0 || curr.y < 0 || curr.x >= width || curr.y >= length) // only should happen at source, we finished fixing path
                reachedSource = true;
            else
            {
                if (solution[curr.x, curr.y][0] == TileType.WALL) // if current node on path is a wall, make it a floor
                    solution[curr.x, curr.y][0] = TileType.FLOOR;
            }
        }

        int w = 0;
        for (float x = bounds.min[0]; x < bounds.max[0]; x += bounds.size[0] / (float)width, w++)
        {
            int l = 0;
            for (float z = bounds.min[2]; z < bounds.max[2]; z += bounds.size[2] / (float)length, l++)
            {
                if ((w >= width) || (l >= width))
                    continue;

                float y = bounds.min[1];

                if ((w == wee) && (l == lee)) // this is the exit
                {
                    // Make wall for exit circle to stand on
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "GOAL_WALL";
                    cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, air_platform_height, bounds.size[2] / (float)length);
                    cube.transform.position = new Vector3(x + bounds.size[0] / (2 * (float)width), y + air_platform_height / 2.0f, z + bounds.size[2] / (2 * (float)length));
                    cube.GetComponent<Renderer>().material = goal_wall_mat;

                    // make exit circle
                    GameObject goal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    goal.name = "GOAL";
                    goal.transform.localScale = new Vector3(bounds.size[0] / (float)width, 0.1f, bounds.size[2] / (float)length);
                    goal.transform.position = new Vector3(x + bounds.size[0] / (2 * (float)width), y + 0.1f + air_platform_height, z + bounds.size[2] / (2 * (float)length));
                    goal.GetComponent<Renderer>().material.color = new Color(0.6f, 2f, 0.8f);
                    goal.AddComponent<BoxCollider>();
                    goal.GetComponent<BoxCollider>().isTrigger = true;
                    goal.GetComponent<BoxCollider>().size = new Vector3(1.0f, grapple_point_height * 15.0f, 1.0f);
                    Destroy(goal.GetComponent<SphereCollider>());
                    goal.AddComponent<Goal>();
                }
                else if (solution[w, l][0] == TileType.WALL)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    // Make border walls reach the ceiling
                    if (w == 0 || l == 0 || l == length - 1 || w == width - 1)
                    {
                        cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, grapple_point_height, bounds.size[2] / (float)length);
                        cube.transform.position = new Vector3(x + bounds.size[0] / (2 * (float)width), y + grapple_point_height / 2.0f, z + bounds.size[2] / (2 * (float)length));
                        //cube.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.4f);
                        cube.GetComponent<Renderer>().material = border_wall_mat;
                        cube.name = "BORDER_WALL";
                    }
                    else
                    {
                        cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, story_height, bounds.size[2] / (float)length);
                        cube.transform.position = new Vector3(x + bounds.size[0] / (2 * (float)width), y + story_height / 2.0f, z + bounds.size[2] / (2 * (float)length));
                        //cube.GetComponent<Renderer>().material.color = new Color(0.6f, 0.8f, 0.8f);
                        cube.GetComponent<Renderer>().material = border_wall_mat;
                        cube.name = "WALL";
                    }
                    
                }
                else if (solution[w, l][0] == TileType.GRAPPLE_POINT)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    float platform_size_y = 1;
                    cube.name = "GRAPPLE_POINT";
                    cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, platform_size_y, bounds.size[2] / (float)length);
                    cube.transform.position = new Vector3(x + bounds.size[0] / (2 * (float)width), y + grapple_point_height - platform_size_y / 2.0f, z + bounds.size[2] / (2 * (float)length));
                    cube.GetComponent<Renderer>().material.color = new Color(5f, 5f, 0f);
                    cube.layer = 8; // set it to be grappleable

                }
                else if (solution[w, l][0] == TileType.AIR_PLAT)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    float platform_size_y = 1;
                    cube.name = "AIR_PLATFORM";
                    cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, platform_size_y, bounds.size[2] / (float)length);
                    float platY;
                    if (w == playerW && l == playerL)
                    {
                        cube.name = "SPAWN_PLATFORM";
                        platY = y + (3.0f * air_platform_height) - platform_size_y / 2.0f;
                    }
                    else
                        platY = Random.Range(y + air_platform_height - platform_size_y / 2.0f, y + (3.0f * air_platform_height) - platform_size_y / 2.0f);
                    cube.transform.position = new Vector3(x + bounds.size[0] / (2 * (float)width), platY, z + bounds.size[2] / (2 * (float)length));
                    cube.GetComponent<Renderer>().material = air_plat_mat;
                }
            }
        }
    }


    // Reloads the scene to give the player a new map to play on
    public void PlayAgain()
    {
        SceneManager.LoadScene("PCGLevel");
    }

    // Reloads the level and lets the player try again
    public void TryLevelAgain()
    {
        // Disable camera
        overhead_cam.gameObject.SetActive(false);
        playedSplash = false;

        // Spawn player at beginning
        player = Instantiate(player_prefab,
            new Vector3(playerX + bounds.size[0] / (2 * (float)width), bounds.min[1] + 3.0f + (3.0f * air_platform_height), playerZ + bounds.size[2] / (2 * (float)length)),
            Quaternion.Euler(0, 90, 0));
        player.name = "PlayerBody";

        player.GetComponent<AudioSource>().PlayOneShot(bgMusic, 0.5f);

        // Reset timer
        timeSpent = 0;

        // Relock cursor, start timer
        // release cursor
        if (Cursor.lockState == CursorLockMode.None)
            Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;

        // Disable lose popup
        lose_popup.SetActive(false);

        // Turn on crosshair
        cursor.SetActive(true);

        // Turn ceiling back on
        ceiling.GetComponent<Renderer>().enabled = true;

    }

    void Update()
    {
        timeSpent += Time.deltaTime;

        // KILL PLAYER IF TOO LOW
        if (player != null)
        {
            if (player.transform.position.y < bounds.min[1] + 1f)
            {
                Destroy(player);
                // set cam and popup active
                overhead_cam.gameObject.SetActive(true);
                lose_popup.gameObject.SetActive(true);

                // play sound if haven't played yet
                if (!playedSplash)
                {
                    overhead_cam.GetComponent<AudioSource>().PlayOneShot(splash, 0.4f);
                    playedSplash = true;
                }

                // release cursor
                if (Cursor.lockState != CursorLockMode.None)
                    Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0;

                // turn off aiming cursor if on
                if (cursor.activeSelf)
                    cursor.SetActive(false);

                ceiling.GetComponent<Renderer>().enabled = false;
            }
        }


        // GOAL HANDLING
        if (player_entered_goal) // the player suceeds here, variable manipulated by House.cs
        {
            // Display time to finish
            finalTime = timeSpent;
            clock_text.text = "Your Time Was: " + finalTime.ToString("00:00");

            // remove player
            Object.Destroy(player);

            // Enable overhead camera
            overhead_cam.gameObject.SetActive(true);
            // play jingle if haven't played yet
            if (!playedJingle)
            {
                overhead_cam.GetComponent<AudioSource>().PlayOneShot(victory);
                playedJingle = true;
            }

            // show winscreen
            win_popup.gameObject.SetActive(true);

            // release cursor
            if (Cursor.lockState != CursorLockMode.None)
                Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;

            // turn off aiming cursor if on
            if (cursor.activeSelf)
                cursor.SetActive(false);

            ceiling.GetComponent<Renderer>().enabled = false;
            return;
        } //
    }
}