using UnityEngine;

// dados de um peixe individual para o sistema de raridade
[CreateAssetMenu(menuName = "Fishing/Fish", fileName = "FishData")]
public class FishData : ScriptableObject
{
    public string fishName;
    public FishRarity rarity = FishRarity.Common;

    [Min(0.1f)]
    public float difficultyMultiplier = 1f;

    [Min(0.1f)]
    public float staminaMultiplier = 1f;

    [Min(0)]
    public int experienceReward = 10;

    public Sprite icon;
    public GameObject fishPrefab;

    [Header("Value")]
    [Min(0)]
    public int valueMin = 10;
    [Min(0)]
    public int valueMax = 50;

    /// <summary>
    /// Returns a random value between valueMin and valueMax.
    /// </summary>
    public int GetRandomValue()
    {
        return Random.Range(valueMin, valueMax + 1);
    }
}
