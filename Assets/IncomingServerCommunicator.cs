using UnityEngine;

public class IncomingServerCommunicator : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;

    public string lastIncomingMessage;

    public void HandleIncomingMessage(string message)
    {
        lastIncomingMessage = message;

        string[] parts = message.Split('_');

        if (parts.Length > 1 && int.TryParse(parts[^1], out _))
        {
            HandleIncomingValue(parts);
        }
        else
        {
            HandleIncomingCommand(message);
        }
    }

    void HandleIncomingCommand(string message)
    {
        switch (message)
        {
            case "FORCE_GAME_STOP":
                playerManager.ResetAllPlayers();
            break;
            
            case "SHOW_RESULTS":
                playerManager.ShowAllPlayerScores();
                break;
            
            case "GAME_STOP":
                playerManager.ResetAllPlayers();
                break;
            
            case "GAME_START":
                playerManager.OnGameStart();
                break;
            
            default:
                Debug.Log("Unaccounted for message: " + message);
                break;

        }
    }
    
    void HandleIncomingValue(string[] message)
    {
        int id = 0;
        int value = 0;
     
        switch ( message[0])
        {
            case "THISID":
                // example format = THISID_2
                value = int.Parse( message[1]);
                PlayerData.SetPlayerID(value);
                playerManager.SetupThisPlayer();
                break;
            
            case "ENEMYID":
                // example format = ENEMYID_2
                value = int.Parse( message[1]);

                if (playerManager.enemyActionDisplayers.ContainsKey(value) || value == PlayerData.id)
                {
                    return;
                }
                playerManager.AddPlayer(value);
                break;
            
            case "SCORE":
                // example format = SCORE_1_14
                playerManager.AddPlayerScore(int.Parse( message[1]), int.Parse( message[2]));
                break;
            
            case "CARD":
                // example format = ENEMY_1_5
                id = int.Parse( message[1]);
                int cardValue = int.Parse( message[2]);
                playerManager.AddCardToPlayerDeck(id, cardValue);
                break;
            
            case "READY":
                // example format = READY_1
                id = int.Parse( message[1]);
                playerManager.OnReady(id);
                break;
            
            case "UNREADY":
                // example format = UNREADY_1
                id = int.Parse( message[1]);
                playerManager.OnUnReady(id);
                break;
            
            case "HIT":
                // example format = HIT_1
                id = int.Parse(  message[1]);
                playerManager.OnHit(id);
                break;
            
            case "STAND":
                // example format = STAND_1
                id = int.Parse( message[1]);
                playerManager.OnStand(id);
                break;
            
            case "STARTTURN":
                // example format = TURN_1
                id = int.Parse( message[1]);
                playerManager.OnTurnStart(id);
                break;
            
            case "ENDTURN":
                // example format = ENDTURN_1
                id = int.Parse( message[1]);
                playerManager.OnTurnOver(id);
                break;
            
            case "OUT":
                // example format = OUT_1
                id = int.Parse( message[1]);
                playerManager.OnOut(id);
                break;
            
            case "WINNER":
                // example format = OUT_1
                id = int.Parse( message[1]);
                playerManager.OnWin(id);
                break;
            default:
                Debug.Log("Unaccounted for value type " +  message[0]);
                break;
        }
    }
}
