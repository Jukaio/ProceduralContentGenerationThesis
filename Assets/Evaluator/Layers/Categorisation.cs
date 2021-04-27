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
        public class Categorisation : Base<Tile.CavernTile, Tile.DungeonSpaceTile, Cavern, DungeonSpace>
        {
            public Categorisation(int width, int height)
                : base(width, height, null, null)
            {
                flooder = new FloodFiller(width, height);
                scanner.initialise(width, height);
                possible_matrix = new Pair[width, height];
            }



            public enum Scanline
            {
                Horizontal,
                Vertical,
                /// <summary>
                /// Top-left to bottom-right diagonal;
                /// See: https://en.wikipedia.org/wiki/Main_diagonal
                /// </summary>
                MainDiagonal,
                /// <summary>
                /// Bottom-left to top-right diagonal;
                /// Opposite of main diagonal, see MainDiagonal for more
                /// </summary>
                AntiDiagonal,
                Count
            }
            public enum Directions
            {
                NorthWest,  // 0
                North,      //  1
                NorthEast,  // 2
                East,       //  3
                SouthEast,  // 4
                South,      //  5
                SouthWest,  // 6
                West,       //  7
                Count
            }
            public Vector2Int[] direction_steps =
            {
                Vector2Int.up + Vector2Int.left,
                Vector2Int.up,
                Vector2Int.up + Vector2Int.right,
                Vector2Int.right,
                Vector2Int.down + Vector2Int.right,
                Vector2Int.down,
                Vector2Int.down + Vector2Int.left,
                Vector2Int.left,
            };
            Vector2Int get_directional_step(Directions dir)
            {
                if (dir == Directions.Count) return Vector2Int.zero;
                return direction_steps[(int)dir];
            }


            public class LineLengthScanner
            {
                public void initialise(int width, int height)
                {
                    for (int i = 0; i < (int)Scanline.Count; i++) {
                        output[i] = new int[width, height];
                        visited[i] = new bool[width, height];
                    }
                }
                private delegate bool BreakAction(int x, int y);
                static int scan(int x, int y,
                                int step_x, int step_y,
                                int depth,
                                Tile.CavernTile[,] input,
                                bool[,] visited)
                {
                    if (x < 0 || y < 0 ||
                        x >= visited.GetLength(0) ||
                        y >= visited.GetLength(1)) {
                        return depth;
                    }

                    if (visited[x, y]) {
                        return depth;
                    }

                    visited[x, y] = true;
                    if (input[x, y].IsCavern) {
                        return scan(x + step_x, y + step_y, step_x, step_y, depth + 1, input, visited);
                    }
                    return depth;
                }

                // Positive and Negative implies the direction on the X-Axis
                private static void analyse_direction(Vector2Int origin,
                                                      Vector2Int step,
                                                      Tile.CavernTile[,] input,
                                                      bool[,] visited,
                                                      int[,] output)
                {
                    if (!visited[origin.x, origin.y]) {
                        visited[origin.x, origin.y] = true;
                        int negative = scan(origin.x - step.x, origin.y - step.y,
                                            -step.x, -step.y,
                                            0,
                                            input,
                                            visited);
                        int positive = scan(origin.x + step.x, origin.y + step.y,
                                            step.x, step.y,
                                            0,
                                            input,
                                            visited);
                        int count = negative + positive + 1;

                        for (var i = origin;
                            i.x < input.GetLength(0) && i.y < input.GetLength(1) && i.x >= 0 && i.y >= 0;
                            i += step) {
                            if (!input[i.x, i.y].IsCavern) {
                                break; // Reached
                            }
                            output[i.x, i.y] = count;
                        }

                        for (var i = origin;
                             i.x < input.GetLength(0) && i.y < input.GetLength(1) && i.x >= 0 && i.y >= 0;
                             i -= step) {
                            if (!input[i.x, i.y].IsCavern) {
                                break; // Reached end
                            }
                            output[i.x, i.y] = count;
                        }
                    }
                }

                public void count_continues_tiles_in_line(List<Cavern> that, Tile.CavernTile[,] layout)
                {
                    clear();

                    for (int direction = 0; direction < (int)Scanline.Count; direction++) {
                        var to_visit = visited[direction];
                        var to_output = output[direction];
                        var step = steps[direction];

                        foreach (var cavern in that) {
                            foreach (var index in cavern.TileIndeces) {
                                analyse_direction(index,
                                                  step,
                                                  layout,
                                                  to_visit,
                                                  to_output);
                            }
                        }
                    }
                }

                public void clear()
                {
                    for (int i = 0; i < (int)Scanline.Count; i++) {
                        for (int iy = 0; iy < visited[i].GetLength(1); iy++) {
                            for (int ix = 0; ix < visited[i].GetLength(0); ix++) {
                                output[i][ix, iy] = 0;
                                visited[i][ix, iy] = false;
                            }
                        }
                    }
                }

                public int[,] this[Scanline dir]
                {
                    get { return output?[(int)dir]; }
                    set { output[(int)dir] = value; }
                }


                private Vector2Int[] steps =
                {
                    new Vector2Int(+1, 0),
                    new Vector2Int(0, +1),
                    new Vector2Int(+1, -1),
                    new Vector2Int(+1, +1),
                };

                private int[][,] output = new int[(int)Scanline.Count][,];
                private bool[][,] visited = new bool[(int)Scanline.Count][,];
            }

            public override void reset()
            {
                base.reset();
                flooder.reset();
                scanner.clear();
                in_caverns.Clear();
                out_corridors.Clear();
                out_rooms.Clear();
                for_each(Size,
                (int x, int y) =>
                {
                    possible_matrix[x, y] = Pair.invalid;
                    possible_matrix[x, y].type = DungeonSpace.Type.Wall;
                });
            }

            DungeonSpace.Type get_type(int x, int y, int corridor_length /* Chebyshev Distance */ )
            {
                if (!Input[x, y].IsCavern) { // not cavern
                    return DungeonSpace.Type.Wall;
                }

                var hori_rule = scanner[Scanline.Horizontal][x, y] < corridor_length;
                var vert_rule = scanner[Scanline.Vertical][x, y] < corridor_length;
                var mdia_rule = scanner[Scanline.MainDiagonal][x, y] < corridor_length;
                var adia_rule = scanner[Scanline.AntiDiagonal][x, y] < corridor_length;
                if (hori_rule && vert_rule && mdia_rule && adia_rule) {
                    return DungeonSpace.Type.Room;
                }
                else if (hori_rule || vert_rule || mdia_rule || adia_rule) {
                    return DungeonSpace.Type.Corridor;
                }
                return DungeonSpace.Type.Room;
            }

            bool is_potential_corrridor(int x, int y, int corridor_length)
            {
                return get_type(x, y, corridor_length) == DungeonSpace.Type.Corridor;
            }

            //bool is_potential_room(int x, int y)
            //{
            //    return possible_ground_type[x, y] == DungeonSpace.Type.Room;
            //}




            HashSet<Space> get_rooms_around(List<DungeonSpace> rooms, int x, int y)
            {
                //List<int> indeces = new List<int>();
                HashSet<Space> ajdacent_rooms = new HashSet<Space>();
                FloodFiller.on_start starter = (int ix, int iy) =>
                {
                    flooder.reset();
                };
                FloodFiller.on_fill adder = (int ix, int iy) =>
                {

                };
                FloodFiller.on_check checker = (int ix, int iy) =>
                {
                    var type = possible_matrix[ix, iy].type;
                    if (type == DungeonSpace.Type.Room) {
                        int adjacent_index = possible_matrix[ix, iy].index;
                        if (!ajdacent_rooms.Contains(rooms[adjacent_index])) {
                            ajdacent_rooms.Add(rooms[adjacent_index]);
                        }
                    }
                    return type == DungeonSpace.Type.Corridor;
                };
                flooder.start_flood_fill(x, y, starter, checker, adder, true);
                return ajdacent_rooms;
            }
            HashSet<Space> get_corridors_around(List<DungeonSpace> corridors, int x, int y)
            {
                //List<int> indeces = new List<int>();
                HashSet<Space> ajdacent_corridors = new HashSet<Space>();
                FloodFiller.on_start starter = (int ix, int iy) =>
                {
                    flooder.reset();
                };
                FloodFiller.on_fill adder = (int ix, int iy) =>
                {

                };
                FloodFiller.on_check checker = (int ix, int iy) =>
                {
                    var type = possible_matrix[ix, iy].type;
                    if (type == DungeonSpace.Type.Corridor) {
                        int adjacent_index = possible_matrix[ix, iy].index;
                        if (!ajdacent_corridors.Contains(corridors[adjacent_index])) {
                            ajdacent_corridors.Add(corridors[adjacent_index]);
                        }
                    }
                    return type == DungeonSpace.Type.Room;
                };
                flooder.start_flood_fill(x, y, starter, checker, adder, true);
                return ajdacent_corridors;
            }

            List<DungeonSpace> identify_space(DungeonSpace.Type type, bool do_diagonal)
            {
                List<DungeonSpace> to_return = new List<DungeonSpace>();
                flooder.reset();
                foreach (var cavern in in_caverns) {
                    foreach (var index in cavern.TileIndeces) {
                        if (possible_matrix[index.x, index.y].type == type) {
                            flooder.start_flood_fill(index.x, index.y,
                                                    (int x, int y) => {
                                                        to_return.Add(new DungeonSpace(to_return.Count, type));
                                                    },
                                                    (int x, int y) => {
                                                        return possible_matrix[x, y].type == type;
                                                    },
                                                    (int x, int y) => {
                                                        to_return[to_return.Count - 1].add(new Vector2Int(x, y));
                                                        possible_matrix[x, y].index = to_return.Count - 1;
                                                    },
                                                    do_diagonal);
                        }
                    }
                }
                return to_return;
            }

            bool out_of_bounds(Vector2Int index) 
            {
                return (index.x < 0 || index.y < 0 || index.x >= Size.x || index.y >= Size.y);
            }
            public void for_each_corridor_index(ref Pair[,] map, List<Vector2Int> indeces)
            {
                int[] neighbour_type_counter = new int[(int)(DungeonSpace.Type.Wall) + 1];
                for (int i = 0; i < indeces.Count; i++) {
                    var index = indeces[i];

                    // Init data structures
                    //DungeonSpace.Type[] adjacent_types = new DungeonSpace.Type[(int)Directions.Count];
                    //for (int n = 0; n < neighbour_type_counter.Length; n++) {
                    //    neighbour_type_counter[n] = 0;
                    //}

                    //// collect data
                    //for (int dir = 1; dir < (int)Directions.Count; dir += 2) {
                    //    var neighbour_index = direction_steps[dir] + index;
                    //    adjacent_types[dir] = out_of_bounds(neighbour_index) ?
                    //                          DungeonSpace.Type.Wall :
                    //                          possible_matrix[neighbour_index.x, neighbour_index.y].type;
                    //    neighbour_type_counter[(int)adjacent_types[dir]]++;
                    //}

                    //// Apply
                    //if (neighbour_type_counter[(int)DungeonSpace.Type.Room] == 0) {
                    //    if (neighbour_type_counter[(int)DungeonSpace.Type.Corridor] > 0) {
                    //        map[index.x, index.y].type = DungeonSpace.Type.Corridor;
                    //    }
                    //}

                }
            }

            public void for_each_corridor(List<DungeonSpace> corridors)
            {
                Pair[,] buffer = possible_matrix.Clone() as Pair[,];
                foreach(var corridor in corridors) {
                    if(corridor.TileIndeces.Count == 0) {
                        continue;
                    }

                    //for_each_corridor_index(ref buffer, corridor.TileIndeces);


                    flooder.reset();
                    HashSet<int> adjacent = new HashSet<int>();
                    flooder.start_flood_fill(corridor.TileIndeces[0].x, corridor.TileIndeces[0].y,
                    (int x, int y) => { },
                    (int x, int y) =>
                    {
                        bool go_next = possible_matrix[x, y].type == DungeonSpace.Type.Corridor;
                        if (!go_next) {
                            if (possible_matrix[x, y].type == DungeonSpace.Type.Room) {
                                if (!adjacent.Contains(possible_matrix[x, y].index)) {
                                    adjacent.Add(possible_matrix[x, y].index);
                                }
                            }
                        }
                        return go_next;
                    },
                    (int x, int y) => { }, false);

                    if (adjacent.Count > 1) {
                        continue;
                    }

                    foreach (var index in corridor.TileIndeces) {
                        buffer[index.x, index.y].type = DungeonSpace.Type.Room;
                    }
                }
                // Apply
                for_each(Size, (int x, int y) =>
                {
                    possible_matrix[x, y] = buffer[x, y];
                });
            }

            public void for_each_room_index(ref Pair[,] map, List<Vector2Int> indeces)
            {

                int[] neighbour_type_counter = new int[(int)(DungeonSpace.Type.Wall) + 1];
                for (int i = 0; i < indeces.Count; i++) {
                    var index = indeces[i];

                    // Init data structures
                    DungeonSpace.Type[] adjacent_types = new DungeonSpace.Type[(int)Directions.Count];
                    for (int n = 0; n < neighbour_type_counter.Length; n++) {
                        neighbour_type_counter[n] = 0;
                    }

                    // collect data
                    for (int dir = 1; dir < (int)Directions.Count; dir+=2) {
                        var neighbour_index = direction_steps[dir] + index;
                        adjacent_types[dir] = out_of_bounds(neighbour_index) ?
                                              DungeonSpace.Type.Wall :
                                              possible_matrix[neighbour_index.x, neighbour_index.y].type;
                        neighbour_type_counter[(int)adjacent_types[dir]]++;
                    }

                    // use data
                    if(neighbour_type_counter[(int)DungeonSpace.Type.Room] == 0) {
                        if(neighbour_type_counter[(int)DungeonSpace.Type.Corridor] > 0) {
                            map[index.x, index.y].type = DungeonSpace.Type.Corridor;
                        }
                    }
                }
            }

            public void for_each_room(List<DungeonSpace> rooms)
            {
                Pair[,] buffer = possible_matrix.Clone() as Pair[,];
                foreach (var room in rooms) {
                    for_each_room_index(ref buffer, room.TileIndeces);


                }


                // Apply
                for_each(Size, (int x, int y) =>
                {
                    possible_matrix[x, y] = buffer[x, y];
                });
            }

            void remove_rooms_surrounded_by_single_corridor(List<DungeonSpace> rooms)
            {
                Pair[,] buffer = possible_matrix.Clone() as Pair[,];
                HashSet<int> adjacent = new HashSet<int>();
                flooder.reset();

                foreach (var room in rooms) {
                    adjacent.Clear();
                    var index = room.TileIndeces[0];
                    flooder.start_flood_fill(index.x, index.y,
                                             (int x, int y) => { },
                                             (int x, int y) =>
                                             {
                                                 bool go_next = possible_matrix[x, y].type == DungeonSpace.Type.Room;
                                                 if (!go_next) {
                                                     if (possible_matrix[x, y].type == DungeonSpace.Type.Corridor) {
                                                         if (!adjacent.Contains(possible_matrix[x, y].index)) {
                                                             adjacent.Add(possible_matrix[x, y].index);
                                                         }
                                                     }
                                                 }
                                                 return go_next;
                                             },
                                             (int x, int y) => { },
                                             true);
                    if (adjacent.Count < 2) {
                        foreach (var tile_index in room.TileIndeces) {
                            buffer[tile_index.x, tile_index.y].type = DungeonSpace.Type.Corridor;
                        }
                    }
                }

                for_each(Size, (int x, int y) =>
                {
                    possible_matrix[x, y] = buffer[x, y];
                });
            }

            void remove_dead_end_corridors(List<DungeonSpace> corridors)
            {
                Pair[,] buffer = possible_matrix.Clone() as Pair[,];
                HashSet<int> adjacent = new HashSet<int>();
                flooder.reset();
                foreach (var corridor in corridors) {
                    adjacent.Clear();
                    var index = corridor.TileIndeces[0];
                    flooder.start_flood_fill(index.x, index.y,
                                             (int x, int y) => { },
                                             (int x, int y) => {
                                                 bool go_next = possible_matrix[x, y].type == DungeonSpace.Type.Corridor;
                                                 if (!go_next) {
                                                     if (possible_matrix[x, y].type == DungeonSpace.Type.Room) {
                                                         if (!adjacent.Contains(possible_matrix[x, y].index)) {
                                                             adjacent.Add(possible_matrix[x, y].index);
                                                         }
                                                     }
                                                 }
                                                 return go_next;
                                             },
                                             (int x, int y) => { },
                                             true);
                    if (adjacent.Count < 2) {
                        foreach (var tile_index in corridor.TileIndeces) {
                            buffer[tile_index.x, tile_index.y].type = DungeonSpace.Type.Room;
                        }
                    }
                }

                for_each(Size, (int x, int y) =>
                {
                    possible_matrix[x, y] = buffer[x, y];
                });
            }

            // from and to should only use Corridor/Room!!
            void clean_up_single_tiles(DungeonSpace.Type from, DungeonSpace.Type to)
            {
                Pair[,] buffer = possible_matrix.Clone() as Pair[,];
                int[] neighbour_type_counter = new int[(int)(DungeonSpace.Type.Wall) + 1];
                foreach (var cavern in in_caverns) {
                    foreach (var index in cavern.TileIndeces) {
                        if(possible_matrix[index.x, index.y].type != from) {
                            continue;
                        }

                        // Init data structures
                        DungeonSpace.Type[] adjacent_types = new DungeonSpace.Type[(int)Directions.Count];
                        for (int n = 0; n < neighbour_type_counter.Length; n++) {
                            neighbour_type_counter[n] = 0;
                        }

                        // collect data
                        for (int dir = 1; dir < (int)Directions.Count; dir += 2) {
                            var neighbour_index = direction_steps[dir] + index;
                            adjacent_types[dir] = out_of_bounds(neighbour_index) ?
                                                  DungeonSpace.Type.Wall :
                                                  possible_matrix[neighbour_index.x, neighbour_index.y].type;
                            neighbour_type_counter[(int)adjacent_types[dir]]++;
                        }

                        // use data
                        if (neighbour_type_counter[(int)from] < 1) {
                            if (neighbour_type_counter[(int)to] > 0) {
                                buffer[index.x, index.y].type = to;
                            }
                        }
                    }

                    for_each(Size, (int x, int y) =>
                    {
                        possible_matrix[x, y] = buffer[x, y];
                    });
                }
            }

            void remove_small_rooms(List<DungeonSpace> rooms)
            {
                foreach (var room in rooms) {
                    if (room.TileIndeces.Count < 6) {
                        foreach (var index in room.TileIndeces) {
                            possible_matrix[index.x, index.y].type = DungeonSpace.Type.Corridor;
                        }
                    }
                }
            }

            public void analyse()
            {
                scanner.count_continues_tiles_in_line(in_caverns, Input);
                foreach (var cavern in in_caverns) {
                    foreach (var index in cavern.TileIndeces) {
                        int x = index.x;
                        int y = index.y;
                        if (is_potential_corrridor(x, y, 5)) {
                            possible_matrix[x, y] = new Pair(-1, DungeonSpace.Type.Corridor);
                        }
                        else {
                            possible_matrix[x, y] = new Pair(-1, DungeonSpace.Type.Room);
                        }
                    }
                }

                clean_up_single_tiles(DungeonSpace.Type.Room, DungeonSpace.Type.Corridor);
                clean_up_single_tiles(DungeonSpace.Type.Corridor, DungeonSpace.Type.Room);

                inbetween_corridors = identify_space(DungeonSpace.Type.Corridor, true);
                inbetween_rooms = identify_space(DungeonSpace.Type.Room, false);

                remove_small_rooms(inbetween_rooms);

                inbetween_corridors = identify_space(DungeonSpace.Type.Corridor, true);
                inbetween_rooms = identify_space(DungeonSpace.Type.Room, false);

                remove_dead_end_corridors(inbetween_corridors);

                out_corridors = identify_space(DungeonSpace.Type.Corridor, true);
                out_rooms = identify_space(DungeonSpace.Type.Room, false);

                for_each(Size, (int x, int y) =>
                {
                    var type = possible_matrix[x, y].type;
                    var index = possible_matrix[x, y].index;
                    switch(type)
                    {
                        case DungeonSpace.Type.Corridor:
                            Output[x, y].set(out_corridors[index]);
                            break;

                        case DungeonSpace.Type.Room:
                            Output[x, y].set(out_rooms[index]);
                            break;

                        case DungeonSpace.Type.Wall:
                            Output[x, y].set((DungeonSpace)null);
                            break;
                    }
                });
            }


            public struct Pair
            {
                public Pair(int index, DungeonSpace.Type type)
                {
                    this.index = index;
                    this.type = type;
                }
                public int index;
                public DungeonSpace.Type type;

                public static Pair invalid = new Pair(-1, DungeonSpace.Type.Invalid);
            }


            public Pair[,] possible_matrix = null;
            public List<DungeonSpace> out_corridors = new List<DungeonSpace>();
            public List<DungeonSpace> out_rooms = new List<DungeonSpace>();

            public List<DungeonSpace> inbetween_corridors;
            public List<DungeonSpace> inbetween_rooms;

            private LineLengthScanner scanner = new LineLengthScanner();


            private FloodFiller flooder = null;
            public readonly List<Cavern> in_caverns = new List<Cavern>();
        }
    }
}
