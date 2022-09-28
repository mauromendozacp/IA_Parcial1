using System;
using System.Collections.Generic;

using UnityEngine;

public class VoronoiController : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [SerializeField] private bool drawSegments = false;
    #endregion

    #region PRIVATE_FIELDS
    private List<Limit> limits = null;
    private List<Sector> sectors = null;
    #endregion

    #region UNITY_CALLS
    private void OnDrawGizmos()
    {
        Draw();
    }
    #endregion

    #region PUBLIC_METHODS
    public void Init()
    {
        sectors = new List<Sector>();

        InitLimits();
    }

    public void SetVoronoi(List<Mine> mines)
    {
        sectors.Clear();
        if (mines.Count == 0) return;

        for (int i = 0; i < mines.Count; i++)
        {
            sectors.Add(new Sector(mines[i]));
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

    public Mine GetMineCloser(Vector3 minerPos)
    {
        if (sectors != null)
        {
            for (int i = 0; i < sectors.Count; i++)
            {
                if (sectors[i].CheckPointInSector(minerPos))
                {
                    return sectors[i].Mine;
                }
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

    private void Draw()
    {
        if (sectors == null) return;

        for (int i = 0; i < sectors.Count; i++)
        {
            sectors[i].DrawSector();

            if (drawSegments)
            {
                sectors[i].DrawSegments();
            }
        }
    }
    #endregion
}
