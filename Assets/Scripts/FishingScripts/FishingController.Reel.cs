using System.Collections;
using UnityEngine;

public partial class FishingController
{
    //chama O WEBrequestManager para guardar o peixe apanhado e guardar numa base de dados

    public WebRequestManager BD;

    private void StartReelingFish()
    {
        // Inicia o minijogo de cliques baseado na raridade do peixe.
        canHookFish = false;
        state = FishingState.Reeling;
        HideBiteBar();
        ShowEscapeBar();
        if (currentFish == null)
        {
            SelectCurrentFish();
        }

        currentRequiredClicks = GetRequiredClicksForCurrentFish();
        currentFishClicks = 0;
        currentEscapeTimeLimit = GetEscapeTimeLimit();
        currentEscapeTimeRemaining = currentEscapeTimeLimit;
        reelClickTimes.Clear();

        UpdateEscapeBar();

        notifications?.ShowFishHooked();
    }

    private void CatchFish()
    {
        // Finaliza a captura e inicia o reset suave.
        notifications?.ShowFishCaught();
        AudioManager.Instance?.PlayCatch();
        int catchValue = currentFish != null
            ? Mathf.RoundToInt(currentFish.GetRandomValue() * GetMoneyMultiplier())
            : 0;
        if (catchUI != null)
        {
            catchUI.Show(currentFish, catchValue);
        }
        ShowFishPreview();
        FishCaught?.Invoke(currentFish, catchValue);

        if (smoothResetRoutine != null)
        {
            StopCoroutine(smoothResetRoutine);
        }

        smoothResetRoutine = StartCoroutine(SmoothCatchReset());

        Debug.Log("Peixe apanhado!");
        // chama o WebRequest para guardar o peixe apanhado.
        BD._PostData();
        // chama o peixe da BD para mostrar o peixe apanhado
        BD._GetData();
    }

    private void ShowFishPreview()
    {
        if (currentFish == null || currentFish.fishPrefab == null)
        {
            return;
        }

        Transform anchor = fishPreviewAnchor != null
            ? fishPreviewAnchor
            : currentBobber != null
                ? currentBobber.transform
                : rodTip;
        if (anchor == null)
        {
            return;
        }

        if (fishPreviewRoutine != null)
        {
            StopCoroutine(fishPreviewRoutine);
        }

        fishPreviewRoutine = StartCoroutine(ShowFishPreviewRoutine(anchor, currentFish.fishPrefab));
    }

    private IEnumerator ShowFishPreviewRoutine(Transform anchor, GameObject fishPrefab)
    {
        GameObject previewInstance = Instantiate(fishPrefab, anchor.position, anchor.rotation, anchor);
        previewInstance.transform.localPosition = Vector3.zero;
        previewInstance.transform.localRotation = Quaternion.identity;

        if (previewInstance.GetComponent<Rigidbody>() != null)
        {
            Rigidbody previewRigidbody = previewInstance.GetComponent<Rigidbody>();
            previewRigidbody.isKinematic = true;
            previewRigidbody.useGravity = false;
        }

        float duration = Mathf.Max(0.01f, fishPreviewDuration);
        yield return new WaitForSeconds(duration);

        if (previewInstance != null)
        {
            Destroy(previewInstance);
        }

        fishPreviewRoutine = null;
    }

    private void RegisterReelClick()
    {
        // Regista o clique do minijogo.
        reelClickTimes.Enqueue(Time.time);

        if (state != FishingState.Reeling)
        {
            return;
        }

        currentFishClicks = Mathf.Clamp(currentFishClicks + GetClickPowerPerPress(), 0, currentRequiredClicks);
        UpdateEscapeBar();

        if (currentFishClicks >= currentRequiredClicks)
        {
            CatchFish();
        }
    }

    private void HandleReelingTimer()
    {
        if (currentEscapeTimeRemaining <= 0f)
        {
            return;
        }

        currentEscapeTimeRemaining = Mathf.Max(0f, currentEscapeTimeRemaining - Time.deltaTime);
        UpdateEscapeBar();

        if (currentEscapeTimeRemaining <= 0f && state == FishingState.Reeling)
        {
            notifications?.ShowFishEscaped();
            ResetFishing();
        }
    }

    private int GetRequiredClicksForCurrentFish()
    {
        FishRarity rarity = currentFish != null ? currentFish.rarity : FishRarity.Common;

        if (settings == null)
        {
            return 3;
        }

        int baseClicks = rarity switch
        {
            FishRarity.Uncommon => settings.uncommonClicksRequired,
            FishRarity.Rare => settings.rareClicksRequired,
            FishRarity.Epic => settings.epicClicksRequired,
            FishRarity.Legendary => settings.legendaryClicksRequired,
            _ => settings.commonClicksRequired,
        };

        float difficultyMultiplier = currentFish != null ? Mathf.Max(0.1f, currentFish.difficultyMultiplier) : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(baseClicks * difficultyMultiplier));
    }

    private void ShowEscapeBar()
    {
        // Mostra a barra de progresso dos cliques.
        if (escapeUI != null)
        {
            escapeUI.Show();
        }
    }

    private void HideEscapeBar()
    {
        // Esconde a barra de progresso dos cliques.
        if (escapeUI != null)
        {
            escapeUI.Hide();
        }
    }

    private void UpdateEscapeBar()
    {
        // Atualiza a barra com o progresso atual de cliques e o tempo restante.
        if (escapeUI == null)
        {
            return;
        }

        int safeRequiredClicks = Mathf.Max(1, currentRequiredClicks);
        escapeUI.SetClicks(currentFishClicks, safeRequiredClicks);
        escapeUI.SetTimer(currentEscapeTimeRemaining, currentEscapeTimeLimit);
    }

    private int GetClickPowerPerPress()
    {
        return playerControl != null ? playerControl.GetClickPowerPerPress() : 1;
    }

    private float GetMoneyMultiplier()
    {
        return playerControl != null ? playerControl.GetMoneyMultiplier() : 1f;
    }

    private float GetRarityBias()
    {
        return playerControl != null ? playerControl.GetRarityBias() : 0f;
    }

    private float GetEscapeTimeLimit()
    {
        float baseEscapeTime = settings != null ? Mathf.Max(0.1f, settings.escapeTime) : 2.5f;
        float escapeBonus = playerControl != null ? playerControl.GetEscapeTimeBonusSeconds() : 0f;
        return Mathf.Max(0.1f, baseEscapeTime + escapeBonus);
    }
}
