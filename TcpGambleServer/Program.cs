using System.Net;
using System.Net.Sockets;
using System.Linq;

public class TcpServer
{
    public enum GameSession
    {
        WAITING_TO_START,
        INPROGRESS,
    }

    public enum GameState
    {
        IDLE,
        WAITING_FOR_PLAYERS,
        WAITING_ALL_PLAYERS_READY,
        DEALING_STARTING_CARDS,
        PLAYING_STATE,
        SHOWING_RESULT,
    }

    public enum ServerCommands
    {
        GAME_START,
        SHOW_RESULTS,
        FORCE_GAME_STOP,
        RESET_DATA
    }

    public enum ValueTypes
    {
        THISID,
        ENEMYID,
        SCORE,
        CARD,
        READY,
        UNREADY,
        HIT,
        STAND,
        STARTTURN,
        ENDTURN,
        OUT,
        WINNER,
        DISCONNECTED,
    }
    
 
    public GameState currentGameState = GameState.IDLE;
    public GameSession currentGameSession = GameSession.WAITING_TO_START;

    public Player activePlayer;

    private readonly Random rnd = new();

    private PlayerHandler playerHandler;
    private OutgoingMessageHandler outgoingMessageHandler;
    private IncomingMessageHandler incomingMessageHandler;

    private bool turnChanging = false;

    public List<Player> players => playerHandler.GetPlayersSnapshot();

    public TcpServer()
    {
        playerHandler = new PlayerHandler(this, null);
        outgoingMessageHandler = new OutgoingMessageHandler(this, playerHandler);
        playerHandler.SetOutgoing(outgoingMessageHandler);
        incomingMessageHandler = new IncomingMessageHandler(this, outgoingMessageHandler, playerHandler);
    }

    public static void Main()
    {
        TcpServer server = new TcpServer();
        server.StartServer(50001);
    }

    public void StartServer(int port)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        currentGameState = GameState.WAITING_FOR_PLAYERS;

        while (true)
        {
            if (currentGameState == GameState.WAITING_FOR_PLAYERS && currentGameSession == GameSession.WAITING_TO_START)
                playerHandler.AcceptNewClients(listener);

            incomingMessageHandler.ProcessIncomingMessages();
            playerHandler.CleanupClients();

            System.Threading.Thread.Sleep(10);
        }
    }


    public void StartGame()
    {

        
        ClearAllMatchData();
        
        var snapshot = players;

        if (snapshot.Count == 0)
            return;

        outgoingMessageHandler.BroadcastCommand(ServerCommands.GAME_START);
        
        // Mark Game As Started, doesnt allow new players to join
        currentGameSession = GameSession.INPROGRESS;

        DealStartingCards();
        PassTurn();
    }

    public void DealStartingCards()
    {
        currentGameState = GameState.DEALING_STARTING_CARDS;

        var snapshot = players;

        foreach (var player in snapshot)
        {
            DealCard(player, true);
            player.score = CalculateScore(player.cardsDrawn);
            outgoingMessageHandler.SendValueToPlayer(player,ValueTypes.SCORE,  new []{player.ID, player.score});
        }

        activePlayer = snapshot.FirstOrDefault(p => p.PlayerState != PlayerStates.OUT);

        if (activePlayer == null)
        {
            DisplayScores();
            return;
        }

        currentGameState = GameState.PLAYING_STATE;
    }

    public void DealCard(Player pPlayer, bool isPrivate)
    {
        if (pPlayer == null) return;

        int card = rnd.Next(0, 13);
        pPlayer.cardsDrawn.Add(card);

        if (isPrivate)
        {
            outgoingMessageHandler.SendValueToPlayer(pPlayer, ValueTypes.CARD, new[]{pPlayer.ID, card});
            outgoingMessageHandler.BroadcastValue(ValueTypes.CARD, new[]{pPlayer.ID, 13},pPlayer);
        }
        else
        {
            outgoingMessageHandler.BroadcastValue(ValueTypes.CARD, new[]{pPlayer.ID, card});
        }
    }

    public void OnHit(Player pPlayer)
    {
        DealCard(pPlayer, false);

        pPlayer.score = CalculateScore(pPlayer.cardsDrawn);
        outgoingMessageHandler.SendValueToPlayer(pPlayer, ValueTypes.SCORE, new []{pPlayer.ID, pPlayer.score});

        if (pPlayer.score >= 21)
        {
            playerHandler.SetPlayerState(pPlayer, PlayerStates.OUT);
            outgoingMessageHandler.BroadcastValue(TcpServer.ValueTypes.OUT, new []{pPlayer.ID});
        }
        else
            playerHandler.SetPlayerState(pPlayer, PlayerStates.WAITING_FOR_TURN);
        PassTurn();
    }

    public void OnStand(Player pPlayer)
    {
        playerHandler.SetPlayerState(pPlayer, PlayerStates.OUT);
        outgoingMessageHandler.BroadcastValue(TcpServer.ValueTypes.OUT, new []{pPlayer.ID});
        PassTurn();
    }

    public void PassTurn()
    {
        var snapshot = players;

        if (snapshot.Count == 0)
            return;

        if (activePlayer == null || !snapshot.Contains(activePlayer))
        {
            activePlayer = snapshot.FirstOrDefault(p => p.PlayerState != PlayerStates.OUT);

            if (activePlayer == null)
            {
                DisplayScores();
                return;
            }

            GiveTurn(activePlayer);
            return;
        }

        outgoingMessageHandler.BroadcastValue(ValueTypes.ENDTURN, new []{activePlayer.ID});

        int currentIndex = snapshot.IndexOf(activePlayer);

        if (currentIndex == -1)
        {
            activePlayer = snapshot.FirstOrDefault(p => p.PlayerState != PlayerStates.OUT);

            if (activePlayer == null)
            {
                DisplayScores();
                return;
            }

            GiveTurn(activePlayer);
            return;
        }

        for (int i = 1; i <= snapshot.Count; i++)
        {
            int index = (currentIndex + i) % snapshot.Count;
            var next = snapshot[index];

            if (next.PlayerState != PlayerStates.OUT)
            {
                GiveTurn(next);
                return;
            }
        }

        DisplayScores();
      
    }

    public void GiveTurn(Player player)
    {
        if (player == null) return;

        if (turnChanging) return;
        turnChanging = true;

        activePlayer = player;
        player.PlayerState = PlayerStates.PLAYING_TURN;

        outgoingMessageHandler.BroadcastValue(ValueTypes.STARTTURN, new []{player.ID});

        turnChanging = false;
    }

    public void DisplayScores()
    {
        // Mark Game As Waiting for start, allow new players to join
        currentGameSession = GameSession.WAITING_TO_START;
        
        currentGameState = GameState.SHOWING_RESULT;
        outgoingMessageHandler.BroadcastCommand(ServerCommands.SHOW_RESULTS);

        Player highestScorePlayer = null;
        int highestScore = 0;

        foreach (var player in playerHandler.GetPlayersSnapshot())
        {
            outgoingMessageHandler.BroadcastValue(TcpServer.ValueTypes.SCORE, new []{player.ID,player.score});

            if (player.score <= 21 && player.score > highestScore)
            {
                highestScore = player.score;
                highestScorePlayer = player;
            }
        }


        if (highestScorePlayer != null)
        {
            DisplayWinner(highestScorePlayer);
        }
        currentGameState = GameState.WAITING_ALL_PLAYERS_READY;
    }

    public void ClearAllMatchData()
    {
        outgoingMessageHandler.BroadcastCommand(ServerCommands.RESET_DATA);
        foreach (var player in playerHandler.GetPlayersSnapshot())
        {
            player.ClearData();
        }
    }

    public void DisplayWinner(Player pPlayer)
    {
        outgoingMessageHandler.BroadcastValue( ValueTypes.WINNER,new []{ pPlayer.ID});
    }

    public int CalculateScore(List<int> cards)
    {
        int total = 0;
        int aces = 0;

        foreach (int card in cards)
        {
            if (card == 0) { total += 11; aces++; }
            else if (card >= 10) total += 10;
            else total += card + 1;
        }

        while (total > 21 && aces > 0)
        {
            total -= 10;
            aces--;
        }

        return total;
    }
}