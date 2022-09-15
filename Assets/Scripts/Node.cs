using System.Collections.Generic;
using UnityEngine;

public class Node
{
    #region ENUMS
    public enum NodeState
    {
        Open, //Abiertos por otro nodo pero no visitados
        Closed, //ya visitados
        Blocked, //obstaculos
        Ready //no abiertos por nadie
    }
    #endregion

    #region STRUCTS
    [System.Serializable]
    public struct NodeWeight
    {
        public Vector2Int position;
        public int weight;
    }

    public struct NodeCustom
    {
        public Vector2Int position;
        public Color color;

        public NodeCustom(Vector2Int position, Color color)
        {
            this.position = position;
            this.color = color;
        }
    }
    #endregion

    #region PUBLIC_FIELDS
    public int ID;
    public Vector2Int position;
    public List<int> adjacentNodeIDs;
    public NodeState state;
    public Color color;
    public int openerID;
    public int weight;
    public int totalWeight;
    private int originalWeight;
    #endregion

    #region CONSTRUCTS
    public Node(int ID, Vector2Int position)
    {
        this.ID = ID;
        this.position = position;

        adjacentNodeIDs = NodeUtils.GetAdjacentsNodeIDs(position);
        state = NodeState.Ready;
        color = Color.white;
        openerID = -1;
        weight = 1;
        totalWeight = weight;
        originalWeight = weight;
    }
    #endregion

    #region PUBLIC_METHODS
    public void Open(int openerID, int parentWight)
    {
        this.openerID = openerID;

        state = NodeState.Open;
        color = Color.green;
        totalWeight = weight + parentWight;
    }

    public void Close()
    {
        state = NodeState.Closed;
        color = Color.blue;
    }
    public void Block()
    {
        state = NodeState.Blocked;
        color = Color.red;
    }

    public void Reset()
    {
        if (state != NodeState.Blocked)
        {
            state = NodeState.Ready;
            openerID = -1;
            weight = originalWeight;
            totalWeight = weight;
        }
    }

    public void SetWeight(int weight)
    {
        this.weight = weight;
        originalWeight = weight;
    }
    #endregion
}