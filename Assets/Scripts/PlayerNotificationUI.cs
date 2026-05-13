using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerNotificationUI : MonoBehaviour
{
    public static PlayerNotificationUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private TMP_Text messageText;

    [Header("Style")]
    [SerializeField] private Color defaultTextColor = Color.white;

    [Header("Timing")]
    [SerializeField] private float defaultDuration = 2.5f;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private bool hideOnStart = true;

    private Coroutine messageRoutine;
    private bool hasMissingReferences;

    public static void ShowGlobal(string message, float duration = -1f)
    {
        if (Instance == null)
        {
            Debug.LogWarning("PlayerNotificationUI nao esta na cena. Adiciona o componente e define as referencias.");
            return;
        }

        Instance.Show(message, duration);
    }

    public static void ShowGlobal(string message, Color textColor, float duration = -1f)
    {
        if (Instance == null)
        {
            Debug.LogWarning("PlayerNotificationUI nao esta na cena. Adiciona o componente e define as referencias.");
            return;
        }

        Instance.Show(message, textColor, duration);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (hideOnStart)
        {
            HideImmediate();
        }
    }

    public void Show(string message, float duration = -1f)
    {
        Show(message, defaultTextColor, duration);
    }

    public void Show(string message, Color textColor, float duration = -1f)
    {
        if (!ValidateReferences())
        {
            return;
        }

        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
        }

        float finalDuration = duration > 0f ? duration : defaultDuration;
        messageRoutine = StartCoroutine(ShowRoutine(message, textColor, finalDuration));
    }

    private IEnumerator ShowRoutine(string message, Color textColor, float duration)
    {
        messageText.text = message;
        messageText.color = textColor;
        yield return FadeTo(1f, fadeDuration);

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        yield return FadeTo(0f, fadeDuration);
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        if (panelGroup == null)
            yield break;

        float start = panelGroup.alpha;
        float time = 0f;
        float safeDuration = Mathf.Max(0.01f, duration);

        while (time < safeDuration)
        {
            time += Time.deltaTime;
            panelGroup.alpha = Mathf.Lerp(start, target, time / safeDuration);
            yield return null;
        }

        panelGroup.alpha = target;
    }

    private void HideImmediate()
    {
        if (panelGroup != null)
        {
            panelGroup.alpha = 0f;
        }

        if (messageText != null)
        {
            messageText.text = string.Empty;
        }
    }

    private bool ValidateReferences()
    {
        if (panelGroup != null && messageText != null)
        {
            return true;
        }

        if (!hasMissingReferences)
        {
            hasMissingReferences = true;
            Debug.LogWarning("PlayerNotificationUI precisa das referencias: PanelGroup e TMP Text.");
        }

        return false;
    }
}
