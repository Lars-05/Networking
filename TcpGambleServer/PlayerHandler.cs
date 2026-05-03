using System.Net;
using System.Net.Sockets;
using System.Text;

public class PlayerHandler
{
    private readonly TcpServer server;


    private OutgoingMessageHandler outgoingMessageHandler;

    private int lobbySize = 2;
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

        Player player;

        lock (playerLock)
        {
            if (players.Count >= lobbySize || server.gameSession == TcpServer.GameSession.INPROGRESS)
            {
                RejectClient(newClient);
                return;
            }

            player = new Player(nextPlayerId++, newClient);
            players.Add(player);

            Console.WriteLine($"Client connected (Player {player.ID})");

            if (players.Count == lobbySize)
            {
                server.currentGameState =
                    TcpServer.GameState.WAITING_ALL_PLAYERS_READY;
            }
        }


        outgoingMessageHandler?.ValueHandler(
            TcpServer.ValueTypes.THISID,
            player,
            player.ID
        );
    }

    public void SetAndUpdatePlayersOfPlayerState(Player pPlayer, PlayerStates pState)
    {
        pPlayer.PlayerState = pState;
        foreach (var player in GetPlayersSnapshot())
        {
            switch (pState)
            {
                case PlayerStates.IDLE:
                    break;

                case PlayerStates.IS_READY:
                    outgoingMessageHandler.ValueHandler(
                        TcpServer.ValueTypes.READY, player, pPlayer.ID
                    );
                    break;

                case PlayerStates.IS_NOT_READY:
                    outgoingMessageHandler.ValueHandler(
                        TcpServer.ValueTypes.UNREADY, player, pPlayer.ID
                    );
                    break;

                case PlayerStates.PLAYING_TURN:
                    outgoingMessageHandler.ValueHandler(
                        TcpServer.ValueTypes.STARTTURN, player, pPlayer.ID
                    );
                    break;

                case PlayerStates.WAITING_FOR_TURN:
                    outgoingMessageHandler.ValueHandler(
                        TcpServer.ValueTypes.ENDTURN, player, pPlayer.ID
                    );
                    break;

                case PlayerStates.OUT:
                    outgoingMessageHandler.ValueHandler(
                        TcpServer.ValueTypes.OUT, player, pPlayer.ID
                    );
                    break;
            }
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
        bool gameShouldStop = false;

        lock (playerLock)
        {
            for (int i = players.Count - 1; i >= 0; i--)
            {
                var player = players[i];

                if (!IsClientConnected(player))
                {
                    Console.WriteLine($"Player {player.ID} disconnected");

                    try { player.stream?.Close(); } catch { }
                    try { player.tcpClient?.Close(); } catch { }

                    player.stream = null;

                    players.RemoveAt(i);

                    Console.WriteLine("Disconnected client removed");

                    if (server.gameSession == TcpServer.GameSession.INPROGRESS)
                        gameShouldStop = true;
                }
            }
        }


        if (gameShouldStop)
        {
            OnAbruptDisconnect();
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



    public void KickPlayer(Player player, string reason)
    {
        if (player == null)
            return;

        Console.WriteLine($"Player {player.ID} kicked: {reason}");

        lock (playerLock)
        {
            players.Remove(player);
        }

        try { player.stream?.Close(); } catch { }
        try { player.tcpClient?.Close(); } catch { }

        player.stream = null;

        server.StopGameAbruptly(reason);
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

        outgoingMessageHandler?.SendCommandToAllPlayers(
            TcpServer.ServerCommands.FORCE_GAME_STOP
        );
    }
}