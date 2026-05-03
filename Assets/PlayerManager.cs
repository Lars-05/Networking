using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private CardManager cardManager;
    [SerializeField] private GameObject playerDisplayerUIPrefab;
    [SerializeField] private Transform playerDisplayerParent;
    [SerializeField] private ThisPlayerActionManager thisPlayerActionManager;

    public Dictionary<int, PlayerActionDisplayer> playerActionDisplayers { get; private set; } 
        = new Dictionary<int, PlayerActionDisplayer>();
    private int highestScoreID;

    public void AddPlayer(int pID)
    {
        if (playerActionDisplayers.ContainsKey(pID))
            return;

        PlayerActionDisplayer newPlayerDisplayer =
            Instantiate(playerDisplayerUIPrefab, playerDisplayerParent)
            .GetComponent<PlayerActionDisplayer>();

        playerActionDisplayers.Add(pID, newPlayerDisplayer);
        newPlayerDisplayer.SetupPlayer(pID);
    }

    private PlayerActionDisplayer GetPlayerDisplayer(int pId)
    {
        playerActionDisplayers.TryGetValue(pId, out var displayer);
        return displayer;
    }

    public void SetupThisPlayer()
    {
        thisPlayerActionManager.Setup();
    }

    public void ClearPlayerDisplayersDecks()
    {
        foreach (var displayer in playerActionDisplayers.Values)
        {
            displayer.ClearDeck();
        }

        thisPlayerActionManager.ClearDeck();
    }

    private int highestScore = 0;
    public void AddPlayerScore(int pId, int score)
    {
        if (score > highestScore)
        {
            highestScoreID = pId;
            highestScore = score;
        }
        
        if (pId == PlayerData.id)
        {
            thisPlayerActionManager.AddScore(score);
            return;
        }

        GetPlayerDisplayer(pId)?.SetScore(score);
    }
    
    public void ShowAllPlayerScores()
    {
        thisPlayerActionManager.ShowScore(PlayerData.id == highestScoreID);

        foreach (var displayer in playerActionDisplayers.Values)
        {
            displayer.ShowScore(displayer.PlayerId == highestScoreID);
        }

        highestScoreID = 0;
    }

    public void AddCardToPlayerDeck(int pId, int pCardId)
    {
        Sprite cardSprite = cardManager.GetCardSprite(pCardId);

        if (pId == PlayerData.id)
        {
            thisPlayerActionManager.AddCardToDeck(cardSprite);
            return;
        }

        GetPlayerDisplayer(pId)?.AddCardToDeck(cardSprite);
    }

    public void RemovePlayer(int pId)
    {
        var displayer = GetPlayerDisplayer(pId);

        if (displayer != null)
        {
            Destroy(displayer.gameObject);
            playerActionDisplayers.Remove(pId);
        }
    }
    
    public void ResetAllPlayers()
    {
        thisPlayerActionManager.Reset();
        foreach (var id in playerActionDisplayers.Keys)
        {
            GetPlayerDisplayer(id)?.Reset();
        }
    }
}