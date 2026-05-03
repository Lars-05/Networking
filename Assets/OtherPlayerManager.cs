using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OtherPlayerManager : MonoBehaviour
{
    [SerializeField] private GameObject playerDisplayerUIPrefab;
    [SerializeField] private Transform parent;
    private Dictionary<int, PlayerActionDisplayer> playerActionDisplayers = new Dictionary<int, PlayerActionDisplayer>();

    void AddPlayer(int pID)
    {
        PlayerActionDisplayer newPlayerDisplayerAction = Instantiate(playerDisplayerUIPrefab, parent).GetComponent<PlayerActionDisplayer>();
        playerActionDisplayers.Add(pID, newPlayerDisplayerAction);
        newPlayerDisplayerAction.SetupPlayer(pID);
    }

    public PlayerActionDisplayer GetPlayerDisplayer(int pID)
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
