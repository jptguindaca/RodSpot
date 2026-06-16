using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    // The active camera transform for this client (usually the local player's camera).
    public Transform camTransform;

    [SerializeField]
    private Vector3 cameraOffset = new Vector3(0f, 1.6f, 0f);
    [SerializeField]
    private GameObject cameraPrefab;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Keep existing MainCamera as fallback if nothing else sets camTransform.
        if (camTransform == null && Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
    }

    public Transform CreateCameraForPlayer(Transform playerTransform)
    {
        if (playerTransform == null) return null;

        // If already assigned and targeting the same player, return it.
        if (camTransform != null)
        {
            var existingFollow = camTransform.GetComponent<CameraFollow>();
            if (existingFollow != null && existingFollow.target == playerTransform)
                return camTransform;
        }

        // Create a new camera GameObject or instantiate the configured prefab.
        GameObject camGO;
        Camera cam = null;
        if (cameraPrefab != null)
        {
            camGO = Instantiate(cameraPrefab);
            cam = camGO.GetComponent<Camera>();
        }
        else
        {
            camGO = new GameObject("PlayerCamera");
            cam = camGO.AddComponent<Camera>();
        }

        // Optional: set as the main camera for convenience (this assumes the "MainCamera" tag exists in the project).
        // Try to tag as MainCamera for convenience.
        try { camGO.tag = "MainCamera"; } catch { }

        // Position and parent for clarity.
        camGO.transform.position = playerTransform.position + cameraOffset;
        camGO.transform.rotation = Quaternion.identity;

        // Attach or configure follow behaviour and set target.
        var follow = camGO.GetComponent<CameraFollow>();
        if (follow == null) follow = camGO.AddComponent<CameraFollow>();
        follow.target = playerTransform;
        follow.offset = cameraOffset;

        camTransform = camGO.transform;
        return camTransform;
    }

   
    public void SetCameraPrefab(GameObject prefab)
    {
        cameraPrefab = prefab;
    }

    public void ReplaceWithPrefabForPlayer(Transform playerTransform)
    {
        if (cameraPrefab == null || playerTransform == null) return;

        // Destroy existing camera GameObject if it exists
        if (camTransform != null)
        {
            Destroy(camTransform.gameObject);
            camTransform = null;
        }

        var camGO = Instantiate(cameraPrefab);
        var follow = camGO.GetComponent<CameraFollow>();
        if (follow == null) follow = camGO.AddComponent<CameraFollow>();
        follow.target = playerTransform;
        follow.offset = cameraOffset;

        camTransform = camGO.transform;
    }
}
