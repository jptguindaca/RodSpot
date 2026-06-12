using UnityEngine;

// Controla XP e nivel da pesca
public class FishingProgression : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FishingController fishingController;
    [SerializeField] private FishingProgressionSettings progressionSettings;
    [SerializeField] private FishingXPUI xpUI;

    [Header("State")]
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private int currentXp = 0;

    private int xpToNextLevel = 100;

    public int CurrentLevel => currentLevel;
    public int CurrentXp => currentXp;
    public int XpToNextLevel => xpToNextLevel;

    private void Awake()
    {
        CacheFishingController();
        RecalculateXpToNextLevel();
        UpdateUi();
    }

    private void OnEnable()
    {
        CacheFishingController();

        if (fishingController != null)
        {
            fishingController.FishCaught += HandleFishCaught;
        }
    }

    private void Start()
    {
        RecalculateXpToNextLevel();
        UpdateUi();
    }

    private void OnDisable()
    {
        if (fishingController != null)
        {
            fishingController.FishCaught -= HandleFishCaught;
        }
    }

    private void HandleFishCaught(FishData fish, int value)
    {
        int xpAmount = fish != null ? Mathf.Max(0, fish.experienceReward) : 0;
        if (xpAmount <= 0)
        {
            UpdateUi();
            return;
        }

        AddExperience(xpAmount);
    }

    public void AddExperience(int xpAmount)
    {
        if (xpAmount <= 0)
        {
            return;
        }

        currentXp += xpAmount;

        while (currentXp >= xpToNextLevel)
        {
            currentXp -= xpToNextLevel;
            currentLevel++;
            RecalculateXpToNextLevel();
        }

        UpdateUi();
    }

    private void RecalculateXpToNextLevel()
    {
        int baseXp = progressionSettings != null ? Mathf.Max(1, progressionSettings.baseXpToNextLevel) : 100;
        float growth = progressionSettings != null ? Mathf.Max(1.01f, progressionSettings.xpGrowthMultiplier) : 1.25f;
        int levelForFormula = Mathf.Max(0, currentLevel);
        xpToNextLevel = Mathf.Max(1, Mathf.RoundToInt(baseXp * Mathf.Pow(growth, levelForFormula)));
    }

    private void UpdateUi()
    {
        if (xpUI != null)
        {
            xpUI.SetProgress(currentLevel, currentXp, xpToNextLevel);
        }
    }

    private void CacheFishingController()
    {
        if (fishingController == null)
        {
            fishingController = FindFirstObjectByType<FishingController>();
        }
    }
}
