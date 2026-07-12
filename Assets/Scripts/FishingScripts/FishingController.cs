using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

/* 
Controla o ciclo principal de pesca; logica separada por ficheiros parciais.

Os ficheiros Partial permitem organizar o codigo em blocos tematicos (input, fisgada, UI, etc) 
    sem criar classes adicionais, mantendo tudo relacionado a pesca num unico componente.

*/
public partial class FishingController : MonoBehaviour
{
    // Estados do ciclo de pesca.
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
    [SerializeField] private Transform fishPreviewAnchor;
    [SerializeField] private GameObject bobberPrefab;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerControl playerControl;
    [SerializeField] private LineRenderer fishingLine;

    [Header("Fish Preview")]
    [SerializeField] private float fishPreviewDuration = 2f;

    [Header("Settings")]
    [SerializeField] private FishingSettings settings;

    [Header("UI")]
    [SerializeField] private FishingNotifications notifications;
    [SerializeField] private FishingBiteUI biteUI;
    [SerializeField] private FishingEscapeUI escapeUI;
    [SerializeField] private FishingCatchUI catchUI;

    private PlayerControls input;
    private FishingState state = FishingState.Idle;

    private GameObject currentBobber;
    private Rigidbody currentBobberRigidbody;
    private Coroutine biteRoutine;
    private Coroutine smoothResetRoutine;
    private Coroutine fishPreviewRoutine;
    private bool canHookFish;
    private bool bobberLandedOnWater;
    private bool isResetting;
    private bool lineCached;
    private float lineStartWidth;
    private float lineEndWidth;
    private Color lineStartColor;
    private Color lineEndColor;
    private int currentRequiredClicks;
    private int currentFishClicks;
    private float currentEscapeTimeLimit;
    private float currentEscapeTimeRemaining;
    private readonly Queue<float> reelClickTimes = new Queue<float>();
    private float castStartTime;
    private bool isChargingCast;
    private FishData currentFish;

    public FishData CurrentFish => currentFish;
    public event Action<FishData, int> FishCaught;

    private void Awake()
    {
        // Valida os ScriptableObjects antes de continuar.
        if (!ValidateSettings())
        {
            enabled = false;
            return;
        }

        CachePlayerControl();

        // Liga callbacks do input da pesca.
        input = new PlayerControls();

        input.Fishing.Cast.started += OnCastStarted;
        input.Fishing.Cast.canceled += OnCastCanceled;
        input.Fishing.Reel.performed += OnReelPerformed;
        input.Fishing.Cancel.performed += OnCancel;

        // Guarda a configuracao inicial da linha para restaurar depois.
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

        // Register with InventoryManager if available to avoid runtime FindObjectOfType calls.
        try
        {
            var inv = Fishing.InventoryManager.Instance;
            inv?.RegisterFishingController(this);
        }
        catch { }
    }

    private void OnEnable()
    {
        if (input != null)
        {
            input.Fishing.Enable();
        }
    }

    private void OnDisable()
    {
        if (input != null)
        {
            input.Fishing.Disable();
        }
    }

    private void Update()
    {
        if (state == FishingState.Reeling)
        {
            HandleReelingTimer();
        }

        // Mantem a linha alinhada com a boia.
        if (currentBobber != null && fishingLine != null)
        {
            UpdateFishingLine();
        }
    }

    private void CachePlayerControl()
    {
        if (playerControl == null)
        {
            playerControl = FindFirstObjectByType<PlayerControl>();
        }
    }
}
