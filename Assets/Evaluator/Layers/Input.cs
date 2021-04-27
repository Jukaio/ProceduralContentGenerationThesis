using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DungeonEvaluation
{
    

    namespace Layer
    {
        public class Input : Base<MooreCell<int>, Tile.TraversabilityTile, int, TraversabilityType>
        {
            public Input(int width, int height)
                : base(width, height, -1, TraversabilityType.Invalid) { }

            public override void reset()
            {
                base.reset();
                out_passable.Clear();
                out_impasslable.Clear();
            }

            public void start(int[,] level)
            {
                for_each(Size,
                (int x, int y) =>
                {
                    Output[x, y].set((TraversabilityType)level[x, y]);
                    if(Output[x, y].IsPassable) {
                        out_passable.Add(new Vector2Int(x, y));
                    }
                    else {
                        out_impasslable.Add(new Vector2Int(x, y));
                    }
                });
            }

            readonly public List<Vector2Int> out_passable = new List<Vector2Int>();
            readonly public List<Vector2Int> out_impasslable = new List<Vector2Int>();

        }
    }
}
