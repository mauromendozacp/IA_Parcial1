using System.Collections.Generic;
using UnityEngine;
public static class NodeUtils
{
    #region PUBLIC_FIELDS
    public static Vector2Int MapSize = Vector2Int.zero;
    public static List<Vector2Int> usedPositions = new List<Vector2Int>();

    public const string baseId = "base";
    public const string mineId = "mine";
    #endregion

    #region PUBLIC_METHODS
    public static List<int> GetAdjacentsNodeIDs(Vector2Int position)
    {
        List<int> IDs = new List<int>();
        IDs.Add(PositionToIndex(new Vector2Int(position.x + 1, position.y)));
        IDs.Add(PositionToIndex(new Vector2Int(position.x, position.y - 1)));
        IDs.Add(PositionToIndex(new Vector2Int(position.x - 1, position.y)));
        IDs.Add(PositionToIndex(new Vector2Int(position.x, position.y + 1)));
        return IDs;
    }

    public static int PositionToIndex(Vector2Int position)
    {
        if (position.x < 0 || position.x >= MapSize.x ||
            position.y < 0 || position.y >= MapSize.y)
            return -1;
        return position.y * MapSize.x + position.x;
    }
    #endregion
}