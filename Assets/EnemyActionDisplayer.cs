using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyActionDisplayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button hitButton;
    [SerializeField] private Button standButton;
    [SerializeField] private Transform cardParent;
    [SerializeField] private GameObject cardHolderPrefab;

    private List<GameObject> cards = new();

    private int score;
    public int PlayerId { get; private set; }

    public void SetupPlayer(int pId)
    {
        PlayerId = pId;
        score = 0;
        playerNameText.text = "Player " + pId;

        readyButton.interactable = false;
        hitButton.interactable = false;
        standButton.interactable = false;

        hitButton.onClick.RemoveAllListeners();
        standButton.onClick.RemoveAllListeners();
        

        readyButton.gameObject.SetActive(true);
        hitButton.gameObject.SetActive(false);
        standButton.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
    }

    public void OnGameStart()
    {
        readyButton.gameObject.SetActive(false);
        hitButton.gameObject.SetActive(true);
        standButton.gameObject.SetActive(true);
    }

    public void OnTurn()
    {

    }

    public void OnTurnOver()
    {
        
    }

    public void SetScore(int pScore)
    {
        score = pScore;
    }

    public void ShowScore(bool isHighestScore)
    {
        hitButton.gameObject.SetActive(false);
        standButton.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(false);

        scoreText.gameObject.SetActive(true);
        scoreText.text = "Score: " + score;
    }
    

    public void Reset()
    {
        ClearDeck();
        SetupPlayer(PlayerId);
    }

    public void ClearDeck()
    {
        foreach (var card in cards)
        {
            Destroy(card);
        }

        cards.Clear();
    }

    public void AddCardToDeck(Sprite cardSprite)
    {
        bool foundEmptySlot = false;

        foreach (var card in cards)
        {
            Image cardImage = card.GetComponent<Image>();

            if (cardImage.sprite != null)
                continue;

            cardImage.sprite = cardSprite;
            foundEmptySlot = true;
            break;
        }

        if (!foundEmptySlot)
        {
            GameObject newCardSlot = Instantiate(cardHolderPrefab, cardParent);
            newCardSlot.GetComponent<Image>().sprite = cardSprite;
            cards.Add(newCardSlot);
        }
    }
}