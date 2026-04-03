using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public class PlayerActionDisplayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText; 
    [SerializeField] private Button readyButton; 
    [SerializeField] private Button hitButton;
    [SerializeField] private Button standButton;
    [SerializeField] private Transform cardDisplayLayout;
    [SerializeField] private GameObject cardHolderPrefab; 
    [SerializeField] private List<GameObject> cards = new List<GameObject>();

    private bool isReady;
    public int playerId {get; private set; }

    void SetupPlayer(int pId)
    {
        playerId = pId;
        playerNameText.text = "Player " + pId;
    }
    
    public void AddCardToDeck(Sprite cardSprite)
    {
        bool foundExistingEmptyCardSlot = false;

        for (int i = 0; i < cards.Count; i++)
        {
            Image cardImage = cards[i].GetComponent<Image>();
            if(cardImage.sprite == null)
                continue;
            
            cardImage.sprite = cardSprite;
            foundExistingEmptyCardSlot = true;
        }

        if (!foundExistingEmptyCardSlot)
        {
            GameObject newCardSlot = Instantiate(cardHolderPrefab, cardDisplayLayout);
            newCardSlot.GetComponent<Image>().sprite = cardSprite;
            cards.Add(newCardSlot);
        }
    }
}
