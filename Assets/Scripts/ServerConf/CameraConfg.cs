using Unity.Netcode;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target == null)
        {
            FindLocalPlayer();
            return;
        }

        transform.position = target.position;
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