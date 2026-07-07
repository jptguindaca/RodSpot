using System;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    [SerializeField] private int startingMoney = 0;

    public int CurrentMoney { get; private set; }

    public event Action<int> MoneyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentMoney = Mathf.Max(0, startingMoney);
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        CurrentMoney += amount;
        MoneyChanged?.Invoke(CurrentMoney);
    }

    public bool CanAfford(int amount)
    {
        return amount >= 0 && CurrentMoney >= amount;
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= 0) return true;
        if (!CanAfford(amount)) return false;

        CurrentMoney -= amount;
        MoneyChanged?.Invoke(CurrentMoney);
        return true;
    }
}