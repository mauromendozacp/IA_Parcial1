using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using UnityEngine;

public class MinersController : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [SerializeField] private Pathfinding.MODE pathfindingMode = default;
    [SerializeField] private GameObject minerPrefab = null;
    [SerializeField] private Transform minersHolder = null;
    [SerializeField] private int minersLength = 0;
    #endregion

    #region PRIVATE_FIELDS
    private ConcurrentBag<MinerAgent> miners = null;
    private ParallelOptions parrallel = null;

    private Vector2Int basePosition = Vector2Int.zero;

    private Func<Vector2Int, Node> onGetNodeByPosition = null;
    private Func<string, Node> onGetNodeBySiteId = null;
    #endregion

    #region CONSTANTS
    private const int multiThreadngCount = 8;
    #endregion

    #region UNITY_CALLS
    private void Update()
    {
        Parallel.ForEach(miners, parrallel, minedou =>
        {
            minedou.UpdateMiner();
        });

        GoMinerToRepose();
    }
    #endregion

    #region PUBLIC_METHODS
    public void Init(Vector2Int basePosition, Func<Vector2Int, Node> onGetNodeByPosition, Func<string, Node> onGetNodeBySiteId)
    {
        this.basePosition = basePosition;
        this.onGetNodeByPosition = onGetNodeByPosition;
        this.onGetNodeBySiteId = onGetNodeBySiteId;

        miners = new();
        parrallel = new ParallelOptions() { MaxDegreeOfParallelism = multiThreadngCount };
    }

    public void SpawnMiners(Node[] map)
    {
        for (int i = 0; i < minersLength; i++)
        {
            GameObject minerGO = Instantiate(minerPrefab, minersHolder);
            minerGO.transform.position = new Vector3(basePosition.x, basePosition.y, 0f);

            MinerAgent miner = minerGO.GetComponent<MinerAgent>();
            miner.SetCallbacks(onGetNodeByPosition, onGetNodeBySiteId);
            miner.Init(pathfindingMode, map, UnityEngine.Random.Range(50f, 100f), basePosition);
            miner.StartMiner();

            miners.Add(miner);
        }
    }
    #endregion

    #region PRIVATE_METHODS
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
