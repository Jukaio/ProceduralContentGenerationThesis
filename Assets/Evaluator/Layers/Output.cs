using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;

namespace DungeonEvaluation
{
    namespace Layer
    {
        public class Output : Base<Tile.DungeonSpaceTile, MooreCell<int>, DungeonSpace, int>
        {
            public Output(int width, int height)
                : base(width, height, null, -1) 
            {
                flooder = new FloodFiller(width, height);
            }


            // change return type based on what we want to output
            // In this case text to file :) 
            public Export.Data end(int id, Main main)
            {
                var space = main.input;
                var traversability = main.traversability;
                var category = main.categorisation;

                Export.Data data = new Export.Data();
                data.room = new Export.Data.Room();
                data.space = new Export.Data.Space();
                data.corridor = new Export.Data.Corridor();

                // Space
                data.levelID = id;
                data.space.unreachableCount = traversability.out_caverns.Count - 1;
                data.space.passableSize = space.out_passable.Count;
                data.space.impassableSize = space.out_impasslable.Count;
                data.space.playableSize = 0;
                foreach(var cavern in traversability.out_caverns) {
                    if(cavern.TileIndeces.Count > data.space.playableSize) {
                        data.space.playableSize = cavern.TileIndeces.Count;
                    }
                }
                data.space.unreachableSize = data.space.passableSize - data.space.playableSize;

                // Rooms
                data.room.count = category.out_rooms.Count;
                data.room.biggestSize = 0;
                data.room.smallestSize = float.MaxValue;
                data.room.averageSize = 0;
                foreach(var room in category.out_rooms) {
                    if(room.TileIndeces.Count > data.room.biggestSize) {
                        data.room.biggestSize = room.TileIndeces.Count;
                    }
                    if (room.TileIndeces.Count < data.room.smallestSize) {
                        data.room.smallestSize = room.TileIndeces.Count;
                    }
                    data.room.averageSize += room.TileIndeces.Count;
                }
                data.room.averageSize /= data.room.count;
                // TODO: Decision per room

                HashSet<DungeonSpace> adjacent = new HashSet<DungeonSpace>();
                flooder.reset();
                foreach (var room in category.out_rooms) {
                    adjacent.Clear();
                    var index = room.TileIndeces[0];
                    flooder.start_flood_fill(index.x, index.y,
                                             (int x, int y) => { },
                                             (int x, int y) => {
                                                if(Input[x, y].Value == null) {
                                                     return false;
                                                }

                                                 bool go_next = Input[x, y].Value.type == DungeonSpace.Type.Room;
                                                 if (!go_next) {
                                                     if (Input[x, y].Value.type == DungeonSpace.Type.Corridor) {
                                                         if (!adjacent.Contains(Input[x, y].Value)) {
                                                             adjacent.Add(Input[x, y].Value);
                                                         }
                                                     }
                                                 }
                                                 return go_next;
                                             },
                                             (int x, int y) => { },
                                             true);
                    data.room.decisionsPerRoom += adjacent.Count;
                }
                data.room.decisionsPerRoom /= data.room.count;

                // Corridors
                data.corridor.count = category.out_corridors.Count;
                data.corridor.biggestSize = 0;
                data.corridor.smallestSize = float.MaxValue;
                data.corridor.averageSize = 0;
                foreach (var corridor in category.out_corridors) {
                    if (corridor.TileIndeces.Count > data.corridor.biggestSize) {
                        data.corridor.biggestSize = corridor.TileIndeces.Count;
                    }
                    if (corridor.TileIndeces.Count < data.corridor.smallestSize) {
                        data.corridor.smallestSize = corridor.TileIndeces.Count;
                    }
                    data.corridor.averageSize += corridor.TileIndeces.Count;
                }
                data.corridor.averageSize /= data.corridor.count;

                return data;
            }

            private FloodFiller flooder = null;

        }
    }
}
