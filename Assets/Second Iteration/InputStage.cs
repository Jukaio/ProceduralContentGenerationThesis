//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.Tilemaps;

//namespace DungeonEvaluation
//{


//    // Identify all Caverns
//    // Separate into passable and impassable space
//    public class InputStage 
//    {
//        public enum TileType
//        {
//            Invalid = -1,
//            Ground,
//            Wall
//        }
//        // Generator to Input Tile(Pre-First Stage Tile) 
//        public interface Evaluatable
//        {
//            abstract TileType[,] Layout { get; }
//        }


//        public void set_input_tile_data(TileType[,] that)
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
//            return to_analyse[x, y] == TileType.Ground;
//        }

//        public void analyse()
//        {
//            if (to_analyse == null) {
//                throw new System.Exception("Data Set to analyse cannot be null!");
//            }

//            for (int y = 0; y < to_analyse.GetLength(1); y++) {
//                for (int x = 0; x < to_analyse.GetLength(0); x++) {
//                    if (to_analyse[x, y] == TileType.Ground) {
//                        flooder.start_flood_fill(x, y, add_new_cavern, is_ground, add_tile_to_cavern);
//                    }
//                }
//            }
//        }

//        public void initialise()
//        {
//        }

//        public void reset()
//        {
//        }

//        public int[,] RoomIndexOfTile { get { return room_indeces_per_tile; } }
//        public List<Space> Caverns { get { return caverns; } }

//        public TileType[,] InputLevel { get { return to_analyse; } }

//        private TileType[,] to_analyse = null;
//        private int[,] room_indeces_per_tile;

//        FloodFiller flooder;

//        private List<Space> caverns = new List<Space>();
//    }
//}
