using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishingBiteUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text percentText;

    [Header("Behavior")]
    [SerializeField] private bool hideOnStart = true;
    [SerializeField] private bool hideOnComplete = true;

    private Coroutine countdownRoutine;
    private bool hasMissingReferences;

    private void Awake()
    {
        if (hideOnStart)
        {
            HideImmediate();
        }
    }

    public void Show(float duration)
    {
        if (!ValidateReferences())
        {
            return;
        }

        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
        }

        countdownRoutine = StartCoroutine(CountdownRoutine(duration));
    }

    public void Hide()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }

        HideImmediate();
    }

    private IEnumerator CountdownRoutine(float duration)
    {
        SetVisible(true);
        SetFill(1f);

        float safeDuration = Mathf.Max(0.01f, duration);
        float timer = 0f;

        while (timer < safeDuration)
        {
            timer += Time.deltaTime;
            SetFill(1f - timer / safeDuration);
            yield return null;
        }

        SetFill(0f);

        if (hideOnComplete)
        {
            SetVisible(false);
        }
    }

    private void HideImmediate()
    {
        SetVisible(false);
        SetFill(0f);
        SetPercent(0f);
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
        }
    }

    private void SetFill(float amount)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(amount);
        }

        SetPercent(amount);
    }

    private void SetPercent(float amount)
    {
        if (percentText == null)
        {
            return;
        }

        int percent = Mathf.RoundToInt(Mathf.Clamp01(amount) * 100f);
        percentText.text = percent.ToString() + "%";
    }

    private bool ValidateReferences()
    {
        if (canvasGroup != null && fillImage != null)
        {
            return true;
        }

        if (!hasMissingReferences)
        {
            hasMissingReferences = true;
            Debug.LogWarning("FishingBiteUI precisa das referencias: CanvasGroup e Image com Fill.");
        }

        return false;
    }
}
