using System;
using Unity.VisualScripting;
using UnityEngine;

public class ButtonShop : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject traderInteractionPanel;    
    [SerializeField] public GameObject traderCompraPanel;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject itemPanel;

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
    }

    public void upgradeButthon()
    {
        SetTab(upgradePanel);
    }

    public void itemButton()
    {
        SetTab(itemPanel);
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
}
