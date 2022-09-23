using System;
using System.Collections.Generic;

using UnityEngine;

public class MActions
{
    public Action<int> onDeposit = null;

    public Func<Vector2Int, Node> onGetNodeByPosition = null;
    public Func<string, Node> onGetNodeBySiteId = null;
    public Func<Vector3, Mine> onGetMineCloser = null;
}

public class MinerAgent : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [Header("Money Settings")]
    [SerializeField] private int maxMoney = 0;
    [SerializeField] private int takeMoney = 0;
    [SerializeField] private float recolectDelay = 0f;

    [Header("Move Settings")]
    [SerializeField] private float positionDelay = 0f;
    [SerializeField] private float speedDelay = 0f;
    #endregion

    #region PRIVATE_FIELDS
    private int id = 0;

    private FSM fsm;

    private Pathfinding pathfinding = null;
    private List<Vector2Int> path = null;
    private int pathIndex = 0;

    private MActions mActions = null;

    private Vector2Int nodePos = Vector2Int.zero;
    private Vector3 targetPos = Vector3.zero;

    private float miningTimer = 0f;
    private float currentPathTimer = 0f;
    private float currentPositionTimer = 0f;
    private bool reposing = false;
    private bool waitPosition = false;

    private int currentMoney = 0;
    private Mine targetMine = null;

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
        OnFindOtherMine,
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
    public void Init(MActions mActions, int id, Pathfinding.MODE mode, Node[] map, Vector2Int minerPos)
    {
        this.mActions = mActions;
        this.id = id;

        pathfinding = new Pathfinding(mode, map);

        speedDelay = speedDelay * UnityEngine.Random.Range(50f, 100f) / 100f;
        nodePos = minerPos;

        SetPosition(new Vector3(minerPos.x, minerPos.y, 0f) + NodeUtils.offset);
    }

    public void StartMiner()
    {
        fsm = new FSM((int)States._Count, (int)Flags._Count);

        StartPathfiding(NodeUtils.mineId, () =>
        {
            fsm.ForceCurretState((int)States.GoToMine);
        });

        SetRelations();
        SetBehaviors();
    }

    public void UpdateMiner()
    {
        fsm.Update();
    }

    public void SwitchRepose()
    {
        reposing = !reposing;

        if (reposing)
        {
            StartPathfiding(NodeUtils.baseId, () =>
            {
                fsm.SetFlag((int)Flags.OnStopMine);
            });
        }
        else
        {
            StartPathfiding(NodeUtils.mineId, () =>
            {
                fsm.SetFlag((int)Flags.OnEndRepose);
            });
        }
    }

    public void UpdateMap(Node[] map)
    {
        pathfinding.UpdateMap(map);
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
        fsm.SetRelation((int)States.Mining, (int)Flags.OnFindOtherMine, (int)States.GoToMine);
        fsm.SetRelation((int)States.GoToDeposit, (int)Flags.OnReachDeposit, (int)States.GoToMine);
        fsm.SetRelation((int)States.GoToDeposit, (int)Flags.OnEmptyMine, (int)States.Idle);

        fsm.SetRelation((int)States.GoToMine, (int)Flags.OnStopMine, (int)States.GoToRepose);
        fsm.SetRelation((int)States.Mining, (int)Flags.OnStopMine, (int)States.GoToRepose);
        fsm.SetRelation((int)States.GoToDeposit, (int)Flags.OnStopMine, (int)States.GoToRepose);
        fsm.SetRelation((int)States.GoToRepose, (int)Flags.OnReachRepose, (int)States.Reposing);
        fsm.SetRelation((int)States.GoToRepose, (int)Flags.OnEndRepose, (int)States.GoToMine);
        fsm.SetRelation((int)States.Reposing, (int)Flags.OnEndRepose, (int)States.GoToMine);
    }

    private void SetBehaviors()
    {
        fsm.AddBehaviour((int)States.Idle, () =>
        {
            Debug.Log("Miner " + id + ": Idle");
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
            if (currentMoney < maxMoney && targetMine != null)
            {
                if (!targetMine.IsEmpty)
                {
                    if (miningTimer < recolectDelay)
                    {
                        miningTimer += deltaTime;
                    }
                    else
                    {
                        miningTimer = 0.0f;
                        currentMoney += targetMine.Take(takeMoney);
                    }
                }
                else
                {
                    StartPathfiding(NodeUtils.mineId, () =>
                    {
                        Debug.Log("Miner " + id + ": " + currentMoney + " recollected.");
                        fsm.SetFlag((int)Flags.OnFindOtherMine);
                    });
                }
            }
            else
            {
                StartPathfiding(NodeUtils.baseId, () =>
                {
                    Debug.Log("Miner " + id + ": " + currentMoney + " recollected.");
                    fsm.SetFlag((int)Flags.OnFullInventory);
                });
            }
        }, () =>
        {
            fsm.SetFlag((int)Flags.OnStopMine);
        });

        fsm.AddBehaviour((int)States.GoToDeposit, () =>
        {
            UpdatePath(() =>
            {
                Debug.Log("Miner " + id + ": " + currentMoney + " deposited.");
                mActions.onDeposit?.Invoke(currentMoney);
                currentMoney = 0;

                StartPathfiding(NodeUtils.mineId, () =>
                {
                    fsm.SetFlag((int)Flags.OnReachDeposit);
                });
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
        },
        () =>
        {
            fsm.SetFlag((int)Flags.OnEndRepose);
        });

        fsm.AddBehaviour((int)States.Reposing, () =>
        {
            Debug.Log("Miner " + id + ": Reposing");
        },
        () =>
        {
            fsm.SetFlag((int)Flags.OnEndRepose);
        });
    }

    private void StartPathfiding(string siteId, Action onSuccess)
    {
        path = pathfinding.GetPath(mActions.onGetNodeByPosition?.Invoke(nodePos), GetNodePath(siteId));

        if (path != null && path.Count > 0)
        {
            nodePos = path[0];
            targetPos = new Vector3(path[0].x, path[0].y, 0f) + NodeUtils.offset;

            pathIndex = 0;
            currentPathTimer = 0f;
            currentPositionTimer = 0f;

            waitPosition = false;

            onSuccess?.Invoke();
        }
        else
        {
            fsm.SetFlag((int)Flags.OnEmptyMine);
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
                    targetPos = new Vector3(path[pathIndex].x, path[pathIndex].y, 0f) + NodeUtils.offset;
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

    private Node GetNodePath(string id)
    {
        Node node = null;

        if (id == NodeUtils.baseId)
        {
            node = mActions.onGetNodeBySiteId?.Invoke(id);
        }
        else if (id == NodeUtils.mineId)
        {
            targetMine = mActions.onGetMineCloser?.Invoke(pos);

            if (targetMine != null)
            {
                node = mActions.onGetNodeByPosition?.Invoke(targetMine.Position);
            }
        }

        return node;
    }
    #endregion
}