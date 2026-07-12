using System.Collections.Generic;
using UnityEngine;

// lista de peixes e selecao aleatoria para o sistema de raridade
[CreateAssetMenu(menuName = "Fishing/Fish Database", fileName = "FishDatabase")]
public class FishDatabase : ScriptableObject
{
    [SerializeField] private List<FishData> fishes = new List<FishData>();

    public FishData GetRandomFish()
    {
        return GetRandomFish(0f);
    }

    public FishData GetRandomFish(float rarityBias)
    {
        if (fishes == null || fishes.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;
        for (int i = 0; i < fishes.Count; i++)
        {
            FishData fish = fishes[i];
            if (fish != null)
            {
                totalWeight += GetFishWeight(fish, rarityBias);
            }
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < fishes.Count; i++)
        {
            FishData fish = fishes[i];
            if (fish == null)
            {
                continue;
            }

            cumulative += GetFishWeight(fish, rarityBias);
            if (roll <= cumulative)
            {
                return fish;
            }
        }

        for (int i = fishes.Count - 1; i >= 0; i--)
        {
            if (fishes[i] != null)
            {
                return fishes[i];
            }
        }

        return null;
    }

    private float GetFishWeight(FishData fish, float rarityBias)
    {
        float bias = Mathf.Max(0f, rarityBias);

        return fish.rarity switch
        {
            FishRarity.Uncommon => Mathf.Max(1f, 25f + bias * 14f),
            FishRarity.Rare => Mathf.Max(1f, 10f + bias * 12f),
            FishRarity.Epic => Mathf.Max(1f, 4f + bias * 8f),
            FishRarity.Legendary => Mathf.Max(1f, 1f + bias * 4f),
            _ => Mathf.Max(1f, 60f - bias * 12f),
        };
    }
}
