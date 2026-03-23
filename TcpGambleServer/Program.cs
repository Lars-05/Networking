using System.Net; // For IPAddress
using System.Net.Sockets;
using System.Text; // For TcpListener, TcpClient



class TcpServer {

	
	enum GameState
	{
		WAITING,
		STARTING,
		INPROGRESS,
		ENDED,
	}

	static GameState gameState = GameState.WAITING;
	enum PlayerStates
	{
		IS_READY,
		IS_NOT_READY,
		PLAYING_TURN,	
		HAD_TURN,
		WAITING_FOR_TURN
		
	}
	enum ServerCommands
	{
		IS_TURN,
		GAME_START,
		SHOW_RESULTS,
		FORCE_GAME_STOP,
		GAME_STOP,
		MATCH_FULL,
		WAITING_FOR_PLAYER_TURN
	}

	class Player
	{
		public int ID;
		public TcpClient tcpClient;
		public NetworkStream stream;
		public PlayerStates PlayerState = PlayerStates.IS_NOT_READY;
		public int score;
		public bool hadTurn;
		public Player(int id, TcpClient client)
		{
			ID = id;
			tcpClient= client;
			stream = client.GetStream();
		}
	}
	static List<Player> players = new List<Player>();

	
	static void Main() {
		StartServer(50001);
	}


	private static int lobbySize = 2;
	static void StartServer(int port) {
	
		TcpListener listener = new TcpListener(IPAddress.Any, port);
		listener.Start();
		Console.WriteLine($"Starting TCP server on port {port} - listening for incoming connection requests");
		Console.WriteLine("Press Q to stop the server");
		
		while (true) {
			
			AcceptNewClients(listener);
			HandleIncomingMessages();
			CleanupClients();
			
			if (gameState == GameState.WAITING)
			{
				if (players.Count == lobbySize && AllPlayersReady())
				{
					StartMatch();
				}
			}
			


			Thread.Sleep(10);
		}
	}

	static bool AllPlayersReady()
	{
		if(players.Count != lobbySize)
			return false;
		
		foreach (Player player in players)
		{
			if (player.PlayerState == PlayerStates.IS_NOT_READY)
			{
				return false;
			}
		}
		return true;
	}

	static void AcceptNewClients(TcpListener listener) {
		// Pending will be true if there is an incoming connection request:
		if (listener.Pending()) {
			TcpClient newClient = listener.AcceptTcpClient();
			if (players.Count + 1 > 2)
			{
				RejectClient(newClient);
				return;
			}
			
			Player newPlayer = new Player(players.Count + 1, newClient);
			players.Add(newPlayer);
			SendMessageToPlayer(newPlayer, "ID_"+newPlayer.ID);
			Console.WriteLine("Accepted a new client");
			
		}
	}

	#region Incoming/Outgoing Message Handling
	
	
	static void HandleIncomingMessages() {
		foreach (Player player in players) {
			
			if (player.tcpClient.Available > 0)
			{
				NetworkStream stream = player.stream;
				int packetLength = player.tcpClient.Available;
				
				byte[] data = new byte[packetLength];
				stream.Read(data, 0, packetLength); // might also not get the entire packet with bad network connections
				
				string message = Encoding.UTF8.GetString(data).Trim();;
				char lastChar = message[message.Length - 1];
				
				Console.WriteLine("Message " + message + " Received from client");
				if (!int.TryParse(lastChar.ToString(), out int number))
				{
					MessageHandler(message, player);
					return;
				}
				ValueHandler(message, player);
			}
		}
	}

	static void ValueHandler(string msg, Player player)
	{
		string[] parts = msg.Trim().Split('_');
		int value = int.Parse(parts[1]);
		switch (parts[0])
		{
			case "SCORE":
				player.score += value;	
				break;
			default:
				Console.WriteLine("Not accounted for value type " + parts[0]); 
			return;
		
		}
	}

	static void MessageHandler(string msg, Player player)
	{
		switch (msg)
		{
			case "IS_READY":
				player.PlayerState = PlayerStates.IS_READY;
				Console.WriteLine("Player " + player.ID + " Is ready");
				if (AllPlayersReady())
				{
					Console.WriteLine("All Player Ready, Starting Game");
					StartMatch();
				}
				else
				{
					Console.WriteLine("Waiting For Other Players To Ready Up");
				}
				break;
			
			case "IS_NOT_READY":
				player.PlayerState = PlayerStates.IS_NOT_READY;
				Console.WriteLine("Player " + player.ID + " Is not ready");
				break;
			
			case "TURN_DONE":
				if(players[0].PlayerState == PlayerStates.HAD_TURN)
				{
					players[1].PlayerState = PlayerStates.HAD_TURN;
					ShowResult();
					return;
				}
				PassTurn();
				break; 
			default:
				Console.WriteLine("Not accounted for message: " + msg);
				return;
		}
	}
	
	#endregion


	static bool EveryoneHadTurn()
	{
		foreach (Player player in players)
		{
			if (player.PlayerState != PlayerStates.HAD_TURN)
			{
				return false;
			}
		}
		return true;
	}
	static void RejectClient(TcpClient client)
	{
		Console.WriteLine($"Client rejected, match is full ({ client.Client.RemoteEndPoint})");
		NetworkStream stream =  client.GetStream();
		byte[] data = Encoding.UTF8.GetBytes("MATCH_FULL\n");
		stream.Write(data, 0, data.Length);
		stream.Flush();
		client.Close();
	}
	static void ResetStats()
	{
		foreach (Player player in players)
		{
			Console.WriteLine("Player " + player.ID + " stats reset");
			player.PlayerState = PlayerStates.IS_NOT_READY;
			player.score = 0;
		}
		Console.WriteLine("Stats reset, Game ended");
		gameState = GameState.WAITING;
	}
	
	static void StartMatch()
	{
		gameState = GameState.INPROGRESS;
		SendCommandToPlayer(players[0], ServerCommands.IS_TURN);
		players[0].PlayerState = PlayerStates.PLAYING_TURN;
		players[1].PlayerState = PlayerStates.WAITING_FOR_TURN;
	}
	
	static void ShowResult()
	{
		gameState = GameState.ENDED;
		SendMessageToPlayer(players[0], "SCORE_" + players[0].score + "/" + players[1].score);
		SendMessageToPlayer(players[1], "SCORE_" + players[1].score + "/" + players[0].score);
		SendCommandToAllPlayers(ServerCommands.SHOW_RESULTS);
		ResetStats();
	}

	static void SendMessageToPlayer(Player player, string message)
	{
		byte[] messageInBytes = Encoding.UTF8.GetBytes(message + "\n");
		player.stream.Write(messageInBytes , 0, messageInBytes.Length);
		player.stream.Flush(); // maybe this helps sometimes -> but not always (clumsy)
		Console.WriteLine("Send " + message + " to player " + player.ID);
	}

	static void SendCommandToAllPlayers(ServerCommands cmd)
	{
		for (int i = 0; i < players.Count; i++)
		{
			SendCommandToPlayer(players[i], cmd);
		}
	}
	
	static void SendCommandToPlayer(Player player, ServerCommands cmd)
	{
		string finalCommand = string.Empty;
		switch (cmd)
		{
			case ServerCommands.IS_TURN:
				finalCommand = "IS_TURN" ;
				break;
			
			case ServerCommands.GAME_START:
				finalCommand = "GAME_START" ;
				break;
				
			case ServerCommands.SHOW_RESULTS:
				finalCommand = "SHOW_RESULTS";
				break;
				
			case ServerCommands.FORCE_GAME_STOP:
				finalCommand = "FORCE_GAME_STOP" ;
				break;
				
			case ServerCommands.GAME_STOP:
				finalCommand = "GAME_STOP" ;
				break;
			
			case ServerCommands.WAITING_FOR_PLAYER_TURN:
				finalCommand = "WAITING_FOR_PLAYER_TURN" ;
				break;
			
			default:
				Console.WriteLine("Not accounted for command: " + cmd);
				return;
		}
		
		SendMessageToPlayer(player, finalCommand);
	}
	
	static void PassTurn()
	{
		players[0].PlayerState = PlayerStates.HAD_TURN;
		Console.WriteLine("Player " + players[0].ID + "Had turn");
		SendCommandToPlayer(players[1], ServerCommands.IS_TURN);
		players[1].PlayerState = PlayerStates.PLAYING_TURN;
		Console.WriteLine("Player " + players[1].ID + "Turn");
	}
	
	
	static void CleanupClients() {
		for (int i = players.Count - 1; i >= 0; i--) {
			if (!IsClientConnected(players[i].tcpClient))
			{
				players[i].tcpClient.Close();
				players.RemoveAt(i);
				Console.WriteLine($"Removing disconnected client. Number of connected clients: {players.Count}");
				if (gameState == GameState.INPROGRESS)
					OnAbruptDisconnect();
			}
		}
	}

	static void OnAbruptDisconnect()
	{
		Console.WriteLine("Opponent abruptly disconnected, resetting game");
		for(int i = 0; i < players.Count; i++)
		{
			SendCommandToPlayer(players[i], ServerCommands.FORCE_GAME_STOP);
		}
		ResetStats();
	}
	
	
	static bool IsClientConnected(TcpClient client)
	{
		try
		{
			return client.Connected;
		}
		catch
		{
			return false;
		}
	}
	
	static bool QuitPressed() {
		if (Console.KeyAvailable) {
			char input = Console.ReadKey(true).KeyChar;
			if (input == 'q') {
				return true;
			}
		}
		return false;
	}
}

