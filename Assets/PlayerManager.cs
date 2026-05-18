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
        if (pId == PlayerData.id)
        {
            thisPlayerActionManager.SetScore(score);
            return;
        }

        GetPlayerDisplayer(pId)?.SetScore(score);
    }

    public void ShowAllPlayerScores()
    {
        thisPlayerActionManager.ShowFinalScore();
        foreach (var enemy in enemyActionDisplayers.Values)
        {
            enemy.ShowScore();
        }
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
    
    
    public void OnWin(int pId)
    {
        
        if (pId == PlayerData.id)
        {
            thisPlayerActionManager.OnWin();
            return;
        }
        foreach (var enemy in enemyActionDisplayers)
        {
            if (enemy.Value.PlayerId == pId)
            {
                enemy.Value.OnWin();
                return;
            }
        }
        
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
    


    public void OnPlayerDisconnect(int pId)
    {
        RemovePlayer(pId);
        ResetGame();
    }

    public void ResetGame()
    {
        thisPlayerActionManager.Reset();
        foreach (var enemy in enemyActionDisplayers)
        {
            enemy.Value.Reset();
        }
    }

    private EnemyActionDisplayer GetPlayerDisplayer(int pId)
    {
        enemyActionDisplayers.TryGetValue(pId, out var displayer);
        return displayer;
    }
}