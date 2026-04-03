using System.Net;
using System.Net.Sockets;

public class TcpServer
{
    #region Enums

    public enum GameSession
    {
        WAITING,
        INPROGRESS,
        ENDED
    }

    public enum GameState
    {
        IDLE,
        WAITING_FOR_PLAYERS,
        WAITING_ALL_PLAYERS_READY,
        DEAL_CARDS,
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
        MATCH_FULL
    }

    public enum ValueTypes
    {
        HAND,
        CARD,
        ENEMYCARD,
        SCORE
    }

    #endregion


    public GameState currentGameState = GameState.IDLE;

    public GameSession gameSession = GameSession.WAITING;

    public Player activePlayer;

    private Random rnd = new();

    private OutgoingMessageHandler outgoingMessageHandler;
    private IncomingMessageHandler incomingMessageHandler;
    private PlayerHandler playerHandler;

    public TcpServer()
    {
        outgoingMessageHandler = new OutgoingMessageHandler(this, playerHandler);
        playerHandler = new PlayerHandler(this, outgoingMessageHandler);
        incomingMessageHandler = new IncomingMessageHandler(this, outgoingMessageHandler, playerHandler);
    }

    public static void Main()
    {
        TcpServer server = new TcpServer();
        server.StartServer(50001);
    }

    public void StartServer(int port)
    {
        TcpListener listener =
            new TcpListener(IPAddress.Any, port);

        listener.Start();

        Console.WriteLine(
            $"TCP server started on port {port}"
        );

        currentGameState =
            GameState.WAITING_FOR_PLAYERS;

        while (true)
        {
            if (currentGameState == GameState.WAITING_FOR_PLAYERS)
            {
                playerHandler.AcceptNewClients(listener);
            }

            incomingMessageHandler.ProcessIncomingMessages();
            playerHandler.CleanupClients();

            Thread.Sleep(10);
        }
    }

    public void StopGameAbruptly(string reason)
    {
        Console.WriteLine("Stopped game: " + reason);
    }

    public List<Player> players => playerHandler.players;

    public void OnHit(Player player)
    {
        DealCard(player);

        player.score =
            CalculateScore(player.cardsDrawn);

        if (player.score >= 21)
        {
            player.PlayerState =
                PlayerStates.TURNS_DONE;
        }

        PassTurn();
    }

    public void OnStand(Player player)
    {
        player.PlayerState =
            PlayerStates.TURNS_DONE;

        PassTurn();
    }

    public void DealCard(Player player)
    {
        int card = rnd.Next(0, 13);

        player.cardsDrawn.Add(card);

        outgoingMessageHandler.SendValueToPlayer(
            player,
            ValueTypes.CARD,
            card
        );
    }

    public int CalculateScore(List<int> cards)
    {
        int total = 0;
        int aces = 0;

        foreach (int card in cards)
        {
            if (card == 0)
            {
                total += 11;
                aces++;
            }
            else if (card >= 10)
            {
                total += 10;
            }
            else
            {
                total += card + 1;
            }
        }

        while (total > 21 && aces > 0)
        {
            total -= 10;
            aces--;
        }

        return total;
    }

    public void PassTurn()
    {
        activePlayer.PlayerState = PlayerStates.WAITING_FOR_TURN;

        int currentIndex = players.IndexOf(activePlayer);

        for (int i = 1; i < players.Count; i++)
        {
            int index =
                (currentIndex + i) % players.Count;

            if (players[index].PlayerState ==
                PlayerStates.TURNS_DONE)
                continue;

            GiveTurn(players[index]);
            break;
        }
    }

    public void GiveTurn(Player player)
    {
        activePlayer = player;

        player.PlayerState =
            PlayerStates.PLAYING_TURN;

        outgoingMessageHandler
            .SendCommandToPlayer(
                player,
                ServerCommands.IS_TURN
            );
    }

    public bool CheckAllPlayerForState(PlayerStates state)
    {
        return players.All(p => p.PlayerState == state);
    }

    public void SetAllPlayersState(PlayerStates state)
    {
        foreach (var player in players)
        {
            player.PlayerState = state;
        }
    }
}