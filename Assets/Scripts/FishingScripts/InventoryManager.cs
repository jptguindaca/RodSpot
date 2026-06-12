using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fishing
{
    [Serializable]
    public class InventoryItem
    {
        public FishData fish;
        public int count;

        public InventoryItem(FishData fish)
        {
            this.fish = fish;
            this.count = 1;
        }
    }

    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        public InventoryUI inventoryUI;

        public List<InventoryItem> items = new List<InventoryItem>();

        public FishingController fishingController;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            if (fishingController != null)
            {
                fishingController.FishCaught += OnFishCaught;
            }
            RefreshUI();
        }

        void OnDestroy()
        {
            if (fishingController != null)
                fishingController.FishCaught -= OnFishCaught;
        }

        /// <summary>
        /// Called by FishingController to register itself without using FindObjectOfType.
        /// </summary>
        public void RegisterFishingController(FishingController fc)
        {
            if (fc == null) return;
            if (fishingController == fc) return;
            if (fishingController != null)
            {
                fishingController.FishCaught -= OnFishCaught;
            }
            fishingController = fc;
            fishingController.FishCaught += OnFishCaught;
            RefreshUI();
        }

        void OnFishCaught(FishData fish)
        {
            AddFish(fish);
        }

        public void AddFish(FishData fish)
        {
            if (fish == null) return;

            var entry = items.Find(i => i.fish == fish);
            if (entry != null)
            {
                entry.count++;
            }
            else
            {
                items.Add(new InventoryItem(fish));
            }

            RefreshUI();

            if (inventoryUI != null)
            {
                inventoryUI.ShowCatchPopup(fish);
                inventoryUI.Show();
            }
        }

        public void RemoveFish(FishData fish)
        {
            var entry = items.Find(i => i.fish == fish);
            if (entry == null) return;
            entry.count--;
            if (entry.count <= 0) items.Remove(entry);
            RefreshUI();
        }

        void RefreshUI()
        {
            if (inventoryUI != null) inventoryUI.Refresh(items);
        }
    }
}
