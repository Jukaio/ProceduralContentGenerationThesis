using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DungeonEvaluation
{
    public enum TraversabilityType
    {
        Invalid = -1,
        Passable,
        Impassable,
    }

    namespace Layer
    {
        public class Traversability : Base<Tile.TraversabilityTile, Tile.CavernTile, TraversabilityType, Cavern>
        {

            public override void reset()
            {
                base.reset();
                flooder.reset();
                out_caverns.Clear();
            }

            public Traversability(int width, int height)
                : base(width, height, TraversabilityType.Invalid, null)
            {
                flooder = new FloodFiller(width, height);
            }

            void add_new_cavern(int x, int y)
            {
                var cavern = new Cavern(out_caverns.Count);
                out_caverns.Add(cavern);
            }
            void add_tile_to_cavern(int x, int y)
            {
                var cavern = out_caverns[out_caverns.Count - 1];
                cavern.add(new Vector2Int(x, y));
                Output[x, y].set(cavern);
            }
            bool is_passable(int x, int y)
            {
                return Input[x, y].IsPassable;
            }

            public void analyse()
            {
                flooder.reset();
                out_caverns.Clear();

                for_each(Size,
                (int x, int y) =>
                {
                    if (Input[x, y].IsPassable) {
                        flooder.start_flood_fill(x, y,
                                                 add_new_cavern,
                                                 is_passable,
                                                 add_tile_to_cavern);
                    }
                });
            }

            public void feed_caverns_forward(Categorisation to)
            {
                foreach (var cavern in out_caverns) {
                    to.in_caverns.Add(cavern);
                }
            }

            private FloodFiller flooder;
            private readonly List<Cavern> out_caverns = new List<Cavern>();
        }
    }
}
