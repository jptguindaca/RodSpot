using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fishing
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        public CanvasGroup panel;
        public Transform contentParent;
        public GameObject entryPrefab;
        public TMP_Text moneyText;

        [Header("Rarity Colors")]
        public Color colorCommon = new Color(0.8f, 0.8f, 0.8f, 1f); // gray
        public Color colorUncommon = new Color(0.2f, 0.8f, 0.2f, 1f); // green
        public Color colorRare = new Color(0.2f, 0.5f, 1f, 1f); // blue
        public Color colorEpic = new Color(0.7f, 0.2f, 1f, 1f); // purple
        public Color colorLegendary = new Color(1f, 0.8f, 0.2f, 1f); // gold

        [Header("Catch Popup")]
        public CanvasGroup catchPopupGroup;
        public Image catchIcon;
        public TMP_Text catchName;
        public TMP_Text catchRarity;
        public TMP_Text catchValue;
        public float catchPopupDuration = 1.5f;

        [Header("Hover Tooltip")]
        public CanvasGroup tooltipGroup;
        public RectTransform tooltipRoot;
        public TMP_Text tooltipName;
        public TMP_Text tooltipRarity;
        public TMP_Text tooltipValue;
        public Vector2 tooltipOffset = new Vector2(36f, -6f);
        public Vector2 tooltipPadding = new Vector2(360f, 220f);
        public Vector2 tooltipSize = new Vector2(320f, 160f);

        [Header("Auto Layout")]
        public Vector2 panelSize = new Vector2(1040f, 680f);
        public Vector2 panelPadding = new Vector2(36f, 36f);
        public Vector2 gridCellSize = new Vector2(108f, 108f);
        public Vector2 gridSpacing = new Vector2(14f, 14f);
        public int gridColumns = 7;
        public Color panelTint = new Color(0.09f, 0.11f, 0.14f, 0.92f);
        public Color tooltipTint = new Color(0.07f, 0.08f, 0.10f, 0.98f);
        public Color tooltipBorderColor = new Color(1f, 1f, 1f, 0.08f);

        [Header("Cursor Control")]
        public CursorConfigs cursorConfigs;

        bool isVisible = false;
        bool tooltipVisible = false;
        PlayerWallet cachedWallet;

        void Start()
        {
            if (cursorConfigs == null)
                cursorConfigs = FindObjectOfType<CursorConfigs>();

            ApplyAutoLayout();
            RefreshMoney();

            if (panel != null) panel.alpha = 0;
            if (catchPopupGroup != null) catchPopupGroup.alpha = 0;
            HideTooltip();
        }

        void OnEnable()
        {
            CacheWallet();
            if (cachedWallet != null)
            {
                cachedWallet.MoneyChanged += HandleMoneyChanged;
            }

            RefreshMoney();
        }

        void OnDisable()
        {
            if (cachedWallet != null)
            {
                cachedWallet.MoneyChanged -= HandleMoneyChanged;
            }
        }

        void Update()
        {
            if (!tooltipVisible) return;
            MoveTooltip(Input.mousePosition);
        }

        public void Show()
        {
            if (panel == null) return;
            panel.alpha = 1;
            panel.blocksRaycasts = true;
            panel.interactable = true;
            RefreshMoney();
            cursorConfigs?.ShowCursor();
            isVisible = true;
        }

        public void Hide()
        {
            if (panel == null) return;
            panel.alpha = 0;
            panel.blocksRaycasts = false;
            panel.interactable = false;
            HideTooltip();
            cursorConfigs?.HideCursor();
            isVisible = false;
        }

        public void Toggle()
        {
            if (isVisible) Hide(); else Show();
        }

        public Color GetRarityColor(FishRarity rarity)
        {
            switch (rarity)
            {
                case FishRarity.Common: return colorCommon;
                case FishRarity.Uncommon: return colorUncommon;
                case FishRarity.Rare: return colorRare;
                case FishRarity.Epic: return colorEpic;
                case FishRarity.Legendary: return colorLegendary;
                default: return colorCommon;
            }
        }

        public void Refresh(List<InventoryItem> items, Action<InventoryItem> onSellItem)
        {
            if (contentParent == null || entryPrefab == null) return;

            for (int i = contentParent.childCount - 1; i >= 0; --i)
                Destroy(contentParent.GetChild(i).gameObject);

            foreach (var it in items)
            {
                var go = Instantiate(entryPrefab, contentParent, false);
                var ui = go.GetComponent<InventoryEntryUI>();
                if (ui != null)
                {
                    ui.Setup(it, GetRarityColor(it.fish.rarity), ShowTooltip, HideTooltip, onSellItem);
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
        }

        public void ShowTooltip(FishData fish, int value, Vector2 screenPosition)
        {
            if (fish == null || tooltipGroup == null) return;

            if (tooltipName != null) tooltipName.text = fish.fishName;

            if (tooltipRarity != null)
            {
                tooltipRarity.text = fish.rarity.ToString();
                tooltipRarity.color = GetRarityColor(fish.rarity);
            }

            if (tooltipValue != null)
                tooltipValue.text = value.ToString() + " coins";

            tooltipGroup.alpha = 1f;
            tooltipGroup.blocksRaycasts = false;
            tooltipGroup.interactable = false;
            tooltipVisible = true;
            MoveTooltip(screenPosition);
        }

        public void HideTooltip()
        {
            if (tooltipGroup != null)
            {
                tooltipGroup.alpha = 0f;
                tooltipGroup.blocksRaycasts = false;
            }
            tooltipVisible = false;
        }

        void MoveTooltip(Vector2 screenPosition)
        {
            if (tooltipRoot == null) return;

            var nextPosition = screenPosition + tooltipOffset;
            var canvas = tooltipRoot.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
            {
                var canvasRect = canvas.transform as RectTransform;
                if (canvasRect != null)
                {
                    var corners = new Vector3[4];
                    canvasRect.GetWorldCorners(corners);
                    var maxX = corners[2].x - tooltipPadding.x;
                    var maxY = corners[2].y - tooltipPadding.y;
                    var minX = corners[0].x + tooltipPadding.x;
                    var minY = corners[0].y + tooltipPadding.y;
                    nextPosition.x = Mathf.Clamp(nextPosition.x, minX, maxX);
                    nextPosition.y = Mathf.Clamp(nextPosition.y, minY, maxY);
                }
            }

            tooltipRoot.position = nextPosition;
        }

        public void ShowCatchPopup(FishData fish, int value)
        {
            if (catchPopupGroup == null || fish == null) return;
            if (catchIcon != null) catchIcon.sprite = fish.icon;
            if (catchName != null) catchName.text = fish.fishName;
            if (catchRarity != null) catchRarity.text = fish.rarity.ToString();
            if (catchValue != null)
            {
                catchValue.text = value.ToString();
                catchValue.color = new Color(1f, 0.8f, 0.2f, 1f);
            }
            StartCoroutine(DoPopup());
        }

        System.Collections.IEnumerator DoPopup()
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.12f;
                if (catchPopupGroup != null) catchPopupGroup.alpha = Mathf.Clamp01(t);
                yield return null;
            }

            yield return new WaitForSeconds(catchPopupDuration);

            t = 1f;
            while (t > 0f)
            {
                t -= Time.deltaTime / 0.12f;
                if (catchPopupGroup != null) catchPopupGroup.alpha = Mathf.Clamp01(t);
                yield return null;
            }
        }

        void ApplyAutoLayout()
        {
            if (panel != null)
            {
                var panelImage = panel.GetComponent<Image>();
                if (panelImage != null)
                    panelImage.color = panelTint;

                panel.interactable = false;
                panel.blocksRaycasts = false;

                var panelRect = panel.transform as RectTransform;
                if (panelRect != null)
                {
                    panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                    panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                    panelRect.pivot = new Vector2(0.5f, 0.5f);
                    panelRect.sizeDelta = panelSize;
                }
            }

            if (moneyText != null)
            {
                // position top-right inside the panel
                var mtRect = moneyText.rectTransform;
                mtRect.anchorMin = new Vector2(1f, 1f);
                mtRect.anchorMax = new Vector2(1f, 1f);
                mtRect.pivot = new Vector2(1f, 1f);
                mtRect.anchoredPosition = new Vector2(-28f, -28f);
                mtRect.sizeDelta = new Vector2(360f, 48f);

                moneyText.alignment = TextAlignmentOptions.Right;
                moneyText.textWrappingMode = TextWrappingModes.NoWrap;
                moneyText.overflowMode = TextOverflowModes.Ellipsis;
                moneyText.fontStyle = FontStyles.Bold;

                // responsive font size
                moneyText.enableAutoSizing = true;
                moneyText.fontSizeMin = 18;
                moneyText.fontSizeMax = 36;

                // color and subtle outline for legibility
                moneyText.color = new Color(1f, 0.86f, 0.18f, 1f);
                var existingOutline = moneyText.GetComponent<Outline>();
                if (existingOutline == null)
                {
                    existingOutline = moneyText.gameObject.AddComponent<Outline>();
                }
                existingOutline.effectColor = new Color(0f, 0f, 0f, 0.65f);
                existingOutline.effectDistance = new Vector2(2f, -2f);
            }

            if (contentParent != null)
            {
                var contentRect = contentParent as RectTransform;
                if (contentRect != null)
                {
                    contentRect.anchorMin = new Vector2(0f, 0f);
                    contentRect.anchorMax = new Vector2(1f, 1f);
                    contentRect.offsetMin = new Vector2(panelPadding.x, panelPadding.y);
                    contentRect.offsetMax = new Vector2(-panelPadding.x, -panelPadding.y);
                }

                var cachedGrid = contentParent.GetComponent<GridLayoutGroup>();
                if (cachedGrid != null)
                {
                    cachedGrid.cellSize = gridCellSize;
                    cachedGrid.spacing = gridSpacing;
                    cachedGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    cachedGrid.constraintCount = Mathf.Max(1, gridColumns);
                    cachedGrid.childAlignment = TextAnchor.UpperLeft;
                }
            }

            if (tooltipGroup != null)
            {
                var tooltipImage = tooltipGroup.GetComponent<Image>();
                if (tooltipImage != null)
                {
                    tooltipImage.color = tooltipTint;

                    var border = tooltipGroup.GetComponent<Outline>();
                    if (border == null)
                        border = tooltipGroup.gameObject.AddComponent<Outline>();

                    border.effectColor = tooltipBorderColor;
                    border.effectDistance = new Vector2(1.5f, -1.5f);
                }

                var tooltipRect = tooltipGroup.transform as RectTransform;
                if (tooltipRect != null)
                {
                    tooltipRect.anchorMin = new Vector2(0f, 1f);
                    tooltipRect.anchorMax = new Vector2(0f, 1f);
                    tooltipRect.pivot = new Vector2(0f, 1f);
                    tooltipRect.sizeDelta = tooltipSize;
                }

                if (tooltipName != null)
                {
                    tooltipName.alignment = TextAlignmentOptions.Left;
                    tooltipName.fontStyle = FontStyles.Bold;
                    tooltipName.textWrappingMode = TextWrappingModes.Normal;
                    tooltipName.overflowMode = TextOverflowModes.Overflow;
                }

                if (tooltipRarity != null)
                {
                    tooltipRarity.alignment = TextAlignmentOptions.Left;
                    tooltipRarity.textWrappingMode = TextWrappingModes.Normal;
                    tooltipRarity.overflowMode = TextOverflowModes.Overflow;
                }

                if (tooltipValue != null)
                {
                    tooltipValue.alignment = TextAlignmentOptions.Left;
                    tooltipValue.textWrappingMode = TextWrappingModes.Normal;
                    tooltipValue.overflowMode = TextOverflowModes.Overflow;
                }
            }
        }

        void RefreshMoney()
        {
            if (moneyText == null)
                return;

            CacheWallet();
            int money = cachedWallet != null ? cachedWallet.CurrentMoney : 0;
            moneyText.text = $"Money: {money}";
        }

        void HandleMoneyChanged(int money)
        {
            RefreshMoney();
        }

        void CacheWallet()
        {
            if (cachedWallet == null)
            {
                cachedWallet = PlayerWallet.Instance;
            }

            if (cachedWallet == null)
            {
                cachedWallet = FindObjectOfType<PlayerWallet>();
            }
        }
    }
}
