using System;
using UnityEngine;

public class MimicServer : MonoBehaviour
{
	[SerializeField] private IncomingServerCommunicator incomingServerCommunicator;

	public enum ServerCommands
	{
		IS_TURN,
		SHOW_RESULTS,
		FORCE_GAME_STOP,
		MATCH_FULL,
		GAME_STOP,
		GAME_START,
		WAITING_FOR_PLAYER_TURN,
		END_TURN
		

	}
	
	public enum ValueCommands
	{
		CARD_1_12,
		THISID_1,
        ENEMYID_2,
        CARD_2_12,
        SCORE_1_12

	}

	public ServerCommands CommandToSend;
	public ValueCommands ValueToSend;

	
    public void SendMessageToClient(ServerCommands command) 
    {
        incomingServerCommunicator.HandleIncomingMessage(CommandToSend.ToString());
    }
    
    public void SendValueToClient(ValueCommands command) 
    {
	    incomingServerCommunicator.HandleIncomingMessage(ValueToSend.ToString());
    }
}

