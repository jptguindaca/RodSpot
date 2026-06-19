using UnityEngine;

public class MainMenuController : MonoBehaviour
{
   public void StartGame()
    {
        GameManager.Instance.ChangeScene("game");
    }
}
