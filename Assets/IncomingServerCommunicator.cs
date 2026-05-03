using UnityEngine;

public class IncomingServerCommunicator : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;
    public string lastIncomingMessage;
    public void HandleIncomingMessage(string message)
    {
        lastIncomingMessage = message;
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
            
            case "TURN_OVER":
                EventBus.RaiseOnTurnOver();
                break;
            
            case "FORCE_GAME_STOP":
                playerManager.ResetAllPlayers();
            break;
            
            case "SHOW_RESULTS":
                playerManager.ShowAllPlayerScores();
                break;
            
            case "STOP_GAME":
                playerManager.ResetAllPlayers();
                break;
            
            default:
                Debug.Log("Unaccounted for message: " + message);
                break;

        }
    }
    
    void HandleIncomingValue(string message)
    {
        //ID_2
        int value = 0;
        string[] messageParts = message.Split('_');
        switch (messageParts[0])
        {
            case "THISID":
                // example format = THISID_2
                value = int.Parse(messageParts[1]);
                PlayerData.SetPlayerID(value);
                playerManager.SetupThisPlayer();
                break;
            
            case "ENEMYID":
                // example format = ENEMYID_2
                value = int.Parse(messageParts[1]);

                if (playerManager.playerActionDisplayers.ContainsKey(value) || value == PlayerData.id)
                {
                    return;
                }
                playerManager.AddPlayer(value);
                break;
            
            case "SCORE":
                // example format = SCORE_1_14
                playerManager.AddPlayerScore(int.Parse(messageParts[1]), int.Parse(messageParts[2]));
                break;
            
            case "CARD":
                // example format = ENEMY_1_5
                int id = int.Parse(messageParts[1]);
                int cardValue = int.Parse(messageParts[2]);
                playerManager.AddCardToPlayerDeck(id, cardValue);
                break;
            
            default:
                Debug.Log("Unaccounted for value type " + messageParts[0]);
                break;
        }
    }
}
