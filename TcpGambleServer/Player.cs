using System.Net;
using System.Net.Sockets;
using System.Text;

public enum PlayerStates
{
    IDLE,
    IS_READY,
    IS_NOT_READY,
    PLAYING_TURN,
    OUT,
    WAITING_FOR_TURN,
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
    public List<int> knownEnemies = new();

    public Player(int id, TcpClient client)
    {
        ID = id;
        tcpClient = client;
        stream = client.GetStream();
    }

    public void AddKnownEnemy(int pEnemyID)
    {
        if (knownEnemies.Contains(pEnemyID))
            return;
        
        knownEnemies.Add(pEnemyID);
    }

    public void ClearData()
    {
        score = 0;
        cardsDrawn.Clear();
        publicCards.Clear();
    }

    public void SetPlayerState(PlayerStates pState)
    {
        PlayerState = pState;
    }
    

    public bool KnownsEnemy(int pEnemyID)
    {
        return knownEnemies.Contains(ID);
        
    }
    
}