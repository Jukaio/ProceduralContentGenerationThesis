using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityTutorialCellularAutomata : Generator
{
    public int gridWidth;
    public int gridHeight;

    public string seed;
    public bool useRandomSeed;

    public int timesToSmooth;

    [Range(0, 100)]
    public int randomFillPercent;

    int[,] grid;
    private System.Random pseudoRandom;

    public override int[,] Layout
    {
        get
        {
            return grid;
        }
    }

    public override Vector2Int Size
    {
        get
        {
            return new Vector2Int(gridWidth, gridHeight);
        }
    }

    void Start()
    {
        if (useRandomSeed) {
            seed = Time.time.ToString();
        }
        pseudoRandom = new System.Random(seed.GetHashCode());

        GenerateMap();
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space)) {
        //    GenerateMap();
        //}
    }

    void GenerateMap()
    {
        grid = new int[gridWidth, gridHeight];
        RandomFillMap();

        for (int i = 0; i < timesToSmooth; i++) {
            SmoothMap();
        }
    }

    void RandomFillMap()
    {
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                if (x == 0 || x == gridWidth - 1 || y == 0 || y == gridHeight - 1) {
                    grid[x, y] = 1;
                }
                else {
                    grid[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                int neighbourWallTiles = GetNeighbourWallCount(x, y);

                if (neighbourWallTiles > 4)
                    grid[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    grid[x, y] = 0;
            }
        }
    }

    void GameOfLife() //not used cuz bad generation can be delted
    {
        //https://rosettacode.org/wiki/Conway%27s_Game_of_Life#C.23
        //int[,] tempGrid = new int[gridWidth, gridHeight];
        //for (int x = 0; x < gridWidth; x++)
        //{
        //    for (int y = 0; y < gridHeight; y++)
        //    {
        //        int neighbourWallTiles = GetNeighbourWallCount(x, y);
        //        int value = grid[x, y];

        //        bool temp = value == 1 && (neighbourWallTiles == 2 || neighbourWallTiles == 3) ||
        //                    value == 0 && (neighbourWallTiles == 3);
        //        tempGrid[x, y] = temp ? 1 : 0;
        //    }
        //}

        //grid = tempGrid;

        //https://www.geeksforgeeks.org/program-for-conways-game-of-life/
        int[,] tempGrid = new int[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                if (grid[x, y] == 1 && GetNeighbourWallCount(x, y) < 2) {
                    tempGrid[x, y] = 0;
                }
                else if (grid[x, y] == 1 && GetNeighbourWallCount(x, y) > 3) {
                    tempGrid[x, y] = 0;
                }
                else if (grid[x, y] == 0 && GetNeighbourWallCount(x, y) == 3) {
                    tempGrid[x, y] = 1;
                }
                else {
                    tempGrid[x, y] = grid[x, y];
                }
            }
        }
        grid = tempGrid;
    }

    int GetNeighbourWallCount(int gridX, int gridY) //moore
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
                if (neighbourX >= 0 && neighbourX < gridWidth && neighbourY >= 0 && neighbourY < gridHeight) {
                    if (neighbourX != gridX || neighbourY != gridY) {
                        wallCount += grid[neighbourX, neighbourY];
                    }
                }
                else {
                    wallCount++; //encourages wall growth
                }
            }
        }
        return wallCount;
    }

    void OnDrawGizmos()
    {
        //if (grid != null) {
        //    for (int x = 0; x < gridWidth; x++) {
        //        for (int y = 0; y < gridHeight; y++) {
        //            Gizmos.color = (grid[x, y] == 1) ? Color.green : Color.gray;
        //            Vector3 pos = new Vector3(-gridWidth / 2 + x + 0.5f, -gridHeight / 2 + y + 0.5f, 0);
        //            Gizmos.DrawCube(pos, Vector3.one);
        //        }
        //    }
        //}
    }

    public override void reset()
    {
        //GenerateMap();
    }

    public override void generate()
    {
        GenerateMap();
    }
}
