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
    [SerializeField] private GameObject bobberPrefab;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LineRenderer fishingLine;

    [Header("Settings")]
    [SerializeField] private FishingSettings settings;

    [Header("UI")]
    [SerializeField] private FishingNotifications notifications;
    [SerializeField] private FishingBiteUI biteUI;
    [SerializeField] private FishingEscapeUI escapeUI;

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
        // Valida os ScriptableObjects antes de continuar.
        if (!ValidateSettings())
        {
            enabled = false;
            return;
        }

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
        // Atualiza a logica de recolha enquanto o peixe esta ferrado.
        if (state == FishingState.Reeling)
        {
            HandleReeling();
        }

        // Mantem a linha alinhada com a boia.
        if (currentBobber != null && fishingLine != null)
        {
            UpdateFishingLine();
        }
    }
}
