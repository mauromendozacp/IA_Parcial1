using System;
using UnityEngine;

public class Mine : MonoBehaviour
{
    #region PRIVATE_FIELDS
    private Action<int> onEmpty = null;

    private int id = 0;
    private int amount = 0;
    private Vector2Int position = Vector2Int.zero;
    private bool isEmpty = false;
    #endregion

    #region PROPERTIES
    public int Id { get => id; }
    public Vector2Int Position { get => position; }
    public bool IsEmpty { get => isEmpty; }
    #endregion

    #region PUBLIC_METHODS
    public void Init(int id, int amount, Vector2Int position, Action<int> onEmpty)
    {
        this.id = id;
        this.amount = amount;
        this.position = position;
        this.onEmpty = onEmpty;
    }

    public int Use(int substract)
    {
        if (!isEmpty)
        {
            int money = amount - substract;

            if (money <= 0)
            {
                substract += money;
                SetEmpty();
            }

            amount -= substract;

            return substract;
        }

        return 0;
    }
    #endregion

    #region PRIVATE_METHODS
    private void SetEmpty()
    {
        isEmpty = true;
        onEmpty?.Invoke(id);
    }
    #endregion
}
