using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// UI de peixe capturado (icone e raridade).
public class FishingCatchUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rarityText;

    [Header("Behavior")]
    [SerializeField] private bool hideOnStart = true;
    [SerializeField] private float showDuration = 2.5f;

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = new Color(0.85f, 0.85f, 0.85f);
    [SerializeField] private Color uncommonColor = new Color(0.4f, 0.9f, 0.45f);
    [SerializeField] private Color rareColor = new Color(0.35f, 0.65f, 1f);
    [SerializeField] private Color epicColor = new Color(1f, 0.6f, 0.9f);
    [SerializeField] private Color legendaryColor = new Color(1f, 0.8f, 0.35f);

    private Coroutine hideRoutine;

    private void Awake()
    {
        if (hideOnStart)
        {
            SetVisible(false);
        }
    }

    public void Show(FishData fish)
    {
        if (!ValidateReferences())
        {
            return;
        }

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }

        ApplyFishData(fish);
        SetVisible(true);
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        float duration = Mathf.Max(0.01f, showDuration);
        yield return new WaitForSeconds(duration);
        SetVisible(false);
        hideRoutine = null;
    }

    private void ApplyFishData(FishData fish)
    {
        if (fish == null)
        {
            nameText.text = "Peixe";
            rarityText.text = "";
            iconImage.sprite = null;
            iconImage.enabled = false;
            return;
        }

        nameText.text = string.IsNullOrWhiteSpace(fish.fishName) ? fish.name : fish.fishName;
        rarityText.text = fish.rarity.ToString();
        iconImage.sprite = fish.icon;
        iconImage.enabled = fish.icon != null;
        rarityText.color = GetRarityColor(fish.rarity);
    }

    private Color GetRarityColor(FishRarity rarity)
    {
        switch (rarity)
        {
            case FishRarity.Uncommon:
                return uncommonColor;
            case FishRarity.Rare:
                return rareColor;
            case FishRarity.Epic:
                return epicColor;
            case FishRarity.Legendary:
                return legendaryColor;
            case FishRarity.Common:
            default:
                return commonColor;
        }
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
        return canvasGroup != null && iconImage != null && nameText != null && rarityText != null;
    }
}
