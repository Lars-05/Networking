using TMPro;
using UnityEngine;

public class ServerDebugPanel : MonoBehaviour
{
    [SerializeField] private
    IncomingServerCommunicator ingoingCommunicator;

    [SerializeField] private
        OutgoingServerCommunicator outgoingCommunicator;
    [SerializeField]
    TextMeshProUGUI fps;
    [SerializeField]
    TextMeshProUGUI latestReceivedMessage;
    [SerializeField]
    TextMeshProUGUI latestSendMessage;
    [SerializeField]
    TextMeshProUGUI playerID;
    [SerializeField]
    TextMeshProUGUI connectionStatus;
    [SerializeField]
    TextMeshProUGUI timeElapsed;

    [SerializeField] private TextClient client; 

    private float timeElapsedTime;
    // Update is called once per frame
    void Update()
    {
        timeElapsedTime += Time.deltaTime;
        fps.text = "FPS: " + 1.0/Time.deltaTime;
        timeElapsed.text = "Time Elapsed: " + timeElapsedTime;
        string playerIdText = PlayerData.idSet? PlayerData.id.ToString() : "Unknown";
        playerID.text = "Player ID: "+ playerIdText;
        if (ingoingCommunicator.lastIncomingMessage != null)
        {
            latestReceivedMessage.text = "Last Recieved Message: " + ingoingCommunicator.lastIncomingMessage;
        }
        if (outgoingCommunicator.lastOutgoingMessage != null)
        {
            latestSendMessage.text = "Last Send Message: " + outgoingCommunicator.lastOutgoingMessage;
        }
        connectionStatus.text = "Connection Status: " + client.serverConnectionStatus;
    }   
}
