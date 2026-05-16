using UnityEngine;

public class ButtonShop : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject currentPanel;
    [SerializeField] public GameObject targetPanel;

    public void ShowUI()
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }

        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
        }
    }
}
