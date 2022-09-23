using System;
using System.Collections.Generic;

using UnityEngine;

public class VoronoiController : MonoBehaviour
{
    #region PRIVATE_FIELDS
    private List<Limit> limits = null;
    private List<Sector> sectors = null;

    private Func<string, Vector2Int> onGetPositionBySiteId = null;
    #endregion

    #region UNITY_CALLS
    private void OnDrawGizmos()
    {
        DrawSectors();
    }
    #endregion

    #region PUBLIC_METHODS
    public void Init(Func<string, Vector2Int> onGetPositionBySiteId)
    {
        this.onGetPositionBySiteId = onGetPositionBySiteId;

        sectors = new List<Sector>();

        InitLimits();
    }

    public void SetVoronoi(List<Mine> mines)
    {
        if (mines.Count == 0) return;

        sectors.Clear();
        for (int i = 0; i < mines.Count; i++)
        {
            sectors.Add(new Sector(mines[i].transform.position));
        }

        for (int i = 0; i < sectors.Count; i++)
        {
            sectors[i].AddSegmentLimits(limits);
        }

        for (int i = 0; i < mines.Count; i++)
        {
            for (int j = 0; j < mines.Count; j++)
            {
                if (i == j) continue;

                sectors[i].AddSegment(mines[i].transform.position, mines[j].transform.position);
            }
        }

        for (int i = 0; i < sectors.Count; i++)
        {
            sectors[i].SetIntersections();
        }
    }

    public Mine GetMineCloser(List<Mine> mines)
    {
        for (int i = 0; i < mines.Count; i++)
        {
            if (!mines[i].IsEmpty)
            {
                return mines[i];
            }
        }

        return null;
    }
    #endregion

    #region PRIVATE_METHODS
    private void InitLimits()
    {
        limits = new List<Limit>();

        Vector2 offset = new Vector2(NodeUtils.offset.x, NodeUtils.offset.y) / 2f;

        limits.Add(new Limit(offset, DIRECTION.LEFT));
        limits.Add(new Limit(new Vector2(0f, NodeUtils.mapSize.y) + offset, DIRECTION.UP));
        limits.Add(new Limit(new Vector2(NodeUtils.mapSize.x, NodeUtils.mapSize.y) + offset, DIRECTION.RIGHT));
        limits.Add(new Limit(new Vector2(NodeUtils.mapSize.x, 0f) + offset, DIRECTION.DOWN));
    }

    private void DrawSectors()
    {
        if (sectors == null) return;

        for (int i = 0; i < sectors.Count; i++)
        {
            sectors[i].DrawSector();
        }
    }
    #endregion
}
