using System.Text;

public class OutgoingMessageHandler
{
    private readonly TcpServer server;
    private readonly PlayerHandler playerHandler;

    public OutgoingMessageHandler(TcpServer server, PlayerHandler playerHandler)
    {
        this.server = server;
        this.playerHandler = playerHandler;
    }

    public void SendMessageToPlayer(Player player, string message)
    {
        if (player == null || player.stream == null)
            return;

        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message + "\n");

            player.stream.Write(bytes, 0, bytes.Length);
            player.stream.Flush();

            Console.WriteLine($"Sent {message} to player {player.ID}");
        }
        catch
        {
            // optionally log error
        }
    }

    public void SendCommandToPlayer(Player player, TcpServer.ServerCommands cmd)
    {
        SendMessageToPlayer(player, cmd.ToString());
    }

    public void SendCommandToAllPlayers(TcpServer.ServerCommands cmd, Player exception = null)
    {
        foreach (var player in playerHandler.GetPlayersSnapshot())
        {
            if (player == null || player == exception || player.stream == null)
                continue;

            SendCommandToPlayer(player, cmd);
        }
    }

    public void SendValueToAllPlayers(TcpServer.ValueTypes type, int value, Player exception = null)
    {
        foreach (var player in playerHandler.GetPlayersSnapshot())
        {
            if (player == null || player == exception || player.stream == null)
                continue;

            ValueHandler(type, player, value);
        }
    }

    public void SendCardToAllPlayers(Player owner, int value, Player exception = null)
    {
        foreach (var player in playerHandler.GetPlayersSnapshot())
        {
            if (player == null || player == exception || player.stream == null)
                continue;

            string message = $"CARD_{owner.ID}_{value}";
            SendMessageToPlayer(player, message);
        }
    }

    public void SendCardToPlayer(Player player, int cardValue)
    {
        if (player == null || player.stream == null)
            return;

        string message = $"CARD_{player.ID}_{cardValue}";
        SendMessageToPlayer(player, message);
    }


    public void ValueHandler(TcpServer.ValueTypes type, Player targetPlayer, int subjectValue)
    {
        if (targetPlayer == null || targetPlayer.stream == null)
            return;

        string message;

        switch (type)
        {
            case TcpServer.ValueTypes.READY:
                message = $"READY_{subjectValue}";
                break;

            case TcpServer.ValueTypes.UNREADY:
                message = $"UNREADY_{subjectValue}";
                break;

            case TcpServer.ValueTypes.STARTTURN:
                message = $"STARTTURN_{subjectValue}";
                break;

            case TcpServer.ValueTypes.ENDTURN:
                message = $"ENDTURN_{subjectValue}";
                break;

            case TcpServer.ValueTypes.HIT:
                message = $"HIT_{subjectValue}";
                break;

            case TcpServer.ValueTypes.STAND:
                message = $"STAND_{subjectValue}";
                break;

            case TcpServer.ValueTypes.CARD:
                message = $"CARD_{targetPlayer.ID}_{subjectValue}";
                break;

            case TcpServer.ValueTypes.SCORE:
                message = $"SCORE_{targetPlayer.ID}_{subjectValue}";
                break;

            case TcpServer.ValueTypes.OUT:
                message = $"OUT_{subjectValue}";
                break;
            
            case TcpServer.ValueTypes.WINNER:
                message = $"WINNER_{subjectValue}";
                break;

            case TcpServer.ValueTypes.THISID:
                SendMessageToPlayer(targetPlayer, $"THISID_{subjectValue}");

                foreach (var player in playerHandler.GetPlayersSnapshot())
                {
                    if (player == targetPlayer)
                        continue;

                    if (!player.knownsEnemy(subjectValue))
                    {
                        SendMessageToPlayer(player, $"ENEMYID_{subjectValue}");
                        player.AddKnownEnemy(subjectValue);
                    }

                    if (!targetPlayer.knownsEnemy(player.ID))
                    {
                        SendMessageToPlayer(targetPlayer, $"ENEMYID_{player.ID}");
                        targetPlayer.AddKnownEnemy(player.ID);
                    }
                }
                return;

            case TcpServer.ValueTypes.ENEMYID:
                message = $"ENEMYID_{subjectValue}";
                targetPlayer.AddKnownEnemy(subjectValue);
                break;

            default:
                return;
        }

        SendMessageToPlayer(targetPlayer, message);
    }
}