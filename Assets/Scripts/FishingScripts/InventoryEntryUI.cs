using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fishing
{
    public class InventoryEntryUI : MonoBehaviour
    {
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text rarityText;
        public TMP_Text countText;

        public void Setup(FishData fish, int count)
        {
            if (icon != null) icon.sprite = fish.icon;
            if (nameText != null) nameText.text = fish.fishName;
            if (rarityText != null) rarityText.text = fish.rarity.ToString();
            if (countText != null) countText.text = count.ToString();
        }
    }
}
