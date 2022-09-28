using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

public class MapController : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [Header("General Settings")]
    [SerializeField] private VoronoiController voronoiController = null;
    [SerializeField] private MineController mineController = null;
    [SerializeField] private Transform holder = null;

    [Header("Map Settings")]
    [SerializeField] private Vector2Int mapSize = Vector2Int.zero;
    [SerializeField] private Vector3 offset = Vector3.zero;
    [SerializeField] private List<Vector2Int> blockeds = null;
    [SerializeField] private List<NodeWeight> weights = null;

    [Header("Base Settings")]
    [SerializeField] private GameObject basePrefab = null;
    [SerializeField] private Vector2Int basePosition = default;

    [Header("Block Settings")]
    [SerializeField] private Transform blockHolder = null;
    [SerializeField] private GameObject blockPrefab = null;
    #endregion

    #region PRIVATE_FIELDS
    private Node[] map = null;

    private MinerBase minerBase = null;
    private List<NodeSite> nodeSites = null;

    private Action<Node[]> onUpdateMap = null;

    private Color subtractAlpha = new Color(0f, 0f, 0f, 0.95f);
    #endregion

    #region PROPERTIES
    public Node[] Map { get => map; }
    #endregion

    #region STRUCTS
    [Serializable]
    public struct NodeSite
    {
        public string id;
        public Vector2Int position;

        public NodeSite(string id, Vector2Int position)
        {
            this.id = id;
            this.position = position;
        }
    }

    [Serializable]
    public struct NodeWeight
    {
        public Vector2Int position;
        public int weight;
    }
    #endregion

    #region UNITY_CALLS
    private void OnDrawGizmos()
    {
        if (map == null)
            return;

        GUIStyle style = new GUIStyle() { fontSize = 8 };

        foreach (Node node in map)
        {
            Vector3 worldPosition = new Vector3(node.position.x, node.position.y, 0.0f) + offset;

            Gizmos.color = Color.white - subtractAlpha;
            Gizmos.DrawWireCube(worldPosition, Vector3.one);
            Handles.Label(worldPosition, node.position.ToString(), style);
        }

        foreach (Node node in map)
        {
            if (node.color != Color.white)
            {
                Vector3 worldPosition = new Vector3(node.position.x, node.position.y, 0.0f) + offset;

                Gizmos.color = node.color;
                Gizmos.DrawWireCube(worldPosition, Vector3.one);
            }
        }
    }
    #endregion

    #region PUBLIC_METHODS
    public void Init(Action<Node[]> onUpdateMap)
    {
        this.onUpdateMap = onUpdateMap;

        map = new Node[mapSize.x * mapSize.y];

        NodeUtils.mapSize = mapSize;
        NodeUtils.offset = offset;
        NodeUtils.usedPositions.AddRange(blockeds);
        NodeUtils.usedPositions.Add(basePosition);

        nodeSites = new List<NodeSite>();
        nodeSites.Add(new NodeSite(NodeUtils.baseId, basePosition));

        SetMapData();

        mineController.Init(voronoiController.SetVoronoi);
        voronoiController.Init();
    }

    public void UpdateMap()
    {
        SetSpecialNodes();
        SpawnBlocks();

        onUpdateMap?.Invoke(map);
    }

    public void SpawnSites()
    {
        SpawnBase();
        SpawnBlocks();
        mineController.SpawnMines();

        for (int i = 0; i < mineController.Mines.Count; i++)
        {
            nodeSites.Add(new NodeSite(NodeUtils.mineId, mineController.Mines[i].Position));
        }
    }

    public Mine GetMineCloser(Vector3 minerPos)
    {
        return voronoiController.GetMineCloser(minerPos);
    }

    public void DepositMoneyInBase(int money)
    {
        minerBase.Deposit(money);
    }

    public Vector2Int GetNodeSitePositionById(string id)
    {
        for (int i = 0; i < nodeSites.Count; i++)
        {
            if (nodeSites[i].id == id)
            {
                return new Vector2Int(nodeSites[i].position.x, nodeSites[i].position.y);
            }
        }

        return Vector2Int.zero;
    }

    public Node GetNodeByPosition(Vector2Int position)
    {
        return map[NodeUtils.PositionToIndex(position)];
    }

    public Node GetNodeBySiteId(string siteId)
    {
        return map[NodeUtils.PositionToIndex(nodeSites.Find(node => node.id == siteId).position)];
    }
    #endregion

    #region PRIVATE_METHODS
    private void SetMapData()
    {
        int ID = 0;
        for (int i = 0; i < mapSize.y; i++)
        {
            for (int j = 0; j < mapSize.x; j++)
            {
                map[ID] = new Node(ID, new Vector2Int(j, i));
                ID++;
            }
        }

        SetSpecialNodes();
    }

    private void SpawnBlocks()
    {
        for (int i = 0; i < blockHolder.childCount; i++)
        {
            GameObject blockGO = blockHolder.GetChild(i).gameObject;
            Destroy(blockGO);
        }

        for (int i = 0; i < blockeds.Count; i++)
        {
            GameObject blockGO = Instantiate(blockPrefab, blockHolder);
            blockGO.transform.position = new Vector3(blockeds[i].x, blockeds[i].y, 0f) + offset;
        }
    }

    private void SetSpecialNodes()
    {
        for (int i = 0; i < blockeds.Count; i++)
        {
            map[NodeUtils.PositionToIndex(blockeds[i])].Block();
        }
        for (int i = 0; i < weights.Count; i++)
        {
            map[NodeUtils.PositionToIndex(weights[i].position)].SetWeight(weights[i].weight);
        }
    }

    private void SpawnBase()
    {
        GameObject baseGO = Instantiate(basePrefab, holder);
        baseGO.transform.position = new Vector3(basePosition.x, basePosition.y, 0f) + offset;

        minerBase = baseGO.GetComponent<MinerBase>();
    }
    #endregion
}
