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

    private MActions mActions = null;
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
    public void Init(MActions mActions)
    {
        this.mActions = mActions;

        miners = new ConcurrentBag<MinerAgent>();
        parrallel = new ParallelOptions() { MaxDegreeOfParallelism = multiThreadngCount };
    }

    public void SpawnMiners(Node[] map, Vector2Int basePosition)
    {
        for (int i = 0; i < minersLength; i++)
        {
            GameObject minerGO = Instantiate(minerPrefab, minersHolder);
            minerGO.transform.position = new Vector3(basePosition.x, basePosition.y, 0f);

            MinerAgent miner = minerGO.GetComponent<MinerAgent>();
            miner.Init(mActions, pathfindingMode, map, basePosition);
            miner.StartMiner();

            miners.Add(miner);
        }
    }

    public void UpdateMapMiners(Node[] map)
    {
        Parallel.ForEach(miners, parrallel, minedou =>
        {
            minedou.UpdateMap(map);
        });
    }
    #endregion

    #region PRIVATE_METHODS
    private void GoMinerToRepose()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Parallel.ForEach(miners, parrallel, minedou =>
            {
                minedou.SwitchRepose();
            });
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
