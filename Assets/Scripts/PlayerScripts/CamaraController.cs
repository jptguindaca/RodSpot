using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

// Controla o zoom e evita oclusao com Cinemachine.
public class CamaraController : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomLerpSpeed = 10f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 3f;

    [Header("Camera Collision")]
    [SerializeField] private LayerMask collisionLayers = ~0;
    [SerializeField] private string ignoreTag = "";
    [SerializeField] private float cameraRadius = 0.3f;
    [SerializeField] private float minDistanceFromTarget = 0.3f;
    [SerializeField] private float returnDamping = 0.4f;
    [SerializeField] private float occludedDamping = 0.2f;
    [SerializeField] private float smoothingTime = 0.05f;

    private PlayerControls controls;
    private CinemachineCamera cam;
    private CinemachineOrbitalFollow orbital;
    private Vector2 scrollDelta;
    private float targetZoom;
    private float currentZoom;

    void Start()
    {
        // Liga o input e guarda referencias do Cinemachine.
        controls = new PlayerControls();
        controls.Enable();
        controls.CameraControls.MouseZoom.performed += HandleMouseScroll;

        cam = GetComponent<CinemachineCamera>();
        orbital = cam.GetComponent<CinemachineOrbitalFollow>();

        targetZoom = currentZoom = orbital.Radius;

        // Garante o componente de desoclusao e configura-o.
        SetupDeoccluder();
    }

    void Update()
    {
        // Atualiza o alvo do zoom pelo scroll e suaviza o raio.
        if (scrollDelta.y != 0)
        {
            if(orbital != null)
            {
                targetZoom = Mathf.Clamp(orbital.Radius - scrollDelta.y * zoomSpeed, minDistance, maxDistance);
                scrollDelta = Vector2.zero;
            }
        }

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomLerpSpeed);
        orbital.Radius = currentZoom;
    }
    
    private void HandleMouseScroll(InputAction.CallbackContext context)
    {
        // Guarda o delta do scroll para o proximo Update.
        scrollDelta = context.ReadValue<Vector2>();
    }

    private void SetupDeoccluder()
    {
        // Adiciona e configura o deoccluder para evitar colisoes.
        if (cam == null)
            return;

        CinemachineDeoccluder deoccluder = GetComponent<CinemachineDeoccluder>();
        if (deoccluder == null)
        {
            deoccluder = gameObject.AddComponent<CinemachineDeoccluder>();
        }

        deoccluder.CollideAgainst = collisionLayers;
        deoccluder.IgnoreTag = ignoreTag;
        deoccluder.MinimumDistanceFromTarget = Mathf.Max(0.01f, minDistanceFromTarget);

        CinemachineDeoccluder.ObstacleAvoidance avoid = deoccluder.AvoidObstacles;
        avoid.Enabled = true;
        avoid.CameraRadius = Mathf.Max(0f, cameraRadius);
        avoid.Damping = Mathf.Max(0f, returnDamping);
        avoid.DampingWhenOccluded = Mathf.Max(0f, occludedDamping);
        avoid.SmoothingTime = Mathf.Max(0f, smoothingTime);
        avoid.Strategy = CinemachineDeoccluder.ObstacleAvoidance.ResolutionStrategy.PullCameraForward;
        deoccluder.AvoidObstacles = avoid;
    }

    public void StopCamera()
    {
        cam.enabled = false;
    }

    public void StartCamera()
    {
        cam.enabled = true;
    }
}
