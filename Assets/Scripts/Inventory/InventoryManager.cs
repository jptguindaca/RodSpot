using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fishing
{
    [Serializable]
    public class InventoryItem
    {
        public FishData fish;
        public int value; // individual value of this catch

        public InventoryItem(FishData fish, int value = 0)
        {
            this.fish = fish;
            this.value = value > 0 ? value : fish.GetRandomValue();
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

        void OnFishCaught(FishData fish, int value)
        {
            AddFish(fish, value);
        }

        public void AddFish(FishData fish)
        {
            if (fish == null) return;

            // Add a new inventory item with a random value for this catch
            var item = new InventoryItem(fish);
            items.Add(item);

            RefreshUI();

            if (inventoryUI != null)
            {
                inventoryUI.ShowCatchPopup(fish, item.value);
                inventoryUI.Show();
            }
        }

        public void AddFish(FishData fish, int value)
        {
            if (fish == null) return;

            var item = new InventoryItem(fish, value);
            items.Add(item);

            RefreshUI();

            if (inventoryUI != null)
            {
                inventoryUI.ShowCatchPopup(fish, item.value);
                inventoryUI.Show();
            }
        }

        public void RemoveFish(FishData fish)
        {
            var entry = items.Find(i => i.fish == fish);
            if (entry != null) items.Remove(entry);
            RefreshUI();
        }

        void RefreshUI()
        {
            if (inventoryUI != null) inventoryUI.Refresh(items);
        }
    }
}
