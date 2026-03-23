using TMPro;
using UnityEngine;

public class ServerDebugPanel : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI fps;
    [SerializeField]
    TextMeshProUGUI latestServerMessage;
    [SerializeField]
    TextMeshProUGUI playerID;
    [SerializeField]
    TextMeshProUGUI connectionStatus;
    [SerializeField]
    TextMeshProUGUI timeElapsed;

    private float timeElapsedTime;
    // Update is called once per frame
    void Update()
    {
        timeElapsedTime += Time.deltaTime;
        fps.text = "FPS: " + 1.0/Time.deltaTime;
        timeElapsed.text = "Time Elapsed: " + timeElapsedTime;
    }   
}
