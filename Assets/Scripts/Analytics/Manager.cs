using UnityEngine;

public class Manager : MonoBehaviour
{
    [Header("URL")]
    [SerializeField] private WebRequestManager _web;

   
    void Start()
    {
        
    }

 public void OnButtonEventClickGET(string buttonType)
    { 
       // _web.OnButtonClick(buttonType);     
    }
 
}
