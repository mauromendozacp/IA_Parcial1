using System;
using System.Collections.Generic;

using UnityEngine;

public class MinerAgent : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [SerializeField] private float positionDelay = 0f;
    [SerializeField] private float speedDelay = 0f;
    #endregion

    #region PRIVATE_FIELDS
    private FSM fsm;

    private Pathfinding pathfinding = null;
    private List<Vector2Int> path = null;
    private int pathIndex = 0;

    private Func<Vector2Int, Node> onGetNodeByPosition = null;
    private Func<string, Node> onGetNodeBySiteId = null;

    private Vector2Int nodePos = Vector2Int.zero;
    private Vector3 targetPos = Vector3.zero;

    private int mineUses = 10;
    private float miningTime = 5f;
    private float reposingTime = 5f;
    private float currentMiningTimer = 0f;
    private float currentReposingTimer = 0f;
    private float currentPathTimer = 0f;
    private float currentPositionTimer = 0f;
    private bool waitPosition = false;

    private Vector3 pos = Vector3.zero;
    private bool posFlag = false;

    private float deltaTime = 0f;
    #endregion

    #region ENUMS
    enum States
    {
        Mining,
        Reposing,
        GoToMine,
        GoToDeposit,
        GoToRepose,
        Idle,

        _Count
    }

    enum Flags
    {
        OnFullInventory,
        OnEndRepose,
        OnReachMine,
        OnReachDeposit,
        OnReachRepose,
        OnEmptyMine,
        OnStopMine,

        _Count
    }
    #endregion

    #region UNITY_CALLS
    private void Update()
    {
        if (posFlag)
        {
            transform.position = pos;
            posFlag = false;
        }

        deltaTime = Time.deltaTime;
    }
    #endregion

    #region PUBLIC_METHODS
    public void SetCallbacks(Func<Vector2Int, Node> onGetNodeByPosition, Func<string, Node> onGetNodeBySiteId)
    {
        this.onGetNodeByPosition = onGetNodeByPosition;
        this.onGetNodeBySiteId = onGetNodeBySiteId;
    }

    public void Init(Pathfinding.MODE mode, Node[] map, Vector2Int minerPos)
    {
        pathfinding = new Pathfinding(mode, map);
        speedDelay = speedDelay * UnityEngine.Random.Range(50f, 100f) / 100f;
        nodePos = minerPos;

        SetPosition(new Vector3(minerPos.x, minerPos.y, 0f));
    }

    public void StartMiner()
    {
        fsm = new FSM((int)States._Count, (int)Flags._Count);

        StartPathfiding(NodeUtils.mineId);
        fsm.ForceCurretState((int)States.GoToMine);

        SetRelations();
        SetBehaviors();
    }

    public void UpdateMiner()
    {
        fsm.Update();
    }

    public void GoToRepose()
    {
        StartPathfiding(NodeUtils.baseId);
        fsm.SetFlag((int)Flags.OnStopMine);
    }
    #endregion

    #region PRIVATE_METHODS
    private void SetPosition(Vector3 pos)
    {
        this.pos = pos;
        posFlag = true;
    }

    private void SetRelations()
    {
        fsm.SetRelation((int)States.GoToMine, (int)Flags.OnReachMine, (int)States.Mining);
        fsm.SetRelation((int)States.Mining, (int)Flags.OnFullInventory, (int)States.GoToDeposit);
        fsm.SetRelation((int)States.GoToDeposit, (int)Flags.OnReachDeposit, (int)States.GoToMine);
        fsm.SetRelation((int)States.GoToDeposit, (int)Flags.OnEmptyMine, (int)States.Idle);

        fsm.SetRelation((int)States.GoToMine, (int)Flags.OnStopMine, (int)States.GoToRepose);
        fsm.SetRelation((int)States.Mining, (int)Flags.OnStopMine, (int)States.GoToRepose);
        fsm.SetRelation((int)States.GoToDeposit, (int)Flags.OnStopMine, (int)States.GoToRepose);
        fsm.SetRelation((int)States.GoToRepose, (int)Flags.OnReachRepose, (int)States.Reposing);
        fsm.SetRelation((int)States.Reposing, (int)Flags.OnEndRepose, (int)States.Mining);
    }

    private void SetBehaviors()
    {
        fsm.AddBehaviour((int)States.Idle, () =>
        {
            Debug.Log("Idle");
        }, () =>
        {
            fsm.SetFlag((int)Flags.OnStopMine);
        });

        fsm.AddBehaviour((int)States.GoToMine, () =>
        {
            UpdatePath(() =>
            {
                fsm.SetFlag((int)Flags.OnReachMine);
            });
        }, () =>
        {
            fsm.SetFlag((int)Flags.OnStopMine);
        });

        fsm.AddBehaviour((int)States.Mining, () =>
        {
            if (currentMiningTimer < miningTime)
            {
                currentMiningTimer += deltaTime;
            }
            else
            {
                currentMiningTimer = 0.0f;
                mineUses--;

                StartPathfiding(NodeUtils.baseId);
                fsm.SetFlag((int)Flags.OnFullInventory);
            }
        }, () =>
        {
            fsm.SetFlag((int)Flags.OnStopMine);
        });

        fsm.AddBehaviour((int)States.GoToDeposit, () =>
        {
            UpdatePath(() =>
            {
                if (mineUses <= 0)
                {
                    fsm.SetFlag((int)Flags.OnEmptyMine);
                }
                else
                {
                    StartPathfiding(NodeUtils.mineId);
                    fsm.SetFlag((int)Flags.OnReachDeposit);
                }
            });
        },
        () =>
        {
            fsm.SetFlag((int)Flags.OnStopMine);
        });

        fsm.AddBehaviour((int)States.GoToRepose, () =>
        {
            UpdatePath(() =>
            {
                fsm.SetFlag((int)Flags.OnReachRepose);
            });
        });

        fsm.AddBehaviour((int)States.Reposing, () =>
        {
            if (currentReposingTimer < reposingTime)
            {
                currentReposingTimer += deltaTime;
            }
            else
            {
                currentReposingTimer = 0.0f;

                StartPathfiding(NodeUtils.mineId);
                fsm.SetFlag((int)Flags.OnEndRepose);
            }
        });
    }

    private void StartPathfiding(string siteId)
    {
        path = pathfinding.GetPath(onGetNodeByPosition?.Invoke(nodePos), onGetNodeBySiteId?.Invoke(siteId));

        if (path != null)
        {
            nodePos = path[0];
            targetPos = new Vector3(path[0].x, path[0].y, 0f);

            pathIndex = 0;
            currentPathTimer = speedDelay;
            currentPositionTimer = 0f;

            SetPosition(new Vector3(path[0].x, path[0].y, 0f));
        }
    }

    private void UpdatePath(Action onFinish)
    {
        if (path == null) return;

        if (!waitPosition)
        {
            currentPathTimer += deltaTime;
            if (currentPathTimer < speedDelay)
            {
                SetPosition(Vector3.Lerp(pos, targetPos, currentPathTimer / speedDelay));
            }
            else
            {
                currentPathTimer = 0f;

                nodePos = path[pathIndex];
                SetPosition(targetPos);

                pathIndex++;
                if (pathIndex < path.Count)
                {
                    targetPos = new Vector3(path[pathIndex].x, path[pathIndex].y, 0f);
                    waitPosition = true;
                }
                else
                {
                    onFinish?.Invoke();
                }
            }
        }
        else
        {
            currentPositionTimer += deltaTime;
            if (currentPositionTimer > positionDelay)
            {
                currentPositionTimer = 0f;
                waitPosition = false;
            }
        }
    }
    #endregion
}