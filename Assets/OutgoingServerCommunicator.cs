using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class OutgoingServerCommunicator : MonoBehaviour
{
    [SerializeField] private TextClient client;
    public enum ClientCommands
    {
        IS_NOT_READY,
        IS_READY,
        TURN_DONE
    }
    
    public enum ClientValueTypes
    {
        SCORE
    }

    public void OnEnable()
    {
        EventBus.SendCommandToServer += PassCommand;
        EventBus.SendValueToServer += PassValue;
    }
    
    public void OnDisable()
    {
        EventBus.SendCommandToServer -= PassCommand;
        EventBus.SendValueToServer -= PassValue;
        
    }
    public void PassValue(ClientValueTypes valueType, int value)
    {
        if(client == null) return;
        string finalMessage = string.Empty; 
        switch (valueType)
        {
            case ClientValueTypes.SCORE:
                finalMessage = "SCORE_" + value;
                break;
        }
        client.SendMessageToClient(finalMessage);
    }
    
    public void PassCommand(ClientCommands command)
    {
        if(client == null) return;
        string finalMessage = string.Empty; 
        switch (command)
        {
            case ClientCommands.IS_READY:
                finalMessage = "IS_READY";
                break;
            
            case ClientCommands.TURN_DONE:
                finalMessage = "TURN_DONE";
                break;
            
            case ClientCommands.IS_NOT_READY:
                finalMessage = "IS_NOT_READY";
                break;
        }
        client.SendMessageToClient(finalMessage);
    }
    
}
