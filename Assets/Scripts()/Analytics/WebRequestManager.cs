using System.Collections;
using System.Net;
using System.Threading;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;

public class WebRequestManager : MonoBehaviour
{

  [SerializeField]  private string _getpath = "http://localhost:8080";
  [SerializeField]  private string _geturl = "/getData";
  [SerializeField] private string _posturl = "/postData";
  [SerializeField] private string _dataurl = "/getDataDB";

    void Start()
    {
        
    }
    public void OnButtonClick(string buttonName)
    {

        if(buttonName == "get")
        {
            StartCoroutine(GetRequest(_getpath + _geturl));
        }
        else if(buttonName == "post")
        {
            StartCoroutine(PostRequest());
        }
        else if (buttonName == "DB")
        {
            StartCoroutine(GetRequest(_getpath + _dataurl));
        }
    }
    private IEnumerator GetRequest(string url)
    {
       UnityWebRequest _webRequest = UnityWebRequest.Get(url);

        _webRequest.timeout = 5;

        yield return _webRequest.SendWebRequest();

        if (validateResponse(_webRequest))
        {
            processingDebug(_webRequest);

            ProcessingRequestGet(_webRequest);
        }
        
    }
    private IEnumerator PostRequest()
    {
        PlayerData.PlayerDataInfo playerDataInfo = new PlayerData.PlayerDataInfo()
        {
            name = "Daniel",
            name_fish = "betta",
            fish_rarity = "common",
        };

       string PlayerInfoJson = PlayerData.CreateJsonFromClass(playerDataInfo);
     
       UnityWebRequest webRequest = UnityWebRequest.Post(_getpath + _posturl,PlayerInfoJson,"application/json");

        webRequest.timeout = 5;

        yield return webRequest.SendWebRequest();

        if (validateResponse(webRequest))
        {

            processingDebug(webRequest);
  
            ProcessingRequestPost(webRequest);

        }

    }

    private bool validateResponse(UnityWebRequest webRequest)
    {

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result ==UnityWebRequest.Result.DataProcessingError)
        {

            Debug.LogError($"Received: ${webRequest.error}");

            return false;

        }

        return true;

    }

    private void processingDebug(UnityWebRequest webRequest)
    {
        Debug.Log($"Received: {webRequest.downloadHandler.text}");
    }

    private void ProcessingRequestGet(UnityWebRequest webRequest)
    {
        string responseText = webRequest.downloadHandler.text;

        PlayerData.PlayerDataInfoToArray playerDataToArray = PlayerData.CreateClassFromJson(responseText);

        for (int i = 0; i < playerDataToArray._playerDataInfoArray.Length; i++)
        {
            PlayerData.PlayerDataInfo playeriInfo = playerDataToArray._playerDataInfoArray[i];

            Debug.Log($"Player Name: {playeriInfo.name}," +
                $"Player Lives: {playeriInfo.name_fish}," +
                $"Player Health: {playeriInfo.fish_rarity}");

        }

        /*PlayerData.PlayerDataInfo playerDataInfo = PlayerData.CreateClassFromJson(responseText);

        Debug.Log( $"Player Name: {playerDataInfo.name}," +
            $"Player Lives: {playerDataInfo.lives}," +\z
            $"Player Health: {playerDataInfo.health}");*/
    }
    private void ProcessingRequestPost(UnityWebRequest webRequest)
    {
       
    }

}
