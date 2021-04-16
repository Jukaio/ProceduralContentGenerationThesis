using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonEvaluation
{
    

    namespace Layer
    {
        public class Input : Base<MooreCell<int>, Tile.TraversabilityTile, int, TraversabilityType>
        {
            public Input(int width, int height)
                : base(width, height, -1, TraversabilityType.Invalid) { }

            public void start(int[,] level)
            {
                for_each(Size,
                (int x, int y) =>
                {
                    Output[x, y].set((TraversabilityType)level[x, y]);
                });
            }
        }
    }
}
