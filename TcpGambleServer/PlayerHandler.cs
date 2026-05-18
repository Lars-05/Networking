using System.Net;
using System.Net.Sockets;
using System.Text;

public class PlayerHandler
{
    private readonly TcpServer server;


    private OutgoingMessageHandler outgoingMessageHandler;


    private readonly object playerLock = new object();
    private int nextPlayerId = 1;

    public List<Player> players = new();

    public PlayerHandler(TcpServer pServer, OutgoingMessageHandler pOutgoingMessageHandler)
    {
        server = pServer;
        outgoingMessageHandler = pOutgoingMessageHandler;
    }


    public void SetOutgoing(OutgoingMessageHandler outgoing)
    {
        this.outgoingMessageHandler = outgoing;
    }


    public void AcceptNewClients(TcpListener listener)
    {
        if (!listener.Pending())
            return;

        TcpClient newClient = listener.AcceptTcpClient();

        Player newPlayer;

        lock (playerLock)
        {
            /// Dont allow connects when the game is inprogress
            if (server.currentGameSession == TcpServer.GameSession.INPROGRESS)
            {
                RejectClient(newClient);
                return;
            }
            
            newPlayer = new Player(nextPlayerId++, newClient);
            players.Add( newPlayer);


            Console.WriteLine($"Client connected (Player { newPlayer.ID})");
            
        }
        
        


        outgoingMessageHandler?.SendValueToPlayer(newPlayer, TcpServer.ValueTypes.THISID, newPlayer.ID);
        
        
        // checks and sets up enemy displayers
        foreach (var player in GetPlayersSnapshot())
        {
            foreach (var enemy in GetPlayersSnapshot())
            {
                if(player == enemy)
                    continue;
                
                if(player.KnownsEnemy(enemy.ID))
                    continue;
                    
                outgoingMessageHandler.SendValueToPlayer(player, TcpServer.ValueTypes.ENEMYID, new []{enemy.ID});
                player.knownEnemies.Add(enemy.ID);
                
            }
        }
    }
    
    public bool CanStartGame()
    {
        lock (playerLock)
        {
            if (players.Count < 2)
                return false;

            return players.All(p => p.PlayerState == PlayerStates.IS_READY);
        }
    }
    
    public void SetPlayerState(Player pPlayer, PlayerStates pState)
    {
        lock (playerLock)
        {
            pPlayer.SetPlayerState(pState);
        }
    }

    public void SetAllPlayerStates(PlayerStates pState)
    {
        foreach (var p in GetPlayersSnapshot())
        {
            SetPlayerState(p, pState);
        }
    }

    public List<Player> GetPlayersSnapshot()
    {
        lock (playerLock)
        {
            return players.ToList();
        }
    }



    public void CleanupClients()
    {

        lock (playerLock)
        {
            for (int i = players.Count - 1; i >= 0; i--)
            {
                var player = players[i];

                if (!IsClientConnected(player))
                {
                    OnPlayerDisconnected(player);


                }
            }
        }
    }

    bool gameShouldStop = false;
    public void OnPlayerDisconnected(Player pPlayer)
    {
        
        Console.WriteLine($"Player {pPlayer.ID} disconnected");

        try { pPlayer.stream?.Close(); } catch { }
        try { pPlayer.tcpClient?.Close(); } catch { }

        pPlayer.stream = null;
        
        Console.WriteLine("Disconnected client removed");

        gameShouldStop = server.currentGameSession == TcpServer.GameSession.INPROGRESS;
                    
        // If game in progess, stop the game and kick the player. If game not in progress, kick the player
        if (gameShouldStop)
        {
            KickPlayer(pPlayer, "Disconnected");
            OnAbruptDisconnect();
        }
        else
        {
            KickPlayer(pPlayer, "Disconnected");
        }
        
    }
    

   


    public bool IsClientConnected(Player player)
    {
        try
        {
            if (player?.tcpClient == null)
                return false;

            if (!player.tcpClient.Connected)
                return false;

            Socket socket = player.tcpClient.Client;

            return !(socket.Poll(1, SelectMode.SelectRead) &&
                     socket.Available == 0);
        }
        catch
        {
            return false;
        }
    }


    public void DeletePlayerDisplayers(int disconnectedPlayerId)
    {
        foreach (var player in GetPlayersSnapshot())
        {
            if (!player.knownEnemies.Contains(disconnectedPlayerId))
                continue;
            
            outgoingMessageHandler.BroadcastValue(TcpServer.ValueTypes.DISCONNECTED, new []{disconnectedPlayerId});
        }
    }
    public void KickPlayer(Player pPlayer, string reason)
    {
        if (pPlayer == null)
            return;
        
        int id = pPlayer.ID;
        Console.WriteLine($"Player {pPlayer.ID} kicked: {reason}");

        lock (playerLock)
        {
            players.Remove(pPlayer);
        }
        
        try { pPlayer.stream?.Close(); } catch { }
        try { pPlayer.tcpClient?.Close(); } catch { }
        pPlayer.stream = null;
        
        DeletePlayerDisplayers(id);
    }


    public void RejectClient(TcpClient client)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes("MATCH_FULL\n");
            var stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }
        catch { }

        client.Close();
    }



    public void OnAbruptDisconnect()
    {
        Console.WriteLine("Player disconnected abruptly");
        server.currentGameSession = TcpServer.GameSession.WAITING_TO_START;
        outgoingMessageHandler?.BroadcastCommand(TcpServer.ServerCommands.FORCE_GAME_STOP);
        // allow players to join again
        server.currentGameState = TcpServer.GameState.WAITING_FOR_PLAYERS;

    }
}