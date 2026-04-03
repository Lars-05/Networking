using System.Net;
using System.Net.Sockets;
using System.Text;

public enum PlayerStates
{
    IDLE,
    IS_READY,
    IS_NOT_READY,
    PLAYING_TURN,
    TURNS_DONE,
    WAITING_FOR_TURN,
    LOOKING_AT_RESULTS
}

public class Player
{
    
    
    public int ID;
    public TcpClient tcpClient;
    public NetworkStream stream;
    public PlayerStates PlayerState = PlayerStates.IDLE;
    public List<int> cardsDrawn = new();
    public List<int> publicCards = new();
    public int score;

    public Player(int id, TcpClient client)
    {
        ID = id;
        tcpClient = client;
        stream = client.GetStream();
    }
    
}