using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Fishing
{
    public class InventoryEntryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image background;
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text valueText;

        FishData currentFish;
        int currentValue;
        Action<FishData, int, Vector2> onHoverEnter;
        Action onHoverExit;

        public void Setup(
            FishData fish,
            int value,
            Color rarityColor,
            Action<FishData, int, Vector2> onHoverEnter,
            Action onHoverExit)
        {
            currentFish = fish;
            currentValue = value;
            this.onHoverEnter = onHoverEnter;
            this.onHoverExit = onHoverExit;

            if (icon != null)
            {
                icon.sprite = fish != null ? fish.icon : null;
                icon.enabled = fish != null && fish.icon != null;
                icon.preserveAspect = true;
                icon.color = Color.white;
            }
            if (nameText != null) nameText.text = string.Empty;
            if (valueText != null) valueText.text = string.Empty;
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
    }
}
