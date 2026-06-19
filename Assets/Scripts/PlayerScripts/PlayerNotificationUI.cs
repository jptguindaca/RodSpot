using System.Collections;
using TMPro;
using UnityEngine;

// Aviso simples tipo toast com fade in/out.
public class PlayerNotificationUI : MonoBehaviour
{
    // Referencia singleton usada quando nao ha referencia direta.
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

    public static void ShowGlobal(string message, float duration = -1f)
    {
        if (Instance == null)
        {
            return;
        }

        Instance.Show(message, duration);
    }

    public static void ShowGlobal(string message, Color textColor, float duration = -1f)
    {
        if (Instance == null)
        {
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
        // Reinicia o aviso atual para mostrar a mensagem imediatamente.
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
        // Faz fade-in, espera e faz fade-out.
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

        // Interpola o alpha ate ao valor alvo.
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
        return panelGroup != null && messageText != null;
    }
}
