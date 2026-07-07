using UnityEngine;

namespace Fishing
{
    public class InventoryInputController : MonoBehaviour
    {
        public KeyCode toggleKey = KeyCode.I;
        public KeyCode closeKey = KeyCode.Escape;

        void Update()
        {
            var inv = InventoryManager.Instance;
            if (inv == null || inv.inventoryUI == null) return;

            if (Input.GetKeyDown(toggleKey))
                inv.inventoryUI.Toggle();

            if (Input.GetKeyDown(closeKey))
                inv.inventoryUI.Hide();
        }
    }
}