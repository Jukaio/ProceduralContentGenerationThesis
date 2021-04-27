using DungeonEvaluation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularAutomataGenerator : Generator
{
    public enum TransitionType {
        Naive,
        GameOfLife,
    }

    private enum CellState
    {
        Invalid = -1,
        Dead = 0, // Ground
        Alive = 1,// Wall
    }

    private enum PseudoRandomGeneratorType {
        UnityRandom,
        CSharpRandom
    }

    [SerializeField] private CellType cell_type;
    [SerializeField] private TransitionType transition_type;
    [SerializeField] private PseudoRandomGeneratorType psuedo_random_type;
    [SerializeField] private Vector2Int size = new Vector2Int(64, 64);
    [Range(0.0f, 1.0f)]
    [SerializeField] private float birth_chance = 0.45f;
    [Range(0, 8)]
    [SerializeField] private int birth_limit = 4;
    [Range(0, 8)]
    [SerializeField] private int death_limit = 3;
    [SerializeField] private int transition_steps = 3;

    private Cell<CellState>[,] cell_matrix;
    private CellState[,] buffer;

    System.Random c_sharp_random;
    public override Vector2Int Size 
    {
        get
        {
            return size;
        }
    }

    private void Start()
    {
        string seed = Time.time.ToString();
        c_sharp_random = new System.Random(seed.GetHashCode());

        cell_matrix = new Cell<CellState>[size.x, size.y];
        buffer = new CellState[size.x, size.y];
        reset();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.S)) {
            transition_step();
        }
    }

    public override int[,] Layout // 0 = Ground; 1 = Wall
    { 
        get 
        {
            int[,] map = new int[size.x, size.y];
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    map[x, y] = (int)cell_matrix[x, y].Value;
                }
            }
            return map;
        } 
    }

    private float random(float min = 0.0f, float max = 1.0f)
    {
        switch (psuedo_random_type) {
            case PseudoRandomGeneratorType.UnityRandom:
                return Random.Range(min, max);
            case PseudoRandomGeneratorType.CSharpRandom:
                return c_sharp_random.Next((int)(min * 100), (int)(max * 100)) / 100.0f;
            default:
                throw new System.Exception("Pseudo Random Generator Type does not exist!");
        }
    }

    public override void reset()
    {
        // initialise cells
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                switch (cell_type)
                {
                    case CellType.Neumann:
                        cell_matrix[x, y] = new NeumannCell<CellState>();
                        break;
                    case CellType.Moore:
                        cell_matrix[x, y] = new MooreCell<CellState>();
                        break;
                    default:
                        throw new System.Exception("DEFAULT CELL TYPE NOT VALID!! BIG ERRRO!");
                }
                cell_matrix[x, y].set(new Vector2Int(x, y));
            }
        }

        // set cell state and neighbours of cells 
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                var type = random() < birth_chance ? CellState.Alive : CellState.Dead;
                cell_matrix[x, y].find_and_set_neighbours(new Vector2Int(x, y), cell_matrix);
                cell_matrix[x, y].set(type);
            }
        }
    }
    private void apply_buffer()
    {
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                cell_matrix[x, y].set(buffer[x, y]);
            }
        }
    }

    private void naive_step(Cell<CellState> cell, int count) {
        var at = cell.Index;
        if (cell.Value == CellState.Alive) {
            buffer[at.x, at.y] = count > death_limit ? CellState.Dead : CellState.Alive;
        }
        else {
            buffer[at.x, at.y] = count < birth_limit ? CellState.Alive : CellState.Dead;
        }
        //if (count > birth_limit)
        //    buffer[at.x, at.y] = CellState.Alive;
        //else if (count < death_limit)
        //    buffer[at.x, at.y] = CellState.Dead;

    }

    private void game_of_life_step(Cell<CellState> cell, int count)
    {
        var at = cell.Index;
        if (cell.Value == CellState.Alive) {
            if (count < 2 || count > 3) {
                buffer[at.x, at.y] = CellState.Dead;
            }
        }
        else {
            if (count == 3) {
                buffer[at.x, at.y] = CellState.Alive;
            }
        }
    }

    private void transition_step()
    {
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                buffer[x, y] = cell_matrix[x, y].Value;
            }
        }

        for (int y = 0; y < size.y; y++) {
            for (int x = 0; x < size.x; x++) {

                var cell = cell_matrix[x, y];
                var count = 0;
                foreach(var n in cell.Neighbours) {
                    if(n.Value == CellState.Alive) {
                        count++;
                    }
                }

                switch (transition_type)
                {
                    case TransitionType.Naive:
                        naive_step(cell, count);
                        break;

                    case TransitionType.GameOfLife:
                        game_of_life_step(cell, count);
                        break;
                }
            }
        }
        apply_buffer();
    }
    public override void generate()
    {
        for (int i = 0; i < transition_steps; i++)
        {
            transition_step();
        }
    }


    private void OnDrawGizmos()
    {
        if (cell_matrix != null)
        {
            var camera_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int index = new Vector2Int((int)camera_pos.x, (int)camera_pos.y);
            if (index.x >= 0 && index.x < cell_matrix.GetLength(0) &&
               index.y >= 0 && index.y < cell_matrix.GetLength(1))
            {
                var cell = cell_matrix[index.x, index.y];
                Gizmos.color = cell.Value == CellState.Alive ? Color.cyan : Color.yellow;
                Gizmos.DrawCube(new Vector3(cell.Index.x + 0.5f, cell.Index.y + 0.5f, 1.0f), Vector3.one);
                foreach(var c in cell.Neighbours) {
                    Gizmos.color = c.Value == CellState.Alive ? Color.cyan : Color.yellow;
                    Gizmos.DrawCube(new Vector3(c.Index.x + 0.5f, c.Index.y + 0.5f, 1.0f), Vector3.one);
                }
            }
        }
    }
}
