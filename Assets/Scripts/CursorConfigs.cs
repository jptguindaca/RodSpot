using UnityEngine;
using UnityEngine.InputSystem;

public class CursorConfigs : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private TraderInteraction traderInteraction;
    [SerializeField] private ButtonShop buttonShop;
    [SerializeField] private string[] actionsToToggle =
    {
        "GamePlay/Move",
        "GamePlay/Jump",
        "GamePlay/Sprint",
        "CameraControls/MouseZoom",
        "Fishing/Cast",
        "Fishing/Reel",
        "Fishing/Aim",
        "Fishing/Cancel",
        "Interactions/Interact",
        "Interactions/BackPack",
        "Interactions/Button1",
        "Interactions/Button2",
        "Interactions/Button3"
    };

    [Header("Components")]
    [SerializeField] private PlayerControl playerControl;
    [SerializeField] private CamaraController cameraController;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Onback(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        if (!IsUiOpen())
        {
            return;
        }

        if (buttonShop != null && buttonShop.traderCompraPanel != null)
        {
            buttonShop.traderCompraPanel.SetActive(false);
        }

        HideCursor();
    }

    private bool IsUiOpen()
    {
        return buttonShop != null
            && buttonShop.traderCompraPanel != null
            && buttonShop.traderCompraPanel.activeSelf;
    }

    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SetActionsEnabled(false);

        if (playerControl != null)
        {
            playerControl.StopMovement();
            playerControl.enabled = false;
        }

        if (cameraController != null)
        {
            cameraController.enabled = false;
            cameraController.StopCamera();
        }
    }

    public void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        SetActionsEnabled(true);

        if (playerControl != null)
        {
            playerControl.enabled = true;
        }

        if (cameraController != null)
        {
            cameraController.enabled = true;
            cameraController.StartCamera();
        }
    }

    private void SetActionsEnabled(bool enabled)
    {
        if (playerInput == null || playerInput.actions == null)
        {
            return;
        }

        foreach (string actionName in actionsToToggle)
        {
            if (string.IsNullOrEmpty(actionName))
            {
                continue;
            }

            InputAction action = playerInput.actions.FindAction(actionName, false);
            if (action == null)
            {
                continue;
            }
            

            if (enabled)
            {
                action.Enable();
            }
            else
            {
                action.Disable();
            }
        }
    }
}
