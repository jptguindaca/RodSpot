using UnityEngine;

// Centraliza as notificacoes da pesca.
public class FishingNotifications : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerNotificationUI notificationUI;
    [SerializeField] private FishingUISettings uiSettings;

    [Header("Messages")]
    [SerializeField] private string castPreparingMessage = "A preparar lancamento...";
    [SerializeField] private string lineCastMessage = "Linha lancada.";
    [SerializeField] private string biteMessage = "Um peixe mordeu! CLICA NO BOTAO DIREITO!";
    [SerializeField] private string fishHookedMessage = "Peixe fisgado! Clica rapido para puxar.";
    [SerializeField] private string fishEscapedMessage = "O peixe fugiu.";
    [SerializeField] private string fishCaughtMessage = "Peixe apanhado!";
    [SerializeField] private string invalidWaterMessage = "Nao da para pescar aqui.";

    public void ShowCastPreparing()
    {
        Notify(castPreparingMessage, uiSettings.infoTextColor);
    }

    public void ShowLineCast()
    {
        Notify(lineCastMessage, uiSettings.infoTextColor);
    }

    public void ShowBite(float hookWindow)
    {
        Notify(biteMessage, uiSettings.warningTextColor, hookWindow);
    }

    public void ShowFishHooked()
    {
        Notify(fishHookedMessage, uiSettings.warningTextColor);
    }

    public void ShowFishEscaped()
    {
        Notify(fishEscapedMessage, uiSettings.errorTextColor);
    }

    public void ShowFishCaught()
    {
        Notify(fishCaughtMessage, uiSettings.successTextColor);
    }

    public void ShowInvalidWater()
    {
        Notify(invalidWaterMessage, uiSettings.errorTextColor);
    }

    private void Notify(string message, Color textColor, float duration = -1f)
    {
        if (notificationUI == null || uiSettings == null || string.IsNullOrEmpty(message))
        {
            return;
        }

        float finalDuration = duration > 0f ? duration : uiSettings.notificationDuration;
        notificationUI.Show(message, textColor, finalDuration);
        Debug.Log(message);
    }
}
