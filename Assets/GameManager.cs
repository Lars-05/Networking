using System;
using TMPro;
using Unity.VisualScripting;     
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public int points {private set; get;}
    [SerializeField] NumberPresenter numberPresenter;
    [SerializeField] GameObject blackjackUI;
    [SerializeField] CardManager cardManager;
    [SerializeField] Button hitButton;
    [SerializeField] Button standButton;
    [SerializeField] TextMeshProUGUI pointsText;
    [SerializeField] private GameObject StartScreenUI;
    [SerializeField] private GameObject BlackjackUI;
    [SerializeField] private GameObject ResultScreenUI;

    private bool isTurn;

    void OnEnable()
    {
        EventBus.OnIsTurn += OnTurn;
        EventBus.OnForceGameStop += OnForceStop;
        EventBus.OnGameStart += OnGameStart;
        EventBus.OnShowResults += OnShowResult;
        //EventBus.OnGameStop += OnGameStop;
    }
    
    private void OnGameStop()
    {
        BlackjackUI.SetActive(false);
        StartScreenUI.SetActive(false);
        ResultScreenUI.SetActive(true);
    }
    private void OnGameStart()
    {
        StartScreenUI.SetActive(false);
        BlackjackUI.SetActive(true);
        ResultScreenUI.SetActive(false);
    }

    public void OnShowResult()
    {
        ResultScreenUI.SetActive(true);
        StartScreenUI.SetActive(false);
        BlackjackUI.SetActive(false);
    }

    public void OnResultAccepted()
    {
        ResultScreenUI.SetActive(false);
        StartScreenUI.SetActive(true);
        BlackjackUI.SetActive(false);
    }
    
    
   
    void OnDisable()
    {
        EventBus.OnIsTurn -= OnTurn;
        EventBus.OnForceGameStop -= OnForceStop;
        EventBus.OnGameStart -= OnGameStart;
        EventBus.OnGameStop -= OnGameStop;
    }
    private void Awake()
    {
        hitButton.interactable = false;
        standButton.interactable = false;
        standButton.onClick.AddListener(OnStand);
        hitButton.onClick.AddListener(OnHit);
        StartScreenUI.SetActive(true);
        BlackjackUI.SetActive(false);
        ResultScreenUI.SetActive(false);
    }

    private void OnTurn()
    {
        numberPresenter.DisplayNumber(0);
        blackjackUI.gameObject.SetActive(true);
        points = 0;
        hitButton.interactable = true;
        standButton.interactable = true;
        isTurn = true;
    }
    

    private void OnHit()
    {
        RollRandomNumber();
        pointsText.text = points.ToString();
        if (points >= 21)
            StopTurn();
    }

    private void OnStand()
    {
        StopTurn();
    }

    private void StopTurn()
    {
        isTurn = false;
        hitButton.interactable = false;
        standButton.interactable = false;
        blackjackUI.gameObject.SetActive(false);
        EventBus.RaiseSendCommandToServer(OutgoingServerCommunicator.ClientCommands.TURN_DONE);
    }
    
    private void OnForceStop()
    {
        isTurn = false;
        hitButton.interactable = false;
        standButton.interactable = false;
        blackjackUI.gameObject.SetActive(false);
    }
    
    void RollRandomNumber()
    {
        Debug.Log("Rolling random number");
        int number = Random.Range(1, 14);
        switch (number)
        {
            case 1:
                number = points + 11 > 21 ? 1 : 11;
                break;
                
            case > 10:
                number = 10;
            break;
        }
        
        points += number;
    }
}
