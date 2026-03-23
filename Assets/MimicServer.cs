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
		WAITING_FOR_PLAYER_TURN

	}

	public ServerCommands CommandToSend;
	
    public void SendMessageToClient(ServerCommands command) 
    {
        incomingServerCommunicator.HandleIncomingMessage(CommandToSend.ToString());
    }
}

