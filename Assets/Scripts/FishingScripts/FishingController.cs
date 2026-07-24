using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

/* 
Controla o ciclo principal de pesca; logica separada por ficheiros parciais.

Os ficheiros Partial permitem organizar o codigo em blocos tematicos (input, fisgada, UI, etc) 
    sem criar classes adicionais, mantendo tudo relacionado a pesca num unico componente.

*/
public partial class FishingController : NetworkBehaviour
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

    public event Action<FishData, int> FishCaught;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log($"FishingController Spawned | Owner:{IsOwner} Client:{OwnerClientId}");

        if (!IsOwner)
        {
            input?.Fishing.Disable();
            return;
        }

        SetupLocalFishingReferences();
        CacheFishingLine();

        input?.Fishing.Enable();
    }
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
        if (IsOwner && input != null)
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
    private void SetupLocalFishingReferences()
    {
        if (!IsOwner)
            return;


        PlayerControl localPlayer = FindObjectsByType<PlayerControl>(FindObjectsSortMode.None).FirstOrDefault(player => player.IsOwner);

        if (localPlayer == null)
        {
            Debug.LogError("Player local não encontrado!");
            return;
        }


        Transform fishingRod = null;

        foreach (Transform child in localPlayer.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Contains("FishingRod"))
            {
                fishingRod = child;
                break;
            }
        }

        if (fishingRod == null)
        {
            Debug.LogError("FishingRod não encontrada!");
            return;
        }


        if (rodTip == null)
        {
            foreach (Transform child in fishingRod.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "RodTip")
                {
                    rodTip = child;
                    break;
                }
            }
        }


        if (fishingLine == null)
        {
            fishingLine = fishingRod.GetComponentInChildren<LineRenderer>(true);
        }

        // Câmara local
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }
        private void CacheFishingLine()

        {

            if (fishingLine == null)
                return;

             lineStartWidth = fishingLine.startWidth;
             lineEndWidth = fishingLine.endWidth;
             lineStartColor = fishingLine.startColor;
             lineEndColor = fishingLine.endColor;

             lineCached = true;

             fishingLine.enabled = false;
             fishingLine.positionCount = 0;
         }
    
}
   

