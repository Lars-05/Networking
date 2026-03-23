using UnityEngine;

public class IncomingServerCommunicator : MonoBehaviour
{
    public void HandleIncomingMessage(string message)
    {
        string lastString = message[message.Length - 1].ToString();

        if (!int.TryParse(lastString, out int result))
        {
            HandleIncomingCommand(message);
            return;
        }

        HandleIncomingValue(message);
    }

    void HandleIncomingCommand(string message)
    {
        switch (message)
        {
            case "IS_TURN":
                EventBus.RaiseOnIsTurn();
                break;
            
            case "GAME_START":
                EventBus.RaiseOnGameStart();
                break;
            
            case "WAITING_FOR_PLAYER_TURN":
                EventBus.RaiseOnWaitingForPlayerTurn();
                break;
            
            case "SHOW_RESULTS":
                break;
            
            case "FORCE_GAME_STOP":
                EventBus.RaiseForceGameStop();
            break;
            
            case "GAME_STOP":
                EventBus.RaiseOnGameStop();
                break;
            
            default:
                Debug.Log("Unaccounted for message: " + message);
                break;

        }
    }
    
    void HandleIncomingValue(string message)
    {
        string[] messageParts = message.Split('_');
        switch (messageParts[0])
        {
            case "ID":
                int value = int.Parse(messageParts[1]);
                PlayerData.ID = value;
                break;
            
            case "SCORE":
                string[] scores = messageParts[1].Split('/');
                EventBus.RaiseOnRecieveResult(int.Parse(scores[0]), int.Parse(scores[1]));
                break;
            
            default:
                Debug.Log("Unaccounted for value type " + messageParts[0]);
                break;
        }
    }
}
