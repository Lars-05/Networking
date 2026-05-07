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
    [SerializeField] private TextMeshProUGUI readyButtonText;
    [SerializeField] private TextMeshProUGUI cardValueText;

    [SerializeField] private Transform cardParent;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Image crownImage;

    private int score;
    private bool isReady;
    private List<GameObject> cards = new();

    private void OnEnable()
    {
        hitButton.onClick.AddListener(OnHit);
        standButton.onClick.AddListener(OnStand);
        readyButton.onClick.AddListener(ReadyButtonClicked);
    }

    private void OnDisable()
    {
        hitButton.onClick.RemoveListener(OnHit);
        standButton.onClick.RemoveListener(OnStand);
        readyButton.onClick.RemoveListener(ReadyButtonClicked);
    }

    public void Awake()
    {
        readyButton.interactable = false;
        standButton.gameObject.SetActive(false);
        hitButton.gameObject.SetActive(false);
    }
    public void Setup()
    {
        crownImage.enabled = false;
        readyButton.gameObject.SetActive(true);
        standButton.gameObject.SetActive(false);
        hitButton.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);

        readyButton.interactable = true;
        
        readyButtonText.text = "Status: Unready";
        readyButton.interactable = true;

        hitButton.interactable = false;
        standButton.interactable = false;

        score = 0;
    }

    public void OnGameStart()
    {
        readyButton.gameObject.SetActive(false);
        standButton.gameObject.SetActive(true);
        hitButton.gameObject.SetActive(true);

        standButton.interactable = false;
        hitButton.interactable = false;
    }

    public void OnTurnStart()
    {
        standButton.interactable = true;
        hitButton.interactable = true;
    }

    public void OnTurnOver()
    {
        standButton.interactable = false;
        hitButton.interactable = false;
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

    public void ClearDeck()
    {
        foreach (var card in cards)
        {
            Destroy(card);
        }

        cards.Clear();
    }

    public void AddScore(int pScore)
    {
        score = pScore;
        cardValueText.text = "Deck Value: " + score;
    }

    public void ShowScore()
    {
        hitButton.gameObject.SetActive(false);
        standButton.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(true);
        OnUnReady();
        scoreText.gameObject.SetActive(true);
        scoreText.text = "Score: " + score;
    }

    public void Reset()
    {
        ClearDeck();
        Setup();
    }

    private void ReadyButtonClicked()
    {
        isReady = !isReady;

        if (isReady)
        {
            OnReady();
        }
        else
        {
            OnUnReady();
        }
    }

    private void OnReady()
    {
        isReady = true;
        EventBus.RaiseSendCommandToServer(
            OutgoingServerCommunicator.ClientCommands.IS_READY);
        readyButtonText.text = "Status: Ready";
    }
    
    private void OnUnReady()
    {
        isReady = false;
        EventBus.RaiseSendCommandToServer(
            OutgoingServerCommunicator.ClientCommands.IS_NOT_READY);

        readyButtonText.text = "Status: Unready";
    }

    public void OnWin()
    {
        crownImage.enabled = true;
    }

    private void OnHit()
    {
        EventBus.RaiseSendCommandToServer(
            OutgoingServerCommunicator.ClientCommands.HIT);
    }

    private void OnStand()
    {
        EventBus.RaiseSendCommandToServer(
            OutgoingServerCommunicator.ClientCommands.STAND);
    }
}