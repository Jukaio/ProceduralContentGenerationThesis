using DungeonEvaluation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonEvaluation
{
    public interface Evaluatable
    {
        abstract int[,] Layout { get; }
    }
}


public abstract class Generator : MonoBehaviour, DungeonEvaluation.Evaluatable
{
    // Initialise the Generator
    public abstract void reset();
    // Generate whatever implementation you use in the derived class
    public abstract void generate();
    // Get a TileType matrix representation of the generated environment useable for the evaluation
    public abstract int[,] Layout { get; }
    // Size
    public abstract Vector2Int Size { get; }

}
