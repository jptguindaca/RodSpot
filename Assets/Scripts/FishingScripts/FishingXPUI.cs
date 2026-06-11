using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// UI da progressao de XP da pesca em formato barra + badge de nivel.
public class FishingXPUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform currentLevelBadgeRect;
    [SerializeField] private Image currentLevelBadgeImage;
    [SerializeField] private TMP_Text currentLevelText;
    [SerializeField] private RectTransform nextLevelBadgeRect;
    [SerializeField] private Image nextLevelBadgeImage;
    [SerializeField] private TMP_Text nextLevelText;
    [SerializeField] private TMP_Text xpText;

    [Header("Behavior")]
    [SerializeField] private bool hideOnStart = false;
    [SerializeField] private bool hideWhenEmpty = false;
    [SerializeField] private float progressAnimationDuration = 0.25f;
    [SerializeField] private float badgePopScale = 1.08f;

    [Header("Text Style")]
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField, Range(0f, 1f)] private float outlineWidth = 0.2f;

    [Header("Format")]
    [SerializeField] private string xpFormat = "{0}/{1}";
    [SerializeField] private string levelFormat = "{0}";
    [SerializeField] private string nextLevelFormat = "{0}";

    private Coroutine progressRoutine;
    private float currentVisibleFill;
    private Vector3 currentBadgeBaseScale = Vector3.one;
    private Vector3 nextBadgeBaseScale = Vector3.one;

    private void Reset()
    {
        AutoBindFromHierarchy();
    }

    private void Awake()
    {
        AutoBindFromHierarchy();

        if (currentLevelBadgeRect != null)
        {
            currentBadgeBaseScale = currentLevelBadgeRect.localScale;
        }

        if (nextLevelBadgeRect != null)
        {
            nextBadgeBaseScale = nextLevelBadgeRect.localScale;
        }

        ApplyTextStyle(currentLevelText);
        ApplyTextStyle(nextLevelText);
        ApplyTextStyle(xpText);

        if (hideOnStart)
        {
            SetVisible(false);
        }
    }

    private void OnValidate()
    {
        AutoBindFromHierarchy();
    }

    public void SetProgress(int currentLevel, int currentXp, int xpToNextLevel)
    {
        float safeRequiredXp = Mathf.Max(1, xpToNextLevel);
        float normalized = Mathf.Clamp01((float)currentXp / safeRequiredXp);

        if (progressRoutine != null)
        {
            StopCoroutine(progressRoutine);
        }

        progressRoutine = StartCoroutine(AnimateProgressRoutine(normalized));

        if (currentLevelText != null)
        {
            currentLevelText.text = currentLevel.ToString();
        }

        if (xpText != null)
        {
            xpText.text = string.Format(xpFormat, currentXp, xpToNextLevel);
        }

        if (nextLevelText != null)
        {
            nextLevelText.text = string.Format(nextLevelFormat, currentLevel + 1);
        }

        if (hideWhenEmpty && currentXp <= 0)
        {
            SetVisible(false);
        }
        else
        {
            SetVisible(true);
        }
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
        }

        if (currentLevelBadgeImage != null)
        {
            currentLevelBadgeImage.enabled = visible;
        }

        if (nextLevelBadgeImage != null)
        {
            nextLevelBadgeImage.enabled = visible;
        }
    }

    private IEnumerator AnimateProgressRoutine(float targetFill)
    {
        float startFill = fillImage != null ? fillImage.fillAmount : currentVisibleFill;
        float duration = Mathf.Max(0.01f, progressAnimationDuration);
        float timer = 0f;

        if (currentLevelBadgeRect != null)
        {
            currentLevelBadgeRect.localScale = currentBadgeBaseScale * badgePopScale;
        }

        if (nextLevelBadgeRect != null)
        {
            nextLevelBadgeRect.localScale = nextBadgeBaseScale * badgePopScale;
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float eased = t * t * (3f - 2f * t);
            currentVisibleFill = Mathf.Lerp(startFill, targetFill, eased);

            if (fillImage != null)
            {
                fillImage.fillAmount = currentVisibleFill;
            }

            if (currentLevelBadgeRect != null)
            {
                currentLevelBadgeRect.localScale = Vector3.Lerp(currentBadgeBaseScale * badgePopScale, currentBadgeBaseScale, eased);
            }

            if (nextLevelBadgeRect != null)
            {
                nextLevelBadgeRect.localScale = Vector3.Lerp(nextBadgeBaseScale * badgePopScale, nextBadgeBaseScale, eased);
            }

            yield return null;
        }

        currentVisibleFill = targetFill;
        if (fillImage != null)
        {
            fillImage.fillAmount = currentVisibleFill;
        }

        if (currentLevelBadgeRect != null)
        {
            currentLevelBadgeRect.localScale = currentBadgeBaseScale;
        }

        if (nextLevelBadgeRect != null)
        {
            nextLevelBadgeRect.localScale = nextBadgeBaseScale;
        }

        progressRoutine = null;
    }

    private void ApplyTextStyle(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        text.color = textColor;

        Material material = text.fontMaterial;
        if (material != null)
        {
            material.SetColor("_OutlineColor", outlineColor);
            material.SetFloat("_OutlineWidth", outlineWidth);
        }
    }

    private void AutoBindFromHierarchy()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (fillImage == null)
        {
            Transform fillTransform = transform.Find("Bar_Fill");
            if (fillTransform != null)
            {
                fillImage = fillTransform.GetComponent<Image>();
            }
        }

        if (xpText == null)
        {
            Transform xpTextTransform = transform.Find("XpBarText");
            if (xpTextTransform != null)
            {
                xpText = xpTextTransform.GetComponent<TMP_Text>();
            }
        }

        Transform currentBadge = transform.Find("Level_Badge/atual");
        if (currentLevelBadgeRect == null && currentBadge != null)
        {
            currentLevelBadgeRect = (RectTransform)currentBadge;
        }

        if (currentLevelBadgeImage == null && currentBadge != null)
        {
            Transform currentBadgeImageTransform = currentBadge.Find("Level_Badge_BG");
            if (currentBadgeImageTransform != null)
            {
                currentLevelBadgeImage = currentBadgeImageTransform.GetComponent<Image>();
            }
        }

        if (currentLevelText == null && currentBadge != null)
        {
            TMP_Text badgeText = currentBadge.GetComponentInChildren<TMP_Text>(true);
            if (badgeText != null)
            {
                currentLevelText = badgeText;
            }
        }

        Transform nextBadge = transform.Find("Level_Badge/proximo");
        if (nextLevelBadgeRect == null && nextBadge != null)
        {
            nextLevelBadgeRect = (RectTransform)nextBadge;
        }

        if (nextLevelBadgeImage == null && nextBadge != null)
        {
            Transform nextBadgeImageTransform = nextBadge.Find("Level_Badge_BG");
            if (nextBadgeImageTransform != null)
            {
                nextLevelBadgeImage = nextBadgeImageTransform.GetComponent<Image>();
            }
        }

        if (nextLevelText == null && nextBadge != null)
        {
            TMP_Text badgeText = nextBadge.GetComponentInChildren<TMP_Text>(true);
            if (badgeText != null)
            {
                nextLevelText = badgeText;
            }
        }
    }
}
