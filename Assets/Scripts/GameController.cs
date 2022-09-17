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

        MActions mActions = new MActions();
        mActions.onGetNodeByPosition = mapController.GetNodeByPosition;
        mActions.onGetNodeBySiteId = mapController.GetNodeBySiteId;
        mActions.onGetMineCloser = mapController.GetMineCloser;

        minersController.Init(mActions);
    }

    private void Start()
    {
        mapController.SpawnSites();
        minersController.SpawnMiners(mapController.Map, mapController.GetNodeSitePositionById(NodeUtils.baseId));
    }
    #endregion
}