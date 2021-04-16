using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonEvaluation.Tile
{
    public class CavernTile : MooreCell<Cavern>
    {
        public bool IsCavern { get { return Value != null; } }
    }
}
