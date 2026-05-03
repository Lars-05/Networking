using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThisPlayerActionManager : MonoBehaviour
{
    [SerializeField] private Button readyButton;
    [SerializeField] private Button standButton;
    [SerializeField] private Button hitButton;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Transform cardParent;
    [SerializeField] private GameObject cardPrefab;

    private int score;
    private List<GameObject> cards = new();
    private bool isReady;

    public void Setup()
    {
        
        readyButton.gameObject.SetActive(true);
        standButton.gameObject.SetActive(false);
        hitButton.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);

        readyButton.interactable = true;
        hitButton.interactable = false;
        standButton.interactable = false;
        score = 0;
    }

    private void OnEnable()
    {
        EventBus.OnIsTurn += OnTurn;
        EventBus.OnGameStart += OnGameStart;
        EventBus.OnTurnOver += OnTurnOver;

        hitButton.onClick.AddListener(OnHit);
        standButton.onClick.AddListener(OnStand);
        readyButton.onClick.AddListener(ReadyButtonClicked);
    }

    private void OnDisable()
    {
        EventBus.OnIsTurn -= OnTurn;
        EventBus.OnGameStart -= OnGameStart;
        EventBus.OnTurnOver -= OnTurnOver;
        
        hitButton.onClick.RemoveListener(OnHit);
        standButton.onClick.RemoveListener(OnStand);
        readyButton.onClick.RemoveListener(ReadyButtonClicked);
    }

    private void ReadyButtonClicked()
    {
        isReady = !isReady;

        if (isReady)
        {
            EventBus.RaiseSendCommandToServer(
                OutgoingServerCommunicator.ClientCommands.IS_READY);
            return;
        }

        EventBus.RaiseSendCommandToServer(
            OutgoingServerCommunicator.ClientCommands.IS_NOT_READY);
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
            GameObject newCardSlot = Instantiate(cardPrefab, cardParent);
            newCardSlot.GetComponent<Image>().sprite = cardSprite;
            cards.Add(newCardSlot);
        }
    }

    private void OnGameStart()
    {
        readyButton.gameObject.SetActive(false);
        standButton.gameObject.SetActive(true);
        hitButton.gameObject.SetActive(true);

        standButton.interactable = false;
        hitButton.interactable = false;
    }

    private void OnTurn()
    {
        standButton.interactable = true;
        hitButton.interactable = true;
    }

    private void OnTurnOver()
    {
        standButton.interactable = false;
        hitButton.interactable = false;
    }

    private void OnHit()
    {
        EventBus.RaiseOnHit();
    }

    private void OnStand()
    {
        EventBus.RaiseOnStand();
    }

    public void AddScore(int pScore)
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
        Setup();
        isReady = false;
    }
}