using UnityEngine;

public class GameController : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [SerializeField] private MapController mapController = null;
    [SerializeField] private MinersController minersController = null;
    #endregion

    #region UNITY_CALLS
    private void Awake()
    {
        mapController.Init();
        minersController.Init(mapController.GetNodeSitePositionById(NodeUtils.baseId), mapController.GetNodeByPosition, mapController.GetNodeBySiteId);
    }

    private void Start()
    {
        mapController.SpawnSites();
        minersController.SpawnMiners(mapController.Map);
    }
    #endregion
}