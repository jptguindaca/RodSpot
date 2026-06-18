using UnityEngine;

// Configuracao da progressao de XP da pesca
[CreateAssetMenu(menuName = "Fishing/Progression Settings", fileName = "FishingProgressionSettings")]
public class FishingProgressionSettings : ScriptableObject
{
    [Min(1)]
    public int baseXpToNextLevel = 100;

    [Min(1.01f)]
    public float xpGrowthMultiplier = 1.25f;
}
