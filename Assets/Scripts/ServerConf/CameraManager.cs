using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    public Transform camTransform;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        camTransform = Camera.main.transform;
    }
}