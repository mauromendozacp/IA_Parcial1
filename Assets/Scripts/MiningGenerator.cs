using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using UnityEditor;
using UnityEngine;

#region STRUCTS
[System.Serializable]
public struct NodeSite
{
    public string id;
    public Vector2Int position;
    public GameObject prefab;
}
#endregion

public class MiningGenerator : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [Header("Map Settings")]
    [SerializeField] private Vector2Int mapSize = Vector2Int.zero;
    [SerializeField] private Vector2Int minerPos = Vector2Int.zero;
    [SerializeField] private List<NodeSite> nodeSites = null;
    [SerializeField] private List<Vector2Int> blockeds = null;
    [SerializeField] private List<Node.NodeWeight> weights = null;

    [Header("Miners Settings")]
    [SerializeField] private Pathfinding.MODE pathfindingMode = default;
    [SerializeField] private GameObject minerPrefab = null;
    [SerializeField] private Transform minersHolder = null;
    [SerializeField] private int minersLength = 0;
    #endregion

    #region PRIVATE_FIELDS
    private Node[] map = null;

    private ConcurrentBag<MinerAgent> miners = null;
    private ParallelOptions parrallel = null;
    #endregion

    #region UNITY_CALLS
    private void Awake()
    {
        NodeUtils.MapSize = mapSize;
        map = new Node[mapSize.x * mapSize.y];

        int ID = 0;
        for (int i = 0; i < mapSize.y; i++)
        {
            for (int j = 0; j < mapSize.x; j++)
            {
                map[ID] = new Node(ID, new Vector2Int(j, i));
                ID++;
            }
        }

        for (int i = 0; i < blockeds.Count; i++)
        {
            map[NodeUtils.PositionToIndex(blockeds[i])].Block();
        }
        for (int i = 0; i < weights.Count; i++)
        {
            map[NodeUtils.PositionToIndex(weights[i].position)].SetWeight(weights[i].weight);
        }
    }

    private void Start()
    {
        for (int i = 0; i < nodeSites.Count; i++)
        {
            GameObject siteGO = Instantiate(nodeSites[i].prefab);
            siteGO.transform.position = GetNodeSitePositionById(nodeSites[i].id);
        }

        miners = new();
        parrallel = new ParallelOptions() { MaxDegreeOfParallelism = 8 };

        for (int i = 0; i < minersLength; i++)
        {
            GameObject minerGO = Instantiate(minerPrefab, minersHolder);
            minerGO.transform.position = new Vector3(minerPos.x, minerPos.y, 0f);

            MinerAgent miner = minerGO.GetComponent<MinerAgent>();
            miner.SetCallbacks(GetNodeByPosition, GetNodeBySiteId);
            miner.Init(pathfindingMode, map, Random.Range(50f, 100f), minerPos);
            miner.StartMiner();

            miners.Add(miner);
        }
    }

    private void Update()
    {
        Parallel.ForEach(miners, parrallel, minedou =>
        {
            minedou.UpdateMiner();
        });

        GoMinerToRepose();
    }

    private void OnDrawGizmos()
    {
        if (map == null)
            return;

        GUIStyle style = new GUIStyle() { fontSize = 10 };

        foreach (Node node in map)
        {
            Gizmos.color = node.color;

            Vector3 worldPosition = new Vector3(node.position.x, node.position.y, 0.0f);
            Gizmos.DrawWireSphere(worldPosition, 0.2f);
            Handles.Label(worldPosition, node.position.ToString(), style);
        }
    }
    #endregion

    #region PRIVATE_METHODS
    private Vector3 GetNodeSitePositionById(string id)
    {
        for (int i = 0; i < nodeSites.Count; i++)
        {
            if (nodeSites[i].id == id)
            {
                return new Vector3(nodeSites[i].position.x, nodeSites[i].position.y, 0f);
            }
        }

        return Vector3.zero;
    }

    private Node GetNodeByPosition(Vector2Int position)
    {
        return map[NodeUtils.PositionToIndex(position)];
    }

    private Node GetNodeBySiteId(string siteId)
    {
        return map[NodeUtils.PositionToIndex(nodeSites.Find(node => node.id == siteId).position)];
    }

    private void GoMinerToRepose()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetTryMiner()?.GoToRepose();
        }
    }

    private MinerAgent GetTryMiner()
    {
        if (miners.TryPeek(out MinerAgent miner))
        {
            return miner;
        };

        return null;
    }
    #endregion
}
