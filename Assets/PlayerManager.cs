using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private CardManager cardManager;
    [SerializeField] private GameObject playerDisplayerUIPrefab;
    [SerializeField] private Transform playerDisplayerParent;
    [SerializeField] private ThisPlayerActionManager thisPlayerActionManager;

    public Dictionary<int, EnemyActionDisplayer> enemyActionDisplayers { get; private set; }
        = new Dictionary<int, EnemyActionDisplayer>();

    private int highestScoreID;
    private int highestScore = 0;
    

    public void OnGameStart()
    {
        thisPlayerActionManager.OnGameStart();
        foreach (var enemy in enemyActionDisplayers)
        {
            enemy.Value.OnGameStart();
        }
    }


    public void SetupThisPlayer()
    {
        thisPlayerActionManager.Setup();
    }

    public void AddPlayer(int pID)
    {
        if (enemyActionDisplayers.ContainsKey(pID))
            return;

        EnemyActionDisplayer newEnemyDisplayer =
            Instantiate(playerDisplayerUIPrefab, playerDisplayerParent)
            .GetComponent<EnemyActionDisplayer>();

        enemyActionDisplayers.Add(pID, newEnemyDisplayer);
        newEnemyDisplayer.SetupPlayer(pID);
    }

    public void RemovePlayer(int pId)
    {
        var displayer = GetPlayerDisplayer(pId);

        if (displayer != null)
        {
            Destroy(displayer.gameObject);
            enemyActionDisplayers.Remove(pId);
        }
    }

    public void ResetAllPlayers()
    {
        thisPlayerActionManager.Reset();

        foreach (var id in enemyActionDisplayers.Keys)
        {
            GetPlayerDisplayer(id)?.Reset();
        }
    }

    public void ClearPlayerDisplayersDecks()
    {
        foreach (var displayer in enemyActionDisplayers.Values)
        {
            displayer.ClearDeck();
        }

        thisPlayerActionManager.ClearDeck();
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

        foreach (var displayer in enemyActionDisplayers.Values)
        {
            displayer.ShowScore(displayer.PlayerId == highestScoreID);
        }

        highestScoreID = 0;
    }

    public void OnHit(int pId)
    {
        foreach (var enemy in enemyActionDisplayers)
        {
            if (enemy.Value.PlayerId == pId)
            {
                enemy.Value.OnHit();
                return;
                
            }
        }
    }

    public void OnStand(int pId)
    {
        foreach (var enemy in enemyActionDisplayers)
        {
            if (enemy.Value.PlayerId == pId)
            {
                enemy.Value.OnStand();
                return;
                
            }
        }
    }

    public void OnReady(int pId)
    {
        foreach (var enemy in enemyActionDisplayers)
        {
            if (enemy.Value.PlayerId == pId)
            {
                enemy.Value.OnReady();
                return;
                
            }
        }
    }

    public void OnUnReady(int pId)
    {
        foreach (var enemy in enemyActionDisplayers)
        {
            if (enemy.Value.PlayerId == pId)
            {
                enemy.Value.OnUnready();
                return;
            }
        }
    }
    
    public void OnTurnOver(int pId)
    {

        if (pId == PlayerData.id)
        {
            thisPlayerActionManager.OnTurnOver();
            return;
        }
        foreach (var enemy in enemyActionDisplayers)
        {
            if (enemy.Value.PlayerId == pId)
            {
                enemy.Value.OnTurnOver();
                return;
            }
        }
    }
    
    
    public void OnWin()
    {
        /*
        if (pId == PlayerData.id)
        {
            thisPlayerActionManager.OnTurnOver();
            return;
        }
        foreach (var enemy in enemyActionDisplayers)
        {
            if (enemy.Value.PlayerId == pId)
            {
                enemy.Value.OnTurnOver();
                return;
            }
        }
        */
    }
    
    public void OnTurnStart(int pId)
    {

        if (pId == PlayerData.id)
        {
            thisPlayerActionManager.OnTurnStart();
            return;
        }
        foreach (var enemy in enemyActionDisplayers)
        {
            if (enemy.Value.PlayerId == pId)
            {
                enemy.Value.OnTurnStart();
                return;
            }
        }
    }
    
    public void OnOut(int pId)
    {
        foreach (var enemy in enemyActionDisplayers)
        {
            if (enemy.Value.PlayerId == pId)
            {
                enemy.Value.OnOut();
                return;
            }
        }
    }

    private EnemyActionDisplayer GetPlayerDisplayer(int pId)
    {
        enemyActionDisplayers.TryGetValue(pId, out var displayer);
        return displayer;
    }
}