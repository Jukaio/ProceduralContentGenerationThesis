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
                                output[i][ix, iy] = -0;
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
            }

            void add_new_potential_corridor(int x, int y)
            {
                var corridor = new DungeonSpace(possible_corridors.Count, DungeonSpace.Type.Corridor);
                possible_corridors.Add(corridor);
            }
            void add_tile_to_potential_corridor(int x, int y)
            {
                var corridor = possible_corridors[possible_corridors.Count - 1];
                corridor.add(new Vector2Int(x, y));
            }
            bool is_potential_corrridor(int x, int y)
            {
                var hori_rule = scanner[Scanline.Horizontal][x, y] < 5;
                var vert_rule = scanner[Scanline.Vertical][x, y] < 5;
                var mdia_rule = scanner[Scanline.MainDiagonal][x, y] < 5;
                var adia_rule = scanner[Scanline.AntiDiagonal][x, y] < 5;

                bool might_be_corridor = hori_rule || vert_rule || mdia_rule || adia_rule;
                bool is_ground = Input[x, y].IsCavern;
                return might_be_corridor && is_ground;
            }

            void add_new_potential_room(int x, int y)
            {
                var room = new DungeonSpace(possible_rooms.Count, DungeonSpace.Type.Room);
                possible_rooms.Add(room);
            }
            void add_tile_to_potential_room(int x, int y)
            {
                var room = possible_rooms[possible_rooms.Count - 1];
                room.add(new Vector2Int(x, y));
            }
            //bool is_potential_room(int x, int y)
            //{
            //    return possible_ground_type[x, y] == DungeonSpace.Type.Room;
            //}




            //HashSet<Space> get_rooms_around(int x, int y)
            //{
            //    //List<int> indeces = new List<int>();
            //    HashSet<Space> ajdacent_rooms = new HashSet<Space>();
            //    FloodFiller.on_start starter = (int ix, int iy) =>
            //    {
            //        flooder.reset();
            //    };
            //    FloodFiller.on_fill adder = (int ix, int iy) =>
            //    {

            //    };
            //    FloodFiller.on_check checker = (int ix, int iy) =>
            //    {
            //        var type = possible_ground_type[ix, iy];
            //        if (type == GroundType.Room) {
            //            int adjacent_index = possible_room_indeces[ix, iy];
            //            if (!ajdacent_rooms.Contains(possible_rooms[adjacent_index])) {
            //                ajdacent_rooms.Add(possible_rooms[adjacent_index]);
            //            }
            //        }
            //        return type == GroundType.Corridor;
            //    };
            //    flooder.start_flood_fill(x, y, starter, checker, adder, true);
            //    return ajdacent_rooms;
            //}
            //HashSet<Space> get_corridors_around(int x, int y)
            //{
            //    //List<int> indeces = new List<int>();
            //    HashSet<Space> ajdacent_corridors = new HashSet<Space>();
            //    FloodFiller.on_start starter = (int ix, int iy) =>
            //    {
            //        flooder.reset();
            //    };
            //    FloodFiller.on_fill adder = (int ix, int iy) =>
            //    {

            //    };
            //    FloodFiller.on_check checker = (int ix, int iy) =>
            //    {
            //        var type = possible_ground_type[ix, iy];
            //        if (type == GroundType.Corridor) {
            //            int adjacent_index = possible_room_indeces[ix, iy];
            //            if (!ajdacent_corridors.Contains(possible_rooms[adjacent_index])) {
            //                ajdacent_corridors.Add(possible_rooms[adjacent_index]);
            //            }
            //        }
            //        return type == GroundType.Room;
            //    };
            //    flooder.start_flood_fill(x, y, starter, checker, adder, true);
            //    return ajdacent_corridors;
            //}



            //class TileMetaData
            //{
            //    public TileMetaData(InputStage.TileType[,] world, LineLengthScanner scanner, int x, int y)
            //    {
            //        index = new Vector2Int(x, y);
            //        type = world[index.x, index.y];
            //        for (int i = 0; i < (int)Scanline.Count; i++) {
            //            scanline_counts[i] = scanner[(Scanline)i][index.x, index.y];
            //        }
            //    }

            //    public Vector2Int index = new Vector2Int(-1, -1);
            //    public InputStage.TileType type = InputStage.TileType.Invalid;
            //    public int[] scanline_counts = new int[(int)Scanline.Count];
            //}

            //TileMetaData[] get_neighbours_around(int x, int y)
            //{
            //    var world = to_analyse.InputLevel;
            //    TileMetaData[] neighbours = new TileMetaData[(int)Directions.Count];

            //    var width = world.GetLength(0);
            //    var height = world.GetLength(1);

            //    bool[] checkable_directions = new bool[(int)Directions.Count];
            //    checkable_directions[(int)Directions.South] = y > 0;
            //    checkable_directions[(int)Directions.North] = y < height - 1;
            //    checkable_directions[(int)Directions.West] = x > 0;
            //    checkable_directions[(int)Directions.East] = x < width - 1;
            //    checkable_directions[(int)Directions.SouthWest] = checkable_directions[(int)Directions.South] && checkable_directions[(int)Directions.West];
            //    checkable_directions[(int)Directions.SouthEast] = checkable_directions[(int)Directions.South] && checkable_directions[(int)Directions.East];
            //    checkable_directions[(int)Directions.NorthWest] = checkable_directions[(int)Directions.North] && checkable_directions[(int)Directions.West];
            //    checkable_directions[(int)Directions.NorthEast] = checkable_directions[(int)Directions.North] && checkable_directions[(int)Directions.East];

            //    if (checkable_directions[(int)Directions.North]) {
            //        neighbours[(int)Directions.North] = new TileMetaData(world, scanner, x, y + 1);
            //    }
            //    if (checkable_directions[(int)Directions.South]) {
            //        neighbours[(int)Directions.South] = new TileMetaData(world, scanner, x, y - 1);
            //    }
            //    if (checkable_directions[(int)Directions.West]) {
            //        neighbours[(int)Directions.West] = new TileMetaData(world, scanner, x - 1, y);
            //    }
            //    if (checkable_directions[(int)Directions.East]) {
            //        neighbours[(int)Directions.East] = new TileMetaData(world, scanner, x + 1, y);
            //    }

            //    if (checkable_directions[(int)Directions.NorthEast]) {
            //        neighbours[(int)Directions.NorthEast] = new TileMetaData(world, scanner, x + 1, y + 1);
            //    }
            //    if (checkable_directions[(int)Directions.NorthWest]) {
            //        neighbours[(int)Directions.NorthWest] = new TileMetaData(world, scanner, x - 1, y + 1);
            //    }
            //    if (checkable_directions[(int)Directions.SouthEast]) {
            //        neighbours[(int)Directions.SouthEast] = new TileMetaData(world, scanner, x + 1, y - 1);
            //    }
            //    if (checkable_directions[(int)Directions.SouthWest]) {
            //        neighbours[(int)Directions.SouthWest] = new TileMetaData(world, scanner, x - 1, y - 1);
            //    }
            //    return neighbours;
            //}



            public void analyse()
            {
                scanner.clear();
                possible_corridors.Clear();
                flooder.reset();

                scanner.count_continues_tiles_in_line(in_caverns, Input);

                // Apply rules based on count
                var world = Input;

                //flooder.reset();
                //foreach (var cavern in in_caverns) {
                //    foreach (var index in cavern.TileIndeces) {
                //        if (is_potential_corrridor(index.x, index.y)) {
                //            flooder.start_flood_fill(index.x, index.y,
                //                                     add_new_potential_corridor,
                //                                     is_potential_corrridor,
                //                                     add_tile_to_potential_corridor,
                //                                     true);
                //        }
                //    }
                //}

                foreach (var cavern in in_caverns) {
                    foreach (var index in cavern.TileIndeces) {
                        if (is_potential_corrridor(index.x, index.y)) {
                            flooder.start_flood_fill(index.x, index.y,
                                                     add_new_potential_corridor,
                                                     is_potential_corrridor,
                                                     add_tile_to_potential_corridor,
                                                     false);
                        }
                    }
                }

                flooder.reset();
                //foreach (var cavern in in_caverns) {
                //    foreach (var index in cavern.TileIndeces) {
                //        flooder.start_flood_fill(index.x, index.y,
                //                                 add_new_potential_room,
                //                                 is_potential_room,
                //                                 add_tile_to_potential_room);
                //    }
                //}

                foreach (var corridor in possible_corridors) {
                    foreach (var index in corridor.TileIndeces) {
                        if (is_potential_corrridor(index.x, index.y)) {
                            int x = index.x;
                            int y = index.y;
                            if (is_potential_corrridor(x, y)) {
                                Output[x, y].set(new DungeonSpace(0, DungeonSpace.Type.Corridor));
                            }
                            else {
                                Output[x, y].set(new DungeonSpace(0, DungeonSpace.Type.Room));
                            }
                        }
                    }
                }

                //for_each(Size,
                //         (int x, int y) =>
                //         {
                //            if(Input[x, y].IsCavern) {
                //                if(is_potential_corrridor(x, y)) {
                //                     Output[x, y].set(new DungeonSpace(0, DungeonSpace.Type.Corridor));
                //                }
                //                else{
                //                    Output[x, y].set(new DungeonSpace(0, DungeonSpace.Type.Room));
                //                }
                //            }
                //            else {
                //                 Output[x, y].set((DungeonSpace)null);
                //            }
                //         });


                //foreach (var corridor in possible_corridors) {
                //    foreach (var index in corridor.TileIndeces) {
                //        Output[index.x, index.y].set(corridor);
                //    }
                //}

                //foreach (var room in possible_rooms) {
                //    foreach (var index in room.TileIndeces) {

                //    }
                //}

                corridors.Clear();
                flooder.reset();
                //foreach (var corridor in possible_corridors) {
                //    var rooms = get_rooms_around(corridor.TileIndeces[0].x, corridor.TileIndeces[0].y);

                //    if (!(rooms.Count > 1)) {
                //        continue;
                //    }

                //    var c = new Space(corridor.Index);
                //    foreach (var index in corridor.TileIndeces) {
                //        var hori_rule = scanner[Scanline.Horizontal][index.x, index.y] < 4;
                //        var vert_rule = scanner[Scanline.Vertical][index.x, index.y] < 4;
                //        var adia_rule = scanner[Scanline.AntiDiagonal][index.x, index.y] < 4;
                //        var mdia_rule = scanner[Scanline.MainDiagonal][index.x, index.y] < 4;

                //        //if (hori_rule && vert_rule && mdia_rule && adia_rule) {
                //        //    continue;
                //        //}

                //        //var neighbours = get_neighbours_around(index.x, index.y);
                //        //bool go_next = false;
                //        //foreach(var tile in neighbours) {
                //        //    if (tile == null) {
                //        //        go_next = true;
                //        //        break;
                //        //    }
                //        //}
                //        //if (go_next) continue;


                //        c.add(index);
                //    }

                //    if (c.TileIndeces.Count > 0) {
                //        corridors.Add(c);
                //    }
                //}
            }




            public List<DungeonSpace> possible_corridors = new List<DungeonSpace>();
            public List<DungeonSpace> possible_rooms = new List<DungeonSpace>();


            public List<Space> corridors = new List<Space>();
            public LineLengthScanner scanner = new LineLengthScanner();


            private FloodFiller flooder = null;
            public readonly List<Cavern> in_caverns = new List<Cavern>();
        }
    }
}
