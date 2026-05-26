using UnityEngine;

// Valores de configuracao do sistema de pesca.
[CreateAssetMenu(menuName = "Fishing/Settings", fileName = "FishingSettings")]
public class FishingSettings : ScriptableObject
{
    [Header("Bobber Settings")]
    public LayerMask waterMask;
    public float catchReturnDuration = 0.5f;
    public float lineFadeDuration = 0.35f;

    [Header("Cast Settings")]
    public float minCastForce = 0f;
    public float maxCastForce = 3f;
    public float maxChargeTime = 1.2f;
    public float upwardForce = 3f;

    [Header("Fishing Settings")]
    public float minBiteTime = 2f;
    public float maxBiteTime = 6f;
    public float hookWindow = 1.5f;

    [Header("Fish Data")]
    public FishDatabase fishDatabase;

    [Header("Reeling Settings")]
    public float fishStamina = 100f;
    public float reelDamagePerSecond = 25f;
    public float escapeRecoveryPerSecond = 10f;

    [Header("Reel Minigame")]
    public float minClicksPerSecond = 2f;
    public float maxClicksPerSecond = 6f;
    public float clickWindow = 0.6f;
    public float minFishDifficulty = 0.2f;
    public float maxFishDifficulty = 0.9f;
    public float escapeTime = 2.5f;
}
