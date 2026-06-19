using Unity.Netcode;
using UnityEngine;

public class MultiServerManager : MonoBehaviour
{
   
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

}
