using TMPro;
using UnityEngine;
using UnityEngine.UI;

// UI da stamina/escape do peixe.
public class FishingEscapeUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text percentText;
    [SerializeField] private TMP_Text timerText;

    [Header("Behavior")]
    [SerializeField] private bool hideOnStart = true;
    [SerializeField] private bool hideWhenFull = true;

    [Header("Colors")]
    [SerializeField] private bool useColorLerp = true;
    [SerializeField] private Color safeColor = new Color(0.35f, 0.9f, 0.45f);
    [SerializeField] private Color dangerColor = new Color(1f, 0.4f, 0.4f);


    private void Awake()
    {
        if (hideOnStart)
        {
            SetVisible(false);
        }
    }

    public void Show()
    {
        // Mostra a barra no inicio da recolha.
        if (!ValidateReferences())
        {
            return;
        }

        SetVisible(true);
        fillImage.fillAmount = 0f;
        SetClicksText(0, 1);
        SetTimerText(0f, 0f);
    }

    public void Hide()
    {
        // Esconde a barra e limpa o texto.
        SetVisible(false);
        fillImage.fillAmount = 0f;
        SetClicksText(0, 1);
        SetTimerText(0f, 0f);
    }

    public void SetProgress(float normalized)
    {
        // Atualiza a barra com um valor normalizado.
        if (!ValidateReferences())
        {
            return;
        }

        float clamped = Mathf.Clamp01(normalized);
        fillImage.fillAmount = clamped;
        SetPercent(clamped);

        if (useColorLerp)
        {
            fillImage.color = Color.Lerp(safeColor, dangerColor, clamped);
        }

        if (hideWhenFull && clamped <= 0f)
        {
            SetVisible(false);
        }
    }

    public void SetClicks(int currentClicks, int requiredClicks)
    {
        if (!ValidateReferences())
        {
            return;
        }

        int safeRequiredClicks = Mathf.Max(1, requiredClicks);
        int safeCurrentClicks = Mathf.Clamp(currentClicks, 0, safeRequiredClicks);
        float normalized = safeCurrentClicks / (float)safeRequiredClicks;

        fillImage.fillAmount = normalized;
        SetClicksText(safeCurrentClicks, safeRequiredClicks);

        if (useColorLerp)
        {
            fillImage.color = Color.Lerp(safeColor, dangerColor, normalized);
        }

        if (hideWhenFull && normalized >= 1f)
        {
            SetVisible(false);
        }
    }

    public void SetTimer(float remainingSeconds, float totalSeconds)
    {
        if (timerText == null)
        {
            return;
        }

        float safeTotalSeconds = Mathf.Max(0.01f, totalSeconds);
        float clampedRemaining = Mathf.Clamp(remainingSeconds, 0f, safeTotalSeconds);
        timerText.text = clampedRemaining.ToString("0.0") + "s";
    }

    private void SetPercent(float amount)
    {
        // Atualiza a percentagem em texto
        if (percentText == null)
        {
            return;
        }

        int percent = Mathf.RoundToInt(Mathf.Clamp01(amount) * 100f);
        percentText.text = percent.ToString() + "%";
    }

    private void SetClicksText(int currentClicks, int requiredClicks)
    {
        // Mostra o progresso como cliques feitos sobre o total.
        if (percentText == null)
        {
            return;
        }

        percentText.text = currentClicks.ToString() + "/" + requiredClicks.ToString();
    }

    private void SetTimerText(float remainingSeconds, float totalSeconds)
    {
        if (timerText == null)
        {
            return;
        }

        float safeTotalSeconds = Mathf.Max(0.01f, totalSeconds);
        float clampedRemaining = Mathf.Clamp(remainingSeconds, 0f, safeTotalSeconds);
        timerText.text = clampedRemaining.ToString("0.0") + "s";
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }

    private bool ValidateReferences()
    {
        return canvasGroup != null && fillImage != null;
    }
}
