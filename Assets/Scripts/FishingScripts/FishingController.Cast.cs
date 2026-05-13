using UnityEngine;

public partial class FishingController
{
    private void CastLine(float chargeRatio)
    {
        // Instancia a boia, prepara a linha e aplica a forca do lance.
        state = FishingState.WaitingBite;

        currentBobber = Instantiate(bobberPrefab, rodTip.position, Quaternion.identity);
        currentBobberRigidbody = currentBobber.GetComponent<Rigidbody>();
        bobberLandedOnWater = false;

        BobberContact contact = currentBobber.AddComponent<BobberContact>();
        contact.Initialize(this);

        if (fishingLine != null)
        {
            if (lineCached)
            {
                fishingLine.startWidth = lineStartWidth;
                fishingLine.endWidth = lineEndWidth;
                fishingLine.startColor = lineStartColor;
                fishingLine.endColor = lineEndColor;
            }
            fishingLine.positionCount = 2;
            fishingLine.useWorldSpace = true;
            fishingLine.enabled = true;
            UpdateFishingLine();
        }

        Vector3 direction = playerCamera.transform.forward;
        float castForce = Mathf.Lerp(settings.minCastForce, settings.maxCastForce, chargeRatio);
        float scaledUpward = Mathf.Lerp(settings.upwardForce * 0.4f, settings.upwardForce, chargeRatio);
        Vector3 force = direction * castForce + Vector3.up * scaledUpward;

        if (currentBobberRigidbody != null)
        {
            currentBobberRigidbody.AddForce(force, ForceMode.Impulse);
        }

        notifications?.ShowLineCast();

        biteRoutine = StartCoroutine(WaitForBite());
    }

    private void UpdateFishingLine()
    {
        // Mantem a linha entre a ponta da cana e a boia.
        if (fishingLine == null || currentBobber == null)
            return;

        fishingLine.SetPosition(0, rodTip.position);
        fishingLine.SetPosition(1, currentBobber.transform.position);
    }
}
