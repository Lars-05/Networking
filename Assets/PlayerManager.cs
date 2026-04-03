using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private GameObject playerDisplayerUIPrefab;
    [SerializeField] private Transform playerDisplayerLayout;
    [SerializeField] private CardManager cardManager;
    private List<PlayerActionDisplayer> players;
    
    public void InstantiateNewPlayer()
    {
        players.Add(Instantiate(playerDisplayerUIPrefab, playerDisplayerLayout).GetComponent<PlayerActionDisplayer>());
    }

    public void GiveCardToPlayer(int pId, int pCardId)
    {
        players[pId].AddCardToDeck(cardManager.GetCardSprite(pCardId));
    }
    
 
    
}
