using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyActionDisplayer : MonoBehaviour
{
    [Header("Player Info")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Cards")]
    [SerializeField] private Transform cardParent;
    [SerializeField] private GameObject cardHolderPrefab;

    [Header("Buttons")]
    [SerializeField] private RectTransform hitButtonRt;
    [SerializeField] private RectTransform standButtonRt;
    [SerializeField] private RectTransform readyButtonRt;
    [SerializeField] private TextMeshProUGUI readyButtonText;
    
    [Header("Additional Fields")]
    [SerializeField] private Image crownImage;
  

    
    [Header("Status Display")]
    [SerializeField] private Image statusDisplayer;
    
    [Header("Status Materials")]
    [SerializeField] private Material turnMaterial;
    [SerializeField] private Material  defaultMaterial;
    [SerializeField] private Material  outMaterial;
    [SerializeField] private Material  winnerMaterial;

    private List<GameObject> cards = new();
    private int score;

    public int PlayerId { get; private set; }

    public void SetupPlayer(int pId)
    {
        readyButtonRt.gameObject.SetActive(true);
        statusDisplayer.material = defaultMaterial;
        crownImage.enabled = false;
        PlayerId = pId;
        score = 0;

        playerNameText.text = "Player " + pId;
        scoreText.gameObject.SetActive(false);
    }

    public void OnGameStart()
    {
        readyButtonRt.gameObject.SetActive(false);
        hitButtonRt.gameObject.SetActive(true);
        standButtonRt.gameObject.SetActive(true);
    }

    public void OnTurnStart()
    {
        statusDisplayer.material = turnMaterial;
    }

    public void OnOut()
    {
        statusDisplayer.material = outMaterial;
    }

    public void OnTurnOver()
    {
        statusDisplayer.material = defaultMaterial;
    }

    public void SetScore(int pScore)
    {
        score = pScore;
        scoreText.text =  score.ToString();
    }

    public void OnWin()
    {
        statusDisplayer.material = winnerMaterial;
        crownImage.enabled = true;
    }

    public void ShowScore()
    {
        hitButtonRt.gameObject.SetActive(false);
        standButtonRt.gameObject.SetActive(false);
        readyButtonRt.gameObject.SetActive(true);

        scoreText.gameObject.SetActive(true);
        scoreText.text = "Score: " + score;
        OnUnready();
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

    public void OnReady()
    {
        readyButtonRt.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Append(readyButtonRt.DOScale(0.97f, 0.06f).SetEase(Ease.OutQuad));
        seq.Append(readyButtonRt.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));

        seq.Join(readyButtonRt.DOShakeRotation(
            duration: 0.15f,
            strength: new Vector3(0, 0, 3f),
            vibrato: 12,
            randomness: 90,
            fadeOut: true
        ));

        readyButtonText.text = "Ready";
    }

    public void OnUnready()
    {
        readyButtonRt.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Append(readyButtonRt.DOScale(0.97f, 0.06f).SetEase(Ease.OutQuad));
        seq.Append(readyButtonRt.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));

        seq.Join(readyButtonRt.DOShakeRotation(
            duration: 0.15f,
            strength: new Vector3(0, 0, 3f),
            vibrato: 12,
            randomness: 90,
            fadeOut: true
        ));

        readyButtonText.text = "Unready";
    }

    public void OnHit()
    {
        hitButtonRt.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Append(hitButtonRt.DOScale(0.97f, 0.06f).SetEase(Ease.OutQuad));
        seq.Append(hitButtonRt.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));

        seq.Join(hitButtonRt.DOShakeRotation(
            duration: 0.15f,
            strength: new Vector3(0, 0, 3f),
            vibrato: 12,
            randomness: 90,
            fadeOut: true
        ));
    }

    public void OnStand()
    {
        standButtonRt.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Append(standButtonRt.DOScale(0.97f, 0.06f).SetEase(Ease.OutQuad));
        seq.Append(standButtonRt.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));

        seq.Join(standButtonRt.DOShakeRotation(
            duration: 0.15f,
            strength: new Vector3(0, 0, 3f),
            vibrato: 12,
            randomness: 90,
            fadeOut: true
        ));
    }
}