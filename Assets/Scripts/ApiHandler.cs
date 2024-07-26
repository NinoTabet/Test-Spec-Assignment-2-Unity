using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ApiHandler : MonoBehaviour
{
    private const string baseUrl = "http://localhost:3000";
    private int points = 10;

    public void SendPoints()
    {
        StartCoroutine(PostPoints(points));
    }

    private IEnumerator PostPoints(int points)
    {
        string url = $"{baseUrl}/points";

        // creates the JSON payload
        string jsonPayload = JsonUtility.ToJson(new PointsData(points));

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || 
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error sending points: " + www.error);
            }
            else
            {
                Debug.Log("Points successfully sent: " + www.downloadHandler.text);
            }
        }
    }

    // helper class for JSON serialization
    [System.Serializable]
    private class PointsData
    {
        public int points;

        public PointsData(int points)
        {
            this.points = points;
        }
    }
}
