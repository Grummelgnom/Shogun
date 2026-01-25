using UnityEngine;

[System.Serializable]
public class Position
{
    public int row;
    public int column;

    public Position(int r, int c)
    {
        row = r;
        column = c;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Position)) return false;
        Position p = (Position)obj;
        return row == p.row && column == p.column;
    }

    public override int GetHashCode()
    {
        return row * 1000 + column;
    }

    public override string ToString()
    {
        return $"({row},{column})";
    }
}
