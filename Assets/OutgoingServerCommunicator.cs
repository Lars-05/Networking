using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class OutgoingServerCommunicator : MonoBehaviour
{
    [SerializeField] private TextClient client;
    public string lastOutgoingMessage;
    public enum ClientCommands
    {
        IS_NOT_READY,
        IS_READY,
        STAND,
        HIT
    }
    
 
    public void OnEnable()
    {
        EventBus.SendCommandToServer += PassCommand;
    }
    
    public void OnDisable()
    {
        EventBus.SendCommandToServer -= PassCommand;
        
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
            
            case ClientCommands.STAND:
                finalMessage = "STAND";
                break;
            
            case ClientCommands.HIT:
                finalMessage = "HIT";
                break;
            
            case ClientCommands.IS_NOT_READY:
                finalMessage = "IS_NOT_READY";
                break;
        }
        SendMessageToClient(finalMessage);
    }
    
    public void SendMessageToClient(string message)
    {
        if(client.writer == null || client == null) return;
        client.writer.WriteLine(message);
        lastOutgoingMessage = message;
    }
    
}
