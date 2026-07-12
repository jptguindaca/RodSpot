using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Fishing
{
    public class InventoryEntryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Image background;
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text valueText;

        InventoryItem currentItem;
        FishData currentFish;
        int currentValue;
        Action<FishData, int, Vector2> onHoverEnter;
        Action onHoverExit;
        Action<InventoryItem> onSellRequested;

        public void Setup(
                InventoryItem item,
            Color rarityColor,
            Action<FishData, int, Vector2> onHoverEnter,
                Action onHoverExit,
                Action<InventoryItem> onSellRequested)
        {
                currentItem = item;
                currentFish = item != null ? item.fish : null;
                currentValue = item != null ? item.value : 0;
            this.onHoverEnter = onHoverEnter;
            this.onHoverExit = onHoverExit;
                this.onSellRequested = onSellRequested;

            if (icon != null)
            {
                    icon.sprite = currentFish != null ? currentFish.icon : null;
                    icon.enabled = currentFish != null && currentFish.icon != null;
                icon.preserveAspect = true;
                icon.color = Color.white;
            }
                if (nameText != null)
                {
                    nameText.text = currentFish != null
                        ? string.IsNullOrWhiteSpace(currentFish.fishName) ? currentFish.name : currentFish.fishName
                        : "Peixe";
                }
                if (valueText != null) valueText.text = currentValue.ToString() + " coins";
            if (background != null) background.color = rarityColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentFish == null) return;
            onHoverEnter?.Invoke(currentFish, currentValue, eventData.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onHoverExit?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (currentItem == null)
            {
                return;
            }

            onSellRequested?.Invoke(currentItem);
        }
    }
}
