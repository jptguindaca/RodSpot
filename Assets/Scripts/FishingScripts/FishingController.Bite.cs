using System.Collections;
using UnityEngine;

public partial class FishingController
{
    private IEnumerator WaitForBite()
    {
        // Espera um tempo aleatorio e abre a janela de fisgada.
        float waitTime = Random.Range(settings.minBiteTime, settings.maxBiteTime);
        yield return new WaitForSeconds(waitTime);

        state = FishingState.FishHooked;
        canHookFish = true;
        ShowBiteBar();

        notifications?.ShowBite(settings.hookWindow);

        yield return new WaitForSeconds(settings.hookWindow);

        if (state == FishingState.FishHooked)
        {
            notifications?.ShowFishEscaped();
            ResetFishing();
        }
    }

    private void ShowBiteBar()
    {
        // Mostra a barra da janela de fisgada.
        if (biteUI != null)
        {
            biteUI.Show(settings.hookWindow);
        }
    }

    private void HideBiteBar()
    {
        // Esconde a barra da janela de fisgada.
        if (biteUI != null)
        {
            biteUI.Hide();
        }
    }
}
