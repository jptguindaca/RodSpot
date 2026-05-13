using UnityEngine;

// Configuracao de UI e cores da pesca.
[CreateAssetMenu(menuName = "Fishing/UI Settings", fileName = "FishingUISettings")]
public class FishingUISettings : ScriptableObject
{
    [Header("Notifications")]
    public float notificationDuration = 2.5f;

    [Header("Colors")]
    public Color infoTextColor = new Color(0.75f, 0.9f, 1f);
    public Color warningTextColor = new Color(1f, 0.85f, 0.35f);
    public Color successTextColor = new Color(0.45f, 0.95f, 0.45f);
    public Color errorTextColor = new Color(1f, 0.45f, 0.45f);
}
