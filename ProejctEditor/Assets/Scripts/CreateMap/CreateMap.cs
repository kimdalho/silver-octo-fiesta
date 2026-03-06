using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ResultData<T>
{
   public bool success;
   public  T data;
   public string log;
}

public class CreateMap : MonoBehaviour
{
    public int blueprintId;
    string access_token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoxLCJyb2xlX3R5cGUiOjMsImlhdCI6MTc2ODc4MTAzMSwiZXhwIjoxODAwMzE3MDMxfQ.4YAlDT9zv91AyEaefLDUqgKjnkC3PyyjDAxblGbMTBI";

    private void Start()
    {
        blueprintId = 6629;
        StartCoroutine(LoadBlueprintData(blueprintId));
    }

    public IEnumerator LoadBlueprintData(int blueprint_id)
    {
        string url = $"/v1/blueprints/{blueprint_id}/objects?designTemplateId={7}";
        ResultData<string> result = new ResultData<string>();
        yield return RequestMap(url, result);

        Debug.Log(result);


    }

    public IEnumerator RequestMap(string _url, ResultData<string> result)
    {
        var baseUrl = "https://ggumindev.me";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(baseUrl + _url))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", "Bearer " + access_token);

            yield return null;
            yield return webRequest.SendWebRequest();

            
            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {                
                result.success = false;
                result.data = null;
                result.log = webRequest.result.ToString();
            }
            else
            {
                string responseText = webRequest.downloadHandler.text;
                result.success = true;
                result.data = responseText;
                result.log = webRequest.result.ToString();                
            }


        }

        yield return null;
    }
}
