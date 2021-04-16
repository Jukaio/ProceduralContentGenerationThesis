using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonEvaluation
{
    namespace Layer
    {
        public class Output : Base<Tile.DungeonSpaceTile, MooreCell<int>, DungeonSpace, int>
        {
            public Output(int width, int height)
                : base(width, height, null, -1) { }


            // change return type based on what we want to output
            // In this case text to file :) 
            public int end()
            {
                return 0;
            }
        }
    }
}
