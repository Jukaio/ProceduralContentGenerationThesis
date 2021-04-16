using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonEvaluation.Tile
{
    public class TraversabilityTile : MooreCell<TraversabilityType>
    {
        public bool IsInvalid { get { return Value == TraversabilityType.Invalid; } }
        public bool IsPassable { get { return Value == TraversabilityType.Passable; } }
        public bool IsImpassable { get { return Value == TraversabilityType.Impassable; } }

    }
}
