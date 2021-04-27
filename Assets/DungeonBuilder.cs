using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonBuilder : MonoBehaviour
{
    public enum GeneratorType
    {
        CellularAutomata00,
        CellularAutomata01,
        Drunkard,
    }
    private Generator generator;
    [SerializeField] private Tilemap walls_tilemap;
    [SerializeField] private Tilemap corridor_tilemap;
    [SerializeField] private Tilemap ground_tilemap;

    [SerializeField] private Tilemap debug_tilemap_00;
    [SerializeField] private Tilemap debug_tilemap_01;

    [SerializeField] private Sprite wall;
    [SerializeField] private Sprite floor;
    [SerializeField] private Sprite corridor;

    private Tile wall_tile;
    private Tile floor_tile;
    private Tile corridor_tile;
    [SerializeField] private GeneratorType generator_type;
    // Evaluation Stages
    //DungeonEvaluation.InputStage first_stage;
    //DungeonEvaluation.SeparationStage second_stage;
    DungeonEvaluation.Main dungeon_evaluation;
    void Start()
    {
        switch (generator_type) {
            case GeneratorType.CellularAutomata00:
                generator = GetComponent<UnityTutorialCellularAutomata>();
                break;
            case GeneratorType.CellularAutomata01:
                generator = GetComponent<CellularAutomataGenerator>();
                break;
            case GeneratorType.Drunkard:
                generator = GetComponent<Drunkard>();
                break;
        }

        dungeon_evaluation = new DungeonEvaluation.Main(generator);
        //first_stage = new DungeonEvaluation.InputStage();
        //second_stage = new DungeonEvaluation.SeparationStage();
        //generator.reset();
        //generator.generate();

        wall_tile = new Tile();
        wall_tile.sprite = wall;
        floor_tile = new Tile();
        floor_tile.sprite = floor;
        corridor_tile = new Tile();
        corridor_tile.sprite = corridor;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            initialise_map();
        }

        if (dungeon_evaluation.categorisation != null) {
            var camera_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int index = new Vector2Int((int)camera_pos.x, (int)camera_pos.y);

            //if (index.x >= 0 && index.x < dungeon_evaluation.categorisation.possible_matrix.GetLength(0) &&
            //    index.y >= 0 && index.y < dungeon_evaluation.categorisation.possible_matrix.GetLength(1)) {

            //    Debug.Log(dungeon_evaluation.categorisation.possible_matrix[index.x, index.y].index);
            //    //Debug.Log(index.ToString() + " " + first_stage.RoomIndexOfTile[index.x, index.y]);
            //}
        }

        //if (first_stage.Caverns != null)
        //{
        //    if(first_stage.Caverns.Count > 0)
        //    {
        //        if (Input.GetKeyDown(KeyCode.UpArrow))
        //        {
        //            draw_cavern_index = Mathf.Clamp(draw_cavern_index + 1, 0, first_stage.Caverns.Count - 1);
        //            foreach(var index in first_stage.Caverns[draw_cavern_index].TileIndeces)
        //            {
        //                walls.SetTile(new Vector3Int(index.x, index.y, 0), null);
        //                ground.SetTile(new Vector3Int(index.x, index.y, 0), floor_tile);
        //            }
        //        }
        //    }
        //}

    }
    public void initialise_map()
    {

        //var level = generator.Layout;
        //first_stage.set_input_tile_data(level);
        //first_stage.analyse();

        //second_stage.set_input_tile_data(first_stage);
        //second_stage.analyse();
        dungeon_evaluation.reset();
        dungeon_evaluation.run();

        debug_tilemap_01.ClearAllTiles();
        for (int y = 0; y < dungeon_evaluation.traversability.Input.GetLength(1); y++) {
            for (int x = 0; x < dungeon_evaluation.traversability.Input.GetLength(0); x++) {
                switch (dungeon_evaluation.traversability.Input[x, y].Value) {
                    case DungeonEvaluation.TraversabilityType.Passable:
                        debug_tilemap_01.SetTile(new Vector3Int(x, y, 0), floor_tile);
                        break;
                    case DungeonEvaluation.TraversabilityType.Impassable:
                        debug_tilemap_01.SetTile(new Vector3Int(x, y, 0), wall_tile);
                        break;
                }
            }
        }


        var layer = dungeon_evaluation.categorisation.possible_matrix;
        for (int y = 0; y < layer.GetLength(1); y++) {
            for (int x = 0; x < layer.GetLength(0); x++) {
                if (layer[x, y].type == DungeonEvaluation.DungeonSpace.Type.Invalid) {
                    continue;
                }
                switch (layer[x, y].type) {
                    case DungeonEvaluation.DungeonSpace.Type.Room:
                        ground_tilemap.SetTile(new Vector3Int(x, y, 0), floor_tile);
                        corridor_tilemap.SetTile(new Vector3Int(x, y, 0), null);
                        walls_tilemap.SetTile(new Vector3Int(x, y, 0), null);
                        break;
                    case DungeonEvaluation.DungeonSpace.Type.Corridor:
                        ground_tilemap.SetTile(new Vector3Int(x, y, 0), null);
                        corridor_tilemap.SetTile(new Vector3Int(x, y, 0), corridor_tile);
                        walls_tilemap.SetTile(new Vector3Int(x, y, 0), null);
                        break;
                    case DungeonEvaluation.DungeonSpace.Type.Wall:
                        ground_tilemap.SetTile(new Vector3Int(x, y, 0), null);
                        corridor_tilemap.SetTile(new Vector3Int(x, y, 0), null);
                        walls_tilemap.SetTile(new Vector3Int(x, y, 0), wall_tile);
                        break;
                }
            }
        }
        //for (int y = 0; y < layer.GetLength(1); y++) {
        //    for (int x = 0; x < layer.GetLength(0); x++) {
        //        debug_tilemap_00.SetTile(new Vector3Int(x, y, 0), wall_tile);
        //    }
        //}

        //foreach (var room in dungeon_evaluation.categorisation.in_caverns) {
        //    foreach (var index in room.TileIndeces) {
        //        int x = index.x;
        //        int y = index.y;
        //        ground_tilemap.SetTile(new Vector3Int(x, y, 0), floor_tile);
        //        walls_tilemap.SetTile(new Vector3Int(x, y, 0), null);
        //    }
        //}
        debug_tilemap_00.ClearAllTiles();
        //foreach (var room in dungeon_evaluation.categorisation.out_rooms) {
        //    foreach (var index in room.TileIndeces) {
        //        int x = index.x;
        //        int y = index.y;
        //        debug_tilemap_00.SetTile(new Vector3Int(x, y, 0), floor_tile);
        //    }
        //}
        foreach (var corridor in dungeon_evaluation.categorisation.inbetween_corridors) {
            foreach (var index in corridor.TileIndeces) {
                int x = index.x;
                int y = index.y;
                debug_tilemap_00.SetTile(new Vector3Int(x, y, 0), corridor_tile);
            }
        }
        //for (int y = 0; y < layer.Input.GetLength(1); y++) {
        //    for (int x = 0; x < layer.Input.GetLength(0); x++) {
        //        if (layer.Input[x, y].Value == null) {
        //            ground.SetTile(new Vector3Int(x, y, 0), null);
        //            walls.SetTile(new Vector3Int(x, y, 0), wall_tile);
        //        }
        //        else {
        //            if (dungeon_evaluation.output.Input[x, y].Value.type == DungeonEvaluation.DungeonSpace.Type.Corridor) {
        //                ground.SetTile(new Vector3Int(x, y, 0), corridor_tile);
        //                walls.SetTile(new Vector3Int(x, y, 0), null);
        //            }
        //            else if(dungeon_evaluation.output.Input[x, y].Value.type == DungeonEvaluation.DungeonSpace.Type.Room) {
        //                ground.SetTile(new Vector3Int(x, y, 0), floor_tile);
        //                walls.SetTile(new Vector3Int(x, y, 0), null);
        //            }
        //        }
        //    }
        //}
        //foreach (var corridor in second_stage.corridors) {
        //    foreach (var index in corridor.TileIndeces) {
        //        ground.SetTile(new Vector3Int(index.x, index.y, 0), null);
        //    }
        //}
        //foreach (var corridor in second_stage.corridors) {
        //    foreach (var index in corridor.TileIndeces) {
        //        walls.SetTile(new Vector3Int(index.x, index.y, 0), floor_tile);
        //        ground.SetTile(new Vector3Int(index.x, index.y, 0), null);
        //    }
        //}



        //for (int direction = 0; direction < (int)DungeonEvaluation.Directions.Count; direction++)
        //{
        //    var output = second_stage.scanner[(DungeonEvaluation.Directions)direction];
        //    for (int y = 0; y < output.GetLength(1); y++)
        //    {
        //        for (int x = 0; x < output.GetLength(0); x++)
        //        {
        //            if (output[x, y] < 5)
        //            {
        //                ground.SetTile(new Vector3Int(x, y, 0), null);
        //            }
        //        }
        //    }

        //}

        //var horizontal_output = second_stage.scanner[DungeonEvaluation.Directions.Horizontal];
        //for (int y = 0; y < horizontal_output.GetLength(1); y++)
        //{
        //    for (int x = 0; x < horizontal_output.GetLength(0); x++)
        //    {
        //        if (horizontal_output[x, y] < 5)
        //        {
        //            ground.SetTile(new Vector3Int(x, y, 0), null);
        //        }
        //    }
        //}
        //var vertical_output = second_stage.scanner[DungeonEvaluation.Directions.Vertical];
        //for (int y = 0; y < vertical_output.GetLength(1); y++)
        //{
        //    for (int x = 0; x < vertical_output.GetLength(0); x++)
        //    {
        //        if (vertical_output[x, y] < 5)
        //        {
        //            ground.SetTile(new Vector3Int(x, y, 0), null);
        //        }
        //    }
        //}
        //for (int y = 0; y < first_stage.RoomIndexOfTile.GetLength(1); y++)
        //{
        //    for (int x = 0; x < first_stage.RoomIndexOfTile.GetLength(0); x++)
        //    {
        //        if (first_stage.RoomIndexOfTile[x, y] != 0)
        //        {
        //            walls.SetTile(new Vector3Int(x, y, 0), null);
        //            ground.SetTile(new Vector3Int(x, y, 0), null);
        //        }
        //    }
        //}
    }
}
