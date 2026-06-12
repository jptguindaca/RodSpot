using System.Collections.Generic;
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

        [Header("Catch Popup")]
        public CanvasGroup catchPopupGroup;
        public Image catchIcon;
        public TMP_Text catchName;
        public TMP_Text catchRarity;
        public float catchPopupDuration = 1.5f;

        bool isVisible = false;

        void Start()
        {
            if (panel != null) panel.alpha = 0; // começa escondido
            if (catchPopupGroup != null) catchPopupGroup.alpha = 0;
        }

        public void Show()
        {
            if (panel == null) return;
            panel.alpha = 1;
            panel.blocksRaycasts = true;
            isVisible = true;
        }

        public void Hide()
        {
            if (panel == null) return;
            panel.alpha = 0;
            panel.blocksRaycasts = false;
            isVisible = false;
        }

        public void Toggle()
        {
            if (isVisible) Hide(); else Show();
        }

        public void Refresh(List<InventoryItem> items)
        {
            if (contentParent == null || entryPrefab == null) return;

            // clear
            for (int i = contentParent.childCount - 1; i >= 0; --i)
                Destroy(contentParent.GetChild(i).gameObject);

            foreach (var it in items)
            {
                var go = Instantiate(entryPrefab, contentParent, false);
                var ui = go.GetComponent<InventoryEntryUI>();
                if (ui != null) ui.Setup(it.fish, it.count);
            }
        }

        public void ShowCatchPopup(FishData fish)
        {
            if (catchPopupGroup == null || fish == null) return;
            if (catchIcon != null) catchIcon.sprite = fish.icon;
            if (catchName != null) catchName.text = fish.fishName;
            if (catchRarity != null) catchRarity.text = fish.rarity.ToString();
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
    }
}
