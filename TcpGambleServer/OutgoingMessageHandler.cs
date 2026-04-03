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
        byte[] bytes = Encoding.UTF8.GetBytes(message + "\n");

        player.stream.Write(bytes, 0, bytes.Length);
        player.stream.Flush();

        Console.WriteLine($"Sent {message} to player {player.ID}");
    }

    public void SendCommandToPlayer(Player player, TcpServer.ServerCommands cmd)
    {
        SendMessageToPlayer(player, cmd.ToString());
    }

    public void SendCommandToAllPlayers(TcpServer.ServerCommands cmd)
    {
        foreach (var player in playerHandler.players)
        {
            SendCommandToPlayer(player, cmd);
        }
    }

    public void SendValueToPlayer(Player player, TcpServer.ValueTypes type, int value)
    {
        string message = $"{type}_{value}";
        SendMessageToPlayer(player, message);
    }
}