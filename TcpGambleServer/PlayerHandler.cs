using System.Net;
using System.Net.Sockets;
using System.Text;

public class PlayerHandler
{
    private readonly TcpServer server;
    private readonly OutgoingMessageHandler outgoingMessageHandler;
  

    private int lobbySize = 2;

    public List<Player> players = new();

    public PlayerHandler(TcpServer pServer, OutgoingMessageHandler pOutgoingMessageHandler)
    {
        this.server = pServer;
        this.outgoingMessageHandler = pOutgoingMessageHandler;
    }

    public void AcceptNewClients(TcpListener listener)
    {
        if (!listener.Pending())
            return;

        TcpClient newClient = listener.AcceptTcpClient();

        if (players.Count >= lobbySize)
        {
            RejectClient(newClient);
            return;
        }

        var player = new Player(players.Count + 1, newClient);
        players.Add(player);

        outgoingMessageHandler.SendMessageToPlayer(
            player,
            "ID_" + player.ID
        );

        Console.WriteLine("Client connected");

        if (players.Count == lobbySize)
        {
            server.currentGameState =
                TcpServer.GameState.WAITING_ALL_PLAYERS_READY;
        }
    }

    public void RejectClient(TcpClient client)
    {
        byte[] data = Encoding.UTF8.GetBytes("MATCH_FULL\n");

        client.GetStream().Write(data, 0, data.Length);
        client.Close();
    }

    public void CleanupClients()
    {
        for (int i = players.Count - 1; i >= 0; i--)
        {
            if (!IsClientConnected(players[i]))
            {
                players[i].tcpClient.Close();
                players.RemoveAt(i);

                Console.WriteLine("Disconnected client removed");

                if (server.gameSession ==
                    TcpServer.GameSession.INPROGRESS)
                {
                    OnAbruptDisconnect();
                }
            }
        }
    }

    public bool IsClientConnected(Player player)
    {
        try
        {
            byte[] beat = new byte[1];

            player.stream.Write(beat, 0, beat.Length);
            player.stream.Flush();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void KickPlayer(Player player, string reason)
    {
        Console.WriteLine(
            $"Player {player.ID} kicked: {reason}"
        );

        player.tcpClient.Close();
        server.StopGameAbruptly(reason);
    }

    public void OnAbruptDisconnect()
    {
        Console.WriteLine("Player disconnected abruptly");

        outgoingMessageHandler.SendCommandToAllPlayers(
            TcpServer.ServerCommands.FORCE_GAME_STOP
        );
    }
}