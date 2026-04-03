using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [SerializeField] private Sprite[] aces;//0
    [SerializeField] private Sprite[] c2;//1
    [SerializeField] private Sprite[] c3;//2
    [SerializeField] private Sprite[] c4;//3
    [SerializeField] private Sprite[] c5;//4
    [SerializeField] private Sprite[] c6;//5
    [SerializeField] private Sprite[] c7;//6
    [SerializeField] private Sprite[] c8;//7
    [SerializeField] private Sprite[] c9;//8
    [SerializeField] private Sprite[] c10;//9
    [SerializeField] private Sprite[] kings;//10
    [SerializeField] private Sprite[] queens;//11
    [SerializeField] private Sprite[] jokers;//12
    [SerializeField] private Sprite[] hiddenCards;//13

    private List<Sprite[]> cardSets;

    void Awake()
    {
        cardSets.Add(aces);
        cardSets.Add(c2);
        cardSets.Add(c3);
        cardSets.Add(c4);
        cardSets.Add(c5);
        cardSets.Add(c6);
        cardSets.Add(c7);
        cardSets.Add(c8);
        cardSets.Add(c9);
        cardSets.Add(c10);
        cardSets.Add(kings);
        cardSets.Add(queens);
        cardSets.Add(jokers);
        cardSets.Add(hiddenCards);
    }
    public Sprite GetCardSprite(int number)
    {
        return cardSets[number][Random.Range(0, cardSets[number].Length)];
    }
    
    
    
}
