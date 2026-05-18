using System.Diagnostics;
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

                if (server.currentGameSession == TcpServer.GameSession.INPROGRESS)
                {
                    playerHandler.KickPlayer(pPlayer, "Send " + msg + " when game was session was " + server.currentGameSession);
                    return;
                }

                pPlayer.PlayerState = PlayerStates.IS_READY;
                playerHandler.SetPlayerState(pPlayer, PlayerStates.IS_READY);
                outgoing.SetAndBroadcastPlayerState(pPlayer, PlayerStates.IS_READY);
                
                
                
                
                
                
                if (playerHandler.CanStartGame())
                {
                    server.ClearAllMatchData();
                    server.StartGame();
                }

                break;

            case "IS_NOT_READY":
                
                if (server.currentGameSession == TcpServer.GameSession.INPROGRESS)
                {
                    playerHandler.KickPlayer(pPlayer, "Send " + msg + " when game was session was " + server.currentGameSession);
                    return;
                }
                
                playerHandler.SetPlayerState(pPlayer, PlayerStates.IS_NOT_READY);
                outgoing.SetAndBroadcastPlayerState(pPlayer, PlayerStates.IS_NOT_READY);
                break;

            case "HIT":
   
                if (server.currentGameSession == TcpServer.GameSession.WAITING_TO_START)
                {
                    playerHandler.KickPlayer(pPlayer, "Send " + msg + " when game was session was INPROGRESS " + server.currentGameSession);
                    return;
                };

                if (pPlayer != server.activePlayer)
                {
                    playerHandler.KickPlayer(pPlayer, "HIT out of turn");
                    return;
                }
                
                outgoing.BroadcastValue(TcpServer.ValueTypes.HIT, new[]{pPlayer.ID});
                server.OnHit(pPlayer);
                break;

            case "STAND":
          
                if (server.currentGameSession == TcpServer.GameSession.WAITING_TO_START)
                {
                    playerHandler.KickPlayer(pPlayer, "Send " + msg + " when game was session was INPROGRESS " + server.currentGameSession);
                    return;
                };

                if (pPlayer != server.activePlayer)
                {
                    playerHandler.KickPlayer(pPlayer, "STAND out of turn");
                    return;
                }
                outgoing.BroadcastValue(TcpServer.ValueTypes.STAND, new[]{pPlayer.ID});
                server.OnStand(pPlayer);
                break;

            default:
                playerHandler.KickPlayer(pPlayer, "Unknown command: " + msg);
                break;
        }
    }
    

}