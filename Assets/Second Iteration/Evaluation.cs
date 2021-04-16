//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Tilemaps;

//namespace DungeonEvaluation
//{
//    public enum InputTileType
//    {
//        Invalid = -1,
//        Ground,
//        Wall
//    }

//    public enum GroundType
//    {
//        None = -1,
//        Room,
//        Corridor
//    }

//    public enum Scanline
//    {
//        Horizontal,
//        Vertical,
//        /// <summary>
//        /// Top-left to bottom-right diagonal;
//        /// See: https://en.wikipedia.org/wiki/Main_diagonal
//        /// </summary>
//        MainDiagonal,
//        /// <summary>
//        /// Bottom-left to top-right diagonal;
//        /// Opposite of main diagonal, see MainDiagonal for more
//        /// </summary>
//        AntiDiagonal,
//        Count
//    }

//    public enum Directions
//    {
//        NorthWest,  // 0
//        North,      //  1
//        NorthEast,  // 2
//        East,       //  3
//        SouthEast,  // 4
//        South,      //  5
//        SouthWest,  // 6
//        West,       //  7
//        Count
//    }

//    // Generator to Input Tile(Pre-First Stage Tile) 
//    public interface Evaluatable
//    {
//        abstract InputTileType[,] Layout { get; }
//    }

//    public class Space
//    {
//        public Space(int index)
//        {
//            this.index = index;
//            tile_indexes = new List<Vector2Int>();
//        }

//        public void add(Vector2Int tile_coordinate)
//        {
//            tile_indexes.Add(tile_coordinate);
//        }

//        public int Index { get { return index; } }
//        public List<Vector2Int> TileIndeces { get { return tile_indexes; } }

//        private int index;
//        private List<Vector2Int> tile_indexes;
//    }

//    public class FirstStage
//    {
//        public void set_input_tile_data(InputTileType[,] that)
//        {
//            to_analyse = that;
//            room_indeces_per_tile = new int[that.GetLength(0), that.GetLength(1)];
//            flooder = new FloodFiller(that.GetLength(0), that.GetLength(1));
//        }

//        void add_new_cavern(int x, int y)
//        {
//            var cavern = new Space(caverns.Count);
//            caverns.Add(cavern);
//        }
//        void add_tile_to_cavern(int x, int y)
//        {
//            var cavern = caverns[caverns.Count - 1];
//            cavern.add(new Vector2Int(x, y));
//            room_indeces_per_tile[x, y] = cavern.Index;
//        }
//        bool is_ground(int x, int y)
//        {
//            return to_analyse[x, y] == InputTileType.Ground;
//        }

//        public void analyse()
//        {
//            if(to_analyse == null) {
//                throw new System.Exception("Data Set to analyse cannot be null!");
//            }

//            for (int y = 0; y < to_analyse.GetLength(1); y++) {
//                for (int x = 0; x < to_analyse.GetLength(0); x++) {
//                    if(to_analyse[x, y] == InputTileType.Ground) {
//                        flooder.start_flood_fill(x, y, add_new_cavern, is_ground, add_tile_to_cavern);
//                    }
//                }
//            }
//        }


//        public int[,] RoomIndexOfTile{ get{ return room_indeces_per_tile; } }
//        public List<Space> Caverns{ get{ return caverns; } }

//        public InputTileType[,] InputLevel { get { return to_analyse; } }

//        private InputTileType[,] to_analyse = null;
//        private int[,] room_indeces_per_tile;

//        FloodFiller flooder;

//        private List<Space> caverns = new List<Space>();
//    }

//    public class SecondStage
//    {
//        public class LineLengthScanner
//        {
//            public void initialise(int width, int height)
//            {
//                for (int i = 0; i < (int)Scanline.Count; i++)
//                {
//                    output[i] = new int[width, height];
//                    visited[i] = new bool[width, height];
//                }
//            }
//            private delegate bool BreakAction(int x, int y);
//            static int scan(int x, int y,
//                            int step_x, int step_y,
//                            int depth,
//                            InputTileType[,] input,
//                            bool[,] visited)
//            {
//                if (x < 0 || y < 0 ||
//                    x >= visited.GetLength(0) ||
//                    y >= visited.GetLength(1))
//                {
//                    return depth;
//                }

//                if (visited[x, y])
//                {
//                    return depth;
//                }

//                visited[x, y] = true;
//                if (input[x, y] == InputTileType.Ground)
//                {
//                    return scan(x + step_x, y + step_y, step_x, step_y, depth + 1, input, visited);
//                }
//                return depth;
//            }

//            // Positive and Negative implies the direction on the X-Axis
//            private static void analyse_direction(Vector2Int origin, 
//                                                  Vector2Int step, 
//                                                  InputTileType[,] input, 
//                                                  bool[,] visited, 
//                                                  int[,] output)
//            {
//                if (!visited[origin.x, origin.y])
//                {
//                    visited[origin.x, origin.y] = true;
//                    int negative = scan(origin.x - step.x, origin.y - step.y,
//                                        -step.x, -step.y,
//                                        0,
//                                        input,
//                                        visited);
//                    int positive = scan(origin.x + step.x, origin.y + step.y,
//                                        step.x, step.y,
//                                        0,
//                                        input,
//                                        visited);
//                    int count = negative + positive + 1;

//                    for(var i = origin; 
//                        i.x < input.GetLength(0) && i.y < input.GetLength(1) && i.x >= 0 && i.y >= 0; 
//                        i += step)
//                    {
//                        if(input[i.x, i.y] != InputTileType.Ground) {
//                            break; // Reached
//                        }
//                        output[i.x, i.y] = count;
//                    }

//                    for (var i = origin;
//                         i.x < input.GetLength(0) && i.y < input.GetLength(1) && i.x >= 0 && i.y >= 0;
//                         i -= step)
//                    {
//                        if (input[i.x, i.y] != InputTileType.Ground) {
//                            break; // Reached end
//                        }
//                        output[i.x, i.y] = count;
//                    }
//                }
//            }

//            public void count_continues_tiles_in_line(FirstStage that)
//            {
//                clear();

//                for (int direction = 0; direction < (int)Scanline.Count; direction++)
//                {
//                    var to_visit = visited[direction];
//                    var to_output = output[direction];
//                    var step = steps[direction];

//                    foreach (var cavern in that.Caverns)
//                    {
//                        foreach (var index in cavern.TileIndeces)
//                        {
//                            analyse_direction(index,
//                                              step,
//                                              that.InputLevel,
//                                              to_visit,
//                                              to_output);
//                        }
//                    }
//                }
//            }

//            public void clear()
//            {
//                for (int i = 0; i < (int)Scanline.Count; i++)
//                {
//                    for (int iy = 0; iy < visited[i].GetLength(1); iy++)
//                    {
//                        for (int ix = 0; ix < visited[i].GetLength(0); ix++)
//                        {
//                            output[i][ix, iy] = -0;
//                            visited[i][ix, iy] = false;
//                        }
//                    }
//                }
//            }

//            public int[,] this[Scanline dir]
//            {
//                get { return output?[(int)dir]; }
//                set { output[(int)dir] = value; }
//            }


//            private Vector2Int[] steps =
//            {
//                new Vector2Int(+1, 0),
//                new Vector2Int(0, +1),
//                new Vector2Int(+1, -1),
//                new Vector2Int(+1, +1),
//            };

//            private int[][,] output = new int[(int)Scanline.Count][,];
//            private bool[][,] visited = new bool[(int)Scanline.Count][,];
//        }

//        void add_new_potential_corridor(int x, int y)
//        {
//            var corridor = new Space(possible_corridors.Count);
//            possible_corridors.Add(corridor);
//        }
//        void add_tile_to_potential_corridor(int x, int y)
//        {
//            var corridor = possible_corridors[possible_corridors.Count - 1];
//            corridor.add(new Vector2Int(x, y));
//        }
//        bool is_potential_corrridor(int x, int y)
//        {
//            var hori_rule = scanner[Scanline.Horizontal][x, y] < 5;
//            var vert_rule = scanner[Scanline.Vertical][x, y] < 5;
//            var mdia_rule = scanner[Scanline.MainDiagonal][x, y] < 4;
//            var adia_rule = scanner[Scanline.AntiDiagonal][x, y] < 4;

//            bool might_be_corridor = hori_rule || vert_rule || mdia_rule || adia_rule;
//            bool is_ground = to_analyse.InputLevel[x, y] == InputTileType.Ground;
//            possible_ground_type[x, y] = might_be_corridor ? GroundType.Corridor : is_ground ? GroundType.Room : GroundType.None;
//            return might_be_corridor && is_ground;
//        }

//        void add_new_potential_room(int x, int y)
//        {
//            var room = new Space(possible_rooms.Count);
//            possible_room_indeces[x, y] = room.Index;
//            possible_rooms.Add(room);
//        }
//        void add_tile_to_potential_room(int x, int y)
//        {
//            var room = possible_rooms[possible_rooms.Count - 1];
//            possible_room_indeces[x, y] = room.Index;
//            room.add(new Vector2Int(x, y));
//        }
//        bool is_potential_room(int x, int y)
//        {
//            return possible_ground_type[x, y] == GroundType.Room;
//        }



//        public void set_input_tile_data(FirstStage that)
//        {
//            to_analyse = that;
//            int width = that.InputLevel.GetLength(0);
//            int height = that.InputLevel.GetLength(1);

//            flooder = new FloodFiller(width, height);
//            possible_room_indeces = new int[width, height];
//            scanner.initialise(width, height);
//            possible_ground_type = new GroundType[width, height];
//            for(int y = 0; y < height; y++) {
//                for (int x = 0; x < width; x++) {
//                    possible_ground_type[x, y] = GroundType.None;
//                }
//            }
//        }

//        HashSet<Space> get_rooms_around(int x, int y)
//        {
//            //List<int> indeces = new List<int>();
//            HashSet<Space> ajdacent_rooms = new HashSet<Space>();
//            FloodFiller.on_start starter = (int ix, int iy) =>
//            {
//                flooder.reset();
//            };
//            FloodFiller.on_fill adder = (int ix, int iy) =>
//            {
                
//            };
//            FloodFiller.on_check checker = (int ix, int iy) =>
//            {
//                var type = possible_ground_type[ix, iy];
//                if(type == GroundType.Room) {
//                    int adjacent_index = possible_room_indeces[ix, iy];
//                    if (!ajdacent_rooms.Contains(possible_rooms[adjacent_index])) {
//                        ajdacent_rooms.Add(possible_rooms[adjacent_index]);
//                    }
//                }
//                return type == GroundType.Corridor;
//            };
//            flooder.start_flood_fill(x, y, starter, checker, adder, true);
//            return ajdacent_rooms;
//        }
//        HashSet<Space> get_corridors_around(int x, int y)
//        {
//            //List<int> indeces = new List<int>();
//            HashSet<Space> ajdacent_corridors = new HashSet<Space>();
//            FloodFiller.on_start starter = (int ix, int iy) =>
//            {
//                flooder.reset();
//            };
//            FloodFiller.on_fill adder = (int ix, int iy) =>
//            {

//            };
//            FloodFiller.on_check checker = (int ix, int iy) =>
//            {
//                var type = possible_ground_type[ix, iy];
//                if (type == GroundType.Corridor) {
//                    int adjacent_index = possible_room_indeces[ix, iy];
//                    if (!ajdacent_corridors.Contains(possible_rooms[adjacent_index])) {
//                        ajdacent_corridors.Add(possible_rooms[adjacent_index]);
//                    }
//                }
//                return type == GroundType.Room;
//            };
//            flooder.start_flood_fill(x, y, starter, checker, adder, true);
//            return ajdacent_corridors;
//        }



//        class TileMetaData
//        {
//            public TileMetaData(InputTileType[,] world, LineLengthScanner scanner, int x, int y)
//            {
//                index = new Vector2Int(x, y);
//                type = world[index.x, index.y];
//                for (int i = 0; i < (int)Scanline.Count; i++) {
//                    scanline_counts[i] = scanner[(Scanline)i][index.x, index.y];
//                }
//            }

//            public Vector2Int index = new Vector2Int(-1, -1);
//            public InputTileType type = InputTileType.Invalid;
//            public int[] scanline_counts = new int[(int)Scanline.Count];
//        }

//        TileMetaData[] get_neighbours_around(int x, int y)
//        {
//            var world = to_analyse.InputLevel;
//            TileMetaData[] neighbours = new TileMetaData[(int)Directions.Count];

//            var width = world.GetLength(0);
//            var height = world.GetLength(1);

//            bool[] checkable_directions = new bool[(int)Directions.Count];
//            checkable_directions[(int)Directions.South] = y > 0;
//            checkable_directions[(int)Directions.North] = y < height - 1;
//            checkable_directions[(int)Directions.West] = x > 0;
//            checkable_directions[(int)Directions.East] = x < width - 1;
//            checkable_directions[(int)Directions.SouthWest] = checkable_directions[(int)Directions.South] && checkable_directions[(int)Directions.West];
//            checkable_directions[(int)Directions.SouthEast] = checkable_directions[(int)Directions.South] && checkable_directions[(int)Directions.East];
//            checkable_directions[(int)Directions.NorthWest] = checkable_directions[(int)Directions.North] && checkable_directions[(int)Directions.West];
//            checkable_directions[(int)Directions.NorthEast] = checkable_directions[(int)Directions.North] && checkable_directions[(int)Directions.East];

//            if(checkable_directions[(int)Directions.North]) {
//                neighbours[(int)Directions.North] = new TileMetaData(world, scanner, x, y + 1);
//            }
//            if (checkable_directions[(int)Directions.South]) {
//                neighbours[(int)Directions.South] = new TileMetaData(world, scanner, x, y - 1);
//            }
//            if (checkable_directions[(int)Directions.West]) {
//                neighbours[(int)Directions.West] = new TileMetaData(world, scanner, x - 1, y);
//            }
//            if (checkable_directions[(int)Directions.East]) {
//                neighbours[(int)Directions.East] = new TileMetaData(world, scanner, x + 1, y);
//            }

//            if (checkable_directions[(int)Directions.NorthEast]) {
//                neighbours[(int)Directions.NorthEast] = new TileMetaData(world, scanner, x + 1, y + 1);
//            }
//            if (checkable_directions[(int)Directions.NorthWest]) {
//                neighbours[(int)Directions.NorthWest] = new TileMetaData(world, scanner, x - 1, y + 1);
//            }
//            if (checkable_directions[(int)Directions.SouthEast]) {
//                neighbours[(int)Directions.SouthEast] = new TileMetaData(world, scanner, x + 1, y - 1);
//            }
//            if (checkable_directions[(int)Directions.SouthWest]) {
//                neighbours[(int)Directions.SouthWest] = new TileMetaData(world, scanner, x - 1, y - 1);
//            }
//            return neighbours;
//        }



//        public void analyse()
//        {
//            scanner.count_continues_tiles_in_line(to_analyse);
//            possible_corridors.Clear();

//            // Apply rules based on count
//            var world = to_analyse.InputLevel;

//            flooder.reset();
//            foreach(var cavern in to_analyse.Caverns) {
//                foreach (var index in cavern.TileIndeces) {
//                    if (is_potential_corrridor(index.x, index.y)) {
//                        flooder.start_flood_fill(index.x, index.y,
//                                             add_new_potential_corridor,
//                                             is_potential_corrridor,
//                                             add_tile_to_potential_corridor,
//                                             true);
//                    }
//                }
//            }

//            flooder.reset();
//            foreach (var cavern in to_analyse.Caverns) {
//                foreach (var index in cavern.TileIndeces) {
//                    flooder.start_flood_fill(index.x, index.y,
//                                             add_new_potential_room,
//                                             is_potential_room,
//                                             add_tile_to_potential_room);
//                }
//            }

//            corridors.Clear();
//            flooder.reset();
//            foreach (var corridor in possible_corridors) {
//                var rooms = get_rooms_around(corridor.TileIndeces[0].x, corridor.TileIndeces[0].y);
//                if(!(rooms.Count > 1)) {
//                    continue;
//                }

//                var c = new Space(corridor.Index);
//                foreach (var index in corridor.TileIndeces) {
//                    var hori_rule = scanner[Scanline.Horizontal][index.x, index.y] < 4;
//                    var vert_rule = scanner[Scanline.Vertical][index.x, index.y] < 4;
//                    var adia_rule = scanner[Scanline.AntiDiagonal][index.x, index.y] < 4;
//                    var mdia_rule = scanner[Scanline.MainDiagonal][index.x, index.y] < 4;

//                    //if (hori_rule && vert_rule && mdia_rule && adia_rule) {
//                    //    continue;
//                    //}

//                    //var neighbours = get_neighbours_around(index.x, index.y);
//                    //bool go_next = false;
//                    //foreach(var tile in neighbours) {
//                    //    if (tile == null) {
//                    //        go_next = true;
//                    //        break;
//                    //    }
//                    //}
//                    //if (go_next) continue;


//                    c.add(index);
//                }

//                if(c.TileIndeces.Count > 0) {
//                    corridors.Add(c);
//                }
//            }
//            //var corrdidor = new Space(-1);
//            //foreach(var room in possible_rooms) {
//            //    if(room.TileIndeces.Count < 3) {
//            //        foreach(var index in room.TileIndeces) {
//            //            corrdidor.add(index);
//            //        }
//            //    }
//            //}
//            //corridors.Add(corrdidor);
//        }



//        FloodFiller flooder;
//        public List<Space> possible_corridors = new List<Space>();
//        public List<Space> possible_rooms = new List<Space>();
//        public int[,] possible_room_indeces = null;
//        public GroundType[,] possible_ground_type = null;

//        public List<Space> corridors = new List<Space>();
//        private FirstStage to_analyse = null;
//        public LineLengthScanner scanner = new LineLengthScanner();
//    }

//}
