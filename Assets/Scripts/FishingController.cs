using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FishingController : MonoBehaviour
{
    private enum FishingState
    {
        Idle,
        Aiming,
        WaitingBite,
        FishHooked,
        Reeling
    }

    [Header("References")]
    [SerializeField] private Transform rodTip;
    [SerializeField] private GameObject bobberPrefab;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LineRenderer fishingLine;

    [Header("Bobber Settings")]
    [SerializeField] private LayerMask Water;
    [SerializeField] private float catchReturnDuration = 0.5f;
    [SerializeField] private float lineFadeDuration = 0.35f;

    [Header("Cast Settings")]
    [SerializeField] private float minCastForce = 0f;
    [SerializeField] private float maxCastForce = 3f;
    [SerializeField] private float maxChargeTime = 1.2f;
    [SerializeField] private float upwardForce = 3f;

    [Header("Fishing Settings")]
    [SerializeField] private float minBiteTime = 2f;
    [SerializeField] private float maxBiteTime = 6f;
    [SerializeField] private float hookWindow = 1.5f;

    [Header("Reeling Settings")]
    [SerializeField] private float fishStamina = 100f;
    [SerializeField] private float reelDamagePerSecond = 25f;
    [SerializeField] private float escapeRecoveryPerSecond = 10f;

    [Header("Reel Minigame")]
    [SerializeField] private float minClicksPerSecond = 2f;
    [SerializeField] private float maxClicksPerSecond = 6f;
    [SerializeField] private float clickWindow = 0.6f;
    [SerializeField] private float minFishDifficulty = 0.2f;
    [SerializeField] private float maxFishDifficulty = 0.9f;
    [SerializeField] private float escapeTime = 2.5f;

    private PlayerControls input;
    private FishingState state = FishingState.Idle;

    private GameObject currentBobber;
    private Rigidbody currentBobberRigidbody;
    private Coroutine biteRoutine;
    private Coroutine smoothResetRoutine;
    private float currentFishStamina;
    private bool canHookFish;
    private bool bobberLandedOnWater;
    private bool isResetting;
    private bool lineCached;
    private float lineStartWidth;
    private float lineEndWidth;
    private Color lineStartColor;
    private Color lineEndColor;
    private float currentRequiredClicksPerSecond;
    private float currentFishDifficulty;
    private float escapeTimer;
    private readonly Queue<float> reelClickTimes = new Queue<float>();
    private float castStartTime;
    private bool isChargingCast;

    private void Awake()
    {
        input = new PlayerControls();

        input.Fishing.Cast.started += OnCastStarted;
        input.Fishing.Cast.canceled += OnCastCanceled;

        input.Fishing.Reel.performed += OnReelPerformed;

        input.Fishing.Cancel.performed += OnCancel;

        if (fishingLine != null)
        {
            lineStartWidth = fishingLine.startWidth;
            lineEndWidth = fishingLine.endWidth;
            lineStartColor = fishingLine.startColor;
            lineEndColor = fishingLine.endColor;
            lineCached = true;
            fishingLine.enabled = false;
            fishingLine.positionCount = 0;
        }
    }

    private void OnEnable()
    {
        input.Fishing.Enable();
    }

    private void OnDisable()
    {
        input.Fishing.Disable();
    }

    private void Update()
    {
        if (state == FishingState.Reeling)
        {
            HandleReeling();
        }

        // Atualiza a linha de pesca se houver boia ativa
        if (currentBobber != null && fishingLine != null)
        {
            UpdateFishingLine();
        }

    }

    private void OnCastStarted(InputAction.CallbackContext context)
    {
        if (state != FishingState.Idle)
            return;

        state = FishingState.Aiming;
        castStartTime = Time.time;
        isChargingCast = true;
        Debug.Log("A preparar lançamento...");
    }

    private void OnCastCanceled(InputAction.CallbackContext context)
    {
        if (state == FishingState.Aiming)
        {
            float holdTime = isChargingCast ? Time.time - castStartTime : 0f;
            float chargeRatio = Mathf.Clamp01(holdTime / Mathf.Max(0.01f, maxChargeTime));
            isChargingCast = false;
            CastLine(chargeRatio);
        }
    }

    private void OnReelPerformed(InputAction.CallbackContext context)
    {
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
        isChargingCast = false;
        ResetFishing();
    }

    private void CastLine(float chargeRatio)
    {
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
        float castForce = Mathf.Lerp(minCastForce, maxCastForce, chargeRatio);
        float scaledUpward = Mathf.Lerp(upwardForce * 0.4f, upwardForce, chargeRatio);
        Vector3 force = direction * castForce + Vector3.up * scaledUpward;

        if (currentBobberRigidbody != null)
        {
            currentBobberRigidbody.AddForce(force, ForceMode.Impulse);
        }

        Debug.Log("Linha lançada.");

        biteRoutine = StartCoroutine(WaitForBite());

        
    }

    private IEnumerator WaitForBite()
    {
        float waitTime = Random.Range(minBiteTime, maxBiteTime);
        yield return new WaitForSeconds(waitTime);

        state = FishingState.FishHooked;
        canHookFish = true;

        Debug.Log("Um peixe mordeu! Clica no botão direito!");

        yield return new WaitForSeconds(hookWindow);

        if (state == FishingState.FishHooked)
        {
            Debug.Log("O peixe fugiu.");
            ResetFishing();
        }
    }

    private void StartReelingFish()
    {
        canHookFish = false;
        state = FishingState.Reeling;
        currentFishStamina = fishStamina;
        reelClickTimes.Clear();
        escapeTimer = 0f;
        currentFishDifficulty = Random.Range(minFishDifficulty, maxFishDifficulty);
        currentRequiredClicksPerSecond = Mathf.Lerp(minClicksPerSecond, maxClicksPerSecond, currentFishDifficulty);

        Debug.Log("Peixe ferrado! Clica rapido para puxar.");
    }

    private void HandleReeling()
    {
        float clickRate = GetCurrentClickRate();
        float pressure = currentRequiredClicksPerSecond > 0f
            ? clickRate / currentRequiredClicksPerSecond
            : 0f;

        pressure = Mathf.Clamp01(pressure);
        currentFishStamina -= reelDamagePerSecond * pressure * Time.deltaTime;
        currentFishStamina += escapeRecoveryPerSecond * (1f - pressure) * Time.deltaTime;
        currentFishStamina = Mathf.Clamp(currentFishStamina, 0f, fishStamina);

        Debug.Log("A puxar peixe: " + currentFishStamina.ToString("F0"));

        if (clickRate < currentRequiredClicksPerSecond)
        {
            escapeTimer += Time.deltaTime;
        }
        else
        {
            escapeTimer = Mathf.Max(0f, escapeTimer - Time.deltaTime);
        }

        if (escapeTimer >= escapeTime)
        {
            Debug.Log("O peixe fugiu.");
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
        Debug.Log("Peixe apanhado!");

        if (smoothResetRoutine != null)
        {
            StopCoroutine(smoothResetRoutine);
        }

        smoothResetRoutine = StartCoroutine(SmoothCatchReset());
    }

    private void ResetFishing()
    {
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
        float duration = Mathf.Max(0.01f, catchReturnDuration);
        float fadeDuration = Mathf.Max(0.01f, lineFadeDuration);

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

    private void RegisterReelClick()
    {
        reelClickTimes.Enqueue(Time.time);
    }

    private float GetCurrentClickRate()
    {
        float now = Time.time;
        float window = Mathf.Max(0.01f, clickWindow);

        while (reelClickTimes.Count > 0 && now - reelClickTimes.Peek() > window)
        {
            reelClickTimes.Dequeue();
        }

        return reelClickTimes.Count / window;
    }

    private void UpdateFishingLine()
    {
        if (fishingLine == null || currentBobber == null)
            return;

        fishingLine.SetPosition(0, rodTip.position);
        fishingLine.SetPosition(1, currentBobber.transform.position);
    }

    private void OnBobberTouched(Collider other)
    {
        if (bobberLandedOnWater)
            return;

        bool hitWater = (Water.value & (1 << other.gameObject.layer)) != 0;

        if (hitWater)
        {
            LandBobber();
        }
        else
        {
            ResetFishing();
        }
    }

    private void LandBobber()
    {
        bobberLandedOnWater = true;

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
    }


    private class BobberContact : MonoBehaviour
    {
        private FishingController controller;
        private bool hasTouched;

        public void Initialize(FishingController controller)
        {
            this.controller = controller;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (hasTouched)
                return;

            hasTouched = true;
            controller.OnBobberTouched(collision.collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTouched)
                return;

            hasTouched = true;
            controller.OnBobberTouched(other);
        }
    }
}