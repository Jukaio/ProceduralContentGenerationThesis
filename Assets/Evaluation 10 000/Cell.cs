using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PCG
{
    public class Cell : Object
    {
        public enum Type
        {
            Invalid = -1,
            Alive = 0,
            Dead,
            Count
        }

        public Type type;
        public Vector2Int coordinate;
        protected List<Cell> neighbours = null;

        public readonly Cell[,] parent;

        enum Direction
        {
            UpLeft      = 0,
            Up          = 1,
            UpRight     = 2,
            Left        = 3,
            Right       = 4,
            DownLeft    = 5,
            Down        = 6,
            DownRight   = 7
        }
        public BoolMatrix9x9 should_check_neighbour = new BoolMatrix9x9();
        bool should_check(Direction dir)
        {
            return should_check_neighbour.ticks[(int)dir];
        }
        public int should_check_count()
        {
            int count = 0;
            foreach(bool test in should_check_neighbour.ticks)
            {
                if (test) count++;
            }
            return count;
        }
        public Cell()
        {
            this.type = Type.Invalid;
            this.coordinate = Vector2Int.zero;
            this.parent = null;
        }
        public Cell(Type type, Vector2Int coord, Cell[,] parent)
        {
            this.type = type;
            this.coordinate = coord;
            this.parent = parent;
        }
        private Cell get_cell_from_parent(Vector2Int index)
        {
            return parent[index.x, index.y];
        }
        public List<Cell> get_neigbours()
        {
            if (neighbours == null)
            {
                neighbours = new List<Cell>();
                bool left = (coordinate + Vector2Int.left).x > 0;
                bool down = (coordinate + Vector2Int.down).y > 0;
                bool right = (coordinate + Vector2Int.right).x < parent.GetLength(0);
                bool up = (coordinate + Vector2Int.up).y < parent.GetLength(1);
                
                

                if (should_check(Direction.Left) && left) neighbours.Add(get_cell_from_parent(coordinate + Vector2Int.left));
                if (should_check(Direction.Down) && down) neighbours.Add(get_cell_from_parent(coordinate + Vector2Int.down));
                if (should_check(Direction.Right) && right) neighbours.Add(get_cell_from_parent(coordinate + Vector2Int.right));
                if (should_check(Direction.Up) && up) neighbours.Add(get_cell_from_parent(coordinate + Vector2Int.up));

                if (should_check(Direction.UpLeft) && left && up) neighbours.Add(get_cell_from_parent(coordinate + Vector2Int.left + Vector2Int.up));
                if (should_check(Direction.DownLeft) && left && down) neighbours.Add(get_cell_from_parent(coordinate + Vector2Int.left + Vector2Int.down));
                if (should_check(Direction.DownRight) && right && down) neighbours.Add(get_cell_from_parent(coordinate + Vector2Int.right + Vector2Int.down));
                if (should_check(Direction.UpRight) && right && up) neighbours.Add(get_cell_from_parent(coordinate + Vector2Int.right + Vector2Int.up));
            }
            /* if neighbours = null do Moore Neighbours */
            return neighbours;
        }

        public int count_neighbours(Type type)
        {
            var cells = get_neigbours();

            var value = 0;
            cells.ForEach((Cell cell) =>
            {
                value += cell.type == type ? 1 : 0;
            });
            return value;
        }
    }
}
