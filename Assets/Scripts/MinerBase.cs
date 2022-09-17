using UnityEngine;

using TMPro;

public class MinerBase : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [SerializeField] private TMP_Text amountText = null;
    #endregion

    #region PRIVATE_FIELDS
    private int amount = 0;
    private bool flag = false;
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
    public void Deposit(int money)
    {
        amount += money;

        flag = true;
    }
    #endregion
}
