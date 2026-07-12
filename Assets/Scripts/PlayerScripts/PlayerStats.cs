using System;
using UnityEngine;

public enum UpgradeType
{
    ClickPower,
    MoneyBonus,
    RarityBonus,
    EscapeTime
}

// Configuracao serializavel de movimento e progressao do jogador.
[Serializable]
public class PlayerStats
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float maxMoveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Fishing Upgrades")]
    [Min(0)] public int clickPowerLevel = 0;
    [Min(0)] public int moneyBonusLevel = 0;
    [Min(0)] public int rarityBonusLevel = 0;
    [Min(0)] public int escapeTimeLevel = 0;

    [Header("Upgrade Limits")]
    [Min(1)] public int maxUpgradeLevel = 5;
    [Min(0)] public int clickPowerBaseCost = 50;
    [Min(0)] public int moneyBonusBaseCost = 75;
    [Min(0)] public int rarityBonusBaseCost = 100;
    [Min(0)] public int escapeTimeBaseCost = 60;
    [Min(1f)] public float upgradeCostGrowth = 1.65f;

    public int GetUpgradeLevel(UpgradeType type)
    {
        return type switch
        {
            UpgradeType.ClickPower => clickPowerLevel,
            UpgradeType.MoneyBonus => moneyBonusLevel,
            UpgradeType.RarityBonus => rarityBonusLevel,
            UpgradeType.EscapeTime => escapeTimeLevel,
            _ => 0,
        };
    }

    public int GetUpgradeCost(UpgradeType type)
    {
        int level = GetUpgradeLevel(type);
        int baseCost = GetBaseCost(type);
        return Mathf.Max(0, Mathf.RoundToInt(baseCost * Mathf.Pow(upgradeCostGrowth, level)));
    }

    public bool CanUpgrade(UpgradeType type)
    {
        return GetUpgradeLevel(type) < maxUpgradeLevel;
    }

    public bool TryUpgrade(UpgradeType type)
    {
        if (!CanUpgrade(type))
        {
            return false;
        }

        switch (type)
        {
            case UpgradeType.ClickPower:
                clickPowerLevel++;
                break;
            case UpgradeType.MoneyBonus:
                moneyBonusLevel++;
                break;
            case UpgradeType.RarityBonus:
                rarityBonusLevel++;
                break;
            case UpgradeType.EscapeTime:
                escapeTimeLevel++;
                break;
        }

        return true;
    }

    public int GetClickPowerPerPress()
    {
        return Mathf.Max(1, 1 + clickPowerLevel);
    }

    public float GetMoneyMultiplier()
    {
        return 1f + moneyBonusLevel * 0.25f;
    }

    public float GetRarityBias()
    {
        return rarityBonusLevel * 0.12f;
    }

    public float GetEscapeTimeBonusSeconds()
    {
        return escapeTimeLevel * 0.35f;
    }

    private int GetBaseCost(UpgradeType type)
    {
        return type switch
        {
            UpgradeType.ClickPower => clickPowerBaseCost,
            UpgradeType.MoneyBonus => moneyBonusBaseCost,
            UpgradeType.RarityBonus => rarityBonusBaseCost,
            UpgradeType.EscapeTime => escapeTimeBaseCost,
            _ => 0,
        };
    }
}