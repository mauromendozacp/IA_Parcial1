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

    #region PUBLIC_FIELDS
    public int ID;
    public Vector2Int position;
    public List<int> adjacentNodeIDs;
    public NodeState state;
    public Color color;
    public int openerID;
    public int weight;
    public int totalWeight;
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
    }

    public Node(int ID, Vector2Int position, NodeState state, int weight)
    {
        this.ID = ID;
        this.position = position;
        this.state = state;
        this.weight = weight;

        adjacentNodeIDs = NodeUtils.GetAdjacentsNodeIDs(position);
        color = Color.white;
        openerID = -1;
        totalWeight = weight;
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
            totalWeight = weight;
        }
    }

    public void SetWeight(int weight)
    {
        this.weight = weight;
        color = Color.cyan;
    }

    public void Update(NodeState state, int weight)
    {
        if (state == NodeState.Blocked)
        {
            Block();
        }

        SetWeight(weight);
    }
    #endregion
}