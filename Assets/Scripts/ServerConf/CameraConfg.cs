using Unity.Netcode;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1.6f, 0f);

    void LateUpdate()
    {
        if (target == null)
        {
            FindLocalPlayer();
            return;
        }

        transform.position = target.position + offset;
    }

    void FindLocalPlayer()
    {
        foreach (var player in FindObjectsOfType<PlayerControl>())
        {
            if (player.IsOwner)
            {
                target = player.transform;
                break;
            }
        }
    }
}