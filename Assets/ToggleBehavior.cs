using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleBehavior : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI buttonText;
    private bool enabled;

    public void OnEnable()
    {
        EventBus.OnGameStart += OnGameStart;
        EventBus.OnGameStop += OnGameStop;
        EventBus.OnForceGameStop += OnGameStop;
    }

    public void OnDisable()
    {
        EventBus.OnGameStart -= OnGameStart;
        EventBus.OnGameStop -= OnGameStop;
        EventBus.OnForceGameStop -= OnGameStop;
    }

    public void OnPressed()
    {
        enabled = !enabled;
        if (enabled)
        {
            buttonText.text = "Not Ready";
            EventBus.RaiseSendCommandToServer(OutgoingServerCommunicator.ClientCommands.IS_READY);
            return;
        }
        buttonText.text = "Ready";
        EventBus.RaiseSendCommandToServer(OutgoingServerCommunicator.ClientCommands.IS_NOT_READY);
    }

    public void OnGameStart()
    {
       gameObject.SetActive(false);
    }

    public void OnGameStop()
    {
       gameObject.SetActive(true);
       buttonText.text = "Not Ready";
       enabled = false;
    }
    
    
}
