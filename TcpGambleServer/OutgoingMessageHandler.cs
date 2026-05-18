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

    public void BroadcastCommand(TcpServer.ServerCommands command)
    {
        foreach (var player in playerHandler.GetPlayersSnapshot())
        {
            if (player == null || player.stream == null)
                continue;

            SendMessageToPlayer(player, command.ToString());
        }
    }

    public void SetAndBroadcastPlayerState(Player player, PlayerStates state)
    {
        TcpServer.ValueTypes valueType = state switch
        {
            PlayerStates.IS_READY => TcpServer.ValueTypes.READY,
            PlayerStates.IS_NOT_READY => TcpServer.ValueTypes.UNREADY,
            PlayerStates.PLAYING_TURN => TcpServer.ValueTypes.STARTTURN,
            PlayerStates.WAITING_FOR_TURN => TcpServer.ValueTypes.ENDTURN,
            PlayerStates.OUT => TcpServer.ValueTypes.OUT,
            _ => default
        };

        if (valueType == default)
            return;

        BroadcastValue(valueType, new[]{player.ID});
    }

    private string BuildValueMessage(TcpServer.ValueTypes valueType, params int[] data)
    {
        return valueType switch
        {
            TcpServer.ValueTypes.READY => $"READY_{data[0]}",
            TcpServer.ValueTypes.UNREADY => $"UNREADY_{data[0]}",
            TcpServer.ValueTypes.STARTTURN => $"STARTTURN_{data[0]}",
            TcpServer.ValueTypes.ENDTURN => $"ENDTURN_{data[0]}",
            TcpServer.ValueTypes.THISID => $"THISID_{data[0]}",
            TcpServer.ValueTypes.ENEMYID=> $"ENEMYID_{data[0]}",
            TcpServer.ValueTypes.HIT => $"HIT_{data[0]}",
            TcpServer.ValueTypes.STAND => $"STAND_{data[0]}",
            TcpServer.ValueTypes.CARD => $"CARD_{data[0]}_{data[1]}",
            TcpServer.ValueTypes.SCORE => $"SCORE_{data[0]}_{data[1]}",
            TcpServer.ValueTypes.OUT => $"OUT_{data[0]}",
            TcpServer.ValueTypes.WINNER => $"WINNER_{data[0]}",
            TcpServer.ValueTypes.DISCONNECTED => $"DISCONNECTED_{data[0]}",
            _ => null
        };
    }

    public void BroadcastValue(TcpServer.ValueTypes valueType, int[] data, Player? excludedPlayer = null)
    {
        string message = BuildValueMessage(valueType, data);
        
        if (message == null)
            return;

        foreach (var player in playerHandler.GetPlayersSnapshot())
        {
            if (player == null || player == excludedPlayer)
                continue;

            SendMessageToPlayer(player, message);
        }
    }

    public void SendValueToPlayer(Player player, TcpServer.ValueTypes valueType, params int[] data)
    {
        string message = BuildValueMessage(valueType, data);

        if (message == null)
        {
            Console.WriteLine("Invalid message type");
            return;
        }

        SendMessageToPlayer(player, message);
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

        }
    }

    public void SendCommandToPlayer(Player player, TcpServer.ServerCommands cmd)
    {
        SendMessageToPlayer(player, cmd.ToString());
    }
}