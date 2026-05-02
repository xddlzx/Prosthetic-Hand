using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class UnityCommandReceiver : MonoBehaviour
{
    [Header("References")]
    public ProstheticHandBoneController handController;

    [Header("Backend")]
    public string backendUrl = "http://127.0.0.1:8000/latest-command";

    [Header("Polling")]
    public float pollEverySeconds = 0.05f;

    private int lastCommandId = -1;
    private bool isRequestRunning = false;

    private void Start()
    {
        Application.runInBackground = true;

        if (handController == null)
        {
            handController = GetComponent<ProstheticHandBoneController>();
        }

        if (handController == null)
        {
            Debug.LogError("UnityCommandReceiver cannot find ProstheticHandBoneController.");
            enabled = false;
            return;
        }

        StartCoroutine(PollBackendLoop());
    }

    private IEnumerator PollBackendLoop()
    {
        while (true)
        {
            if (!isRequestRunning)
            {
                StartCoroutine(GetLatestCommand());
            }

            yield return new WaitForSeconds(pollEverySeconds);
        }
    }

    private IEnumerator GetLatestCommand()
    {
        isRequestRunning = true;

        string urlWithCacheBreaker = backendUrl + "?t=" + Time.realtimeSinceStartup;

        using (UnityWebRequest request = UnityWebRequest.Get(urlWithCacheBreaker))
        {
            request.timeout = 1;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Could not connect to backend: " + request.error);
                isRequestRunning = false;
                yield break;
            }

            string json = request.downloadHandler.text;

            WebHandCommand command = JsonUtility.FromJson<WebHandCommand>(json);

            if (command == null)
            {
                Debug.LogWarning("Could not understand backend JSON: " + json);
                isRequestRunning = false;
                yield break;
            }

            if (command.id != lastCommandId)
            {
                lastCommandId = command.id;

                Debug.Log("Received command id: " + command.id);
                handController.ApplyJsonCommand(json);
            }
        }

        isRequestRunning = false;
    }
}