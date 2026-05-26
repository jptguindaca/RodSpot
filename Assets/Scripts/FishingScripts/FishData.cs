using UnityEngine;

// Dados de um peixe individual para o sistema de raridade.
[CreateAssetMenu(menuName = "Fishing/Fish", fileName = "FishData")]
public class FishData : ScriptableObject
{
    public string fishName;
    public FishRarity rarity = FishRarity.Common;

    [Min(0.1f)]
    public float difficultyMultiplier = 1f;

    [Min(0.1f)]
    public float staminaMultiplier = 1f;

    public Sprite icon;
    public GameObject fishPrefab;
}
