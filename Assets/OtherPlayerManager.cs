using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OtherPlayerManager : MonoBehaviour
{
    [SerializeField] private GameObject playerDisplayerUIPrefab;
    [SerializeField] private Transform parent;
    private Dictionary<int, EnemyActionDisplayer> playerActionDisplayers = new Dictionary<int, EnemyActionDisplayer>();

    void AddPlayer(int pID)
    {
        EnemyActionDisplayer newEnemyDisplayerAction = Instantiate(playerDisplayerUIPrefab, parent).GetComponent<EnemyActionDisplayer>();
        playerActionDisplayers.Add(pID, newEnemyDisplayerAction);
        newEnemyDisplayerAction.SetupPlayer(pID);
    }

    public EnemyActionDisplayer GetPlayerDisplayer(int pID)
    {
        foreach (int ids in playerActionDisplayers.Keys)
        {
            if (pID != ids)
            {
                continue;
            }
            return playerActionDisplayers[ids];
        }
        return null;
    }
    
}
