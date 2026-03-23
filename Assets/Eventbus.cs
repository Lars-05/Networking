using System;
using UnityEngine;

public static class EventBus
{
    public static event Action<OutgoingServerCommunicator.ClientCommands> SendCommandToServer;
    public static event Action<OutgoingServerCommunicator.ClientValueTypes, int> SendValueToServer;
    public static event Action OnIsTurn;
    public static event Action OnWaitingForPlayerTurn;
    public static event Action OnShowResults;
    public static event Action OnForceGameStop;
    public static event Action OnGameStart;
    public static event Action<int, int> OnRecieveResult;
    public static event Action OnGameStop;

    
    public static void RaiseOnRecieveResult(int ownScore, int enemyScore)
    {
        Debug.Log("fffff");
        OnShowResults?.Invoke();
        OnRecieveResult?.Invoke(ownScore, enemyScore);
      
    }

    public static void RaiseOnGameStop()
    {
        OnGameStop?.Invoke();
    }
    
    public static void RaiseOnGameStart()
    {
        OnGameStart?.Invoke();
    }
    public static void RaiseForceGameStop()
    {
        OnForceGameStop?.Invoke();
    }
    public static void RaiseSendCommandToServer(OutgoingServerCommunicator.ClientCommands command)
    {
        SendCommandToServer?.Invoke(command);
    }
    public static void RaiseSendValueToServer(OutgoingServerCommunicator.ClientValueTypes valueType, int value)
    {
        SendValueToServer?.Invoke(valueType, value);
    }
    
    public static void RaiseOnIsTurn()
    {
        OnIsTurn?.Invoke();
    }
    
    public static void RaiseOnWaitingForPlayerTurn()
    {
        OnWaitingForPlayerTurn?.Invoke();
    }
    



}