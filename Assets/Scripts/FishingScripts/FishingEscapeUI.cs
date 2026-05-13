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
        SetProgress(1f);
    }

    public void Hide()
    {
        // Esconde a barra e limpa o texto.
        SetVisible(false);
        SetPercent(0f);
    }

    public void SetProgress(float normalized)
    {
        // Atualiza o valor e a cor da barra.
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

    private void SetPercent(float amount)
    {
        // Atualiza a percentagem em texto.
        if (percentText == null)
        {
            return;
        }

        int percent = Mathf.RoundToInt(Mathf.Clamp01(amount) * 100f);
        percentText.text = percent.ToString() + "%";
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
        }
    }

    private bool ValidateReferences()
    {
        return canvasGroup != null && fillImage != null;
    }
}
