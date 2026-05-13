using System.Collections;
using UnityEngine;

public partial class FishingController
{
    private void ResetFishing()
    {
        // Para corrotinas e limpa o estado atual.
        if (smoothResetRoutine != null && !isResetting)
        {
            StopCoroutine(smoothResetRoutine);
            smoothResetRoutine = null;
        }

        if (biteRoutine != null)
        {
            StopCoroutine(biteRoutine);
            biteRoutine = null;
        }

        CleanupFishing();
    }

    private IEnumerator SmoothCatchReset()
    {
        // Recolhe a boia e faz fade da linha.
        isResetting = true;
        canHookFish = false;
        state = FishingState.Idle;

        if (biteRoutine != null)
        {
            StopCoroutine(biteRoutine);
            biteRoutine = null;
        }

        if (currentBobber == null)
        {
            CleanupFishing();
            isResetting = false;
            smoothResetRoutine = null;
            yield break;
        }

        if (currentBobberRigidbody != null)
        {
            if (!currentBobberRigidbody.isKinematic)
            {
                currentBobberRigidbody.linearVelocity = Vector3.zero;
                currentBobberRigidbody.angularVelocity = Vector3.zero;
            }
            currentBobberRigidbody.useGravity = false;
            currentBobberRigidbody.isKinematic = true;
        }

        Vector3 startPos = currentBobber.transform.position;
        float duration = Mathf.Max(0.01f, settings.catchReturnDuration);
        float fadeDuration = Mathf.Max(0.01f, settings.lineFadeDuration);

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float normalized = t / duration;
            float eased = normalized * normalized * (3f - 2f * normalized);
            currentBobber.transform.position = Vector3.Lerp(startPos, rodTip.position, eased);

            if (fishingLine != null && lineCached)
            {
                float fadeT = Mathf.Clamp01(t / fadeDuration);
                fishingLine.startWidth = Mathf.Lerp(lineStartWidth, 0f, fadeT);
                fishingLine.endWidth = Mathf.Lerp(lineEndWidth, 0f, fadeT);

                Color startColor = lineStartColor;
                Color endColor = lineEndColor;
                startColor.a = Mathf.Lerp(lineStartColor.a, 0f, fadeT);
                endColor.a = Mathf.Lerp(lineEndColor.a, 0f, fadeT);
                fishingLine.startColor = startColor;
                fishingLine.endColor = endColor;

                UpdateFishingLine();
            }

            yield return null;
        }

        CleanupFishing();
        isResetting = false;
        smoothResetRoutine = null;
    }

    private void CleanupFishing()
    {
        // Limpa objetos temporarios e volta ao estado idle.
        HideBiteBar();
        HideEscapeBar();

        if (currentBobber != null)
        {
            Destroy(currentBobber);
            currentBobber = null;
        }

        currentBobberRigidbody = null;
        bobberLandedOnWater = false;

        if (fishingLine != null)
        {
            fishingLine.enabled = false;
        }

        state = FishingState.Idle;
        canHookFish = false;
        reelClickTimes.Clear();
        escapeTimer = 0f;

        Debug.Log("Sistema de pesca reiniciado.");
    }
}
