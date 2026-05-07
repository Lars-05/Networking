using System.Net;
using System.Net.Sockets;
using System.Linq;

public class TcpServer
{
    public enum GameSession
    {
        WAITING,
        INPROGRESS,
    }

    public enum GameState
    {
        IDLE,
        WAITING_FOR_PLAYERS,
        WAITING_ALL_PLAYERS_READY,
        DEALING_STARTING_CARDS,
        PLAYING_STATE,
        SHOW_RESULT,
        RESET_ROUND
    }

    public enum ServerCommands
    {
        GAME_START,
        WAITING_FOR_PLAYER_TURN,
        IS_TURN,
        REVEAL_CARDS,
        END_TURN,
        SHOW_RESULTS,
        GAME_STOP,
        FORCE_GAME_STOP,
        MATCH_FULL,
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
    public GameSession gameSession = GameSession.WAITING;

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
            if (currentGameState == GameState.WAITING_FOR_PLAYERS)
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

        outgoingMessageHandler.SendCommandToAllPlayers(ServerCommands.GAME_START);

        gameSession = GameSession.INPROGRESS;

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
            outgoingMessageHandler.ValueHandler(ValueTypes.SCORE, player, player.score);
        }

        activePlayer = snapshot.FirstOrDefault(p => p.PlayerState != PlayerStates.OUT);

        if (activePlayer == null)
        {
            DisplayScores();
            return;
        }

        currentGameState = GameState.PLAYING_STATE;
    }

    public void DealCard(Player player, bool isPrivate)
    {
        if (player == null) return;

        int card = rnd.Next(0, 13);
        player.cardsDrawn.Add(card);

        if (isPrivate)
        {
            outgoingMessageHandler.SendCardToPlayer(player, card);
            outgoingMessageHandler.SendCardToAllPlayers(player, 13, player);
        }
        else
        {
            outgoingMessageHandler.SendCardToAllPlayers(player, card);
        }
    }

    public void OnHit(Player pPlayer)
    {
        DealCard(pPlayer, false);

        pPlayer.score = CalculateScore(pPlayer.cardsDrawn);
        outgoingMessageHandler.ValueHandler(ValueTypes.SCORE, pPlayer, pPlayer.score);

        if (pPlayer.score >= 21)
            playerHandler.SetAndUpdatePlayersOfPlayerState(pPlayer, PlayerStates.OUT);
        else
            playerHandler.SetAndUpdatePlayersOfPlayerState(pPlayer, PlayerStates.WAITING_FOR_TURN);

        PassTurn();
    }

    public void OnStand(Player pPlayer)
    {
        playerHandler.SetAndUpdatePlayersOfPlayerState(pPlayer, PlayerStates.OUT);
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

        outgoingMessageHandler.SendValueToAllPlayers(ValueTypes.ENDTURN, activePlayer.ID);

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

        outgoingMessageHandler.SendValueToAllPlayers(ValueTypes.STARTTURN, player.ID);

        turnChanging = false;
    }

    public void DisplayScores()
    {
        gameSession = GameSession.WAITING;
        currentGameState = GameState.WAITING_ALL_PLAYERS_READY;
        outgoingMessageHandler.SendCommandToAllPlayers(ServerCommands.SHOW_RESULTS);

        Player highestScorePlayer = null;
        int highestScore = 0;

        foreach (var player in playerHandler.GetPlayersSnapshot())
        {
            outgoingMessageHandler.SendValueToAllPlayers( TcpServer.ValueTypes.SCORE , player.score);

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
    }

    public void ClearAllMatchData()
    {
        outgoingMessageHandler.SendCommandToAllPlayers(ServerCommands.RESET_DATA);
        foreach (var player in playerHandler.GetPlayersSnapshot())
        {
            player.ClearData();
        }
    }

    public void DisplayWinner(Player pPlayer)
    {
        outgoingMessageHandler.SendValueToAllPlayers( ValueTypes.WINNER, pPlayer.ID);
    }

    public void StopGameAbruptly(string reason)
    {
        gameSession = GameSession.WAITING;
        currentGameState = GameState.WAITING_FOR_PLAYERS;
        activePlayer = null;
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

    public void SetAllPlayersState(PlayerStates state)
    {
        foreach (var player in players)
            player.PlayerState = state;
    }

    public bool CheckAllPlayerForState(PlayerStates state)
    {
        return players.All(p => p.PlayerState == state);
    }
}