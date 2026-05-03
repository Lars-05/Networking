using System;
using UnityEngine;

public static class EventBus
{
    public static event Action<OutgoingServerCommunicator.ClientCommands> SendCommandToServer;
  
    public static event Action OnIsTurn;

    public static event Action OnTurnOver;
    public static event Action OnWaitingForPlayerTurn;
    public static event Action OnShowResults;
    public static event Action OnForceGameStop;
    
    public static event Action OnHit;
    public static event Action OnStand;
    public static event Action OnGameStart;
    public static event Action<int, int> OnRecieveResult;
    public static event Action OnGameStop;

    
    public static void RaiseOnRecieveResult(int ownScore, int enemyScore)
    {
        OnShowResults?.Invoke();
        OnRecieveResult?.Invoke(ownScore, enemyScore);
    }

    public static void RaiseOnGameStop()
    {
        OnGameStop?.Invoke();
    }
    
    public static void RaiseOnHit()
    {
        OnHit?.Invoke();
    }

    
    public static void RaiseOnStand()
    {
        OnStand?.Invoke();
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
    
    public static void RaiseOnIsTurn()
    {
        OnIsTurn?.Invoke();
    }
    
    public static void RaiseOnTurnOver()
    {
        OnTurnOver?.Invoke();
    }
    
    public static void RaiseOnWaitingForPlayerTurn()
    {
        OnWaitingForPlayerTurn?.Invoke();
    }
    



}