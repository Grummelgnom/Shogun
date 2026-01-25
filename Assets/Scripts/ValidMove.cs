using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class ValidMove
{
    public Position from;
    public Position to;
    public List<Position> path;

    public ValidMove()
    {
        from = null;
        to = null;
        path = new List<Position>();
    }

    public override string ToString()
    {
        return $"Move from {from} to {to} (Path length: {path.Count})";
    }
}
