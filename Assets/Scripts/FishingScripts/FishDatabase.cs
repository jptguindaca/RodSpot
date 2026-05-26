using System.Collections.Generic;
using UnityEngine;

// Lista de peixes e selecao aleatoria para o sistema de raridade.
[CreateAssetMenu(menuName = "Fishing/Fish Database", fileName = "FishDatabase")]
public class FishDatabase : ScriptableObject
{
    [SerializeField] private List<FishData> fishes = new List<FishData>();

    public FishData GetRandomFish()
    {
        if (fishes == null || fishes.Count == 0)
        {
            return null;
        }

        int attempts = 0;
        while (attempts < fishes.Count)
        {
            int index = Random.Range(0, fishes.Count);
            FishData fish = fishes[index];
            if (fish != null)
            {
                return fish;
            }

            attempts++;
        }

        return null;
    }
}
