using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fishing
{
    public class InventoryEntryUI : MonoBehaviour
    {
        public Image background;
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text valueText;

        public void Setup(FishData fish, int value, Color rarityColor)
        {
            if (icon != null) icon.sprite = fish.icon;
            if (nameText != null) nameText.text = fish.fishName;
            if (valueText != null) valueText.text = value.ToString();
            if (background != null) background.color = rarityColor;
        }
    }
}
