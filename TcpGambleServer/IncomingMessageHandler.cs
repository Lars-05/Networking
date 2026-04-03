using System.Text;

public class IncomingMessageHandler
{
    private readonly TcpServer server;
    private readonly OutgoingMessageHandler outgoing;
    private readonly PlayerHandler playerHandler;

    public IncomingMessageHandler(TcpServer server, OutgoingMessageHandler outgoing, PlayerHandler pPlayerHandler)
    {
        this.server = server;
        this.outgoing = outgoing;
        this.playerHandler = pPlayerHandler;
    }

    public void ProcessIncomingMessages()
    {
        foreach (var player in server.players)
        {
            if (player.tcpClient.Available <= 0)
                continue;

            byte[] data = new byte[player.tcpClient.Available];

            int bytesRead = player.stream.Read(data, 0, data.Length);

            if (bytesRead <= 0)
                continue;

            string message = Encoding.UTF8
                .GetString(data, 0, bytesRead)
                .Trim();

            Console.WriteLine($"Received from player {player.ID}: {message}");

            Handle(message, player);
        }
    }
    
    public void Handle(string msg, Player player)
    {
        switch (msg)
        {
            case "IS_READY":
                if (server.currentGameState != TcpServer.GameState.WAITING_ALL_PLAYERS_READY)
                    return;

                player.PlayerState = PlayerStates.IS_READY;

                if (server.CheckAllPlayerForState(PlayerStates.IS_READY))
                {
                    //server.StartMatch();
                }
                break;

            case "IS_NOT_READY":
                player.PlayerState = PlayerStates.IS_NOT_READY;
                break;

            case "HIT":
                if (server.gameSession != TcpServer.GameSession.INPROGRESS)
                    return;

                if (player != server.activePlayer)
                {
                    playerHandler.KickPlayer(player, "HIT out of turn");
                    return;
                }

                server.OnHit(player);
                break;

            case "STAND":
                if (server.gameSession != TcpServer.GameSession.INPROGRESS)
                    return;

                if (player != server.activePlayer)
                {
                    playerHandler.KickPlayer(player, "STAND out of turn");
                    return;
                }

                server.OnStand(player);
                break;

            default:
                playerHandler.KickPlayer(player, "Unknown command");
                break;
        }
    }
}