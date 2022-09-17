using System;
using TMPro;
using UnityEngine;

public class Mine : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [SerializeField] private int amount = 0;
    [SerializeField] private TMP_Text amountText = null;
    #endregion

    #region PRIVATE_FIELDS
    private Action<int> onEmpty = null;

    private int id = 0;
    private Vector2Int position = Vector2Int.zero;
    private bool isEmpty = false;

    private bool flag = false;
    #endregion

    #region PROPERTIES
    public int Id { get => id; }
    public Vector2Int Position { get => position; }
    public bool IsEmpty { get => isEmpty; }
    #endregion

    #region UNITY_CALLS
    private void Update()
    {
        if (!flag) return;

        amountText.text = amount.ToString();
        flag = false;
    }
    #endregion

    #region PUBLIC_METHODS
    public void Init(int id, Vector2Int position, Action<int> onEmpty)
    {
        this.id = id;
        this.position = position;
        this.onEmpty = onEmpty;

        amountText.text = amount.ToString();
    }

    public int Take(int substract)
    {
        if (!isEmpty)
        {
            int money = amount - substract;

            if (money <= 0)
            {
                substract += money;
                SetEmpty();
            }

            SetAmount(amount - substract);

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

    private void SetAmount(int amount)
    {
        if (amount < 0)
        {
            amount = 0;
        }
        this.amount = amount;

        flag = true;
    }
    #endregion
}
