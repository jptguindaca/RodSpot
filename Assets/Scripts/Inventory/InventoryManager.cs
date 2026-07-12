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
            }
        }

        public bool SellFish(InventoryItem item)
        {
            if (item == null)
            {
                return false;
            }

            if (!items.Remove(item))
            {
                return false;
            }

            var wallet = PlayerWallet.Instance;
            if (wallet != null)
            {
                wallet.AddMoney(Mathf.Max(0, item.value));
            }

            RefreshUI();
            return true;
        }

        void RefreshUI()
        {
            if (inventoryUI != null) inventoryUI.Refresh(items, item => SellFish(item));
        }
    }
}
