using System.Collections;
using UnityEngine;

public partial class FishingController
{
    //chama O WEBrequestManager para guardar o peixe apanhado e guardar numa base de dados

    public WebRequestManager BD;

    private void StartReelingFish()
    {
        // Inicia o minijogo de recolha e stamina.
        canHookFish = false;
        state = FishingState.Reeling;
        HideBiteBar();
        ShowEscapeBar();
        if (currentFish == null)
        {
            SelectCurrentFish();
        }

        float difficultyMultiplier = currentFish != null ? Mathf.Max(0.1f, currentFish.difficultyMultiplier) : 1f;
        float staminaMultiplier = currentFish != null ? Mathf.Max(0.1f, currentFish.staminaMultiplier) : 1f;
        currentFishStaminaMax = settings.fishStamina * staminaMultiplier;
        currentFishStamina = currentFishStaminaMax;
        reelClickTimes.Clear();
        escapeTimer = 0f;
        currentFishDifficulty = Random.Range(settings.minFishDifficulty, settings.maxFishDifficulty);
        currentRequiredClicksPerSecond = Mathf.Lerp(settings.minClicksPerSecond, settings.maxClicksPerSecond, currentFishDifficulty);
        currentRequiredClicksPerSecond *= difficultyMultiplier;

        notifications?.ShowFishHooked();
    }

    private void HandleReeling()
    {
        // Atualiza stamina, escape e UI durante a recolha.
        float clickRate = GetCurrentClickRate();
        float pressure = currentRequiredClicksPerSecond > 0f
            ? clickRate / currentRequiredClicksPerSecond
            : 0f;

        pressure = Mathf.Clamp01(pressure);
        currentFishStamina -= settings.reelDamagePerSecond * pressure * Time.deltaTime;
        currentFishStamina += settings.escapeRecoveryPerSecond * (1f - pressure) * Time.deltaTime;
        float staminaLimit = currentFishStaminaMax > 0f ? currentFishStaminaMax : settings.fishStamina;
        currentFishStamina = Mathf.Clamp(currentFishStamina, 0f, staminaLimit);

        UpdateEscapeBar();

        Debug.Log("A puxar peixe: " + currentFishStamina.ToString("F0"));

        if (clickRate < currentRequiredClicksPerSecond)
        {
            escapeTimer += Time.deltaTime;
        }
        else
        {
            escapeTimer = Mathf.Max(0f, escapeTimer - Time.deltaTime);
        }

        if (escapeTimer >= settings.escapeTime)
        {
            notifications?.ShowFishEscaped();
            ResetFishing();
            return;
        }

        if (currentFishStamina <= 0f)
        {
            CatchFish();
        }
    }

    private void CatchFish()
    {
        // Finaliza a captura e inicia o reset suave.
        notifications?.ShowFishCaught();
        int catchValue = currentFish != null ? currentFish.GetRandomValue() : 0;
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
        // Regista o clique para calcular taxa de cliques
        reelClickTimes.Enqueue(Time.time);
    }

    private float GetCurrentClickRate()
    {
        // Calcula cliques por segundo dentro da janela.
        float now = Time.time;
        float window = Mathf.Max(0.01f, settings.clickWindow);

        while (reelClickTimes.Count > 0 && now - reelClickTimes.Peek() > window)
        {
            reelClickTimes.Dequeue();
        }

        return reelClickTimes.Count / window;
    }

    private void ShowEscapeBar()
    {
        // Mostra a barra de stamina/escape
        if (escapeUI != null)
        {
            escapeUI.Show();
        }
    }

    private void HideEscapeBar()
    {
        // Esconde a barra de stamina/escape
        if (escapeUI != null)
        {
            escapeUI.Hide();
        }
    }

    private void UpdateEscapeBar()
    {
        // Atualiza a barra de stamina com o valor atual
        if (escapeUI == null)
        {
            return;
        }

        float safeFishStamina = Mathf.Max(0.01f, currentFishStaminaMax > 0f ? currentFishStaminaMax : settings.fishStamina);
        escapeUI.SetProgress(currentFishStamina / safeFishStamina);
    }
}
