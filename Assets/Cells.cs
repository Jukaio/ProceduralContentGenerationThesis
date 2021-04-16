using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellType {
    Neumann,
    Moore
}

// Generic Cell
public abstract class Cell<Data> //: Object
{
    public Cell()
    {
        neighbours = new List<Cell<Data>>();
    }
    public Cell(Data data)
    {
        neighbours = new List<Cell<Data>>();
        set(data);
    }
    public void set(Data data) 
    {
        value = data;
    }
    public void set(Vector2Int index) 
    {
        this.index = index;
    }
    public void set(List<Cell<Data>> neighbours)
    {
        neighbours.Clear();
        for (int i = 0; i < neighbours.Count; i++){
            this.neighbours.Add(neighbours[i]);
        }
    }
    protected void add(Cell<Data> neighbour)
    {
        neighbours.Add(neighbour);
    }


    public abstract void find_and_set_neighbours(Vector2Int at, Cell<Data>[,] grid);

    public Data Value{ get{ return value; } }
    public List<Cell<Data>> Neighbours{ get{ return neighbours; } }
    public Vector2Int Index{ get{ return index; } }

    private Vector2Int index;
    private Data value;
    private List<Cell<Data>> neighbours;
}

public class NeumannCell<Data> : Cell<Data>
{
    // 4 neighbours 
    /*  X   O   X
     *  O   C   O
     *  X   O   X */
    private Cell<Data> get_cell(Vector2Int coordinate, Cell<Data>[,] from)
    {
        return from[coordinate.x, coordinate.y];
    }

    public override void find_and_set_neighbours(Vector2Int coordinate, Cell<Data>[,] grid)
    {
        Vector2Int max = new Vector2Int(grid.GetLength(0), grid.GetLength(1));
        bool left   = (coordinate + Vector2Int.left).x >= 0;
        bool down   = (coordinate + Vector2Int.down).y >= 0;
        bool right  = (coordinate + Vector2Int.right).x < max.x;
        bool up     = (coordinate + Vector2Int.up).y < max.y;

        if (left)   add(get_cell(coordinate + Vector2Int.left, grid));
        if (down)   add(get_cell(coordinate + Vector2Int.down, grid));
        if (right)  add(get_cell(coordinate + Vector2Int.right, grid));
        if (up)     add(get_cell(coordinate + Vector2Int.up, grid));
    }

}

public class MooreCell<Data> : Cell<Data>
{
    // 8 neighbours 
    /*  O   O   O
     *  O   C   O
     *  O   O   O */

    private Cell<Data> get_cell(Vector2Int coordinate, Cell<Data>[,] from)
    {
        return from[coordinate.x, coordinate.y];
    }
    public override void find_and_set_neighbours(Vector2Int coordinate, Cell<Data>[,] grid)
    {
        Vector2Int max = new Vector2Int(grid.GetLength(0), grid.GetLength(1));
        bool left   = (coordinate + Vector2Int.left).x >= 0;
        bool down   = (coordinate + Vector2Int.down).y >= 0;
        bool right  = (coordinate + Vector2Int.right).x < max.x;
        bool up     = (coordinate + Vector2Int.up).y < max.y;

        if (left)   add(get_cell(coordinate + Vector2Int.left, grid));
        if (down)   add(get_cell(coordinate + Vector2Int.down, grid));
        if (right)  add(get_cell(coordinate + Vector2Int.right, grid));
        if (up)     add(get_cell(coordinate + Vector2Int.up, grid));

        if (left && up)     add(get_cell(coordinate + Vector2Int.left + Vector2Int.up, grid));
        if (left && down)   add(get_cell(coordinate + Vector2Int.left + Vector2Int.down, grid));
        if (right && down)  add(get_cell(coordinate + Vector2Int.right + Vector2Int.down, grid));
        if (right && up)    add(get_cell(coordinate + Vector2Int.right + Vector2Int.up, grid));
    }
}

