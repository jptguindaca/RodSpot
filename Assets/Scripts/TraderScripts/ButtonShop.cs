using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonShop : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject traderInteractionPanel;
    [SerializeField] public GameObject traderCompraPanel;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject itemPanel;
    [SerializeField] private TMP_Text moneyText;

    [Header("Systems")]
    [SerializeField] private PlayerControl playerControl;
    [SerializeField] private PlayerWallet playerWallet;
    [SerializeField] private Fishing.InventoryManager inventoryManager;
    [SerializeField] private Fishing.InventoryUI traderSellUI;

    [Header("Upgrade Colors")]
    [SerializeField] private Color levelFilledColor = new Color(0.35f, 0.95f, 0.12f, 1f);
    [SerializeField] private Color levelEmptyColor = new Color(1f, 1f, 1f, 0.72f);
    [SerializeField] private Color costAffordableColor = new Color(1f, 0.86f, 0.18f, 1f);
    [SerializeField] private Color costLockedColor = new Color(1f, 0.35f, 0.35f, 1f);

    private readonly List<UpgradeRowUI> upgradeRows = new List<UpgradeRowUI>();
    private bool upgradeRowsCached;

    private static readonly (string RowName, UpgradeType Type, string FallbackLabel)[] UpgradeDefinitions = new[]
    {
        ("Upgrades_box", UpgradeType.ClickPower, "Cliques por segundo"),
        ("Upgrades_box2", UpgradeType.MoneyBonus, "mais dinheiro"),
        ("Upgrades_box3", UpgradeType.RarityBonus, "% raridade"),
        ("Upgrades_box4", UpgradeType.EscapeTime, "mais tempo para o peixe escapar"),
    };

    private sealed class UpgradeRowUI
    {
        public UpgradeType upgradeType;
        public GameObject root;
        public TMP_Text titleText;
        public TMP_Text costText;
        public Button[] buttons;
        public Image[] levelMarkers;
    }

    private void Awake()
    {
        CacheSystems();
        CacheUpgradeRows();
    }

    private void Start()
    {
        RefreshAllUpgradeRows();
    }

    private void OnEnable()
    {
        CacheSystems();
        SubscribeWallet();
        RefreshAllUpgradeRows();
        RefreshMoneyText();
    }

    private void OnDisable()
    {
        UnsubscribeWallet();
    }

    public void ShowUI()
    {
        if (traderInteractionPanel != null)
        {
            traderInteractionPanel.SetActive(false);
        }

        if (traderCompraPanel != null)
        {
            traderCompraPanel.SetActive(true);
        }

        SetTab(upgradePanel);
        RefreshAllUpgradeRows();
        RefreshMoneyText();
    }

    public void upgradeButthon()
    {
        SetTab(upgradePanel);
        RefreshAllUpgradeRows();
        RefreshMoneyText();
    }

    public void itemButton()
    {
        SetTab(itemPanel);
        RefreshTraderSellUI();
    }

    public void PauseMenu()
    {
    }

    public void BuyClickPowerUpgrade()
    {
        TryBuyUpgrade(UpgradeType.ClickPower);
    }

    public void BuyMoneyUpgrade()
    {
        TryBuyUpgrade(UpgradeType.MoneyBonus);
    }

    public void BuyRarityUpgrade()
    {
        TryBuyUpgrade(UpgradeType.RarityBonus);
    }

    public void BuyEscapeTimeUpgrade()
    {
        TryBuyUpgrade(UpgradeType.EscapeTime);
    }

    private void SetTab(GameObject targetPanel)
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(targetPanel == upgradePanel);
        }

        if (itemPanel != null)
        {
            itemPanel.SetActive(targetPanel == itemPanel);
        }
    }

    private void CacheSystems()
    {
        if (playerControl == null)
        {
            playerControl = FindFirstObjectByType<PlayerControl>();
        }

        if (playerWallet == null)
        {
            playerWallet = FindFirstObjectByType<PlayerWallet>();
        }

        if (inventoryManager == null)
        {
            inventoryManager = Fishing.InventoryManager.Instance;
        }

        if (traderSellUI == null && inventoryManager != null)
        {
            traderSellUI = inventoryManager.inventoryUI;
        }
    }

    private void SubscribeWallet()
    {
        if (playerWallet != null)
        {
            playerWallet.MoneyChanged -= HandleMoneyChanged;
            playerWallet.MoneyChanged += HandleMoneyChanged;
        }
    }

    private void UnsubscribeWallet()
    {
        if (playerWallet != null)
        {
            playerWallet.MoneyChanged -= HandleMoneyChanged;
        }
    }

    private void CacheUpgradeRows()
    {
        if (upgradeRowsCached || upgradePanel == null)
        {
            return;
        }

        upgradeRows.Clear();

        for (int i = 0; i < UpgradeDefinitions.Length; i++)
        {
            var definition = UpgradeDefinitions[i];
            Transform rowTransform = FindDeepChild(upgradePanel.transform, definition.RowName);
            if (rowTransform == null)
            {
                continue;
            }

            var rowUi = new UpgradeRowUI
            {
                upgradeType = definition.Type,
                root = rowTransform.gameObject,
                titleText = FindTitleText(rowTransform, definition.FallbackLabel),
                costText = EnsureCostText(rowTransform),
                buttons = rowTransform.GetComponentsInChildren<Button>(true),
                levelMarkers = FindLevelMarkers(rowTransform),
            };

            BindButtons(rowUi);
            upgradeRows.Add(rowUi);
        }

        upgradeRowsCached = true;
    }

    private void BindButtons(UpgradeRowUI rowUi)
    {
        if (rowUi == null || rowUi.buttons == null)
        {
            return;
        }

        for (int i = 0; i < rowUi.buttons.Length; i++)
        {
            Button button = rowUi.buttons[i];
            if (button == null)
            {
                continue;
            }

            UpgradeType upgradeType = rowUi.upgradeType;
            button.onClick.AddListener(() => TryBuyUpgrade(upgradeType));
        }
    }

    private bool TryBuyUpgrade(UpgradeType upgradeType)
    {
        CacheSystems();

        if (playerControl == null || playerWallet == null)
        {
            return false;
        }

        if (!playerControl.CanUpgrade(upgradeType))
        {
            RefreshAllUpgradeRows();
            return false;
        }

        int cost = playerControl.GetUpgradeCost(upgradeType);
        if (!playerWallet.SpendMoney(cost))
        {
            RefreshAllUpgradeRows();
            return false;
        }

        if (!playerControl.TryUpgrade(upgradeType))
        {
            playerWallet.AddMoney(cost);
            RefreshAllUpgradeRows();
            return false;
        }

        RefreshAllUpgradeRows();
        return true;
    }

    private void RefreshAllUpgradeRows()
    {
        CacheUpgradeRows();

        for (int i = 0; i < upgradeRows.Count; i++)
        {
            RefreshUpgradeRow(upgradeRows[i]);
        }
    }

    private void HandleMoneyChanged(int money)
    {
        RefreshMoneyText();
        RefreshAllUpgradeRows();
    }

    private void RefreshMoneyText()
    {
        if (moneyText == null)
        {
            return;
        }

        CacheSystems();

        int money = playerWallet != null ? playerWallet.CurrentMoney : 0;
        moneyText.text = $"Money: {money}";
    }

    private void RefreshTraderSellUI()
    {
        CacheSystems();

        if (inventoryManager == null || itemPanel == null)
        {
            return;
        }

        var sourceUi = inventoryManager.inventoryUI;
        if (sourceUi == null || sourceUi.entryPrefab == null)
        {
            return;
        }

        for (int i = itemPanel.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(itemPanel.transform.GetChild(i).gameObject);
        }

        foreach (var item in inventoryManager.items)
        {
            if (item == null || item.fish == null)
            {
                continue;
            }

            GameObject entryObject = Instantiate(sourceUi.entryPrefab, itemPanel.transform, false);
            Fishing.InventoryEntryUI entryUi = entryObject.GetComponent<Fishing.InventoryEntryUI>();
            if (entryUi != null)
            {
                entryUi.Setup(
                    item,
                    sourceUi.GetRarityColor(item.fish.rarity),
                    sourceUi.ShowTooltip,
                    sourceUi.HideTooltip,
                    soldItem =>
                    {
                        if (inventoryManager.SellFish(soldItem))
                        {
                            RefreshTraderSellUI();
                        }
                    });
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(itemPanel.transform as RectTransform);
    }

    private void RefreshUpgradeRow(UpgradeRowUI rowUi)
    {
        if (rowUi == null)
        {
            return;
        }

        int level = playerControl != null ? playerControl.GetUpgradeLevel(rowUi.upgradeType) : 0;
        bool canUpgrade = playerControl != null && playerControl.CanUpgrade(rowUi.upgradeType);
        int cost = playerControl != null ? playerControl.GetUpgradeCost(rowUi.upgradeType) : 0;
        bool canAfford = playerWallet == null || playerWallet.CanAfford(cost);

        if (rowUi.levelMarkers != null)
        {
            for (int i = 0; i < rowUi.levelMarkers.Length; i++)
            {
                Image marker = rowUi.levelMarkers[i];
                if (marker == null)
                {
                    continue;
                }

                marker.color = i < level ? levelFilledColor : levelEmptyColor;
            }
        }

        if (rowUi.costText != null)
        {
            if (!canUpgrade)
            {
                rowUi.costText.text = "MAX";
                rowUi.costText.color = levelFilledColor;
            }
            else
            {
                rowUi.costText.text = $"Custo: {cost}";
                rowUi.costText.color = canAfford ? costAffordableColor : costLockedColor;
            }
        }

        if (rowUi.buttons != null)
        {
            for (int i = 0; i < rowUi.buttons.Length; i++)
            {
                Button button = rowUi.buttons[i];
                if (button == null)
                {
                    continue;
                }

                button.interactable = canUpgrade && canAfford;
            }
        }

        if (rowUi.titleText != null && string.IsNullOrWhiteSpace(rowUi.titleText.text))
        {
            rowUi.titleText.text = GetDefaultLabel(rowUi.upgradeType);
        }
    }

    private TMP_Text EnsureCostText(Transform rowTransform)
    {
        if (rowTransform == null)
        {
            return null;
        }

        Transform existing = rowTransform.Find("CostText");
        if (existing != null)
        {
            return existing.GetComponent<TMP_Text>();
        }

        TMP_Text template = rowTransform.GetComponentInChildren<TMP_Text>(true);
        if (template == null)
        {
            return null;
        }

        GameObject costObject = Instantiate(template.gameObject, rowTransform, false);
        costObject.name = "CostText";

        TMP_Text costText = costObject.GetComponent<TMP_Text>();
        if (costText != null)
        {
            costText.text = string.Empty;
            costText.alignment = TextAlignmentOptions.Right;
            costText.fontSize = Mathf.Max(14f, costText.fontSize * 0.75f);
            costText.color = costAffordableColor;
        }

        RectTransform rectTransform = costObject.transform as RectTransform;
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.08f);
            rectTransform.anchorMax = new Vector2(1f, 0.92f);
            rectTransform.pivot = new Vector2(1f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(-76f, -18f);
            rectTransform.sizeDelta = new Vector2(170f, 24f);
        }

        return costText;
    }

    private TMP_Text FindTitleText(Transform rowTransform, string fallbackLabel)
    {
        if (rowTransform == null)
        {
            return null;
        }

        TMP_Text[] texts = rowTransform.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null || text.gameObject.name == "CostText")
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(text.text))
            {
                return text;
            }
        }

        if (texts.Length > 0 && texts[0] != null)
        {
            texts[0].text = fallbackLabel;
            return texts[0];
        }

        return null;
    }

    private Image[] FindLevelMarkers(Transform rowTransform)
    {
        if (rowTransform == null)
        {
            return Array.Empty<Image>();
        }

        GridLayoutGroup grid = rowTransform.GetComponentInChildren<GridLayoutGroup>(true);
        if (grid == null)
        {
            return Array.Empty<Image>();
        }

        Transform gridRoot = grid.transform;
        List<Image> markers = new List<Image>();

        for (int i = 0; i < gridRoot.childCount; i++)
        {
            Transform child = gridRoot.GetChild(i);
            Image marker = child.GetComponent<Image>();
            if (marker != null)
            {
                markers.Add(marker);
            }
        }

        return markers.ToArray();
    }

    private Transform FindDeepChild(Transform root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }

            Transform nested = FindDeepChild(child, childName);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private string GetDefaultLabel(UpgradeType upgradeType)
    {
        return upgradeType switch
        {
            UpgradeType.ClickPower => "Cliques por segundo",
            UpgradeType.MoneyBonus => "mais dinheiro",
            UpgradeType.RarityBonus => "% raridade",
            UpgradeType.EscapeTime => "mais tempo para o peixe escapar",
            _ => "Upgrade",
        };
    }
}
