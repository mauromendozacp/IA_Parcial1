using System;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiController : MonoBehaviour
{
    #region PRIVATE_FIELDS
    private Func<string, Vector2Int> onGetPositionBySiteId = null;
    #endregion

    #region PUBLIC_METHODS
    public void Init(Func<string, Vector2Int> onGetPositionBySiteId)
    {
        this.onGetPositionBySiteId = onGetPositionBySiteId;
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
}
