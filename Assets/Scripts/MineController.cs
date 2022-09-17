using System;
using System.Collections.Generic;

using UnityEngine;

public class MineController : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [SerializeField] private GameObject prefab = null;
    [SerializeField] private Transform holder = null;
    [SerializeField] private int length = 0;
    #endregion

    #region PRIVATE_FIELDS
    private Action onMineEmpty = null;

    private List<Mine> minesList = null;
    private List<Vector2Int> freePositions = null;
    #endregion

    #region PROPERTIES
    public List<Mine> Mines { get => minesList; }
    #endregion

    #region PUBLIC_METHODS
    public void Init(Action onMineEmpty)
    {
        this.onMineEmpty = onMineEmpty;

        minesList = new List<Mine>();
        freePositions = new List<Vector2Int>();

        SetFreePositions();
    }

    public void SpawnMines()
    {
        System.Random random = new System.Random();

        for (int i = 0; i < length; i++)
        {
            GameObject mineGO = Instantiate(prefab, holder);

            Vector2Int position = GetRandomPosition(random);
            mineGO.transform.position = new Vector3(position.x, position.y, 0f);

            Mine mine = mineGO.GetComponent<Mine>();
            mine.Init(i, position, OnMineEmpty);

            minesList.Add(mine);
        }
    }
    #endregion

    #region PRIVATE_METHODS
    private void SetFreePositions()
    {
        for (int i = 0; i < NodeUtils.MapSize.x; i++)
        {
            for (int j = 0; j < NodeUtils.MapSize.y; j++)
            {
                freePositions.Add(new Vector2Int(i, j));
            }
        }

        freePositions.RemoveAll((pos) => NodeUtils.usedPositions.Contains(pos));
    }

    private Vector2Int GetRandomPosition(System.Random random)
    {
        int index = random.Next(freePositions.Count);

        Vector2Int position = freePositions[index];

        freePositions.Remove(position);
        NodeUtils.usedPositions.Add(position);

        return position;
    }

    private void OnMineEmpty(int id)
    {
        minesList.RemoveAll((mine) => mine.Id == id);

        onMineEmpty?.Invoke();
    }
    #endregion
}
