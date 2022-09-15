using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapController : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [SerializeField] private Vector2Int mapSize = Vector2Int.zero;
    [SerializeField] private Transform sitesHolder = null;
    [SerializeField] private List<NodeSite> nodeSites = null;
    [SerializeField] private List<Vector2Int> blockeds = null;
    [SerializeField] private List<Node.NodeWeight> weights = null;
    #endregion

    #region PRIVATE_FIELDS
    private Node[] map = null;
    #endregion

    #region PROPERTIES
    public Node[] Map { get => map; }
    #endregion

    #region STRUCTS
    [System.Serializable]
    public struct NodeSite
    {
        public string id;
        public Vector2Int position;
        public GameObject prefab;
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
            Gizmos.color = node.color;

            Vector3 worldPosition = new Vector3(node.position.x, node.position.y, 0.0f);
            Gizmos.DrawWireCube(worldPosition, Vector3.one);
            Handles.Label(worldPosition, node.position.ToString(), style);
        }
    }
    #endregion

    #region PUBLIC_METHODS
    public void Init()
    {
        NodeUtils.MapSize = mapSize;
        map = new Node[mapSize.x * mapSize.y];

        SetMapData();
    }

    public void SpawnNodeSites()
    {
        for (int i = 0; i < nodeSites.Count; i++)
        {
            GameObject siteGO = Instantiate(nodeSites[i].prefab, sitesHolder);
            Vector2Int intPosition = GetNodeSitePositionById(nodeSites[i].id);

            siteGO.transform.position = new Vector3(intPosition.x, intPosition.y, 0f);
        }
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

        for (int i = 0; i < blockeds.Count; i++)
        {
            map[NodeUtils.PositionToIndex(blockeds[i])].Block();
        }
        for (int i = 0; i < weights.Count; i++)
        {
            map[NodeUtils.PositionToIndex(weights[i].position)].SetWeight(weights[i].weight);
        }
    }
    #endregion
}
