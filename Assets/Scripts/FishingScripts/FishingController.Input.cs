using UnityEngine;
using UnityEngine.InputSystem;

// Callbacks do sistema de input para a pesca.
public partial class FishingController
{
    private void OnCastStarted(InputAction.CallbackContext context)
    {
        // Inicia o carregamento do lancamento.
        if (state != FishingState.Idle)
            return;

        state = FishingState.Aiming;
        castStartTime = Time.time;
        isChargingCast = true;
        notifications?.ShowCastPreparing();
    }

    private void OnCastCanceled(InputAction.CallbackContext context)
    {
        // Lanca a linha com base no tempo carregado.
        if (state == FishingState.Aiming)
        {
            float holdTime = isChargingCast ? Time.time - castStartTime : 0f;
            float chargeRatio = Mathf.Clamp01(holdTime / Mathf.Max(0.01f, settings.maxChargeTime));
            isChargingCast = false;
            CastLine(chargeRatio);
        }
    }

    private void OnReelPerformed(InputAction.CallbackContext context)
    {
        // Inicia a recolha ou regista cliques.
        if (state == FishingState.FishHooked && canHookFish)
        {
            StartReelingFish();
        }

        if (state == FishingState.Reeling)
        {
            RegisterReelClick();
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        // Cancela a pesca e reseta o estado.
        isChargingCast = false;
        ResetFishing();
    }
}
