using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Drunkard : Generator
{
    public int gridWidth;
    public int gridHeight;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int fillPercent = 10;
    private int fillAmount = 0;
    private Vector2Int currentPos;

    int[,] grid;
    System.Random pseudoRandom;

    public override int[,] Layout // 0 = Ground; 1 = Wall
    {
        get
        {
            return grid;
        }
    }
    public override Vector2Int Size => new Vector2Int(gridWidth, gridHeight);

    void Start()
    {
        if (useRandomSeed) {
            seed = Time.time.ToString();
        }
        pseudoRandom = new System.Random(seed.GetHashCode());
    }


    void FillMap()
    {
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                grid[x, y] = 1;
            }
        }
    }

    void Walk()
    {
        currentPos = new Vector2Int(pseudoRandom.Next(0, gridWidth), pseudoRandom.Next(0, gridHeight));
        grid[currentPos.x, currentPos.y] = 0;

        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        while (fillAmount < gridHeight * gridWidth * fillPercent / 100) {
            currentPos += directions[pseudoRandom.Next(0, 4)];
            currentPos.x = Mathf.Clamp(currentPos.x, 1, gridWidth - 2);
            currentPos.y = Mathf.Clamp(currentPos.y, 1, gridHeight - 2);
            if (grid[currentPos.x, currentPos.y] == 1) {
                grid[currentPos.x, currentPos.y] = 0;
                fillAmount++;
            }
        }
    }

    public override void reset()
    {
        grid = new int[gridWidth, gridHeight];
        fillAmount = 0;
    }

    public override void generate()
    {
        FillMap();
        Walk();
    }
}