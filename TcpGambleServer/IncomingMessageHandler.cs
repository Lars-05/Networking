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


    
    public void Handle(string msg, Player pPlayer)
    {
        switch (msg)
        {
            case "IS_READY":
                
           
                if (server.currentGameState != TcpServer.GameState.WAITING_ALL_PLAYERS_READY)
                    return;

                pPlayer.PlayerState = PlayerStates.IS_READY;

                playerHandler.SetAndUpdatePlayersOfPlayerState(pPlayer, PlayerStates.IS_READY);
                if (server.CheckAllPlayerForState(PlayerStates.IS_READY) && playerHandler.players.Count > 1)
                {
                    
                    server.ClearAllMatchData();
                    server.StartGame();
                }
                break;

            case "IS_NOT_READY":
           
    
                playerHandler.SetAndUpdatePlayersOfPlayerState(pPlayer, PlayerStates.IS_NOT_READY);
                outgoing.SendValueToAllPlayers(TcpServer.ValueTypes.UNREADY, pPlayer.ID);
                break;

            case "HIT":
   
                if (server.gameSession != TcpServer.GameSession.INPROGRESS && server.currentGameState == TcpServer.GameState.PLAYING_STATE)
                    return;

                if (pPlayer != server.activePlayer)
                {
                    playerHandler.KickPlayer(pPlayer, "HIT out of turn");
                    return;
                }
                
                outgoing.SendValueToAllPlayers(TcpServer.ValueTypes.HIT, pPlayer.ID);
                server.OnHit(pPlayer);
                break;

            case "STAND":
          
                if (server.gameSession != TcpServer.GameSession.INPROGRESS && server.currentGameState == TcpServer.GameState.PLAYING_STATE)
                    return;

                if (pPlayer != server.activePlayer)
                {
                    playerHandler.KickPlayer(pPlayer, "STAND out of turn");
                    return;
                }
                outgoing.SendValueToAllPlayers(TcpServer.ValueTypes.STAND, pPlayer.ID);
                server.OnStand(pPlayer);
                break;

            default:
                playerHandler.KickPlayer(pPlayer, "Unknown command: " + msg);
                break;
        }
    }
}