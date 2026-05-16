using UnityEngine;
using UnityEngine.InputSystem;
public class TraderInteraction : MonoBehaviour
{
    [SerializeField] private CursorConfigs cursorConfigs;

    [Header("UI")]
    [SerializeField] private GameObject interactText;
    [SerializeField] private GameObject interactTextTrader;

    [SerializeField] private GameObject FishingRod;

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    private bool playerInRange;

    private void Awake()
    {
        if (interactText != null)
        {
            interactText.SetActive(false);
        }
        if (interactTextTrader != null)
        {
            interactTextTrader.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed += OnInteract;
            interactAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = true;
        SetPromptVisible(true);
        FishingRod.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = false;
        SetPromptVisible(false);
        interactText.SetActive(false);
        interactTextTrader.SetActive(false);
        FishingRod.SetActive(true);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!playerInRange)
        {
            return;
        }

        Interact();
    }

    private void SetPromptVisible(bool visible)
    {
        if (interactText != null)
        {
            interactText.SetActive(visible);
        }
    }

    private void Interact()
    {
        // TODO: open trade UI, start dialogue, etc.
        Debug.Log("Trader interaction triggered");
        if (interactTextTrader != null)
        {
            interactTextTrader.SetActive(true);
            interactText.SetActive(false);
            FishingRod.SetActive(false);
            cursorConfigs.ShowCursor();
        }

    }
}
