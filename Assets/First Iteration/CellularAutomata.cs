using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Runtime.CompilerServices;

namespace PCG
{
    public class CellularAutomata : MonoBehaviour
    {
        // Environment
        [SerializeField] private Tilemap walls;
        [SerializeField] private Tilemap ground;
        private Cell[,] cells;
        private Cell[,] cell_buffer;
        [Space]
        // Cellular Automata parameters
        [SerializeField] private Vector2Int size = new Vector2Int(64, 64);
        [Range(0.0f, 1.0f)]
        [SerializeField] private float birth_chance = 0.45f;
        [Range(0, 8)]
        [SerializeField] private int birth_limit = 4;
        [Range(0, 8)]
        [SerializeField] private int death_limit = 3;
        [SerializeField] private int transition_steps = 2;
        [Space]
        // Reference GameObjects for visualisation
        [SerializeField] private GameObject floor;
        [SerializeField] private GameObject wall;
        [SerializeField] public Cell cell_prototype = new Cell();

        private static Cell[,] copy(Cell[,] from)
        {
            Cell[,] to = new Cell[from.GetLength(0), from.GetLength(1)];
            for (int y = 0; y < from.GetLength(1); y++)
            {
                for (int x = 0; x < from.GetLength(0); x++)
                {
                    to[x, y] = from[x, y];
                }
            }
            return to;
        }

        private Cell make_cell(Cell.Type type, Vector2Int coord, Cell[,] parent)
        {
            return new Cell(type, coord, parent);
        }

        private Tile wall_tile = null;
        private void Start()
        {
            cell_buffer = new Cell[size.x, size.y];
            cells = new Cell[size.x, size.y];

            wall_tile = new Tile();
            wall_tile.sprite = wall.GetComponent<SpriteRenderer>().sprite;

            var ground_tile = new Tile();
            ground_tile.sprite = floor.GetComponent<SpriteRenderer>().sprite;
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    walls.SetTile(new Vector3Int(x, y, 0), null);
                    ground.SetTile(new Vector3Int(x, y, 0), ground_tile);
                }
            }
            initialise();
            generate();
            create();
        }

        private void initialise()
        {
            for(int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    Cell.Type type = Random.Range(0, 1.0f) < birth_chance ? Cell.Type.Alive : Cell.Type.Dead;
                    cells[x, y] = make_cell(type, new Vector2Int(x, y), cells);
                    cell_buffer[x, y] = cells[x, y];
                }
            }
        }

        private void transition_step()
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    var cell = cells[x, y];
                    var index = new Vector2Int(x, y);
                    var count = cell.count_neighbours(Cell.Type.Alive);
                    if (cell.type == Cell.Type.Alive)
                    {
                        Cell.Type type = count < birth_limit ? Cell.Type.Dead : Cell.Type.Alive;
                        var temp = make_cell(type, index, cells);
                        cell_buffer[x, y] = temp;
                    }
                    else
                    {
                        Cell.Type type = count > death_limit ? Cell.Type.Alive : Cell.Type.Dead;
                        var temp = make_cell(type, index, cells);
                        cell_buffer[x, y] = temp;
                    }
                }
                cells = copy(cell_buffer);
            }
        }

        private void generate()
        {
            for (int i = 0; i < transition_steps; i++)
            {
                transition_step();
            }
        }

        

        private void create()
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    var cell = cells[x, y];
                    var index = new Vector3Int(x, y, 0);

                    if (cell.type != Cell.Type.Invalid)
                    {
                        var temp = cell.type == Cell.Type.Dead ?
                                   null :
                                   wall_tile;
                        walls.SetTile(index, temp);
                    }
                }
            }
        }
    }
}

