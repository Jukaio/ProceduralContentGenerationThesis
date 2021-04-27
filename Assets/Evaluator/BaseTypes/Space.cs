using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DungeonEvaluation
{
    public class Space
    {
        public Space(int index)
        {
            this.index = index;
            tile_indexes = new List<Vector2Int>();
        }

        public void add(Vector2Int tile_coordinate)
        {
            tile_indexes.Add(tile_coordinate);
        }

        public int Index { get { return index; } }
        public List<Vector2Int> TileIndeces { get { return tile_indexes; } }

        private int index;
        private List<Vector2Int> tile_indexes;
    }
    public class Cavern : Space
    {
        public Cavern(int index)
            : base(index)
        { }
    }

    public class DungeonSpace : Space
    {
        public enum Type
        {
            Invalid = -1,
            Room,
            Corridor,
            Wall
        }
        public DungeonSpace(int index, Type type)
        : base(index)
        {
            this.type = type;
        }

        public Type type;
        public List<DungeonSpace> neighbours = new List<DungeonSpace>();
    }
}
